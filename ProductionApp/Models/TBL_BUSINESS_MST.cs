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
    
    public partial class TBL_BUSINESS_MST
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_BUSINESS_MST()
        {
            this.TBL_GROUP_MST = new HashSet<TBL_GROUP_MST>();
        }
    
        public int ID { get; set; }
        public string BIZ_NAME { get; set; }
        public Nullable<int> ACTIVATE { get; set; }
        public Nullable<System.DateTime> TS_2 { get; set; }
        public string TS_2_USER { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_GROUP_MST> TBL_GROUP_MST { get; set; }
    }
}
