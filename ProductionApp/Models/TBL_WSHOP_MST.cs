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
    
    public partial class TBL_WSHOP_MST
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_WSHOP_MST()
        {
            this.TBL_USERS_MST = new HashSet<TBL_USERS_MST>();
        }
    
        public int WSHOP_ID { get; set; }
        public string NAME { get; set; }
        public Nullable<int> PLANT_ID { get; set; }
        public string UPDATED_BY { get; set; }
        public Nullable<System.DateTime> TS_1 { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_USERS_MST> TBL_USERS_MST { get; set; }
    }
}
