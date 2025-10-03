using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagementlibrary.Entity
{
    public class LoginUser
    {
        public List<MenuItem> AccessibleMenus { get; set; } = new List<MenuItem>();
    }
}
