using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagementLibray.Entity
{
    public class DataAnalytic
    {
        public int ID { get; set; }
        public DateTime Date { get; set; }
        public double PanelCountPass { get; set; }
        public double PanelCountReject { get; set; }
        public double PanelCountRework{ get; set; }
        public string Created_by { get; set; }
        public DateTime Created_Date { get; set; }
        public string Updated_by { get; set; }
        public DateTime Updated_Date { get; set; }
    }
}
