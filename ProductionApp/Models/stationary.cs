using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionApp.Models
{
    public class stationary
    {
        public string orderID { get; set; }

        public string statiID { get; set; }
        public string name { get; set; }
        public int Qty { get; set; }
        public Double Price { get; set; }
        public string Unit { get; set; }      
        public string Note { get; set; }
        public decimal Total
        {
            get { return decimal.Parse((Price * Qty).ToString()); }
        }
    }
}