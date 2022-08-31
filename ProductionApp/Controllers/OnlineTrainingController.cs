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
    public class OnlineTrainingController:BaseController
    {                                   
        // GET: OnlineTraining
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Information()
        {
            return View();
        }
       

        public ActionResult AllQuestions()
        {
            return View(db.HR_Question.ToList());
        }

        public ActionResult AllExams()
        {
            dynamic mymodel = new System.Dynamic.ExpandoObject();
            mymodel.LstCourse = db.HR_Training_Plan.ToList();
            mymodel.LstExam = db.HR_Exam.ToList();
            return View(mymodel);
        }

        public ActionResult TestOnline()
        {
            return View();
        }

        public ActionResult getOneQuestionDetail(int id)
        {
            var ds = db.HR_Exam.Where(a => a.ExamID.ToString() == id.ToString());
            return Json(ds, JsonRequestBehavior.AllowGet);
           
        }

        public ActionResult getCourseByQuestion(int macauhoi)
        {
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var ds = db.HR_Question.Where(a => a.Question_ID == macauhoi).ToList();
                return Json(ds, JsonRequestBehavior.AllowGet);
        }
            catch (Exception ex)
            {
                Response.StatusCode = (int) HttpStatusCode.BadRequest;
                return Json(ex.Message);
    }
            finally
            {
                db.Configuration.ProxyCreationEnabled = proxyCreation;
            }
        }

         public ActionResult GetDetailExam(int ExamID)
        {
          
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var ds = db.HR_Exam.Where(a => a.ExamID.ToString() == ExamID.ToString()).SingleOrDefault();
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

        public ActionResult EditDetailExam(int ExamID, string ExamName, int CourseID, int Time, DateTime Date, double Score, int NumberQuestion )
        {
            var kq = "";
            var ds= db.HR_Exam.Where(a => a.ExamID == ExamID).SingleOrDefault();
             var lstQues = (from b in db.HR_Exam_Detail
                              where b.ExamID == ExamID
                             select new 
                             {
                                QuestionID = b.QuestionID
                             }).ToList();
            var CauHoiDaCoTrongDe = lstQues.Count;
            var CourseName = (from b in db.HR_Training_Plan
                             where b.id == CourseID
                             select new
                             {
                                 CourseName = b.CourseName
                             }).SingleOrDefault();
           
           // var NganHangCauHoi = db.HR_Question.Where(s=>s.CourseID == CourseID).OrderBy(r => Guid.NewGuid()).ToList();
            var NganHangCauHoi = db.HR_Question.Where(s=>s.CourseID == CourseID && s.HR_Exam_Detail.Count==0).OrderBy(r => Guid.NewGuid()).ToList();
            var lstCurrentQues = db.HR_Exam_Detail.Where(s => s.ExamID == ExamID ).OrderBy(r => Guid.NewGuid()).ToList();
            if (ds != null)
            {
                ds.NameExam = ExamName;
                ds.CourseID = CourseID;
                ds.Time = Time;
                ds.Date = Date;
                ds.Point = Score;
                ds.QuestionNumber = NumberQuestion;
                var BoSung = CauHoiDaCoTrongDe - NumberQuestion;
                if (BoSung<0 && NganHangCauHoi.Count+ CauHoiDaCoTrongDe >= NumberQuestion)
                {
                    // randomQuestion( NumberQuestion);
                    var tmp = new List<HR_Exam_Detail>();
                   
                    foreach (var item in NganHangCauHoi)
                    {
                        tmp.Add(new HR_Exam_Detail() { ExamID =ds.ExamID,QuestionID=item.Question_ID });
                        BoSung++;
                        if (BoSung == 0)
                            break;
                    }
                    db.HR_Exam_Detail.AddRange(tmp);
                }
                if(BoSung > 0 && NganHangCauHoi.Count + CauHoiDaCoTrongDe >= NumberQuestion)
                {
                    foreach (var item in lstCurrentQues)
                    {
                        db.HR_Exam_Detail.Remove(item);
                        BoSung--;
                        if (BoSung == 0)
                            break;
                    }
                    db.SaveChanges();
                }
                else
                {
                    kq = "Ngân hàng câu hỏi của khóa học " + CourseName + "không đủ để ra đề. Vui lòng bổ sung ngân hàng câu hỏi.";
                }
            }
                    
                   
                
            
                db.SaveChanges();
                kq = "Updated successfully!";
            return Json(new { msg = kq }, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult ReportResult()
        {
            dynamic mymodel = new System.Dynamic.ExpandoObject();
            mymodel.ResultTest = db.HR_Result.ToList();
            mymodel.TrainingPlan = db.HR_Training_Plan.ToList();
            mymodel.LstEmployee = db.HR_Training_Emp.ToList();
            mymodel.LstExam = db.HR_Exam.ToList();
            return View(mymodel);
        }

        public ActionResult LoadDateExamByCourse(string CourseID)
        {
          
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var ds = db.HR_Exam.Where(s => s.CourseID.ToString() == CourseID).ToList();
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

        public ActionResult getResultByCourseDate(int CourseID, DateTime DateReport)
        {
            
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var ds = from b in db.HR_Result
                         join s in db.HR_Exam on b.ExamID equals s.ExamID
                         join c in db.HR_Training_Plan on s.CourseID equals c.id
                         join e in db.HR_Training_Emp on  b.Emp_ID.ToString() equals e.Emp_ID.ToString()
                         where s.CourseID == CourseID &&  s.Date.Value.Year == DateReport.Year
                       && s.Date.Value.Month == DateReport.Month
                       && s.Date.Value.Day == DateReport.Day
                         select new { ResultID = b.ResultID, ExamID = b.ExamID, DateExam =  DbFunctions.Right("00" + s.Date.Value.Day, 2) + "/" + DbFunctions.Right("00" + s.Date.Value.Month, 2) + "/" + s.Date.Value.Year, CourseName = c.CourseName, ExamName = s.NameExam, Point = b.Point, EmployeeName = e.NAME };

                var dataSource = ds.ToList();
                return Json(dataSource, JsonRequestBehavior.AllowGet);
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

        public ActionResult UpdateEditState(int AnswerID, int QuestionID, int State)
        {
            var kq = "";
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                db.Configuration.ProxyCreationEnabled = false;

                var ans = db.HR_Answer.Single(s => s.AnswerID == AnswerID);

                if (ans.State == 0)
                {
                    ans.QuestionID = QuestionID;
                    ans.AnswerID = AnswerID;
                    ans.State = 1;
                }
                else
                {
                    ans.QuestionID = QuestionID;
                    ans.AnswerID = AnswerID;
                    ans.State = 0;
                }

                db.SaveChanges();
                kq = "Edited the correct answer successfully!";
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

            return Json(new { msg = kq }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateQuestionAnswer(int QuestionID,int CourseID,string ContentQuestion, int AnswerID, string ContentAnswer)
        {
            var kq = "";
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                db.Configuration.ProxyCreationEnabled = false;

                var ques = db.HR_Question.Single(s => s.Question_ID == QuestionID);
                    ques.Content = ContentQuestion;
                    ques.CourseID = CourseID;

                var ans = db.HR_Answer.Single(s => s.AnswerID == AnswerID);
                   ans.Content = ContentAnswer;

                db.SaveChanges();
                kq = "Edited the question information successfully!";
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

            return Json(new { msg = kq }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetOneDetails(int id)
        {
            dynamic mymodel = new System.Dynamic.ExpandoObject();
            mymodel.Question = db.HR_Question.Where(a => a.Question_ID ==id).ToList();
            mymodel.Course = db.HR_Training_Plan.ToList();
            var ktraLst = db.HR_Answer.Where(a => a.QuestionID == id).ToList();
            if(ktraLst !=null)
            {
                mymodel.ds = db.HR_Answer.Where(a => a.QuestionID.ToString() == id.ToString()).Select(a =>
               new DetailQuestion
               {
                   ContentAns = a.Content,
                   AnswerID = a.AnswerID,
                   State = (int)a.State,
                   ContentQues = a.HR_Question.Content,
                   QuestionID = a.HR_Question.Question_ID
               }).ToList();
            }
            ViewBag.maacauhoi = id;
            return View(mymodel);
        }



        [HttpPost]
        public ActionResult UploadAnswer(string id)
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
                                var ContentAnswer = workSheet.Cells[rowIterator, 1].Value;
                                var Key = workSheet.Cells[rowIterator, 2].Value;

                                if (ContentAnswer == null || ContentAnswer.ToString() == "")
                                {
                                    mss = "Vui lòng kiểm tra lại dữ liệu, nội dung câu trả lời không được rỗng!";
                                    break;
                                }
                                var ds = db.HR_Answer.ToList();
                                foreach(var p in ds)
                                {
                                    if(p.Content == ContentAnswer.ToString())
                                    {
                                        db.HR_Answer.Remove(p);
                                    }
                                   
                                };

                                var AnswerRecord = new HR_Answer()
                                {
                                    Content = ContentAnswer.ToString(),
                                    QuestionID = Convert.ToInt32(id),
                                    State = Convert.ToInt32(Key)
                                };
                                    db.HR_Answer.Add(AnswerRecord);
                                db.SaveChanges();
                                mss = "Upload thành công!";
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
            return RedirectToAction("GetOneDetails", "OnlineTraining", new { @id = id });
        }

        [HttpPost]
        public ActionResult UploadQuestion()
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
                                var ContentQuestion = workSheet.Cells[rowIterator, 1].Value;
                                var CourseID = workSheet.Cells[rowIterator, 2].Value;

                                if (ContentQuestion == null || ContentQuestion.ToString() == "")
                                {
                                    mss = "Vui lòng kiểm tra lại dữ liệu, nội dung câu trả lời không được rỗng!";
                                    break;
                                }
                                else
                                {
                                    var QuestionRecord = db.HR_Question.SingleOrDefault(t => t.Content == ContentQuestion.ToString());
                                    if (QuestionRecord != null)
                                    {
                                        QuestionRecord.Content = ContentQuestion.ToString();
                                        QuestionRecord.CourseID = Convert.ToInt32(CourseID) ;
                                    }

                                    else
                                    {
                                        var ques = new HR_Question()
                                        {
                                            Content = ContentQuestion.ToString(),
                                            CourseID = Convert.ToInt32(CourseID)
                                        };
                                        db.HR_Question.Add(ques);
                                    }
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
            return RedirectToAction("AllQuestions");
        }

        [HttpPost]
        public ActionResult UploadExam()
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
                                var ExamName = workSheet.Cells[rowIterator, 1].Value;
                                var CourseID = workSheet.Cells[rowIterator, 2].Value;
                                var QuestionNumber = workSheet.Cells[rowIterator, 3].Value;
                                var Time = workSheet.Cells[rowIterator, 4].Value;
                                var Date = workSheet.Cells[rowIterator, 5].Value;
                                var Point = workSheet.Cells[rowIterator, 6].Value;
                                var State = "0";
                                var NumberQuestion = Convert.ToInt32(QuestionNumber);
                                if (ExamName == null || ExamName.ToString() == "")
                                {
                                    mss = "Vui lòng kiểm tra lại dữ liệu, nội dung câu trả lời không được rỗng!";
                                    break;
                                }
                                else
                                {
                                    var QuestionRecord = db.HR_Exam.SingleOrDefault(t => t.NameExam == ExamName.ToString());
                                   
                                    if (QuestionRecord != null)
                                    {
                                        QuestionRecord.NameExam = ExamName.ToString();
                                        QuestionRecord.CourseID = Convert.ToInt32(CourseID);
                                        QuestionRecord.QuestionNumber = Convert.ToInt32(QuestionNumber);
                                        QuestionRecord.Date =  Convert.ToDateTime(Date);
                                        QuestionRecord.Time = Convert.ToInt32(Time);
                                        QuestionRecord.Point = Convert.ToDouble(Point);
                                        QuestionRecord.State = State.ToString();
                                        EditDetailExam(QuestionRecord.ExamID, QuestionRecord.NameExam, Convert.ToInt32(CourseID), Convert.ToInt32(Time), Convert.ToDateTime(Date), Convert.ToDouble(Point), Convert.ToInt32(QuestionNumber));
;
                                    }

                                    else
                                    {
                                        var NganHangCauHoi = db.HR_Question.Where(s => s.CourseID.ToString() == CourseID.ToString()).OrderBy(r => Guid.NewGuid()).ToList();
                                        var BoSung = NganHangCauHoi.Count - NumberQuestion; var d = 0;
                                        if (BoSung >= 0)
                                        {
                                            var exam = new HR_Exam()
                                            {
                                                NameExam = ExamName.ToString(),
                                                CourseID = Convert.ToInt32(CourseID),
                                                QuestionNumber = Convert.ToInt32(QuestionNumber),
                                                Date = Convert.ToDateTime(Date),
                                                Point = Convert.ToDouble(Point),
                                                Time = Convert.ToInt32(Time),
                                                State = State.ToString()
                                            };
                                            db.HR_Exam.Add(exam);
                                            var tmp = new List<HR_Exam_Detail>();

                                            foreach (var item in NganHangCauHoi)
                                            {
                                                tmp.Add(new HR_Exam_Detail() { ExamID = exam.ExamID, QuestionID = item.Question_ID });
                                                d++;
                                                if (d == NumberQuestion)
                                                    break;
                                            }
                                            db.HR_Exam_Detail.AddRange(tmp);
                                        }
                                       
                                        else
                                        {
                                            break;
                                        }
                                        
                                    };
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
                }
            }
            ViewBag.Status = "Thành công!";
            return RedirectToAction("AllExams");
        }

        public ActionResult GetAnExam(int id, string Emp_ID)
        {
            dynamic mymodel = new System.Dynamic.ExpandoObject();
            var ds = db.HR_Result.Where(x => x.ExamID == id && x.Emp_ID.ToString() == Emp_ID).SingleOrDefault();
            if (ds!=null)
            {
                return RedirectToAction("Information");
            }

            else
            {
                mymodel.LstExam = db.HR_Exam.Where(a => a.ExamID.ToString() == id.ToString());
                mymodel.ktraLstQues = db.HR_Exam_Detail.Where(a => a.ExamID == id).ToList();
                mymodel.LstQuestion = db.HR_Question.ToList();
                mymodel.LstAnswer = db.HR_Answer.ToList();
                string[] arrAns = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "K", "L", "M", "N", "O", "P", "Q" };
                mymodel.AnsName = arrAns;
                ViewBag.ExamID = id;
                ViewBag.EmployeeID = Emp_ID;
                return View(mymodel);
            }
            
        }

       
        public ActionResult DeletetAnswer(int AnswerID)
        {
            var ds = db.HR_Answer.Where(s => s.AnswerID == AnswerID).SingleOrDefault();
            db.HR_Answer.Remove(ds);
            db.SaveChanges();
            return Json(new { msg = "Deleted successfully!" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExportExxcel()
        {
            ProcessStartInfo sInf = new ProcessStartInfo();
            sInf.FileName = "EXCEL.EXE";
            sInf.Arguments = @"E:\List_Questions.xlsx";
            Process.Start(sInf);
            return RedirectToAction("AllQuestions");
        }

        public ActionResult ResultExamForEmp()
        {
            return View();
        }


    }
}