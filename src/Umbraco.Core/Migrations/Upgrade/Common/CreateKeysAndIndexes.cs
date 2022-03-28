using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Migrations.Upgrade.Common
{
    public class CreateKeysAndIndexes : MigrationBase
    {
        public CreateKeysAndIndexes(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            // remove those that may already have keys
            Delete.KeysAndIndexes(Constants.DatabaseSchema.Tables.KeyValue).Do();
            Delete.KeysAndIndexes(Constants.DatabaseSchema.Tables.PropertyData).Do();

            // re-create *all* keys and indexes
            var tables = SqlSyntax.GetTablesInSchema(Context.Database);
            foreach (var x in DatabaseSchemaCreator.OrderedTables)
            {
                // prevent creation of keys for tables from newer migrations
                var tableDefinition = DefinitionFactory.GetTableDefinition(x, SqlSyntax);
                if (tables.InvariantContains(tableDefinition.Name))
                    Create.KeysAndIndexes(x).Do();
            }
        }
    }
}
