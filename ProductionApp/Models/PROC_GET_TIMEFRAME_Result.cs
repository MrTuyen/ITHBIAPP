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
    
    public partial class PROC_GET_TIMEFRAME_Result
    {
        public int FRAME_ID { get; set; }
        public Nullable<System.TimeSpan> BEGIN { get; set; }
        public System.TimeSpan END { get; set; }
        public Nullable<int> REFER { get; set; }
        public Nullable<double> PIECE { get; set; }
        public Nullable<int> SHIFT_BEGIN { get; set; }
    }
}
