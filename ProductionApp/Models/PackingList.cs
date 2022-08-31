using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZXing;
using System.Drawing;
using System.Drawing.Imaging;
namespace ProductionApp.Models
{
    public class PackingList
    {
        public String PalletId { get; set; }
        public string wk { get; set; }
        public String Barcode { get; set; }
        public String Priority { get; set; }


        public String Wl { get; set; }

        public string Ts1 { get; set; }
        public String Size { get; set; }
        public TBL_GROUP_MST Group { get; set; }
        public String Color { get; set; }
        public String Line { get; set; }


        public int QtyCarton { get; set; }
        public TBL_Carton_Mst Carton { get; set; }
        public TBL_BUSINESS_MST Business { get; set; }

        public String PkgStyle { get; set; }//Mã đóng gói
        public String SellStyle { get; set; }//mã bán
        public String MnfStyle { get; set; }//mã sản xuất

        



    }
}