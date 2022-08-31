using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionApp.Models {

    public class TBL_LTS_NONREC_LOT_DTO {
        public TBL_LTS_Details LtsDetails { get; set; }
        public string Asst_WL { get; set; }
        public string Comp_WL { get; set; }
        public string Asst_SKU { get; set; }
        public string Selling_Style { get; set; }
        public string Group { get; set; }
        public string Size { get; set; }
        public string Loc_Name { get; set; }
        public string StatusApproveDisplay { get; set; }
        public Nullable<double> Rec_Qty { get; set; }
        public Nullable<double> FQ_Qty { get; set; }
        public Nullable<double> Odd_Qty { get; set; }
        public Nullable<double> Irr_Qty { get; set; }
        public Nullable<double> Sample_Qty { get; set; }
        public Nullable<double> ThrowOut_Qty { get; set; }
        public Nullable<double> LTS { get; set; }
        public string Aging { get; set; }
        public Nullable<System.DateTime> Issued_date { get; set; }
    }
    public class TBL_LTS_DETAILS_DTO {
        public string Asst_WL { get; set; }
        public string Comp_WL { get; set; }
        public string Asst_SKU { get; set; }
        public string Selling_Style { get; set; }
        public string Group { get; set; }
        public string Size { get; set; }
        public string ltsNumberKey { get; set; }
        public string ltsNumber { get; set; }
        public Nullable<int> status { get; set; }
        public string Loc_Name { get; set; }
        public Nullable<double> Rec_Qty { get; set; }
        public Nullable<double> FQ_Qty { get; set; }
        public Nullable<double> Odd_Qty { get; set; }
        public Nullable<double> Irr_Qty { get; set; }
        public Nullable<double> Sample_Qty { get; set; }
        public Nullable<double> ThrowOut_Qty { get; set; }
        public Nullable<double> LTS { get; set; }
        public string Aging { get; set; }
        public Nullable<System.DateTime> Issued_date { get; set; }

        public Nullable<int> Prod_PIC { get; set; }
        public string Prod_Status { get; set; }
        public string Prod_Mail { get; set; }
        public Nullable<System.DateTime> Prod_Date { get; set; }

        public Nullable<int> Odd_PIC { get; set; }
        public string Odd_Status { get; set; }
        public string Odd_Mail { get; set; }
        public Nullable<System.DateTime> Odd_Date { get; set; }

        public Nullable<int> IA_PIC { get; set; }
        public string IA_Status { get; set; }
        public string IA_Mail { get; set; }
        public Nullable<System.DateTime> IA_Date { get; set; }

        public Nullable<int> PM_PIC { get; set; }
        public string PM_Status { get; set; }
        public string PM_Mail { get; set; }
        public Nullable<System.DateTime> PM_Date { get; set; }

        public Nullable<System.DateTime> Submitted_date { get; set; }
    }
}
