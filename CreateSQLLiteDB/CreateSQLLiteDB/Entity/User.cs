using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateSQLLiteDB.Entity
{
    public class User
    {
        public string UserId { get; set; }
        public string Empid { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Domain { get; set; }
        public string Created_by { get; set; }
        public DateTime Created_Date { get; set; }
        public string Updated_by { get; set; }
        public DateTime? Updated_Date { get; set; }
        public DateTime? Pwd_Change_Date { get; set; }
        public DateTime? Last_Login_Date { get; set; }
        public DateTime? Logout_Date { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public string DomainName { get; set; }
    }

}
