using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProductionApp.Models;
using System.IO;
using System.Data.Entity;
using System.Globalization;
using ProductionApp.Controllers;
using ProductionApp.Helpers;
using System.Net;
using OfficeOpenXml;
using System.Diagnostics;

namespace ProductionApp.Controllers
{
    public class TestOnlineController:BaseController
    {
        // GET: TestOnline
        public ActionResult Index()
        {
            return View();
        }

      
        public ActionResult CheckInfor(string EmpID)
        {

            DateTime today =  Utilities.GetDate_VietNam(DateTime.Now);
                dynamic mymodel = new System.Dynamic.ExpandoObject();
            mymodel.dataSource = (from b in db.HR_Employee_Course
                                  join s in db.HR_Exam on b.Course_ID equals s.CourseID
                                  join c in db.HR_Training_Plan on s.CourseID equals c.id
                                  where b.Emp_ID.ToString() == EmpID && DbFunctions.Right("00" + today.Day.ToString(), 2) == DbFunctions.Right("00" + s.Date.Value.Day, 2)
                                   && DbFunctions.Right("00" + today.Month.ToString(), 2) == DbFunctions.Right("00" + s.Date.Value.Month, 2)
                                   && today.Year == s.Date.Value.Year
                                  select new InforExamToAccept
                                  {
                                      Emp_ID = b.Emp_ID,
                                      ExamID = s.ExamID,
                                      ExamName = s.NameExam,
                                     CourseID = s.CourseID.Value,
                                      CourseName = c.CourseName,
                                      Point = s.Point.Value,
                                      State = s.State,
                                      Time = s.Time.Value,
                                      Date = DbFunctions.Right("00" + s.Date.Value.Day, 2) + "/" + DbFunctions.Right("00" + s.Date.Value.Month, 2) + "/" + s.Date.Value.Year,
                                      QuestionNumber = s.QuestionNumber.Value
                                  }).ToList();
                // return Json(dataSource, JsonRequestBehavior.AllowGet);
                return View(mymodel);
          
        }


        public ActionResult getQuestion(int ExamID)
        {
            var kq = "";
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var ds = db.HR_Exam_Detail.Where(x => x.ExamID == ExamID).ToList();
            return Json(ds, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(ex.Message);
            }
            finally
            {
                db.Configuration.ProxyCreationEnabled = proxyCreation;
            }

        }

        public ActionResult GetKeyExam(int CourseID, int ExamID)
        {
         
            var ListKeyAnswers = (from  exam_detail in db.HR_Exam_Detail 
                                  join answer in db.HR_Answer on exam_detail.QuestionID equals answer.QuestionID
                                  join exam in db.HR_Exam on exam_detail.ExamID equals exam.ExamID
                                  where answer.State == 1 && exam.CourseID == CourseID && exam.ExamID == ExamID
                                  select new
                                  {
                                      QuestionID = answer.QuestionID,
                                      AnswerID = answer.AnswerID,
                                     
                                  }).ToList();
            
           
          
            return Json(ListKeyAnswers, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
             public ActionResult GetResult(int ExamID, float Score, int EmpID)
        {

            //var kq = "";
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                var ds = db.HR_Result.Where(x => x.ExamID == ExamID && x.Emp_ID == EmpID).SingleOrDefault();
                if(ds != null)
                {
                    ds.Point = Score;
                }
                else
                {

                    var r = new HR_Result();
                    r.ExamID = ExamID;
                    r.Point = Math.Round(Score,1) ;
                    r.Emp_ID = EmpID;
                    db.HR_Result.Add(r);
                }
                db.SaveChanges();
                return Json(new { msg = "Successfully!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(ex.Message);
            }
            finally
            {
                db.Configuration.ProxyCreationEnabled = proxyCreation;
            }
        }

        public ActionResult ShowResult(int ExamID, int EmpID, float Rate, int CorrectAnswers)
        {

            dynamic mymodel = new System.Dynamic.ExpandoObject();
            //mymodel.getOneResult = db.HR_Result.Where(x => x.ExamID == ExamID && x.Emp_ID == EmpID).ToList();

            mymodel.getOneResult = (
                from re in db.HR_Result
                join x in db.HR_Exam on re.ExamID equals x.ExamID
                join c in db.HR_Training_Plan on x.CourseID equals c.id
                join e in db.HR_Training_Emp on re.Emp_ID.ToString() equals e.Emp_ID
                where re.ExamID == ExamID && re.Emp_ID == EmpID
                select new ResultExamForEmp
                {
                    EmployeeName = e.NAME,
                    ExamName = x.NameExam,
                    Score = re.Point.ToString(),
                    Time = x.Time.ToString(),
                    CourseName = c.CourseName,
                    QuestionNumber = x.QuestionNumber.ToString(),
                    Rate = Rate,
                    CorrectAnswers = CorrectAnswers,
                }).ToList();

          
            return View(mymodel);
        }
    }
}