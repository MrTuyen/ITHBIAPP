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
    
    public partial class KPI_Y_Review
    {
        public int ID { get; set; }
        public string KPI { get; set; }
        public string DeptName { get; set; }
        public string EmpID { get; set; }
        public string Position { get; set; }
        public string EvaluatorID { get; set; }
        public string ManagerID { get; set; }
        public string KPIs { get; set; }
        public string Q1 { get; set; }
        public string Q2 { get; set; }
        public string Q3 { get; set; }
        public string Q4 { get; set; }
        public string Comments { get; set; }
        public Nullable<int> SubmittedByEmp { get; set; }
        public Nullable<System.DateTime> SubmittedDate { get; set; }
        public Nullable<int> ApprovedByManager { get; set; }
        public Nullable<System.DateTime> ApprovedDate { get; set; }
        public Nullable<int> ReviewedByEvaluator { get; set; }
        public Nullable<System.DateTime> ReviewedDate { get; set; }
        public Nullable<int> Period { get; set; }
        public string Quarter_KPI_Result { get; set; }
        public string KPI_Weight { get; set; }
        public string Actual_Target { get; set; }
        public string KPI_Bonus { get; set; }
        public string Final_Score { get; set; }
        public string Note { get; set; }
        public string CoreValues { get; set; }
        public string Strength { get; set; }
        public string AreasImprove { get; set; }
        public string Overal_Checkbox { get; set; }
        public string Overal_Comment { get; set; }
        public string Level { get; set; }
        public string CmtManager { get; set; }
    }
}
