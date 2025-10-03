using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagementLibray.Entity
{
    public  class Audit
    {
        public int AuditID { get; set; }
        public DateTime AuditDate { get; set; }
        public string UserName { get; set; }
        public string Detail { get; set; }
        public string Created_by { get; set; }
        public DateTime Created_Date { get; set; }
        public string Updated_by { get; set; }
        public DateTime Updated_Date { get; set; }
    }
}
