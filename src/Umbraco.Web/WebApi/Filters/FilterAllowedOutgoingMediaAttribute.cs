using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Filters;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// This inspects the result of the action that returns a collection of content and removes 
    /// any item that the current user doesn't have access to
    /// </summary>
    internal class FilterAllowedOutgoingMediaAttribute : ActionFilterAttribute
    {
        private readonly Type _outgoingType;
        private readonly string _propertyName;

        public FilterAllowedOutgoingMediaAttribute(Type outgoingType)
        {
            _outgoingType = outgoingType;
        }

        public FilterAllowedOutgoingMediaAttribute(Type outgoingType, string propertyName)
            : this(outgoingType)
        {
            _propertyName = propertyName;
        }

        /// <summary>
        /// Returns true so that other filters can execute along with this one
        /// </summary>
        public override bool AllowMultiple
        {
            get { return true; }
        }

        protected virtual int GetUserStartNode(IUser user)
        {
            return user.StartMediaId;
        }

        protected virtual int RecycleBinId
        {
            get { return Constants.System.RecycleBinMedia; }
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Response == null) return;

            var user = UmbracoContext.Current.Security.CurrentUser;
            if (user == null) return;

            var objectContent = actionExecutedContext.Response.Content as ObjectContent;
            if (objectContent != null)
            {
                var collection = GetValueFromResponse(objectContent);

                if (collection != null)
                {
                    var items = Enumerable.ToList(collection);

                    FilterItems(user, items);

                    //set the return value
                    SetValueForResponse(objectContent, items);
                }
            }

            base.OnActionExecuted(actionExecutedContext);
        }

        protected virtual void FilterItems(IUser user, IList items)
        {
            FilterBasedOnStartNode(items, user);

            FilterBasedOnPermissions(items, user, ApplicationContext.Current.Services.UserService);
        }

        internal void FilterBasedOnStartNode(IList items, IUser user)
        {
            var toRemove = new List<dynamic>();
            foreach (dynamic item in items)
            {
                var hasPathAccess = (item != null && UserExtensions.HasPathAccess(item.Path, GetUserStartNode(user), RecycleBinId));
                if (!hasPathAccess)
                {
                    toRemove.Add(item);
                }
            }

            foreach (var item in toRemove)
            {
                items.Remove(item);
            }
        }

        internal virtual void FilterBasedOnPermissions(IList items, IUser user, IUserService userService)
        {
            var args = new Umbraco.Core.Events.GettingPermissionsEventArgs(user, null, items.Cast<dynamic>().Select(i => (int)i.Id).ToArray());
            Umbraco.Web.Editors.MediaController.RaiseGettingPermissionsEvent(args);

            if (args.Permissions != null)
            {
                var dict = args.Permissions.ToDictionary(p => p.EntityId);
                var toRemove = new List<dynamic>();
                foreach (dynamic item in items)
                {
                    EntityPermission perm;
                    if (!dict.TryGetValue(item.Id, out perm) || !perm.AssignedPermissions.Contains("A"))
                    {
                        toRemove.Add(item);
                    }
                }

                foreach (var item in toRemove)
                {
                    items.Remove(item);
                }
            }
        }

        private void SetValueForResponse(ObjectContent objectContent, dynamic newVal)
        {
            if (TypeHelper.IsTypeAssignableFrom(_outgoingType, objectContent.Value.GetType()))
            {
                objectContent.Value = newVal;
            }
            else if (_propertyName.IsNullOrWhiteSpace() == false)
            {
                //try to get the enumerable collection from a property on the result object using reflection
                var property = objectContent.Value.GetType().GetProperty(_propertyName);
                if (property != null)
                {
                    property.SetValue(objectContent.Value, newVal);
                }
            }

        }

        internal dynamic GetValueFromResponse(ObjectContent objectContent)
        {
            if (TypeHelper.IsTypeAssignableFrom(_outgoingType, objectContent.Value.GetType()))
            {
                return objectContent.Value;
            }

            if (_propertyName.IsNullOrWhiteSpace() == false)
            {
                //try to get the enumerable collection from a property on the result object using reflection
                var property = objectContent.Value.GetType().GetProperty(_propertyName);
                if (property != null)
                {
                    var result = property.GetValue(objectContent.Value);
                    if (result != null && TypeHelper.IsTypeAssignableFrom(_outgoingType, result.GetType()))
                    {
                        return result;
                    }
                }
            }

            return null;
        }
    }
}
