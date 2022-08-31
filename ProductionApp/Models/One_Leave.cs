using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionApp.Models
{
    public class One_Leave
    {
        public string leaveID { get; set; }
        public string leaveName { get; set; }
        public float total { get; set; }
        public float used { get; set; }
        public float remai { get; set; }
        public string fromdate { get; set; }
        public string todate { get; set; }
        public int total_date { get; set; }
    }
}