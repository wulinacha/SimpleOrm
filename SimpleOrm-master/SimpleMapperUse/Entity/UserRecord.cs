using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMapperUse
{
    public class UserRecord
    {
        public int ID { get; set; }
        public DateTime MaxDate { get; set; }
        public DateTime MinDate { get; set; }
        public string Status { get; set; }
        public string Device { get; set; }
        public int EarlyRecordStatus { get; set; }
        public int LaterRecordStatus { get; set; }
        public string Veridify { get; set; }
        public int EarlyTime { get; set; }
        public int LaterTime { get; set; }
        public int TrueTime { get; set; }
        public int UserID { get; set; }
        public int IsSetOA { get; set; }
    }
}
