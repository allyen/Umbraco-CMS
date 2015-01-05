using System;
using System.Collections.Generic;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Events
{
    public class GettingPermissionsEventArgs : EventArgs
    {
        public IUser User { get; private set; }

        public List<EntityPermission> Permissions { get; set; }

        public int[] NodeIds { get; set; }

        public GettingPermissionsEventArgs(IUser user, List<EntityPermission> permissions, int[] nodeIds)
        {
            User = user;
            Permissions = permissions;
            NodeIds = nodeIds;
        }
    }
}