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
    
    public partial class IT_Fixed_Asset_BK
    {
        public int ID { get; set; }
        public string TAG { get; set; }
        public string SERIAL { get; set; }
        public Nullable<int> MODEL { get; set; }
        public string PUR_DATE { get; set; }
        public string WARRANTY { get; set; }
        public Nullable<int> DIVISION { get; set; }
        public Nullable<int> DEPT { get; set; }
        public string USER { get; set; }
        public string NOTES { get; set; }
        public string STATUS { get; set; }
        public string COUNTSHEET { get; set; }
    
        public virtual IT_PC_Model_MST IT_PC_Model_MST { get; set; }
        public virtual TBL_DEPARTMENT_MST TBL_DEPARTMENT_MST { get; set; }
        public virtual TBL_DIVISION_MST TBL_DIVISION_MST { get; set; }
    }
}