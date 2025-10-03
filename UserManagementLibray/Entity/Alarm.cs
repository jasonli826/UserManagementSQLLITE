using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagementLibray.Entity
{
    public class AlarmMessage
    {
        public int ID { get; set; }
        public string Alarm{ get; set; }
        public string Alarm_Description { get; set; }
        public DateTime RaiseTime { get; set; }
        public DateTime AcknowledgeTime { get; set; }
        public string Created_by { get; set; }
        public DateTime Created_Date { get; set; }
        public string Updated_by { get; set; }
        public DateTime Updated_Date { get; set; }
        public int AlarmNumeric { get; set; }

    }

}
