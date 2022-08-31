using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionApp.Models
{
    public class SupplyExport
    {
        public int ID { get; set; }
        public string CreateDate { get; set; }
        public string RequestDate { get; set; }
        public string Company { get; set; }
        public string PlantCD { get; set; }
        public string Document { get; set; }
        public string NM { get; set; }
        public string A { get; set; }
        public string Check { get; set; }
        public string Item { get; set; }
        public string Weight { get; set; }
        public string Uom1 { get; set; }
        public string Location { get; set; }
        public string ConfirmDate { get; set; }
        public string GroupName { get; set; }
        public string Requester { get; set; }
        public IEnumerable<WH_Item_location> WH_Item_location { get; set; }

    }
}