using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagementlibrary.Entity
{
    public class MenuItem
    {
        public int MenuID { get; set; }
        public string Parent_Menu { get; set; }
        public string Child_Menu { get; set; }
        public int? Sno { get; set; }
    }
}
