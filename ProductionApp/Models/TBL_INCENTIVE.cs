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
    
    public partial class TBL_INCENTIVE
    {
        public int ID { get; set; }
        public Nullable<int> WEEK { get; set; }
        public Nullable<int> YEAR { get; set; }
        public string GROUP { get; set; }
        public Nullable<long> EMPLOYEE_ID { get; set; }
        public string EMPLOYEE_NAME { get; set; }
        public Nullable<double> WORK_HOURS { get; set; }
        public Nullable<double> ON_STANDARD { get; set; }
        public Nullable<double> OFF_STANDARD { get; set; }
        public Nullable<double> PLANT_EFF { get; set; }
        public Nullable<double> DOL_EFF { get; set; }
        public Nullable<System.DateTime> TS_1 { get; set; }
        public string TS_1_USER { get; set; }
        public Nullable<int> BIZ_ID { get; set; }
    }
}