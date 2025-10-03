using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagementLibray.Entity
{
    public class PanelCountSummaryDto
    {
        public DateTime Date { get; set; }
        public double TotalPass { get; set; }
        public double TotalReject { get; set; }
        public double TotalRework { get; set; }

        public double UPH { get; set; } // Panels per hour

    }

}
