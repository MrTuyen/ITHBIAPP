using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionApp.Models
{
    public class OT_One
    {
        public string OTCD { get; set; }
        public string OT_Name { get; set; }
        public string OTDate { get; set; }
        public int WWork { get; set; }
        public int HoursFrom { get; set; }
        public int HoursTo { get; set; }
        public int MinFrom { get; set; }
        public int MinTo { get; set; }
        public double total { get; set; }
        public int van { get; set; }
        public string address { get; set; }
    }
}