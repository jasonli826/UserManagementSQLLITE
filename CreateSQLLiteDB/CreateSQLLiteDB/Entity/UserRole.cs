using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateSQLLiteDB.Entity
{
    public class UserRole
    {
        public int ID {get;set;}
        public string UserId { get; set; }
        public int RoleId { get; set; }
    }
}
