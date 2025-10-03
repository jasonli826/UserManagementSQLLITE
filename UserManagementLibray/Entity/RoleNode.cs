using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagementLibray.Entity
{
    public class RoleNode
    {
        public int RoleId { get; set; }
        public string Role_Name { get; set; }
        public int PriorityIndex { get; set; }
        public int? ParentRoleID { get; set; }
        public List<RoleNode> Children { get; set; } = new List<RoleNode>();

        public bool IsExpanded { get; set; } = false;
    }
}
