//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ProductionApp.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class STA_Order_Item
    {
        public int OrderID { get; set; }
        public string StaId { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<int> Qty { get; set; }
        public string Note { get; set; }
    
        public virtual STA_Item STA_Item { get; set; }
        public virtual STA_Orders STA_Orders { get; set; }
    }
}
