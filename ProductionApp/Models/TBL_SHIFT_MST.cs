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
    
    public partial class TBL_SHIFT_MST
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_SHIFT_MST()
        {
            this.TBL_CASE_LABEL = new HashSet<TBL_CASE_LABEL>();
            this.HR_Meals_Order = new HashSet<HR_Meals_Order>();
        }
    
        public int SHIFT_ID { get; set; }
        public string NAME { get; set; }
        public Nullable<System.TimeSpan> BEGIN_TS { get; set; }
        public Nullable<System.TimeSpan> END_TS { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CASE_LABEL> TBL_CASE_LABEL { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<HR_Meals_Order> HR_Meals_Order { get; set; }
    }
}