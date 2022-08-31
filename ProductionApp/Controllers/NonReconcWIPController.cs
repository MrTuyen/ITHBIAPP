using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml;
using ProductionApp.Models;

namespace ProductionApp.Controllers
{
    public class NonReconcWIPController:BaseController
    {
        // GET: MasterData
        public ActionResult Index()
        {
            if ((UserModels)Session["SignedInUser"] == null)
            {
                return RedirectToAction("NeedLogin", "Notification");
            }
            return View("UploadNonReconcWip");
        }

        public ActionResult UploadNonReconcWip()
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
                            List<TBL_NON_RECONC_WIP> all_record = db.TBL_NON_RECONC_WIP.Where(t => t.WL != null).ToList();
                            if (all_record != null)
                            { 
                                db.TBL_NON_RECONC_WIP.RemoveRange(all_record);
                                db.SaveChanges();
                            }
                            for (int rowIterator = 12; rowIterator <= noOfRow; rowIterator++)
                            {
                                MesRow = rowIterator;
                                string WL = (workSheet.Cells[rowIterator, 2].Value == null ? "" : workSheet.Cells[rowIterator, 2].Value.ToString());
                                string Age = (workSheet.Cells[rowIterator, 18].Value == null ? "" : workSheet.Cells[rowIterator, 18].Value.ToString());
                                if (WL.Trim() != "" && Age.Trim() != "" && !IsWLExits(WL))
                                {
                                    TBL_NON_RECONC_WIP tmp_record = new TBL_NON_RECONC_WIP();
                                    tmp_record.WL = WL;
                                    tmp_record.AGE = Convert.ToInt16(Age);
                                    tmp_record.TS_1_USER = ((UserModels)Session["SignedInUser"]).Username;
                                    tmp_record.TS_1 = DateTime.Now;
                                    db.TBL_NON_RECONC_WIP.Add(tmp_record);
                                    db.SaveChanges();
                                }

                            }
                        }
                        ViewBag.Status = "Upload Sucessful.";
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Status = "Error, need contact to IT. " + e.Message + ",  Row " + Convert.ToString(MesRow);
                }
            }
            return View("UploadNonReconcWip");
        }

        public bool IsWLExits(string name)
        {
            TBL_NON_RECONC_WIP WC_record = db.TBL_NON_RECONC_WIP.Where(t => t.WL == name).SingleOrDefault();
            if (WC_record != null)
                return true;
            return false;

        }

    }
}