using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagementLibray.Entity
{
    public class AlarmChartData
    {
        public List<string> Labels { get; set; }
        public List<double> AlarmTotalCount { get; set; }
    }
}
