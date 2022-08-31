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
using System.Data.Entity.SqlServer;
using System.Net;

namespace ProductionApp.Controllers
{
    public class TraineeController:BaseController
    {

        // GET: Trainee
        public ActionResult Index()
        {
            dynamic mymodel = new System.Dynamic.ExpandoObject();
            mymodel.departments = db.TBL_DEPARTMENT_MST.ToList();
            mymodel.emp = db.HR_Training_Emp.OrderBy(a => a.Emp_ID).ToList();

            return View(mymodel);
        }

        public JsonResult GetOneDetails(int id)
        {
            //var ds = db.HR_Employee_Course.Where(a => a.Emp_ID.ToString() == id.ToString());
            //return Json(ds, JsonRequestBehavior.AllowGet);
            var ds = db.HR_Employee_Course.Where(a => a.Emp_ID.ToString() == id.ToString()).Select(a =>
                new
                {
                    CourseName = a.HR_Training_Plan.CourseName,
                    Training_Date = DbFunctions.Right("00" + a.Training_Date.Value.Day, 2) + "/" + DbFunctions.Right("00" + a.Training_Date.Value.Month, 2) + "/" + a.Training_Date.Value.Year,
                    Trainner = a.Trainner,

                }

                ).ToList();
            return Json(ds, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetCourse()
        {

            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                //set ProxyCreation to false
                db.Configuration.ProxyCreationEnabled = false;
                var ds = db.TBL_DEPARTMENT_MST.ToList();
                return Json(ds, JsonRequestBehavior.AllowGet);
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


        public ActionResult SearchOneTrainee(string id)
        {
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                //set ProxyCreation to false
                db.Configuration.ProxyCreationEnabled = false;
                var ds = db.HR_Training_Emp.Where(s => s.Emp_ID.ToString() == id).ToList();
                return Json(ds, JsonRequestBehavior.AllowGet);
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

        [HttpPost]
        public ActionResult UploadEmployee()
        {

            if (Request != null)
            {
                int MesRow = 0;
                try
                {

                    HttpPostedFileBase file = Request.Files["UploadedFile"];
                    if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                    {
                        var mss = "";
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
                            for (var rowIterator = 3; rowIterator <= noOfRow; rowIterator++)
                            {
                                MesRow = rowIterator;
                                var empId = workSheet.Cells[rowIterator, 1].Value;
                                var name = workSheet.Cells[rowIterator, 2].Value;
                                var department = workSheet.Cells[rowIterator, 3].Value;
                                var title = workSheet.Cells[rowIterator, 4].Value;
                                var trainingDate = workSheet.Cells[rowIterator, 5].Value;
                                var trainner = workSheet.Cells[rowIterator, 6].Value;
                                var updatedBy = workSheet.Cells[rowIterator, 7].Value;
                                var courseId = workSheet.Cells[rowIterator, 8].Value;
                                if (empId == null || empId.ToString() == "" || courseId == null || courseId.ToString() == "")
                                {
                                    mss = "Vui lòng kiểm tra lại dữ liệu, mã nhân viên hoặc mã khóa học không được rỗng!";
                                    break;
                                }


                                else
                                {

                                    var emRecord = db.HR_Training_Emp.SingleOrDefault(t => t.Emp_ID.ToString() == empId.ToString());
                                    //var list = from c in db.HR_Employee_Course
                                    //           where c.Emp_ID.ToString() == empId.ToString() && c.Course_ID.ToString() == courseId.ToString()
                                    //           select c;

                                    if (emRecord == null)
                                    {
                                        emRecord = new HR_Training_Emp()
                                        {
                                            Emp_ID = empId.ToString(),
                                            NAME = name.ToString(),
                                            DEPARTMENT = Convert.ToInt32(department),
                                            UpdatedBy = updatedBy.ToString(),

                                        };
                                        db.HR_Training_Emp.Add(emRecord);

                                    }

                                    db.HR_Employee_Course.RemoveRange(db.HR_Employee_Course.Where(a => a.Emp_ID.ToString() == empId.ToString() && a.Course_ID.ToString() == courseId.ToString()));
                                    var empCourse = new HR_Employee_Course();
                                    {
                                        empCourse.Emp_ID = empId.ToString();
                                        empCourse.Course_ID = Convert.ToInt32(courseId);
                                        empCourse.Title = title.ToString();
                                        // em.CourseName = CourseName.ToString();
                                        empCourse.Training_Date = (DateTime)trainingDate;
                                        empCourse.Trainner = trainner.ToString();
                                    }
                                    db.HR_Employee_Course.Add(empCourse);


                                    db.SaveChanges();
                                    mss = "Upload thành công!";
                                }
                            }

                        }
                        ViewBag.Status = mss;
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
    }
}