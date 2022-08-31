using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionApp.Models
{
    public class TdsTimeSupportGroup
    {
        public string Line { get; set; }
        public double? HC { get; set; }
        public double? Total { get; set; }
        public double? SupportHours { get; set; }
        public double? SupportEff { get; set; }
        public double? Payment { get; set; }

    }
}