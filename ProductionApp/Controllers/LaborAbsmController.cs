using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml;
using ProductionApp.Models;
using System.Data.Entity;

namespace ProductionApp.Controllers
{
    public class LaborAbsmController:BaseController
    {
        public ActionResult Index()
        {
            if ((UserModels)Session["SignedInUser"] == null)
            {
                return RedirectToAction("NeedLogin", "Notification");
            }
            List<PROC_GET_TOP5_ABSM_UPDATE_Result> top5 = (from item in db.GetTop5AbsmUpdate() select item).ToList();
            return View("UploadLaborAbsm", top5);
        }

        public ActionResult UploadLaborAbsm()
        {
            if (Request != null)
            {
                int MesRow = 0;
                string OtherMes = null;
                UserModels usr = (UserModels)Session["SignedInUser"];
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
                            
                            DateTime date = Convert.ToDateTime(workSheet.Cells[3, 5].Value.ToString());
                            db.Database.ExecuteSqlCommand("delete from TBL_LABOR_ABSENTEEISM where datediff(dd, DATE, {0}) = 0", date );
                            MesRow = 3;
                            for (int rowIterator = 3; rowIterator <= noOfRow; rowIterator++)
                            {
                                    MesRow = rowIterator;
                                    string line = workSheet.Cells[rowIterator, 2].Value.ToString();
                                    int Absm_Qty = Convert.ToInt32(workSheet.Cells[rowIterator, 3].Value.ToString());
                                    double OT = Convert.ToDouble(workSheet.Cells[rowIterator, 4].Value.ToString());
                                    date = Convert.ToDateTime(workSheet.Cells[4, 5].Value.ToString());
                                    int Total_Labor = Convert.ToInt32(workSheet.Cells[rowIterator, 6].Value.ToString());

                                    PROC_GET_GROUP_BY_NAME_Result group_record = db.GetGroupByName(line.Substring(0, 3)).SingleOrDefault();

                                    if (group_record != null)
                                    {
                                        TBL_LABOR_ABSENTEEISM absm_record = db.TBL_LABOR_ABSENTEEISM.Where(t => t.DATE == date && t.GROUP_ID == group_record.GROUP_ID).SingleOrDefault();
                                        if(absm_record == null)
                                            { 
                                                TBL_LABOR_ABSENTEEISM tmp = new TBL_LABOR_ABSENTEEISM();
                                                tmp.DATE = date;
                                                tmp.ABSENTEEISM = Absm_Qty;
                                                tmp.GROUP_ID = group_record.GROUP_ID;
                                                tmp.LABOR = Total_Labor;
                                                tmp.OT = OT;
                                                tmp.TS_1 = DateTime.Now;
                                                tmp.TS_1_USER = usr.Username;
                                                db.TBL_LABOR_ABSENTEEISM.Add(tmp);
                                                db.SaveChanges();
                                            }
                                        else
                                        {
                                            absm_record.LABOR += Total_Labor;
                                            absm_record.OT += OT;
                                            absm_record.ABSENTEEISM += Absm_Qty;
                                            db.SaveChanges();
                                        }
                                    }
                                    else
                                    {
                                        OtherMes = ";Group/Line not found!";
                                    }
                            }
                        }
                        ViewBag.Status = "Upload Sucessful.";
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Status = "Error, contact to IT. " + e.Message + ",  " + Convert.ToString(MesRow) + ":" + OtherMes;
                }
            }
            List<PROC_GET_TOP5_ABSM_UPDATE_Result> top5 = (from item in db.GetTop5AbsmUpdate() select item).ToList();
            return View("UploadLaborAbsm",top5);
        }
    }
}