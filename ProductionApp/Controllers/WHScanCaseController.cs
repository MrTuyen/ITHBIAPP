using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProductionApp.Models;
using OfficeOpenXml;
using System.Globalization;


namespace ProductionApp.Controllers
{
    public class WHScanCaseController:BaseController
    {
        // GET: MasterData
        public ActionResult Index()
        {
            if ((UserModels)Session["SignedInUser"] == null)
            {
                return RedirectToAction("NeedLogin", "Notification");
            }
            return View("UploadWHCase");
        }

        // CHECK CASE LABLE EXIST OR NOT
        public Boolean checkCaseExist(string label)
        {
            string tmp = "";
            tmp = (from item in db.TBL_WH_CASE.Where(t => t.LABEL_ID.Equals(label)) select item.LABEL_ID).SingleOrDefault();
            if (tmp != null)
                return true;
            return false;
        }


        ///INSERT WH CASE LABEL
        public ActionResult UploadWHCase()
        {
            if (Request != null)
            {
                int rowErr = 0;
                try
                {
                    HttpPostedFileBase file = Request.Files["UploadedFile"];
                    if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                    {
                        string fileName = file.FileName;
                        string fileContentType = file.ContentType;
                        byte[] fileBytes = new byte[file.ContentLength];
                        var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                        using (var package = new ExcelPackage(file.InputStream))
                        {
                            var currentSheet = package.Workbook.Worksheets;
                            var workSheet = currentSheet.First();
                            var noOfCol = workSheet.Dimension.End.Column;
                            var noOfRow = workSheet.Dimension.End.Row;

                            for (int rowIterator = 6; rowIterator <= noOfRow; rowIterator++)
                            {
                                rowErr = rowIterator;
                                if (workSheet.Cells[rowIterator, 2].Value.ToString().Length == 9)
                                {
                                    TimeSpan timespan1 = new TimeSpan(0, 12, 0, 0);
                                    var CsStatus = workSheet.Cells[rowIterator, 8].Value;
                                    //var acb = workSheet.Cells[rowIterator, 9] as Range).Value2;
                                    
                                    if (!checkCaseExist(workSheet.Cells[rowIterator, 2].Value.ToString().Trim()) && CsStatus != null )
                                        if(CsStatus.ToString().Trim() == "PP" || CsStatus.ToString().Trim() == "Manifested" || CsStatus.ToString().Trim() == "Pickup Complete" || CsStatus.ToString().Trim() == "Transmitted")
                                        {
                                            TBL_WH_CASE casetmp = new TBL_WH_CASE();
                                            casetmp.LABEL_ID = workSheet.Cells[rowIterator, 2].Value.ToString().Trim();
                                            casetmp.QUANTITY = Convert.ToDouble(workSheet.Cells[rowIterator, 4].Value.ToString().Trim()); //+ (Convert.ToDouble(arrTmp[1]) / 12);
                                            casetmp.STATUS = workSheet.Cells[rowIterator, 8].Value.ToString().Trim();
                                            //DateTime dateValue = DateTime.FromOADate((workSheet.Cells[rowIterator, 9] as Range).Value2);
                                            //String dtString = ((ExcelRange)workSheet.Cells[rowIterator, 9].Value.ToString();
                                            //DateTime dt = DateTime.Parse(ConvertToDateTime(dtString));
                                            //string abb = (workSheet.Cells[rowIterator, 9].Value.ToString());

                                            //String dtString = ((Excel.Range)ws.Cells[row, "C"]).Value2.ToString();
                                            //DateTime dt = DateTime.Parse(ConvertToDateTime(dtString));

                                            var a = workSheet.Cells[rowIterator, 9].Value.ToString();
                                            DateTime b = DateTime.FromOADate(Convert.ToDouble(a));


                                        //CultureInfo enUS = new CultureInfo("en-US");
                                        //string dateString = abb;
                                        //DateTime dateValue;

                                        // Parse date with no style flags.
                                        //dateString = " 5/01/2009 8:30 AM";
                                        //var aaaa = (DateTime.TryParseExact(dateString, "MM/dd/yyyyy hh:mm:ss tt", enUS, DateTimeStyles.AllowLeadingWhite, out dateValue));
                                        //var sdfd = DateTime.Parse(workSheet.Cells[rowIterator, 9].Value.ToString(), CultureInfo.InvariantCulture);
                                        //var sdf = DateTime.ParseExact(workSheet.Cells[rowIterator, 9].Value.ToString(), "mm/dd/yyyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                                            casetmp.STATUS_DATE = b + timespan1;
                                            casetmp.WLOT_ID = (workSheet.Cells[rowIterator, 10].Value == null ? "OddLot" : workSheet.Cells[rowIterator, 10].Value.ToString().Trim());
                                            casetmp.TS_1_USER = ((UserModels)Session["SignedInUser"] == null ? null : ((UserModels)Session["SignedInUser"]).Username.ToString());
                                            casetmp.TS_1 = DateTime.Now;
                                            db.TBL_WH_CASE.Add(casetmp);
                                            db.SaveChanges();
                                        }
                                    //else
                                    //{
                                    //    TBL_WH_CASE casetmp = db.TBL_WH_CASE.SingleOrDefault(T => T.LABEL_ID.Equals(workSheet.Cells[rowIterator, 2].Value.ToString().Trim()));
                                    //    casetmp.STATUS = workSheet.Cells[rowIterator, 8].Value.ToString().Trim();
                                    //    casetmp.STATUS_DATE = Convert.ToDateTime(workSheet.Cells[rowIterator, 9].Value.ToString().Trim());
                                    //    casetmp.STATUS_DATE = Convert.ToDateTime(workSheet.Cells[rowIterator, 9].Value.ToString().Trim()) + timespan1;
                                    //    casetmp.TS_1_USER = ((UserModels)Session["SignedInUser"] == null ? null : ((UserModels)Session["SignedInUser"]).Username.ToString());
                                    //    casetmp.TS_1 = DateTime.Now;
                                    //    db.SaveChanges();
                                    //}
                                }
                            }
                        }
                        ViewBag.Status = "Upload Sucessful.";
                    }

            }
                catch (Exception e)
            {
                ViewBag.Status = e.InnerException + "Fail Upload at row: " + rowErr.ToString();
            }
        }

            return View("UploadWHCase");
        }

    }
}