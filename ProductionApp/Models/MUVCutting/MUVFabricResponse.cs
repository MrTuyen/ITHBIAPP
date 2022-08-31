using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionApp.Models.MUVCutting
{
    public class MUVFabricResponse
    {
        public string Fabric { get; set; }
        public decimal Dz { get; set; }
        public decimal Amount { get; set; }
        public decimal RunRate { get; set; }
    }
}