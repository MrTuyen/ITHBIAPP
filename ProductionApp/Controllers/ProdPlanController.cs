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
    public class ProdPlanController:BaseController
    {
        // GET: MasterData
        public ActionResult Index()
        {
            if (userLogin == null)
            {
                return RedirectToAction("NeedLogin", "Notification");
            }
            return RedirectToAction("UploadProdPlan");
        }

        public ActionResult UploadProdPlan()
        {
            if (Request != null)
            {
                int MesRow=0, MesCol=0;
                string OtherMes = null;
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
                            string labor, sah;
                            string style;
                            string pl_date, target, group, line;

                            List<TBL_PROD_PLAN> NewerPlan = db.TBL_PROD_PLAN.Where(t => DbFunctions.TruncateTime(t.PLAN_DATE) >= DateTime.Now).ToList();
                            if (NewerPlan != null)
                            {
                                //for (int i = 0; i < NewerPlan.Count; i++)
                                //{
                                DateTime from_date = Convert.ToDateTime(workSheet.Cells[1, 11].Value.ToString());
                                DateTime to_date = Convert.ToDateTime(workSheet.Cells[1, 35].Value.ToString());
                                //    db.TBL_PROD_PLAN.Remove(NewerPlan[i]);
                                //    db.SaveChanges();
                                db.Database.ExecuteSqlCommand("delete from TBL_PROD_PLAN where PLAN_DATE between {0} and {1}", from_date, to_date );
                                //}
                            }


                            for (int rowIterator = 3; rowIterator <= noOfRow; rowIterator++)
                            {
                                MesRow = rowIterator;
                                labor = workSheet.Cells[rowIterator, 9].Value.ToString();
                                for (int i = 11; i <= 35; i += 4)
                                {
                                    MesCol = i;
                                    pl_date = (workSheet.Cells[1, i].Value == null ? null : workSheet.Cells[1, i].Value.ToString());
                                    if (Convert.ToDateTime(pl_date).Date >= DateTime.Now.Date)
                                    { 
                                            sah = (workSheet.Cells[rowIterator, i + 1].Value == null ? null : workSheet.Cells[rowIterator, i + 1].Value.ToString());
                                            style = (workSheet.Cells[rowIterator, i - 1].Value== null? null : workSheet.Cells[rowIterator, i - 1].Value.ToString());
                                            target = (workSheet.Cells[rowIterator, i + 2].Value == null ? null : workSheet.Cells[rowIterator, i + 2].Value.ToString());
                                            group = (workSheet.Cells[rowIterator, 2].Value == null ? null : workSheet.Cells[rowIterator, 2].Value.ToString());
                                            line = (workSheet.Cells[rowIterator, 3].Value == null ? null : workSheet.Cells[rowIterator, 3].Value.ToString());
                                        if (pl_date != null && sah != null && style != null && target!= null && sah != "#REF!" && style != "#REF!" && target != "#REF!" && line != null)
                                            {
                                            TBL_GROUP_MST Group_record = db.TBL_GROUP_MST.Where(t => t.GROUP_NAME == group).SingleOrDefault();
                                            //TBL_GROUP_MST Group_record = (from item in db.TBL_GROUP_MST where item.GROUP_NAME.Equals(@group) select item).SingleOrDefault() ;
                                            if (Group_record == null) OtherMes = ";Group/Line not found!";
                                            // insert new record
                                            TBL_PROD_PLAN tmp = new TBL_PROD_PLAN();
                                                tmp.PLAN_DATE = Convert.ToDateTime(pl_date);
                                                tmp.SAH = Convert.ToDouble(sah);
                                                tmp.STYLE = style;
                                                tmp.TARGET_QTY = Convert.ToDouble(target);
                                                tmp.GROUP_ID = Group_record.GROUP_ID;
                                                tmp.LABOR = Convert.ToDouble(labor);
                                                tmp.TS_1 = DateTime.Now;
                                                tmp.LINE_ID = Convert.ToInt16(line);
                                                tmp.TS_1_USER = userLogin.Username;
                                                db.TBL_PROD_PLAN.Add(tmp);
                                                db.SaveChanges();
                                            }
                                    }
                                }
                            }
                        }
                        ViewBag.Status = "Upload Sucessful.";
                    }
            }
            catch (Exception e)
            {
                ViewBag.Status =  "Error, contact to IT. " + e.Message + ",  " + Convert.ToString(MesRow) + ":" + Convert.ToString(MesCol) + OtherMes;
            }
        }
            return View("UploadProdPlan");
        }

    }
}