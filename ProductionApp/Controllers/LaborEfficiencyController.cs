using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml;
using ProductionApp.Models;
using System.Data.Entity;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ProductionApp.Controllers
{
    public class LaborEfficiencyController:BaseController
    {
        // GET: MasterData
        public ActionResult Index()
        {
            if ((UserModels)Session["SignedInUser"] == null)
            {
                //return RedirectToAction("NeedLogin", "Notification");
            }
            return View("UploadLaborEfficiency");
        }

        public ActionResult UploadLaborEfficiency_Text()
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
                        //var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                        //using (var package = new ExcelPackage(file.InputStream))
                        StreamReader sr = new StreamReader(file.InputStream);
                        //foreach (string row in sr.)
                        StringBuilder strbuild = new StringBuilder();
                        //strbuild.AppendFormat(sr.ReadLine());

                        while (sr.Peek() >= 0)
                        {
                            string abcd = sr.ReadLine();
                            abcd = Regex.Replace(abcd, @"\s+", " ");
                            //strbuild.AppendFormat(sr.ReadLine());
                            //string abcd = sr.ReadLine().Replace("  ", " ");
                            string[] ab = abcd.Split(' ');
                            //word = Regex.Replace(word, @"\s", "");
                            for (int b = 0; b < ab.Count(); b++)
                            {
                                var a = ab[b];
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
            //List<PROC_GET_TOP5_ABSM_UPDATE_Result> top5 = (from item in db.GetTop5AbsmUpdate() select item).ToList();
            return View("UploadLaborEfficiency");
        }

        public ActionResult UploadIncentive()
        {
            if (Request != null)
            {
                int MesRow = 0, MesCol = 0;
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

                            string[] WY = string.Concat(workSheet.Cells[3, 5].Value.ToString()).Split(' ', '.');
                            int Week = Convert.ToInt16(WY[3]);
                            int year = Convert.ToInt16(WY[2]);

                            TBL_INCENTIVE ExistWeek = db.TBL_INCENTIVE.Where(t => t.WEEK == Week && t.YEAR == year).FirstOrDefault();
                            if (ExistWeek != null)
                            {
                                db.Database.ExecuteSqlCommand("delete from TBL_INCENTIVE where WEEK = {0} and year = {1}", Week, year);
                            }


                            int Biz =0; string Group ="";
                            for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                            {
                                var NullRow = workSheet.Cells[rowIterator, 1].Value;
                            if (NullRow != null)
                            {
                                if (string.Concat(workSheet.Cells[rowIterator, 1].Value.ToString()) == "SupPlt:")
                                {
                                    string[] SupPlt = string.Concat(workSheet.Cells[rowIterator, 2].Value.ToString()).Split(' ');
                                    Biz = Convert.ToInt16(SupPlt[0]);
                                    Group = workSheet.Cells[rowIterator, 5].Value == null? "": workSheet.Cells[rowIterator, 5].Value.ToString();

                                }
                                else
                                {
                                    //workSheet.Cells[rowIterator, 1].Value
                                    int n;
                                    bool isNumeric = int.TryParse(workSheet.Cells[rowIterator, 1].Value.ToString(), out n);
                                    //if(workSheet.Cells[rowIterator, 1].Value != null )
                                    if (workSheet.Cells[rowIterator, 1].Value.ToString() != "" && string.Concat(workSheet.Cells[rowIterator, 1].Value.ToString()).Length == 6 && isNumeric)
                                    {
                                        TBL_INCENTIVE IncRecord = new TBL_INCENTIVE();
                                        IncRecord.WEEK = Week;
                                        IncRecord.YEAR = year;
                                        IncRecord.BIZ_ID = Biz;
                                        IncRecord.GROUP = Group;
                                        IncRecord.EMPLOYEE_ID = Convert.ToInt64(workSheet.Cells[rowIterator, 1].Value.ToString());
                                        IncRecord.EMPLOYEE_NAME = (workSheet.Cells[rowIterator, 1].Value.ToString());
                                        IncRecord.WORK_HOURS = Convert.ToDouble(workSheet.Cells[rowIterator, 3].Value.ToString());
                                        IncRecord.ON_STANDARD = Convert.ToDouble(workSheet.Cells[rowIterator, 7].Value.ToString());
                                        IncRecord.OFF_STANDARD = Convert.ToDouble(workSheet.Cells[rowIterator, 8].Value.ToString());
                                        IncRecord.PLANT_EFF = Convert.ToDouble(workSheet.Cells[rowIterator, 10].Value.ToString());
                                        IncRecord.DOL_EFF = Convert.ToDouble(workSheet.Cells[rowIterator, 11].Value.ToString());
                                        IncRecord.TS_1 = DateTime.Now;
                                        IncRecord.TS_1_USER = "Nanguye";
                                        db.TBL_INCENTIVE.Add(IncRecord);
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
                    ViewBag.Status = "Error, contact to IT. " + e.Message + ",  " + Convert.ToString(MesRow) + ":" + Convert.ToString(MesCol) + OtherMes;
                }
            }
            return View("UploadIncentive");
        }

    }
}