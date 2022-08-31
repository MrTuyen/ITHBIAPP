using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml;
using ProductionApp.Models;

namespace ProductionApp.Controllers
{
    public class WorkCentralController:BaseController
    {
        // GET: MasterData
        public ActionResult Index()
        {
            if ((UserModels)Session["SignedInUser"] == null)
            {
                return RedirectToAction("NeedLogin", "Notification");
            }
            return View("UploadWorkCentral");
        }

        public ActionResult UploadWorkCentral()
        {
            if (Request != null)
            {
                int MesRow = 0;
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
                            for (int rowIterator = 3; rowIterator <= noOfRow; rowIterator++)
                            {
                                MesRow = rowIterator;
                                string celling = workSheet.Cells[rowIterator, 1].Value.ToString();
                                string wc = workSheet.Cells[rowIterator, 9].Value.ToString();
                                string constr = workSheet.Cells[rowIterator, 10].Value == null ? "": workSheet.Cells[rowIterator, 10].Value.ToString();
                                string unit = workSheet.Cells[rowIterator, 11].Value.ToString();
                                if (celling != null && wc != null && wc.ToString() != "#N/A" && wc.ToString() != "UNKNOWN" && constr != null)
                                {
                                    TBL_SELLING_WC SellWC_Record = db.TBL_SELLING_WC.Where(t => t.SELLING_STYLE == celling).SingleOrDefault();
                                    if (SellWC_Record == null)
                                    {
                                        int WC_ID;
                                        if (GetWCID(wc) != 0)
                                        {
                                            WC_ID = GetWCID(wc);
                                        }
                                        else
                                        {
                                            TBL_WC_MST WC_tmp = new TBL_WC_MST();
                                            WC_tmp.WC_GROUP = wc;
                                            db.TBL_WC_MST.Add(WC_tmp);
                                            db.SaveChanges();
                                            WC_ID = GetWCID(wc);
                                        }
                                        TBL_SELLING_WC SellWC_tmp = new TBL_SELLING_WC();
                                        SellWC_tmp.WC_ID = WC_ID;
                                        SellWC_tmp.SELLING_STYLE = celling;
                                        SellWC_tmp.TS_1 = DateTime.Now;
                                        SellWC_tmp.CONSTRUCTION = constr;
                                        SellWC_tmp.UNIT = unit;
                                        SellWC_tmp.TS_1_USER = ((UserModels)Session["SignedInUser"]).Username;
                                        db.TBL_SELLING_WC.Add(SellWC_tmp);
                                        db.SaveChanges();
                                    }
                                }

                            }
                        }
                        ViewBag.Status = "Upload Sucessful.";
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Status = "Error, Data is invalid. " + " Row No " + Convert.ToString(MesRow) + ". " + e.Message;
                }
            }
            return View("UploadWorkCentral");
        }

        public int GetWCID(string name)
        {
            TBL_WC_MST WC_record = db.TBL_WC_MST.Where(t => t.WC_GROUP == name).SingleOrDefault();
            if (WC_record != null)
                return WC_record.ID;
            return 0;

        }
    }
}