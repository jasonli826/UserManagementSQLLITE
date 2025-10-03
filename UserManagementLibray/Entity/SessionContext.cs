using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagementlibrary.Entity
{
    public static class SessionContext
    {
        public static string UserId { get; set; }

        public static List<int> RoleIds { get; set; }
        public static string UserRole { get; set; }
    }
}
