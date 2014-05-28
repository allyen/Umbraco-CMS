using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository specific to the Recycle Bins
    /// available for Content and Media.
    /// </summary>
    internal class RecycleBinRepository : DisposableObject
    {
        private readonly IDatabaseUnitOfWork _unitOfWork;

        public RecycleBinRepository(IDatabaseUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Gets a list of files, which are referenced on items in the Recycle Bin.
        /// The list is generated by the convention that a file is referenced by 
        /// the Upload data type or a property type with the alias 'umbracoFile'.
        /// </summary>
        /// <param name="nodeObjectType"></param>
        /// <returns></returns>
        public List<string> GetFilesInRecycleBin(Guid nodeObjectType)
        {
            var db = _unitOfWork.Database;

            //Issue query to get all trashed content or media that has the Upload field as a property
            //The value for each field is stored in a list: FilesToDelete<string>()
            //Alias: Constants.Conventions.Media.File and PropertyEditorAlias: Constants.PropertyEditors.UploadField
            var sql = new Sql();
            sql.Select("DISTINCT(dataNvarchar)")
                .From<PropertyDataDto>()
                .InnerJoin<NodeDto>().On<PropertyDataDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<PropertyTypeDto>().On<PropertyDataDto, PropertyTypeDto>(left => left.PropertyTypeId, right => right.Id)
                .InnerJoin<DataTypeDto>().On<PropertyTypeDto, DataTypeDto>(left => left.DataTypeId, right => right.DataTypeId)
                .Where("umbracoNode.trashed = '1' AND umbracoNode.nodeObjectType = @NodeObjectType AND dataNvarchar IS NOT NULL AND (cmsPropertyType.Alias = @FileAlias OR cmsDataType.propertyEditorAlias = @PropertyEditorAlias)",
                    new { FileAlias = Constants.Conventions.Media.File, NodeObjectType = nodeObjectType, PropertyEditorAlias = Constants.PropertyEditors.UploadFieldAlias });

            var files = db.Fetch<string>(sql);
            return files;
        }

        /// <summary>
        /// Gets a list of Ids for each of the items in the Recycle Bin.
        /// </summary>
        /// <param name="nodeObjectType"></param>
        /// <returns></returns>
        public List<int> GetIdsOfItemsInRecycleBin(Guid nodeObjectType)
        {
            var db = _unitOfWork.Database;

            var idsSql = new Sql();
            idsSql.Select("DISTINCT(id)")
                .From<NodeDto>()
                .Where<NodeDto>(x => x.Trashed && x.NodeObjectType == nodeObjectType);

            var ids = db.Fetch<int>(idsSql);
            return ids;
        }

        /// <summary>
        /// Empties the Recycle Bin by running single bulk-Delete queries
        /// against the Content- or Media's Recycle Bin.
        /// </summary>
        /// <param name="nodeObjectType"></param>
        /// <returns></returns>
        public bool EmptyRecycleBin(Guid nodeObjectType)
        {
            var db = _unitOfWork.Database;

            //Construct and execute delete statements for all trashed items by 'nodeObjectType'
            var deletes = new List<string>
                          {
                              FormatDeleteStatement("umbracoUser2NodeNotify", "nodeId"),
                              FormatDeleteStatement("umbracoUser2NodePermission", "nodeId"),
                              FormatDeleteStatement("umbracoRelation", "parentId"),
                              FormatDeleteStatement("umbracoRelation", "childId"),
                              FormatDeleteStatement("cmsTagRelationship", "nodeId"),
                              FormatDeleteStatement("umbracoDomains", "domainRootStructureID"),
                              FormatDeleteStatement("cmsDocument", "nodeId"),
                              FormatDeleteStatement("cmsPropertyData", "contentNodeId"),
                              FormatDeleteStatement("cmsPreviewXml", "nodeId"),
                              FormatDeleteStatement("cmsContentVersion", "ContentId"),
                              FormatDeleteStatement("cmsContentXml", "nodeId"),
                              FormatDeleteStatement("cmsContent", "nodeId"),
                              "UPDATE umbracoNode SET parentID = '-20' WHERE trashed = '1' AND nodeObjectType = @NodeObjectType",
                              "DELETE FROM umbracoNode WHERE trashed = '1' AND nodeObjectType = @NodeObjectType"
                          };

            //Wraps in transaction - this improves performance and also ensures
            // that if any of the deletions fails that the whole thing is rolled back.
            using (var trans = db.GetTransaction())
            {
                try
                {
                    foreach (var delete in deletes)
                    {
                        db.Execute(delete, new { NodeObjectType = nodeObjectType });
                    }

                    trans.Complete();

                    return true;
                }
                catch (Exception ex)
                {
                    trans.Dispose();
                    LogHelper.Error<RecycleBinRepository>("An error occurred while emptying the Recycle Bin: " + ex.Message, ex);
                    return false;
                }
            }
        }

        /// <summary>
        /// Deletes all files passed in.
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public bool DeleteFiles(IEnumerable<string> files)
        {
            try
            {
                var fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
                Parallel.ForEach(files, file =>
                {
                    if (file.IsNullOrWhiteSpace()) return;
                    if (UmbracoConfig.For.UmbracoSettings().Content.UploadAllowDirectories)
                    {
                        var relativeFilePath = fs.GetRelativePath(file);
                        var parentDirectory = System.IO.Path.GetDirectoryName(relativeFilePath);
                        fs.DeleteDirectory(parentDirectory, true);
                    }
                    else
                    {
                        fs.DeleteFile(file, true);
                    }
                });

                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error<RecycleBinRepository>("An error occurred while deleting files attached to deleted nodes: " + ex.Message, ex);
                return false;
            }
        }

        private string FormatDeleteStatement(string tableName, string keyName)
        {
            //This query works with sql ce and sql server:
            //DELETE FROM umbracoUser2NodeNotify WHERE umbracoUser2NodeNotify.nodeId IN 
            //(SELECT nodeId FROM umbracoUser2NodeNotify as TB1 INNER JOIN umbracoNode as TB2 ON TB1.nodeId = TB2.id WHERE TB2.trashed = '1' AND TB2.nodeObjectType = 'C66BA18E-EAF3-4CFF-8A22-41B16D66A972')
            return
                string.Format(
                    "DELETE FROM {0} WHERE {0}.{1} IN (SELECT TB1.{1} FROM {0} as TB1 INNER JOIN umbracoNode as TB2 ON TB1.{1} = TB2.id WHERE TB2.trashed = '1' AND TB2.nodeObjectType = @NodeObjectType)",
                    tableName, keyName);
        }

        /// <summary>
        /// Dispose disposable properties
        /// </summary>
        /// <remarks>
        /// Ensure the unit of work is disposed
        /// </remarks>
        protected override void DisposeResources()
        {
            _unitOfWork.DisposeIfDisposable();
        }
    }
}
