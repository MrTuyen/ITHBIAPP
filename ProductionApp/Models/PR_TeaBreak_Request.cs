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
    
    public partial class PR_TeaBreak_Request
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PR_TeaBreak_Request()
        {
            this.PR_TeaBreak_Items = new HashSet<PR_TeaBreak_Items>();
        }
    
        public int ID { get; set; }
        public Nullable<System.DateTime> RequestDate { get; set; }
        public string RequestBy { get; set; }
        public Nullable<int> DeptID { get; set; }
        public string ManagerMail { get; set; }
        public Nullable<int> ManagerStatus { get; set; }
        public Nullable<System.DateTime> ManagerDate { get; set; }
        public string HRManagerMail { get; set; }
        public Nullable<int> HRManagerStatus { get; set; }
        public Nullable<System.DateTime> HRManagerDate { get; set; }
        public string HRProcessMail { get; set; }
        public Nullable<int> HRProcessStatus { get; set; }
        public Nullable<System.DateTime> HRProcessDate { get; set; }
        public string Content { get; set; }
        public Nullable<int> Status { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PR_TeaBreak_Items> PR_TeaBreak_Items { get; set; }
        public virtual TBL_DEPARTMENT_MST TBL_DEPARTMENT_MST { get; set; }
    }
}
