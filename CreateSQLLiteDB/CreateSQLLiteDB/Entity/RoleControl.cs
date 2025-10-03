using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateSQLLiteDB.Entity
{
    public class RoleControl
    {
        public int RoleId { get; set; }
        public int MenuId { get; set; }
        public string Created_by { get; set; }
        public DateTime Created_Date { get; set; }
        public string Updated_by { get; set; }
        public DateTime? Updated_Date { get; set; }
    }

}
