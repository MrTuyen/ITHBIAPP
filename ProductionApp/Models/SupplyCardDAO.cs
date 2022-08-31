using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionApp.Models
{
    public class SupplyCard
    {
        public string WL { get; set; }
        public DateTime? Createdate { get; set; }
        public DateTime? RequestDate { get; set; }
        public string location_Name { get; set; }
        public string GROUP_NAME { get; set; }
        public string unit_Name { get; set; }
     //   public int? SupplyID { get; set; }
        public string SupplyName { get; set; }
        public double? Quantity { get; set; }
        public double? QuantityOut { get; set; }
        public string Note { get; set; }
        public List<WH_Item_location> WH_Item_location { get; set; }

      

    }
}