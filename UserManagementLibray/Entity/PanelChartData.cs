using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagementLibray.Entity
{
    public class PanelChartData
    {
        public List<string> Labels { get; set; }
        public List<double> TotalPass { get; set; }
        public List<double> TotalReject { get; set; }
        public List<double> TotalRework { get; set; }
    }
   
}
