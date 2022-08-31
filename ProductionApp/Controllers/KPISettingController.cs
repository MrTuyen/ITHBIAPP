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
using System;
using System.Collections.Generic;

namespace ProductionApp.Controllers
{
    public class KPISettingController : BaseController
    {
        // GET: KPI_Setting
        public ActionResult Index()
        {
            dynamic mymodel = new System.Dynamic.ExpandoObject();
            mymodel.TBL_DEPARTMENT_MST = db.TBL_DEPARTMENT_MST.ToList();
            mymodel.KPI_Manager = db.TBL_USERS_MST.Where(x => x.ROLE_ID == 3).ToList();
            mymodel.KPI_Emp_List = db.KPI_Emp_List.ToList();
            return View(mymodel);
        }


        public ActionResult getKPISettingByEmp(string EmpID, int Period)
        {
            dynamic mymodel = new System.Dynamic.ExpandoObject();
            mymodel.KPISetting = db.KPI_Setting.Where(x => x.EmpID == EmpID && x.Period == Period).ToList();
            mymodel.getOneInfor = db.KPI_Setting.Where(x => x.EmpID == EmpID && x.Period == Period).Take(1);
             mymodel.TBL_Users = db.TBL_USERS_MST.ToList();
            var user = db.TBL_USERS_MST.Where(x => x.USERNAME == EmpID).SingleOrDefault();
            var Emp = db.KPI_Emp_List.Where(x => x.EmpEmail == user.EMAIL).SingleOrDefault();
            ViewBag.EmpID = Emp.EmpID;
            ViewBag.EmpName = Emp.EmpName;
            ViewBag.Username = user.USERNAME;
            return View(mymodel);

        }
        public ActionResult getOneForQuarter(int Type,string EmpID, int Period)
        { 
            dynamic mymodel = new System.Dynamic.ExpandoObject();
            mymodel.KPISetting = db.KPI_Setting.Where(x => x.EmpID == EmpID && x.Period == Period).ToList();
            mymodel.getOneInfor = db.KPI_Setting.Where(x => x.EmpID == EmpID && x.Period == Period).Take(1);
            mymodel.KPI_Core_Values = db.KPI_Core_Values.ToList();
            mymodel.TBL_Users = db.TBL_USERS_MST.ToList();
            mymodel.getOneKPI = db.KPI_Q_Review.Where(x => x.EmpID == EmpID && x.Period == Period).Take(1);
            mymodel.KPIQReview = db.KPI_Q_Review.Where(x => x.EmpID == EmpID && x.Period == Period).ToList();
            var quarter = "";
            foreach (KPI_Q_Review p in mymodel.getOneKPI)
            {
                ViewBag.SubmittedByEmp = p.SubmittedByEmp;
                if ((p.Jan != null && p.Feb != null && p.Mar != null)) quarter = "Q1";
                if ((p.Apr != null && p.May != null && p.Jun != null)) quarter = "Q2";
                if ((p.Jul != null && p.Aug != null && p.Sep != null)) quarter = "Q3";
                if ((p.Oct != null && p.Nov != null && p.Dec != null)) quarter = "Q4";
            }
            ViewBag.Quarter = quarter;
             ViewBag.EmpID = EmpID;
            ViewBag.passType = Type;
            return View(mymodel);

        }
        
        public ActionResult OneQuarterByMgrSup(int Type, string EmpID, int Period)
        {
            dynamic mymodel = new System.Dynamic.ExpandoObject();
           // mymodel.KPISetting = db.KPI_Setting.Where(x => x.EmpID == EmpID && x.Period == Period).ToList();
            var KpiSetting = db.KPI_Setting.Where(x => x.EmpID == EmpID && x.Period == Period).FirstOrDefault();
            ViewBag.CoreValues = KpiSetting.CoreValues;
            mymodel.KPI_Core_Values = db.KPI_Core_Values.ToList();
            mymodel.TBL_Users = db.TBL_USERS_MST.ToList();
            mymodel.KPIQReview = db.KPI_Q_Review.Where(x => x.EmpID == EmpID && x.Period == Period).ToList();
            mymodel.getOneKPI = db.KPI_Q_Review.Where(x => x.EmpID == EmpID && x.Period == Period).Take(1);
            var emp = db.KPI_Emp_List.Where(x => x.EmpID == EmpID).SingleOrDefault();
            var quarter = "";
            foreach(KPI_Q_Review p in mymodel.getOneKPI)
            {
                if ((p.Jan != null && p.Feb != null && p.Mar != null) ) quarter = "Q1";
                if ((p.Apr != null && p.May != null && p.Jun != null) ) quarter = "Q2";
                if ((p.Jul != null && p.Aug != null && p.Sep != null) ) quarter = "Q3";
                if ((p.Oct != null && p.Nov != null && p.Dec != null) ) quarter = "Q4";
            }
            ViewBag.Quarter = quarter;
            ViewBag.EmpID = EmpID;
            ViewBag.passType = Type;
            return View(mymodel);

        }

        public ActionResult getOneYearEndKpi(int Type, string EmpID, int Period)
        {
            dynamic mymodel = new System.Dynamic.ExpandoObject();
            var KpiSetting = db.KPI_Setting.Where(x => x.EmpID == EmpID && x.Period == Period).FirstOrDefault();
            ViewBag.CoreValues = KpiSetting.CoreValues;
            mymodel.KPI_Core_Values = db.KPI_Core_Values.ToList();
            mymodel.TBL_Users = db.TBL_USERS_MST.ToList();
            mymodel.KPIQReview = db.KPI_Y_Review.Where(x => x.EmpID == EmpID && x.Period == Period).ToList();
            mymodel.getOneKPI = db.KPI_Y_Review.Where(x => x.EmpID == EmpID && x.Period == Period).Take(1);
            var emp = db.KPI_Emp_List.Where(x => x.EmpID == EmpID).SingleOrDefault();
            var quarter = "";
            foreach (KPI_Y_Review p in mymodel.getOneKPI)
            {
                ViewBag.Overal_Comment = p.Overal_Comment;
            }
            ViewBag.Quarter = quarter;
            ViewBag.EmpID = EmpID;
            ViewBag.passType = Type;
            return View(mymodel);

        }

        public ActionResult DetailQuarterlyKPI(int Type, string EmpID, int Period)
        {
            dynamic mymodel = new System.Dynamic.ExpandoObject();
            mymodel.KPISetting = db.KPI_Setting.Where(x => x.EmpID == EmpID && x.Period == Period).ToList();
            mymodel.getOneInfor = db.KPI_Setting.Where(x => x.EmpID == EmpID && x.Period == Period).Take(1);
            mymodel.KPI_Core_Values = db.KPI_Core_Values.ToList();
            mymodel.TBL_Users = db.TBL_USERS_MST.ToList();
            mymodel.getOneKPI = db.KPI_Q_Review.Where(x => x.EmpID == EmpID && x.Period == Period).Take(1);
            ViewBag.EmpID = EmpID;
            ViewBag.passType = Type;
            return View(mymodel);

        }

        public ActionResult getAllAPI()
        {
            return View();
        }
        public ActionResult getAll_Quarterly()
        {
            return View();
        }
        public ActionResult AllYearEndPA()
        {
            return View();
        }

        public ActionResult YearEndPA(int Type, string EmpID, int Period)
        {
            dynamic mymodel = new System.Dynamic.ExpandoObject();
            mymodel.KPISetting = db.KPI_Setting.Where(x => x.EmpID == EmpID && x.Period == Period).ToList();
            var KpiSetting = db.KPI_Setting.Where(x => x.EmpID == EmpID && x.Period == Period).FirstOrDefault();
            ViewBag.CoreValues = KpiSetting.CoreValues;
            mymodel.getOneInfor = db.KPI_Setting.Where(x => x.EmpID == EmpID && x.Period == Period).Take(1);
            mymodel.KPI_Core_Values = db.KPI_Core_Values.ToList();
            mymodel.TBL_Users = db.TBL_USERS_MST.ToList();
            mymodel.getOneKPI = db.KPI_Y_Review.Where(x => x.EmpID == EmpID && x.Period == Period).Take(1);
            var ds = db.KPI_Y_Review.Where(x => x.EmpID == EmpID && x.Period == Period).ToList();
            mymodel.KPIQReview = ds;
            ViewBag.Count = ds.Count;
            foreach (KPI_Y_Review p in mymodel.getOneKPI)
            {
                ViewBag.SubmittedByEmp = p.SubmittedByEmp;
                ViewBag.Evaluator = p.EvaluatorID;
                ViewBag.EmpComments = p.Comments;
                ViewBag.ReviewedByEvaluator = p.ReviewedByEvaluator;
                ViewBag.Strength = p.Strength;
                ViewBag.AreasImprove = p.AreasImprove;
                ViewBag.CoreValues = p.CoreValues;
                ViewBag.Overal_Checkbox = p.Overal_Checkbox;
                ViewBag.Overal_Comment = p.Overal_Comment;
            }

            ViewBag.EmpID = EmpID;
            ViewBag.passType = Type;
            return View(mymodel);
        }

        public ActionResult getKPIByUsername(int role, string userid)
        {
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                List<KPI_Setting> list = new List<KPI_Setting>();
                if (role == 5)
                {
                     list = db.KPI_Setting.Where(x => x.EmpID == userid).ToList();
                }
                if (role == 3)
                {
                    list = db.KPI_Setting.Where(x => x.ManagerID == userid).ToList();
                }
                if (role == 1002)
                {
                    list = db.KPI_Setting.Where(s => s.SupervisorID == userid).ToList();
                }

                return Json(list, JsonRequestBehavior.AllowGet);
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

        public ActionResult getKPIQuarterByUsername(int role, string userid)
        {
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                List<KPI_Q_Review> list = new List<KPI_Q_Review>();
                if (role == 5)
                {
                    list = db.KPI_Q_Review.Where(x => x.EmpID == userid).ToList();
                }
                if (role == 3)
                {
                    list = db.KPI_Q_Review.Where(x => x.ManagerID == userid).ToList();
                }
                if (role == 1002)
                {
                    list = db.KPI_Q_Review.Where(s => s.EvaluatorID == userid).ToList();
                }

                return Json(list, JsonRequestBehavior.AllowGet);
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

        public ActionResult SearchKPI(string EmpID, int Period)
        {
            int kq = 0; ;
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var getOne = db.KPI_Setting.Where(x => x.EmpID == EmpID && x.Period == Period).ToList();
                if (getOne.Count > 0)
                {
                    kq = 1;
                }
                else
                {
                    kq = 2;
                }

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

            return Json(kq, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SearchKPIQuarter(string EmpID, int Period)
        {
            int kq = 0; ;
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var getOne = db.KPI_Q_Review.Where(x => x.EmpID == EmpID && x.Period == Period).ToList();
                if (getOne.Count > 0)
                {
                    kq = 1;
                }
                else
                {
                    kq = 2;
                }

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

            return Json(kq, JsonRequestBehavior.AllowGet);
        }

        public ActionResult checkKPI(string EmpID, int Period)
        {
            int kq = 0; ;
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var getOne = db.KPI_Setting.Where(x => x.EmpID == EmpID && x.Period == Period && x.EmpSubmitted == 1 && x.SubmittedDate != null).ToList();
                if (getOne.Count > 0)
                {
                    kq = 1;
                }
                if (getOne.Count <= 0)
                {
                    kq = 2;
                }
                
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

            return Json(kq, JsonRequestBehavior.AllowGet);
        }

        public ActionResult checkKPI_Quarter(string EmpID, int Period)
        {
            int kq = 0; ;
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var getOne = db.KPI_Q_Review.Where(x => x.EmpID == EmpID && x.Period == Period && x.SubmittedByEmp == 1 && x.SubmittedDate != null).ToList();
                if (getOne.Count > 0)
                {
                    kq = 1;
                }
                if (getOne.Count <= 0)
                {
                    kq = 2;
                }

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

            return Json(kq, JsonRequestBehavior.AllowGet);
        }

        public ActionResult checkKPI_YearEndPA(string EmpID, int Period)
        {
            int kq = 0; ;
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var getOne = db.KPI_Y_Review.Where(x => x.EmpID == EmpID && x.Period == Period && x.SubmittedByEmp == 1 && x.SubmittedDate != null).ToList();
                if (getOne.Count > 0)
                {
                    kq = 1;
                }
                if (getOne.Count <= 0)
                {
                    kq = 2;
                }

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

            return Json(kq, JsonRequestBehavior.AllowGet);
        }
        public ActionResult checkKPI_YearExist(string EmpID, int Period)
        {
            int kq = 0; ;
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var getOne = db.KPI_Y_Review.Where(x => x.EmpID == EmpID && x.Period == Period).ToList();
                if (getOne.Count > 0)
                {
                    kq = 1;
                }
                if (getOne.Count <= 0)
                {
                    kq = 0;
                }

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

            return Json(kq, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SendMailToEvaluator(string type, string EmpID, string EvaluatorID, int Period)
        {
            var Employee = db.TBL_USERS_MST.Where(s => s.USERNAME == EmpID).FirstOrDefault();
            var Evaluator = db.TBL_USERS_MST.Where(s => s.USERNAME == EvaluatorID).FirstOrDefault();
            var body = "";
            if (type == "Setting")
            {
                body = string.Format("Dear {0}, " +
                                                     "<br/>{1} send you {0}' KPI." +
                                                     "<br/>Review it on this link:  (<a style='color:blue' href='{2}'>Click this link</a>) " +
                                                     "<br/>",
                               Evaluator.FULLNAME,
                                userLogin.Fullname,
                                string.Format("/KPISetting/getKPISettingByEmp/?EmpID={0}&Period={1}", EmpID, Period)
                                );
            }
            if (type == "Quarterly")
            {
                body = string.Format("Dear {0}, " +
                                                     "<br/>{1} send you {0}' KPI." +
                                                     "<br/>Review it on this link:  (<a style='color:blue' href='{2}'>Click this link</a>) " +
                                                     "<br/>",
                               Evaluator.FULLNAME,
                               userLogin.Fullname ,
                                string.Format("/KPISetting/getKPISettingByEmp/?EmpID={0}&Period={1}", EmpID, Period)
                                );
            }
            if (type == "Year")
            {
                body = string.Format("Dear {0}, " +
                                                     "<br/>{1} send you {0}' KPI." +
                                                     "<br/>Review it on this link:  (<a style='color:blue' href='{2}'>Click this link</a>) " +
                                                     "<br/>",
                               Evaluator.FULLNAME,
                               userLogin.Fullname ,
                                string.Format("/KPISetting/getKPISettingByEmp/?EmpID={0}&Period={1}", EmpID, Period)
                                );
            }

            //send mail
            Utilities.SendEmail("[HYC] KPI Setting request.", Employee.EMAIL, Evaluator.EMAIL, "", body);

            var kq = "Sent email successfully!";
            return Json(new { msg = kq }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SendMailToManager(string type, string EmpID, string ManagerID, int Period, string EvaluatorID)
        {
            var Evaluator = db.TBL_USERS_MST.Where(s => s.USERNAME == EvaluatorID).FirstOrDefault();
            var Manager = db.TBL_USERS_MST.Where(s => s.USERNAME == ManagerID).FirstOrDefault();
            var body = "";
            if (type == "Setting")
            {
                string.Format("Dear {0}, " +
                                                        "<br/>{1} send you this KPI." +
                                                        "<br/>Review and Approve it on this link:  (<a style='color:blue' href='{2}'>Click this link</a>) " +
                                                        "<br/>",
                                   Manager.FULLNAME,
                                   userLogin.Fullname ,
                                   string.Format("/KPISetting/getKPISettingByEmp/?EmpID={0}&Period={1}", EmpID, Period)
                                   );
            }

            if (type == "Quarterly")
            {
                string.Format("Dear {0}, " +
                                                        "<br/>{1} send you this KPI." +
                                                        "<br/>Review and Approve it on this link:  (<a style='color:blue' href='{2}'>Click this link</a>) " +
                                                        "<br/>",
                                   Manager.FULLNAME,
                                   userLogin.Fullname ,
                                   string.Format("/KPISetting/getOneForQuarter/?Type=2&EmpID={1}&Period={1}", EmpID, Period)
                                   );
            }

            if (type == "Year")
            {
                string.Format("Dear {0}, " +
                                                        "<br/>{1} send you this KPI." +
                                                        "<br/>Review and Approve it on this link:  (<a style='color:blue' href='{2}'>Click this link</a>) " +
                                                        "<br/>",
                                   Manager.FULLNAME,
                                   userLogin.Fullname ,
                                   string.Format("/KPISetting/YearEndPA/?Type=1&EmpID={1}&Period={1}", EmpID, Period)
                                   );
            }

            //send mail
            Utilities.SendEmail("[HYC] KPI Setting request.", Evaluator.EMAIL, Manager.EMAIL, "", body);
            var kq = "Sent email successfully!";
            return Json(new { msg = kq }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SendMailToEmployee(string type, string EmpID, string ManagerID, int Period, string CmtManager)
        {

            var Employee = db.TBL_USERS_MST.Where(s => s.USERNAME == EmpID).FirstOrDefault();
            var Manager = db.TBL_USERS_MST.Where(s => s.USERNAME == ManagerID).FirstOrDefault();
            var bodyToEmp = "";
            if(type == "Setting")
            {
                bodyToEmp = string.Format("Dear {0}, " +
                                                       "<br/>{1} send you your KPI." +
                                                       "<br/>You need to edit it because {3}" +
                                                       "<br/>You need to edit it on this link:  (<a style='color:blue' href='{2}'>Click this link</a>) " +
                                                       "<br/>",
                                  EmpID,
                                  userLogin.Fullname ,
                                  CmtManager,
                                  string.Format("/KPISetting/getKPISettingByEmp/?EmpID={0}&Period={1}", EmpID, Period)
                                  );
            }
            if(type == "Quarterly")
            {
                bodyToEmp = string.Format("Dear {0}, " +
                                                       "<br/>{1} send you your KPI." +
                                                       "<br/>You need to edit it on this link:  (<a style='color:blue' href='{2}'>Click this link</a>) " +
                                                       "<br/>",
                                  EmpID,
                                  userLogin.Fullname ,
                                 string.Format("/KPISetting/OneQuarterByMgrSup/?Type=2&EmpID={0}&Period={1}", EmpID, Period)
                                  );
            }

            if (type == "Year")
            {
                bodyToEmp = string.Format("Dear {0}, " +
                                                       "<br/>{1} send you your KPI." +
                                                       "<br/>You need to edit it on this link:  (<a style='color:blue' href='{2}'>Click this link</a>) " +
                                                       "<br/>",
                                  EmpID,
                                  userLogin.Fullname ,
                                 string.Format("/KPISetting/YearEndPA/?Type=2&EmpID={0}&Period={1}", EmpID, Period)
                                  );
            }
            //send mail
            Utilities.SendEmail("[HYC] KPI Setting request.", Manager.EMAIL, Employee.EMAIL, "", bodyToEmp);

            var kq = "Sent email successfully!";
            return Json(new { msg = kq }, JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public ActionResult AddKpiSetting(string EmpID, string EvaluatorID, string ManagerID, string Position, 
            int Period, string DepartmentID, string CmtEmployee, string CoreValue, string KPIitems, string YearKPI, 
            string Q1, string Q2, string Q3, string Q4, string KPI_Weight, string Note, string Level)
        {
            var kq = "";
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                if (KPIitems != "" && KPI_Weight != ""  && YearKPI != "" && Q1 != "" && Q2 != "" && Q3 != "" && Q4 != "")
                {
                    var ds = db.KPI_Setting.Where(x => x.EmpID == EmpID && x.Period == Period && x.EmpSubmitted == 0 && x.SubmittedDate == null).ToList();
                    if (ds.Count != 0)
                    {
                        foreach (KPI_Setting d in ds)
                        {
                            db.KPI_Setting.Remove(d);
                        }
                    }
                        var r = new KPI_Setting();
                        r.EmpID = EmpID;
                        r.DeptName = DepartmentID;
                        r.ManagerID = ManagerID;
                        r.SupervisorID = EvaluatorID;
                        r.Position = Position;
                        r.Period = Period;
                        r.CmtEmp = CmtEmployee;
                        r.CoreValues = CoreValue;
                        r.KPIitems = KPIitems;
                        r.KPIYear = YearKPI;
                        r.Q1 = Q1;
                        r.Q2 = Q2;
                        r.Q3 = Q3;
                        r.Q4 = Q4;
                        r.KPIweight = KPI_Weight;
                        r.Note = Note;
                        r.EmpSubmitted = 1;
                        r.SubmittedDate = Utilities.GetDate_VietNam(DateTime.Now);
                        r.Level = Level;
                        db.KPI_Setting.Add(r);
                        db.SaveChanges();
                       
                    }
                kq = "Submitted successfully!";
              
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
        public ActionResult AddKpiQuarter(string EmpID, string EvaluatorID, string ManagerID, string Position, int Period,
       string DepartmentID, string CmtEmployee, string KPIs, string QuarterKPI, string m1, string m2, string m3,
       string m4, string m5, string m6, string m7, string m8, string m9, string m10, string m11, string m12,
        string KPI_Weight, string KPI_Result, string ActualTarget, string KPI_Bonus, string FinalScore,
        string Note, string Level, string Quarter)
        {
            int kq = 0;
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {

                var ds = db.KPI_Q_Review.Where(x => x.EmpID == EmpID && x.Period == Period && x.SubmittedByEmp == 0 && x.SubmittedDate == null).ToList();
                if (ds.Count != 0)
                {
                    foreach (KPI_Q_Review d in ds)
                    {
                        db.KPI_Q_Review.Remove(d);
                    }
                }
                var r = new KPI_Q_Review();
                r.EmpID = EmpID;
                r.DeptName = DepartmentID;
                r.ManagerID = ManagerID;
                r.EvaluatorID = EvaluatorID;
                r.Position = Position;
                r.Period = Period;
                if (Quarter == "Q1")
                {
                    r.Jan = m1;
                    r.Feb = m2;
                    r.Mar = m3;
                }
                if (Quarter == "Q2")
                {
                    r.Apr = m4;
                    r.May = m5;
                    r.Jun = m6;
                }
                if (Quarter == "Q3")
                {
                    r.Jul = m7;
                    r.Aug = m8;
                    r.Sep = m9;
                }
                if (Quarter == "Q4")
                {
                    r.Oct = m10;
                    r.Nov = m11;
                    r.Dec = m12;
                }
                r.Comments = CmtEmployee;
                r.QuarterKPI = QuarterKPI;
                r.KPIs = KPIs;
                r.KPI_Bonus = KPI_Bonus;
                r.Actual_Target = ActualTarget;
                r.KPI_Weight = KPI_Weight;
                r.Note = Note;
                r.KPI_Result = KPI_Result;
                r.Final_Score = FinalScore;
                r.SubmittedByEmp = 1;
                r.SubmittedDate = Utilities.GetDate_VietNam(DateTime.Now);
                r.Level = Level;
                db.KPI_Q_Review.Add(r);
                db.SaveChanges();
                kq = 1;
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
        public ActionResult AddKpiYearEndPA(string EmpID, string EvaluatorID, string ManagerID, string Position, int Period,
    string DepartmentID, string CmtEmployee, string KPIs, string QuarterKPI, string m1, string m2, string m3, string m4,
     string KPI_Weight, string KPI_Result, string ActualTarget, string KPI_Bonus, string FinalScore,
     string Note, string Level)
        {
            int kq = 0;
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {

                var ds = db.KPI_Y_Review.Where(x => x.EmpID == EmpID && x.Period == Period && x.SubmittedByEmp == 0 && x.SubmittedDate == null).ToList();
                if (ds.Count != 0)
                {
                    foreach (KPI_Y_Review d in ds)
                    {
                        db.KPI_Y_Review.Remove(d);
                    }
                }
                var r = new KPI_Y_Review();
                r.EmpID = EmpID;
                r.DeptName = DepartmentID;
                r.ManagerID = ManagerID;
                r.EvaluatorID = EvaluatorID;
                r.Position = Position;
                r.Period = Period;
                r.Q1 = m1;
                r.Q2 = m2;
                r.Q3 = m3;
                r.Q4 = m4;
                r.Comments = CmtEmployee;
                r.KPI = QuarterKPI;
                r.KPIs = KPIs;
                r.KPI_Bonus = KPI_Bonus;
                r.Actual_Target = ActualTarget;
                r.KPI_Weight = KPI_Weight;
                r.Note = Note;
                r.Quarter_KPI_Result = KPI_Result;
                r.Final_Score = FinalScore;
                r.SubmittedByEmp = 1;
                r.SubmittedDate = Utilities.GetDate_VietNam(DateTime.Now);
                r.Level = Level;
                db.KPI_Y_Review.Add(r);
                db.SaveChanges();
                kq = 1;
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

        public ActionResult Review(string EmpID, int Period)
        {
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                var ds = db.KPI_Setting.Where(x => x.EmpID == EmpID && x.Period == Period).ToList();
                if (ds != null)
                {
                    foreach (KPI_Setting p in ds)
                    {
                        p.EvaluatorReviewed = 1;
                        p.ReviewedDate = Utilities.GetDate_VietNam(DateTime.Now);
                    }
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
        [HttpPost]
        public ActionResult Review_Quarter(string EmpID, int Period, string Strengths, string Areas, string CoreValueResults)
        {
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                var ds = db.KPI_Q_Review.Where(x => x.EmpID == EmpID && x.Period == Period).ToList();
                if (ds != null)
                {
                    foreach (KPI_Q_Review p in ds)
                    {
                        p.Strength = Strengths;
                        p.AreasImprove = Areas;
                        p.CoreValues = CoreValueResults;
                        p.ReviewedByEvaluator = 1;
                        p.ReviewedDate = Utilities.GetDate_VietNam(DateTime.Now);
                    }
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
        [HttpPost]
        public ActionResult Review_Year(string EmpID, int Period, string Strengths, string Areas, string CoreValueResults, string Overal_Checkbox, string Overal_Comment)
        {
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                var ds = db.KPI_Y_Review.Where(x => x.EmpID == EmpID && x.Period == Period).ToList();
                if (ds != null)
                {
                    foreach (KPI_Y_Review p in ds)
                    {
                        p.Strength = Strengths;
                        p.AreasImprove = Areas;
                        p.CoreValues = CoreValueResults;
                        p.Overal_Checkbox = Overal_Checkbox;
                        p.Overal_Comment = Overal_Comment;
                        p.ReviewedByEvaluator = 1;
                        p.ReviewedDate = Utilities.GetDate_VietNam(DateTime.Now);
                    }
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
        public ActionResult getInforLogin(string username)
        {

            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                //set ProxyCreation to false
                db.Configuration.ProxyCreationEnabled = false;
                var User = db.TBL_USERS_MST.Where(s => s.USERNAME == username.ToString()).SingleOrDefault();
                var getInforEmp = db.KPI_Emp_List.Where(s => s.EmpEmail == User.EMAIL).SingleOrDefault();
                var InforMgr = db.TBL_USERS_MST.Where(s => s.EMAIL == getInforEmp.MgrEmail).SingleOrDefault();
                var InforEval = db.TBL_USERS_MST.Where(s => s.EMAIL == getInforEmp.SupEmail).SingleOrDefault();
                var result = new { Emp = getInforEmp, Manager = InforMgr, Evaluator = InforEval };
                return Json(result, JsonRequestBehavior.AllowGet);
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
        public ActionResult Approve(string EmpID, int Period, string CmtManager)
        {
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                var ds = db.KPI_Setting.Where(x => x.EmpID == EmpID && x.Period == Period).ToList();
                if (ds != null)
                {
                    foreach (KPI_Setting p in ds)
                    {
                        p.ManagerApproved = 1;
                        p.ApprovedDate = Utilities.GetDate_VietNam(DateTime.Now);
                        p.CmtManager = CmtManager;
                    }
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
        public ActionResult Revise(string EmpID, int Period, string CmtManager)
        {
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                var ds = db.KPI_Setting.Where(x => x.EmpID == EmpID && x.Period == Period).ToList();
                if (ds != null)
                {
                    foreach (KPI_Setting p in ds)
                    {
                        p.ManagerApproved = 0;
                        p.ApprovedDate = null;
                        p.EvaluatorReviewed = 0;
                        p.ReviewedDate = null;
                        p.EmpSubmitted = 0;
                        p.SubmittedDate = null;
                        p.CmtManager = CmtManager;
                    }
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
        public ActionResult ApproveQuarter(string EmpID, int Period)
        {
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                var ds = db.KPI_Q_Review.Where(x => x.EmpID == EmpID && x.Period == Period).ToList();
                if (ds != null)
                {
                    foreach (KPI_Q_Review p in ds)
                    {
                        p.ApprovedByManager = 1;
                        p.ApprovedDate = Utilities.GetDate_VietNam(DateTime.Now);
                    }
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
        public ActionResult ApproveYearEndPA(string EmpID, int Period)
        {
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                var ds = db.KPI_Y_Review.Where(x => x.EmpID == EmpID && x.Period == Period).ToList();
                if (ds != null)
                {
                    foreach (KPI_Y_Review p in ds)
                    {
                        p.ApprovedByManager = 1;
                        p.ApprovedDate = Utilities.GetDate_VietNam(DateTime.Now);
                    }
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
        public ActionResult ReviseQuarter(string EmpID, int Period, string CmtManager)
        {
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                var ds = db.KPI_Q_Review.Where(x => x.EmpID == EmpID && x.Period == Period).ToList();
                if (ds != null)
                {
                    foreach (KPI_Q_Review p in ds)
                    {
                        p.ApprovedByManager = 0;
                        p.ApprovedDate = null;
                        p.ReviewedByEvaluator = 0;
                        p.ReviewedDate = null;
                        p.SubmittedByEmp = 0;
                        p.SubmittedDate = null;
                        p.CmtManager = CmtManager;
                    }
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
        public ActionResult ReviseYearEndPA(string EmpID, int Period, string CmtManager)
        {
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                var ds = db.KPI_Y_Review.Where(x => x.EmpID == EmpID && x.Period == Period).ToList();
                if (ds != null)
                {
                    foreach (KPI_Y_Review p in ds)
                    {
                        p.ApprovedByManager = 0;
                        p.ApprovedDate = null;
                        p.ReviewedByEvaluator = 0;
                        p.ReviewedDate = null;
                        p.SubmittedByEmp = 0;
                        p.SubmittedDate = null;
                        p.CmtManager = CmtManager;
                    }
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
        public ActionResult checkQuarterlyKPI(string EmpID, int Period, int Role)
        {
            int kq = 0 ;
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var getOne = new  List<KPI_Q_Review>();
                getOne = db.KPI_Q_Review.Where(x => x.EmpID == EmpID && x.Period == Period).ToList();
                if(getOne.Count > 0)
                {
                    foreach(KPI_Q_Review p in getOne)
                    {
                        if (Role == 3 && p.ApprovedByManager != 0)
                        {
                            kq = 1;
                        }
                        if (Role == 5 &&  p.SubmittedByEmp != 0)
                        {
                            kq = 2;
                        }
                        if (Role == 1002 && p.ReviewedByEvaluator != 0)
                        {
                            kq = 3;
                        }
                    }
                }
              
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

            return Json(kq, JsonRequestBehavior.AllowGet);
        }
    }


}