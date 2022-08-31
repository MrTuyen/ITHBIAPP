using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionApp.Models
{
    public class TeaBreak
    {
        public int GroupID { get; set; }

        public int ID { get; set; }
        public string Name_Group { get; set; }
        public string Name_TeaBreak { get; set; }
        public Double Price { get; set; }
        public int Qty { get; set; }
        public Double Total
        {
            get { return  Price * Qty; }
        }

    }
}