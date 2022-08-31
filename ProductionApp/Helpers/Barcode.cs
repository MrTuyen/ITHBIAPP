using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZXing;

namespace ProductionApp.Helpers
{
    public class Barcode : Controller
    {
        public  String Create(string barcode, int width, int height)
        {
            Image img = null;
            using (var ms = new MemoryStream())
            {
                var writer = new BarcodeWriter() { Format = BarcodeFormat.CODE_128 };
                writer.Options.Height = height;
                writer.Options.Width = width;
                writer.Options.PureBarcode = true;
                img = writer.Write(barcode);
                img.Save(ms, ImageFormat.Jpeg);
                var tmp = "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
                return tmp;
            }



            //using (MemoryStream memoryStream = new MemoryStream())
            //{
            //    using (Bitmap bitMap = new Bitmap(barcode.Length * 40, 80))
            //    {
            //        using (Graphics graphics = Graphics.FromImage(bitMap))
            //        {
            //            Font oFont = new Font("IDAutomationHC39M", 16);
            //            PointF point = new PointF(2f, 2f);
            //            SolidBrush whiteBrush = new SolidBrush(Color.White);
            //            graphics.FillRectangle(whiteBrush, 0, 0, bitMap.Width, bitMap.Height);
            //            SolidBrush blackBrush = new SolidBrush(Color.DarkBlue);
            //            graphics.DrawString("*" + barcode + "*", oFont, blackBrush, point);
            //        }

            //        bitMap.Save(memoryStream, ImageFormat.Jpeg);

            //        ViewBag.BarcodeImage = "data:image/png;base64," + Convert.ToBase64String(memoryStream.ToArray());
            //    }
            //}  
        }

       
    }
}