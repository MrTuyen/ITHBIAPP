using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionApp.Models
{
    public class OT_More
    {
        public int id { get; set; }
        public string OTCD { get; set; }
        public string OT_Name { get; set; }
        public string OTDate { get; set; }
        public int WWork { get; set; }
        public int HoursFrom { get; set; }
        public int HoursTo { get; set; }
        public int MinFrom { get; set; }
        public int MinTo { get; set; }
        public double total { get; set; }
        public string empID { get; set; }
        public string EmpName { get; set; }
        public string EmpEmail { get; set; }
        public int Van { get; set; }
        public string Address { get; set; }

    }
}