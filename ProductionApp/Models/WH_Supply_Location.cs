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
    
    public partial class WH_Supply_Location
    {
        public int ID { get; set; }
        public Nullable<int> SupplyID { get; set; }
        public Nullable<int> LocationID { get; set; }
        public Nullable<double> inventory { get; set; }
    
        public virtual WH_Location_MST WH_Location_MST { get; set; }
        public virtual WH_Supply_MST WH_Supply_MST { get; set; }
    }
}