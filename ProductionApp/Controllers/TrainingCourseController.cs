using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProductionApp.Models;
using OfficeOpenXml;
using System.IO;
using System.Data.Entity;
using System.Globalization;
using ProductionApp.Controllers;
using ProductionApp.Helpers;
using System.Net;
using System.Windows;
using DocumentFormat.OpenXml.ExtendedProperties;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;

namespace ProductionApp.Controllers
{
    public class TrainingCourseController:BaseController
    {
        // GET: TrainingCourse
        public ActionResult Index()
        {
            double? d = 0; double? h = 0; double? c = 0;
            dynamic mymodel = new System.Dynamic.ExpandoObject();
            mymodel.Course = db.HR_Training_Plan.ToList();
            var ds = db.HR_Training_Plan.ToList();
            foreach (var p in ds)
            {
                d = d + p.Duration;
                h = h + p.TotalHours;
                c = c + p.Cost;
            }
            ViewBag.duration = d;
            ViewBag.totalHour = h;
            ViewBag.cost = c;
            return View(mymodel);
        }

        public ActionResult getOneCourse(string id)
        {

            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                //set ProxyCreation to false
                db.Configuration.ProxyCreationEnabled = false;
                var data = db.HR_Training_Plan.Where(s => s.id.ToString() == id.ToString()).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(ex.Message);
            }
            finally
            {
                //restore ProxyCreation to its original state
                db.Configuration.ProxyCreationEnabled = proxyCreation;
            }
        }
        
        public static bool IsNumber(string aNumber)
        {
            Double temp_big_int;
            var is_number = Double.TryParse(aNumber, out temp_big_int);
            return is_number;
        }

        public ActionResult UploadCourse()
        {

            if (Request != null)
            {
                int MesRow = 0;
                try
                {

                    HttpPostedFileBase file = Request.Files["UploadedFile"];
                    if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                    {
                        DateTime today = Utilities.GetDate_VietNam(DateTime.Now);
                       
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

                            //valid format and get level
                            var level1 = "A";
                            var level2 = "I";
                            var cellA = workSheet.Cells[10, 1].Value;
                            if (cellA.ToString().Contains("A."))
                            {
                                for (var rowIterator = 12; rowIterator <= noOfRow; rowIterator++)
                                {
                                    MesRow = rowIterator;
                                    cellA = workSheet.Cells[rowIterator, 1].Value;
                                    if (cellA == null)
                                    {
                                        break;
                                    }
                                    if (cellA.ToString().Contains("B.") || cellA.ToString().Contains("C."))
                                    {
                                        level1 = cellA.ToString().Split('.')[0].Trim();
                                        level2 = "I";
                                        continue;
                                    }
                                    if (cellA.ToString().Contains("I.") || cellA.ToString().Contains("II.") || cellA.ToString().Contains("III."))
                                    {
                                        level2 = cellA.ToString().Split('.')[0].Trim();
                                        continue;
                                    }
                                    if (cellA.ToString().Contains("IV"))
                                    {
                                        level2 = "IV";
                                        continue;
                                    }
                                    

                                    var courseName = workSheet.Cells[rowIterator, 2].Value.ToString();
                                    var trainer = workSheet.Cells[rowIterator, 3].Value.ToString();
                                    var Internal = workSheet.Cells[rowIterator, 4].Value ?? "F";
                                    var external = workSheet.Cells[rowIterator, 5].Value ?? "F";
                                    var category = workSheet.Cells[rowIterator, 6].Value.ToString();
                                    var targetAudience = workSheet.Cells[rowIterator, 7].Value.ToString();
                                    var direct = workSheet.Cells[rowIterator, 8].Value ?? "F";
                                    var indirect = workSheet.Cells[rowIterator, 9].Value ?? "F";
                                    var officeStaff = workSheet.Cells[rowIterator, 10].Value ?? "F";
                                    var supervisor = workSheet.Cells[rowIterator, 11].Value ?? "F";
                                    var manager = workSheet.Cells[rowIterator, 12].Value ?? "F";
                                    var estimatedPax = Convert.ToDouble(workSheet.Cells[rowIterator, 13].Value ?? 0);
                                    var duration = Convert.ToDouble(workSheet.Cells[rowIterator, 14].Value ?? 0);
                                    var totalHours = Convert.ToDouble(workSheet.Cells[rowIterator, 15].Value ?? 0);
                                    var jan = workSheet.Cells[rowIterator, 16].Value ?? "F";
                                    var feb = workSheet.Cells[rowIterator, 17].Value ?? "F";
                                    var mar = workSheet.Cells[rowIterator, 18].Value ?? "F";
                                    var apr = workSheet.Cells[rowIterator, 19].Value ?? "F";
                                    var may = workSheet.Cells[rowIterator, 20].Value ?? "F";
                                    var jun = workSheet.Cells[rowIterator, 21].Value ?? "F";
                                    var jul = workSheet.Cells[rowIterator, 22].Value ?? "F";
                                    var aug = workSheet.Cells[rowIterator, 23].Value ?? "F";
                                    var sep = workSheet.Cells[rowIterator, 24].Value ?? "F";
                                    var oct = workSheet.Cells[rowIterator, 25].Value ?? "F";
                                    var nov = workSheet.Cells[rowIterator, 26].Value ?? "F";
                                    var dec = workSheet.Cells[rowIterator, 27].Value ?? "F";
                                    var cost = Convert.ToDouble(workSheet.Cells[rowIterator, 28].Value ?? 0);
                                    var explanation = workSheet.Cells[rowIterator, 29].Value.ToString();

                                    //db.HR_Training_Plan.RemoveRange(db.HR_Training_Plan.Where(a => a.CourseName == courseName));

                                    var course = db.HR_Training_Plan.SingleOrDefault(a => a.CourseName == courseName) ?? new HR_Training_Plan();
                                    course.CourseName = courseName;
                                    course.Trainer = trainer;
                                    course.Internal = Internal.ToString();
                                    course.External = external.ToString();
                                    course.Category = category;
                                    course.TargetAudience = targetAudience;
                                    course.Direct = direct.ToString();
                                    course.Indirect = indirect.ToString();
                                    course.OfficeStaff = officeStaff.ToString();
                                    course.Supervisor = supervisor.ToString();
                                    course.Manager = manager.ToString();
                                    course.EstimatedPax = estimatedPax;
                                    course.Duration = duration;
                                    course.TotalHours = totalHours;
                                    course.Cost = cost;
                                    course.Jan = jan.ToString();
                                    course.Feb = feb.ToString();
                                    course.Mar = mar.ToString();
                                    course.Apr = apr.ToString();
                                    course.May = may.ToString();
                                    course.Jun = jun.ToString();
                                    course.Jul = jul.ToString();
                                    course.Aug = aug.ToString();
                                    course.Sep = sep.ToString();
                                    course.Oct = oct.ToString();
                                    course.Nov = nov.ToString();
                                    course.Dec = dec.ToString();
                                    course.Explanation = explanation;
                                    course.Year = today.Year;
                                    course.Level1 = level1;
                                    course.Level2 = level2;

                                    if (course.id == 0)
                                        db.HR_Training_Plan.Add(course);
                                    db.SaveChanges();
                                }
                            }
                            else
                            {
                                ViewBag.Status = "File format invalid!";
                            }

                        }
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Status = "Error, need contact to IT. " + e.Message + ",  Row " + Convert.ToString(MesRow);

                    //Utilities.WriteLogException(e, "Trainee/Index");
                    //   return RedirectToAction("Index");
                }
            }
            ViewBag.Status = "Thành công!";
            return RedirectToAction("Index");
        }

        public ActionResult EditCourse(string id, string Jan, string Feb, string Mar, string Apr, string May, string Jun,
            string Jul, string Aug, string Sep, string Oct, string Nov, string Dec, string Explanation)
        {

            var kq = "";
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                //set ProxyCreation to false
                db.Configuration.ProxyCreationEnabled = false;

                var tc = db.HR_Training_Plan.Single(s => s.id.ToString() == id);

                tc.Jan = Jan;
                tc.Feb = Feb;
                tc.Mar = Mar;
                tc.Apr = Apr;
                tc.May = May;
                tc.Jun = Jun;
                tc.Jul = Jul;
                tc.Aug = Aug;
                tc.Sep = Sep;
                tc.Oct = Oct;
                tc.Nov = Nov;
                tc.Dec = Dec;
                tc.Explanation = Explanation;
                db.SaveChanges();
                kq = "Edit successfully!";
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(ex.Message);
            }
            finally
            {
                //restore ProxyCreation to its original state
                db.Configuration.ProxyCreationEnabled = proxyCreation;
            }

            return Json(new { msg = kq }, JsonRequestBehavior.AllowGet);
            //  return RedirectToAction("Student");
        }

    }

    //    public ActionResult EditCourse(int id, string CourseName, string Trainer, string Internal, string External, string Category,
    //    string TargetAudience, string Direct,
    //    string Indirect, string OfficeStaff, string Supervisor, string Manager, int EstimatedPax, float Duration, float TotalHours,
    //    string Cost, string Jan, string Feb, string Mar, string Apr, string May, string Jun, string Jul, string Aug, string Sep, string Oct,
    //    string Nov, string Dec, string Explanation, int Year)
    //    {

    //        var kq = "";
    //        bool proxyCreation = db.Configuration.ProxyCreationEnabled;
    //        try
    //        {
    //            //set ProxyCreation to false
    //            db.Configuration.ProxyCreationEnabled = false;

    //            var tc = db.HR_Training_Plan.Single(s => s.id == id);

    //            tc.CourseName = CourseName;
    //            tc.Trainer = Trainer;
    //            tc.Internal = Internal;
    //            tc.External = External;
    //            tc.Category = Category;
    //            tc.TargetAudience = TargetAudience;
    //            tc.Direct = Direct;
    //            tc.Indirect = Indirect;
    //            tc.OfficeStaff = OfficeStaff;
    //            tc.Supervisor = Supervisor;
    //            tc.Manager = Manager;
    //            tc.EstimatedPax = EstimatedPax;
    //            tc.Duration = Duration;
    //            tc.TotalHours = EstimatedPax * (int)Duration;
    //            tc.Cost = Cost;
    //            tc.Jan = Jan;
    //            tc.Feb = Feb;
    //            tc.Mar = Mar;
    //            tc.Apr = Apr;
    //            tc.May = May;
    //            tc.Jun = Jun;
    //            tc.Jul = Jul;
    //            tc.Aug = Aug;
    //            tc.Sep = Sep;
    //            tc.Oct = Oct;
    //            tc.Nov = Nov;
    //            tc.Dec = Dec;
    //            tc.Explanation = Explanation;
    //            tc.Year = Year;

    //            db.SaveChanges();
    //            kq = "Edit successfully!";
    //        }
    //        catch (Exception ex)
    //        {
    //            Response.StatusCode = (int)HttpStatusCode.BadRequest;
    //            return Json(ex.Message);
    //        }
    //        finally
    //        {
    //            //restore ProxyCreation to its original state
    //            db.Configuration.ProxyCreationEnabled = proxyCreation;
    //        }

    //        return Json(new { msg = kq }, JsonRequestBehavior.AllowGet);
    //        //  return RedirectToAction("Student");
    //    }

    //}
}