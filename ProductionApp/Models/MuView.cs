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
    public class MuView
    {
        public string GroupName { get; set; }
        public string asstWO { get; set; }
        public string Item { get; set; }
        public string Week { get; set; }
        public string MnfStyle { get; set; }
        public string PkgStyle { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public double DM { get; set; }
        public double Cost { get; set; }
        public double Standard { get; set; }
        public double Actual { get; set; }

 




    }
}