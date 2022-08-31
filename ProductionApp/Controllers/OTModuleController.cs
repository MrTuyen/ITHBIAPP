using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProductionApp.Models;
using System.Globalization;
using System.Net;
using OfficeOpenXml;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using OfficeOpenXml.Style;
using ProductionApp.Helpers;


namespace ProductionApp.Controllers {
    public class OTModuleController:BaseController {
        public ActionResult Index() {
            if(userLogin == null) {
                return RedirectToAction("NeedLogin" ,"Notification");
            }

            return RedirectToAction("List_OT");
        }

        public ActionResult List_OT() {


            if(userLogin != null) {
                if(userLogin.Username.ToLower() == "admin") {
                    ViewBag.list = db.OL_OT_Details.OrderByDescending(s => s.OT_ID);
                    ViewBag.per = 3;
                } else {
                    var hr_team = db.TBL_SYSTEM.Where(s => s.value.ToLower() == userLogin.Email.ToLower() & s.value3 == "HR_CB").ToList();
                    if(hr_team.Count > 0) {
                        ViewBag.list = db.OL_OT_Details.Where(s => s.AppStatus != -3 & s.HRStatus != 2).OrderByDescending(s => s.OT_ID);
                        ViewBag.per = 2;
                    } else {
                        ViewBag.list = db.OL_OT_Details.Where(s => (s.ApproverEmail.ToLower() == userLogin.Email.ToLower() || s.ReqEmail.ToLower() == userLogin.Email.ToLower())).OrderByDescending(s => s.OT_ID).Take(100);
                        ViewBag.per = 1;
                    }
                }

            } else {
                ViewBag.list = "";
            }



            return View();

        }
        public static int GetWeekOrderInYear(DateTime time) {
            //CultureInfo myCI = CultureInfo.CurrentCulture;

            // TimeZoneInfo hwZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            //time= TimeZoneInfo.ConvertTime(time ,hwZone);

            CultureInfo myCI =CultureInfo.GetCultureInfo("vi-VN");
            System.Globalization.Calendar myCal = myCI.Calendar;
            CalendarWeekRule myCWR = myCI.DateTimeFormat.CalendarWeekRule;
            DayOfWeek myFirstDOW = myCI.DateTimeFormat.FirstDayOfWeek;

            return myCal.GetWeekOfYear(time ,myCWR ,myFirstDOW);
        }
        public ActionResult Creat_OT() {
            ViewBag.leave_code = db.OL_OTCode.ToList();
            ViewBag.group = db.TBL_GROUP_MST.ToList();

            //ViewData["balance"] = db.OL_LeaveBalance.Single(s => s.Username == user.Username);
            var user1 =        db.TBL_USERS_MST.Single(s => s.USERNAME == userLogin.Username);
            ViewData["user"] = user1;
            var dept = db.TBL_DEPARTMENT_MST.Single(s => s.DEPT_ID == user1.DEPT);
            ViewData["dept"] = dept;
            var emp = new OL_User_Approver();
            try {
                emp = db.OL_User_Approver.Single(s => s.UserCD == user1.USERNAME);
                ViewData["app"] = emp;
                ViewBag.user_list = db.OL_User_Approver.Where(s => s.Section == dept.DEPT_ID).ToList();
            } catch(Exception e) {
                ViewData["app"] = new OL_User_Approver();
                ViewBag.user_list = new List<OL_User_Approver>();
                TempData["msg"] = "<script>alert('Your info not exist, Please contact to HR team!');</script>";
            }


            ViewBag.hr = db.TBL_SYSTEM.Where(s => s.value3 == "HR_CB").ToList();
            try {
                ViewData["month"] = db.OL_OT_Record_M.Single(s => s.EmpID == emp.EmpID);
            } catch(Exception e) {
                ViewData["month"] = new OL_OT_Record_M();
            }

            ViewBag.hours = db.OL_OT_Hours.ToList();
            ViewBag.min = db.OL_OT_Mins.ToList();
            ViewBag.week = GetWeekOrderInYear(Utilities.GetDate_VietNam(DateTime.Now));
            return View();
        }
        public ActionResult Date(string date) {
            int week = 0;
            if(date != "") {
                week = GetWeekOrderInYear(Utilities.GetDate_VietNam(DateTime.Parse(date)));
            }
            return Json(week ,JsonRequestBehavior.AllowGet);
        }
        public ActionResult Week_one(int week) {
            double week1 = 0;
            try {

                var user1 = db.OL_User_Approver.Single(s => s.UserCD == userLogin.Username);
                if(user1 != null) {
                    week1 = Week(week ,user1.EmpID);

                }
            } catch(Exception e) {

            }
            return Json(week1 ,JsonRequestBehavior.AllowGet);
        }
        public ActionResult Month_one(DateTime? date) {
            double week1 = 0;
            try {

                var user1 = db.OL_User_Approver.Single(s => s.UserCD == userLogin.Username);
                if(user1 != null) {
                    week1 = Month(date ?? Utilities.GetDate_VietNam(DateTime.Now) ,user1.EmpID);

                }
            } catch(Exception e) {
                Console.WriteLine(e);
                throw;
            }
            return Json(week1 ,JsonRequestBehavior.AllowGet);
        }
        public ActionResult Week_one1(int week ,string empid) {

            return Json(Week(week ,empid) ,JsonRequestBehavior.AllowGet);
        }
        public ActionResult Month_one1(DateTime? date ,string empid) {

            return Json(Month(date ?? Utilities.GetDate_VietNam(DateTime.Now) ,empid) ,JsonRequestBehavior.AllowGet);
        }
        public ActionResult Month_more(string empid) {
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try {
                //set ProxyCreation to false
                db.Configuration.ProxyCreationEnabled = false;
                var ds = db.OL_OT_Record_M.FirstOrDefault(s => s.EmpID == empid) ?? new OL_OT_Record_M();
                return Json(ds ,JsonRequestBehavior.AllowGet);
            } catch(Exception ex) {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Thông tin OT của bạn chưa được cập nhật, Vui lòng liên hệ HR Team!");
            } finally {
                //restore ProxyCreation to its original state
                db.Configuration.ProxyCreationEnabled = proxyCreation;
            };
        }
        public ActionResult Add_OT_One(string OTCD ,string date ,int week ,int HoursFrom ,
          int HoursTo ,int MinFrom ,int MinTo ,double total_one ,int van ,string address) {
            string ms = "";
            if(Session["OT_one"] == null) {
                Session["OT_one"] = new List<OT_One>();
            }
            var name = db.OL_OTCode.Single(s => s.OTCD == OTCD);
            var emp1 = db.OL_User_Approver.SingleOrDefault(s => s.UserCD == userLogin.Username);
            if(date != "") {
                if(emp1 != null) {
                    var oneLeave = Session["OT_one"] as List<OT_One>;
                    var totalHoursTo = oneLeave.Sum(a => a.total) + (HoursTo - HoursFrom) + (double)(MinTo - MinFrom) / 60;
                    //if(totalHoursTo <= 4)
                    //{
                    //if(Week(week ,emp1.EmpID) + totalHoursTo <= 16) {
                    //    if(Month(DateTime.Parse(date) ,emp1.EmpID) + totalHoursTo <= 40) {
                    //        if(total_one + totalHoursTo < 300) {
                                //if(oneLeave.FirstOrDefault(m => m.OTCD == OTCD) == null)
                                //{

                                var newItem = new OT_One() {
                                    OTCD = OTCD ,
                                    OT_Name = name.OTName ,
                                    OTDate = date ,

                                    WWork = week ,
                                    HoursFrom = HoursFrom ,
                                    HoursTo = HoursTo ,
                                    MinFrom = MinFrom ,
                                    MinTo = MinTo ,
                                    total = (HoursTo - HoursFrom) + (double)(MinTo - MinFrom) / 60 ,
                                    van = address.Length > 5 ? 1 : 0 ,
                                    address = address

                                };
                                oneLeave.Add(newItem);

                                ms = "OK";
                    //        } else {
                    //            ms = "Số giờ trong năm vượt quá 300h!/Number of hours in the year exceeds 300 hours!";
                    //        }
                    //    } else {
                    //        ms = "Số giờ trong tháng vượt quá 40h!/Number of hours in a month exceeds 40 hours!";
                    //    }
                    //} else {
                    //    ms = "Số giờ trong tuần vượt quá 16h!/The number of hours per week exceeds 16 hours!";
                    //}
                    //}
                    //else
                    //{
                    //    ms = "Số giờ trong ngày vượt quá 4h!/Number of hours per day exceeds 4 hours!";
                    //}
                } else {
                    ms = "Your info not exist, Please contact to HR team!";
                }
            } else {
                ms = "Please select OT date!";
            }

            return Json(ms ,JsonRequestBehavior.AllowGet);
        }
        public ActionResult Add_Morefull(string OTCD ,string date ,int week ,int HoursFrom ,
          int HoursTo ,int MinFrom ,int MinTo ,string empid ,double total_more ,int van ,string address1) {
            string ms = "";

            if(Session["OT_more"] == null) // Nếu giỏ hàng chưa được khởi tạo
            {
                Session["OT_more"] = new List<OT_More>();  // Khởi tạo Session["giohang"] là 1 List<CartItem>
            }
            var name = db.OL_OTCode.Single(s => s.OTCD == OTCD);
            var oneLeave = Session["OT_more"] as List<OT_More>;

            var emp = db.OL_User_Approver.SingleOrDefault(s => s.EmpID == empid);
            if(date != "") {
                if(emp != null && date != "") {
                    var totalHoursTo = oneLeave.Where(a => a.empID == empid).Sum(a => a.total) + (HoursTo - HoursFrom) + (double)(MinTo - MinFrom) / 60;
                    //if(totalHoursTo <= 4)
                    //{
                    //if(Week(week ,empid) + totalHoursTo <= 16) {
                    //    if(Month(DateTime.Parse(date) ,empid) + totalHoursTo <= 40) {
                    //        if(total_more + totalHoursTo < 300) {

                                var newItem = new OT_More {
                                    id = ma_full() ,
                                    OTCD = OTCD ,
                                    OT_Name = name.OTName ,
                                    OTDate = date ,
                                    WWork = week ,
                                    HoursFrom = HoursFrom ,
                                    HoursTo = HoursTo ,
                                    MinFrom = MinFrom ,
                                    MinTo = MinTo ,
                                    empID = empid ,
                                    EmpName = emp.EmpName ,
                                    EmpEmail = emp.EmpEmail ,
                                    total = (HoursTo - HoursFrom) + (double)(MinTo - MinFrom) / 60 ,
                                    Van = address1.Length > 5 ? 1 : 0 ,
                                    Address = address1 ,
                                };
                                oneLeave.Add(newItem);
                                ms = "OK";
                    //        } else {
                    //            ms = "Số giờ trong năm vượt quá 300h!/Number of hours in the year exceeds 300 hours!";
                    //        }
                    //    } else {
                    //        ms = "Số giờ trong tháng vượt quá 40h!/Number of hours in a month exceeds 40 hours!";
                    //    }
                    //} else {
                    //    ms = "Số giờ trong tuần vượt quá 16h!/The number of hours per week exceeds 16 hours!";
                    //}
                    //}
                    //else
                    //{
                    //    ms = "Số giờ trong ngày vượt quá 4h!/Number of hours per day exceeds 4 hours!";
                    //}
                } else {
                    ms = "Your info not exist, Please contact to HR team!";
                }
            } else {
                ms = "Please select OT date!";
            }
            return Json(ms ,JsonRequestBehavior.AllowGet);
        }
        int ma_full() {

            var List = new List<OT_One>();
            if(Session["OT_one"] != null) {
                List = (List<OT_One>)Session["OT_one"];

            }

            return List.Count + 1;
        }
        public ActionResult Load() {

            List<OT_One> List = null;
            if(Session["OT_one"] != null) {
                List = (List<OT_One>)Session["OT_one"];

            }

            return Json(List ,JsonRequestBehavior.AllowGet);

        }
        public ActionResult Load1() {

            List<OT_More> List = null;
            if(Session["OT_more"] != null) {
                List = (List<OT_More>)Session["OT_more"];

            }

            return Json(List ,JsonRequestBehavior.AllowGet);

        }

        public JsonResult load_cb(string id) {
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try {
                //set ProxyCreation to false
                db.Configuration.ProxyCreationEnabled = false;
                var ds = db.TBL_SYSTEM.Where(s => s.id == id).FirstOrDefault();
                return Json(ds ,JsonRequestBehavior.AllowGet);
            } catch(Exception ex) {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(ex.Message);
            } finally {
                //restore ProxyCreation to its original state
                db.Configuration.ProxyCreationEnabled = proxyCreation;
            };
        }


        public ActionResult OT_One(string section ,int dept ,string approver ,
           string approverMail ,string nameHR ,string mailHR ,string note) {
            string ms = "";

            try {

                var item = Session["OT_one"] as List<OT_One>;
                if(item != null) {
                    var detail = new OL_OT_Details {
                        ApproverName = approver ,
                        ApproverEmail = approverMail ,
                        Dept = dept ,
                        Section = section ,
                        ReqName = userLogin.Username ,
                        ReqEmail = userLogin.Email ,
                        AppStatus = -1 ,
                        HRStatus = -2 ,
                        HRName = nameHR ,
                        HREmail = mailHR ,
                        Notes = note ,
                        EmpSubmit = Utilities.GetDate_VietNam(DateTime.Now)
                    };
                    //detail.OT_ID = ma1();

                    db.OL_OT_Details.Add(detail);
                    db.SaveChanges();

                    foreach(var t in item) {
                        var item1 = new OL_OT_Item {
                            OT_ID = detail.OT_ID ,
                            OTCD = t.OTCD ,
                            OTDate = DateTime.Parse(t.OTDate) ,
                            OTDate1 = DateTime.Parse(t.OTDate) ,
                            OTTimeIn = (t.HoursFrom < 10 ? "0" + t.HoursFrom : t.HoursFrom.ToString()) + ":" + (t.MinFrom < 10 ? "0" + t.MinFrom : t.MinFrom.ToString()) + ":00" ,
                            OTTimeOut = (t.HoursTo < 10 ? "0" + t.HoursTo : t.HoursTo.ToString()) + ":" + (t.MinTo < 10 ? "0" + t.MinTo : t.MinTo.ToString()) + ":00" ,
                            OTNo = t.total ,
                            Reason = note ,
                            WWork = t.WWork.ToString() ,
                            Van = t.van ,
                            VanToAdd = t.address
                        };

                        var emp = db.OL_User_Approver.SingleOrDefault(s => s.EmpEmail == userLogin.Email);
                        item1.EmpID = emp.EmpID;
                        db.OL_OT_Item.Add(item1);
                        update_Week(int.Parse(item1.WWork) ,item1.EmpID ,double.Parse(item1.OTNo.ToString()));
                        update_Month(DateTime.Parse(item1.OTDate.ToString()) ,item1.EmpID ,double.Parse(item1.OTNo.ToString()));
                    }
                    db.SaveChanges();
                    ms = "Thành công/Success!";
                    Utilities.SendEmail("Phiếu làm thêm giờ #" + detail.OT_ID + "/Overtime request need your approval" ,userLogin.Email ,detail.ApproverEmail ,userLogin.Email ,"Dear " + detail.ApproverName + ",<br/><br/>Vui lòng phê duyệt phiếu làm thêm giờ #" + detail.OT_ID + ".<br/><span style='color:#0070c0;font-style: italic;'>Please approve or reject leave request #" + detail.OT_ID + ".</span> ");

                }
            } catch(Exception e) {
                Utilities.WriteLogException(e ,"OTMODULE/OT_One");
                ms = "Nhân viên không đúng/Employee code is incorrect";
            }
            Session.Remove("OT_one");
            Session.Remove("OT_more");
            return Json(ms ,JsonRequestBehavior.AllowGet);

        }

        public ActionResult App_OT_More(string section ,int dept ,string approver ,
           string approverMail ,string nameHR ,string mailHR ,string note) {
            string ms = "";

            try {

                //   var user1 = db.TBL_USERS_MST.Single(s => s.USERNAME == user.Username);
                var item = Session["OT_more"] as List<OT_More>;
                if(item != null) {
                    var detail = new Models.OL_OT_Details {
                        ApproverName = approver ,
                        ApproverEmail = approverMail ,
                        Dept = dept ,
                        Section = section ,
                        ReqName = userLogin.Username ,
                        ReqEmail = userLogin.Email ,
                        AppStatus = -1 ,
                        HRStatus = -2 ,
                        HRName = nameHR ,
                        HREmail = mailHR ,
                        Notes = note ,
                        EmpSubmit = Utilities.GetDate_VietNam(DateTime.Now)
                    };

                    db.OL_OT_Details.Add(detail);
                    db.SaveChanges();

                    foreach(var t in item) {
                        var item1 = new OL_OT_Item {
                            OT_ID = detail.OT_ID ,
                            OTCD = t.OTCD ,
                            OTDate = DateTime.Parse(t.OTDate) ,
                            OTDate1 = DateTime.Parse(t.OTDate) ,
                            OTTimeIn = (t.HoursFrom < 10 ? "0" + t.HoursFrom : t.HoursFrom.ToString()) + ":" + (t.MinFrom < 10 ? "0" + t.MinFrom : t.MinFrom.ToString()) + ":00" ,
                            OTTimeOut = (t.HoursTo < 10 ? "0" + t.HoursTo : t.HoursTo.ToString()) + ":" + (t.MinTo < 10 ? "0" + t.MinTo : t.MinTo.ToString()) + ":00" ,
                            OTNo = t.total ,
                            WWork = t.WWork.ToString() ,
                            EmpID = t.empID ,
                            Reason = note ,
                            Van = t.Van ,
                            VanToAdd = t.Address
                        };
                        db.OL_OT_Item.Add(item1);
                        update_Week(int.Parse(item1.WWork) ,item1.EmpID ,double.Parse(item1.OTNo.ToString()));
                        update_Month(DateTime.Parse(item1.OTDate.ToString()) ,item1.EmpID ,
                            double.Parse(item1.OTNo.ToString()));
                    }

                    db.SaveChanges();
                    ms = "Thành công/Success!";
                    Utilities.SendEmail("Phiếu làm thêm giờ #" + detail.OT_ID + "/Overtime request need your approval" ,userLogin.Email ,detail.ApproverEmail ,userLogin.Email ,"Dear " + detail.ApproverName + ",<br/><br/>Vui lòng phê duyệt phiếu làm thêm giờ #" + detail.OT_ID + ".<br/><span style='color:#0070c0;font-style: italic;'>Please approve or reject leave request #" + detail.OT_ID + ".</span> ");

                }
            } catch(Exception e) {
                Utilities.WriteLogException(e ,"OTMODULE/App_OT_More");
                TempData["msg"] = "<script>alert('Error, need contact to IT')</script>";
            }
            Session.Remove("OT_more");
            Session.Remove("OT_one");
            return Json(ms ,JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public ActionResult Delete(string id) {
            List<OT_One> one = Session["OT_one"] as List<OT_One>;
            OT_One itemXoa = one.FirstOrDefault(m => m.OTCD == id);
            if(itemXoa != null) {
                one.Remove(itemXoa);
            }
            return Json("" ,JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Delete1(int id) {
            List<OT_More> one = Session["OT_more"] as List<OT_More>;
            OT_More itemXoa = one.FirstOrDefault(m => m.id == id);
            if(itemXoa != null) {
                one.Remove(itemXoa);
            }
            return Json("" ,JsonRequestBehavior.AllowGet);
        }
        public ActionResult Edit_OT(int id) {
            var request = db.OL_OT_Details.Find(id);

            ViewData["dept"] = db.TBL_DEPARTMENT_MST.Single(s => s.DEPT_ID == request.Dept);
            ViewData["item"] = db.OL_OT_Item.Where(s => s.OT_ID == id).OrderBy(a => a.ID).ToList();
            ViewData["leave"] = db.OL_OT_Details.Find(id);
            ViewData["manager"] = db.OL_User_Approver.First(s => s.UserCD == request.ReqName);
            ViewData["hrteam"] = db.TBL_SYSTEM.Single(s => s.value == request.HREmail && s.value3 == "HR_CB");
            return View();
        }
        [HttpPost]
        public ActionResult Approve(int id ,string dept) {
            var request = db.OL_OT_Details.Find(id);
            var user1 = db.OL_User_Approver.Single(s => s.EmpID == dept);
            if(user1.ApproverEmail.ToLower() == userLogin.Email.ToLower()) {
                try {
                    request.AppStatus = 1;
                    request.AppSubmitted = Utilities.GetDate_VietNam(DateTime.Now);
                    db.Entry(request).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    Helpers.Utilities.SendEmail("Phiếu làm thêm giờ #" + request.OT_ID + " đã được duyệt / Overtime request #" + request.OT_ID + " has been approved." ,request.ApproverEmail ,request.HREmail ,request.ReqEmail ,"Hi HR teams,<br/><br/>Phiếu làm thêm giờ #" + request.OT_ID + " đã được phê duyệt, vui lòng xử lý.<br/><span style='color:#0070c0;font-style: italic;'>Request #" + request.OT_ID + " has been approved. Please process this request.</span> ");
                    TempData["msg"] = "<script>alert('Thành công/Success');</script>";

                } catch(Exception e) {
                    Utilities.WriteLogException(e);
                    TempData["msg"] = "<script>alert('Error, need contact to IT')</script>";
                }
            } else {
                TempData["msg"] = "<script>alert('Bạn không thể phê duyệt/You cannot approve');</script>";
            }
            return RedirectToAction("List_OT");
        }


        [HttpPost]
        public ActionResult HRteam_Approve(int id ,string hrteam) {
            var request = db.OL_OT_Details.Find(id);


            var hr = db.TBL_SYSTEM.SingleOrDefault(s => s.value.ToLower() == userLogin.Email.ToLower() & s.value3 == "HR_CB");
            if(hr != null) {
                if(request.AppStatus == 1) {
                    try {
                        request.HRStatus = 2;

                        request.HRSubmit = Utilities.GetDate_VietNam(DateTime.Now);
                        db.Entry(request).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        Helpers.Utilities.SendEmail("Phiếu làm thêm giờ #" + request.OT_ID + " đã xử lý /Overtime request #" + request.OT_ID + " has been processed" ,request.HREmail ,request.ReqEmail ,"" ,"Dear " + request.TBL_USERS_MST.FULLNAME + ",<br/><br/>Phiếu làm thêm giờ của bạn đã được xử lý.<br/><span style='color:#0070c0;font-style: italic;'>Request has been processed.</span>");
                        TempData["msg"] = "<script>alert('Thành công/Success');</script>";
                    } catch {
                        TempData["msg"] = "<script>alert('Thất bại/Fail');</script>";
                    }
                } else {
                    TempData["msg"] = "<script>alert('Request chưa đủ điều kiện/Request is not eligible');</script>";
                }
            } else {
                TempData["msg"] = "<script>alert('Bạn không thể phê duyệt/You cannot approve');</script>";
            }


            return RedirectToAction("List_OT");
        }
        [HttpPost]
        public ActionResult SendMail(int id ,string body ,string dept1) {
            var request = db.OL_OT_Details.Find(id);
            var user1 = db.OL_User_Approver.Single(s => s.EmpID == dept1);
            if(user1.ApproverEmail.ToLower() == userLogin.Email.ToLower() || userLogin.Username.ToLower() == "admin") {

                request.AppStatus = -3;
                db.Entry(request).State = EntityState.Modified;
                var ls = request.OL_OT_Item;
                foreach(var olOtItem in ls) {
                    update_Week(int.Parse(olOtItem.WWork) ,olOtItem.EmpID ,-1 * double.Parse(olOtItem.OTNo.ToString().Trim()));
                    update_Month(DateTime.Parse(olOtItem.OTDate.ToString().Trim()) ,olOtItem.EmpID ,-1 * double.Parse(olOtItem.OTNo.ToString().Trim()));
                }
                db.SaveChanges();
                TempData["msg"] = "<script>alert('Success');</script>";
                Helpers.Utilities.SendEmail("Phiếu làm thêm giờ #" + request.OT_ID + " đã bị từ chối/ Overtime request has been rejected" ,request.ApproverEmail ,request.ReqEmail ,"" ,"Dear " + request.TBL_USERS_MST.FULLNAME + ",<br/><br/>Yêu cầu bị từ chối.<br/><span style='color:#0070c0;font-style: italic;'>Request has been rejected.</span><br/><br/>" + body);

            } else {
                TempData["msg"] = "<script>alert('Bạn không thể từ chối/You cannot reject');</script>";
            }

            return RedirectToAction("List_OT");
        }
        int kiemtra1() {
            int i = 1;
            if(userLogin != null) {
                var manager1 = db.TBL_USERS_MST.Single(s => s.USERNAME == userLogin.Username);
                var loc1 = db.TBL_SYSTEM.ToList();
                foreach(TBL_SYSTEM l in loc1) {
                    if(l.value == manager1.EMAIL & l.value3 == "HR_CB") {
                        i = 0;
                    }
                }

            }
            return i;
        }
        [HttpPost]
        public ActionResult Upload_Week() {
            if(kiemtra1() == 0) {

                if(Request != null) {
                    int MesRow = 0;
                    try {
                        HttpPostedFileBase file = Request.Files["UploadedFile"];
                        if((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName)) {

                            var mss = "";
                            string fileName = file.FileName;
                            string fileContentType = file.ContentType;
                            byte[] fileBytes = new byte[file.ContentLength];
                            var data = file.InputStream.Read(fileBytes ,0 ,Convert.ToInt32(file.ContentLength));
                            using(var package = new ExcelPackage(file.InputStream)) {
                                var currentSheet = package.Workbook.Worksheets;
                                var workSheet = currentSheet.First();
                                var noOfCol = workSheet.Dimension.End.Column;
                                var noOfRow = workSheet.Dimension.End.Row;


                                for(var rowIterator = 3; rowIterator <= noOfRow; rowIterator++) {

                                    MesRow = rowIterator;
                                    if(workSheet.Cells[rowIterator ,1].Value == null )
                                        break;
                                    var id = workSheet.Cells[rowIterator ,1].Value ?? 0;
                                    var w1 = workSheet.Cells[rowIterator ,2].Value ?? 0;
                                    var w2 = workSheet.Cells[rowIterator ,3].Value ?? 0;
                                    var w3 = workSheet.Cells[rowIterator ,4].Value ?? 0;
                                    var w4 = workSheet.Cells[rowIterator ,5].Value ?? 0;
                                    var w5 = workSheet.Cells[rowIterator ,6].Value ?? 0;
                                    var w6 = workSheet.Cells[rowIterator ,7].Value ?? 0;
                                    var w7 = workSheet.Cells[rowIterator ,8].Value ?? 0;
                                    var w8 = workSheet.Cells[rowIterator ,9].Value ?? 0;
                                    var w9 = workSheet.Cells[rowIterator ,10].Value ?? 0;
                                    var w10 = workSheet.Cells[rowIterator ,11].Value ?? 0;
                                    var w11 = workSheet.Cells[rowIterator ,12].Value ?? 0;
                                    var w12 = workSheet.Cells[rowIterator ,13].Value ?? 0;
                                    var w13 = workSheet.Cells[rowIterator ,14].Value ?? 0;
                                    var w14 = workSheet.Cells[rowIterator ,15].Value ?? 0;
                                    var w15 = workSheet.Cells[rowIterator ,16].Value ?? 0;
                                    var w16 = workSheet.Cells[rowIterator ,17].Value ?? 0;
                                    var w17 = workSheet.Cells[rowIterator ,18].Value ?? 0;
                                    var w18 = workSheet.Cells[rowIterator ,19].Value ?? 0;
                                    var w19 = workSheet.Cells[rowIterator ,20].Value ?? 0;
                                    var w20 = workSheet.Cells[rowIterator ,21].Value ?? 0;
                                    var w21 = workSheet.Cells[rowIterator ,22].Value ?? 0;
                                    var w22 = workSheet.Cells[rowIterator ,23].Value ?? 0;
                                    var w23 = workSheet.Cells[rowIterator ,24].Value ?? 0;
                                    var w24 = workSheet.Cells[rowIterator ,25].Value ?? 0;
                                    var w25 = workSheet.Cells[rowIterator ,26].Value ?? 0;
                                    var w26 = workSheet.Cells[rowIterator ,27].Value ?? 0;
                                    var w27 = workSheet.Cells[rowIterator ,28].Value ?? 0;
                                    var w28 = workSheet.Cells[rowIterator ,29].Value ?? 0;
                                    var w29 = workSheet.Cells[rowIterator ,30].Value ?? 0;
                                    var w30 = workSheet.Cells[rowIterator ,31].Value ?? 0;
                                    var w31 = workSheet.Cells[rowIterator ,32].Value ?? 0;
                                    var w32 = workSheet.Cells[rowIterator ,33].Value ?? 0;
                                    var w33 = workSheet.Cells[rowIterator ,34].Value ?? 0;
                                    var w34 = workSheet.Cells[rowIterator ,35].Value ?? 0;
                                    var w35 = workSheet.Cells[rowIterator ,36].Value ?? 0;
                                    var w36 = workSheet.Cells[rowIterator ,37].Value ?? 0;
                                    var w37 = workSheet.Cells[rowIterator ,38].Value ?? 0;
                                    var w38 = workSheet.Cells[rowIterator ,39].Value ?? 0;
                                    var w39 = workSheet.Cells[rowIterator ,40].Value ?? 0;
                                    var w40 = workSheet.Cells[rowIterator ,41].Value ?? 0;
                                    var w41 = workSheet.Cells[rowIterator ,42].Value ?? 0;
                                    var w42 = workSheet.Cells[rowIterator ,43].Value ?? 0;
                                    var w43 = workSheet.Cells[rowIterator ,44].Value ?? 0;
                                    var w44 = workSheet.Cells[rowIterator ,45].Value ?? 0;
                                    var w45 = workSheet.Cells[rowIterator ,46].Value ?? 0;
                                    var w46 = workSheet.Cells[rowIterator ,47].Value ?? 0;
                                    var w47 = workSheet.Cells[rowIterator ,48].Value ?? 0;
                                    var w48 = workSheet.Cells[rowIterator ,49].Value ?? 0;
                                    var w49 = workSheet.Cells[rowIterator ,50].Value ?? 0;
                                    var w50 = workSheet.Cells[rowIterator ,51].Value ?? 0;
                                    var w51 = workSheet.Cells[rowIterator ,52].Value ?? 0;
                                    var w52 = workSheet.Cells[rowIterator ,53].Value ?? 0;
                                    var w53 = workSheet.Cells[rowIterator ,54].Value ?? 0;

                                    if(id == "") {
                                        mss = "Vui lòng kiểm tra lại dữ liệu";
                                        break;
                                    } else {

                                        var emRecord = db.OL_OT_Record_W.SingleOrDefault(t => t.EmpID == id.ToString().Trim());

                                        if(emRecord == null) {
                                            emRecord = new OL_OT_Record_W() {
                                                EmpID = id.ToString() ,
                                                C1 = float.Parse(w1.ToString()) ,
                                                C2 = float.Parse(w2.ToString()) ,
                                                C3 = float.Parse(w3.ToString()) ,
                                                C4 = float.Parse(w4.ToString()) ,
                                                C5 = float.Parse(w5.ToString()) ,
                                                C6 = float.Parse(w6.ToString()) ,
                                                C7 = float.Parse(w7.ToString()) ,
                                                C8 = float.Parse(w8.ToString()) ,
                                                C9 = float.Parse(w9.ToString()) ,
                                                C10 = float.Parse(w10.ToString()) ,
                                                C11 = float.Parse(w11.ToString()) ,
                                                C12 = float.Parse(w12.ToString()) ,
                                                C13 = float.Parse(w13.ToString()) ,
                                                C14 = float.Parse(w14.ToString()) ,
                                                C15 = float.Parse(w15.ToString()) ,
                                                C16 = float.Parse(w16.ToString()) ,
                                                C17 = float.Parse(w17.ToString()) ,
                                                C18 = float.Parse(w18.ToString()) ,
                                                C19 = float.Parse(w19.ToString()) ,
                                                C20 = float.Parse(w20.ToString()) ,
                                                C21 = float.Parse(w21.ToString()) ,
                                                C22 = float.Parse(w22.ToString()) ,
                                                C23 = float.Parse(w23.ToString()) ,
                                                C24 = float.Parse(w24.ToString()) ,
                                                C25 = float.Parse(w25.ToString()) ,
                                                C26 = float.Parse(w26.ToString()) ,
                                                C27 = float.Parse(w27.ToString()) ,
                                                C28 = float.Parse(w28.ToString()) ,
                                                C29 = float.Parse(w29.ToString()) ,
                                                C30 = float.Parse(w30.ToString()) ,
                                                C31 = float.Parse(w31.ToString()) ,
                                                C32 = float.Parse(w32.ToString()) ,
                                                C33 = float.Parse(w33.ToString()) ,
                                                C34 = float.Parse(w34.ToString()) ,
                                                C35 = float.Parse(w35.ToString()) ,
                                                C36 = float.Parse(w36.ToString()) ,
                                                C37 = float.Parse(w37.ToString()) ,
                                                C38 = float.Parse(w38.ToString()) ,
                                                C39 = float.Parse(w39.ToString()) ,
                                                C40 = float.Parse(w40.ToString()) ,
                                                C41 = float.Parse(w41.ToString()) ,
                                                C42 = float.Parse(w42.ToString()) ,
                                                C43 = float.Parse(w43.ToString()) ,
                                                C44 = float.Parse(w44.ToString()) ,
                                                C45 = float.Parse(w45.ToString()) ,
                                                C46 = float.Parse(w46.ToString()) ,
                                                C47 = float.Parse(w47.ToString()) ,
                                                C48 = float.Parse(w48.ToString()) ,
                                                C49 = float.Parse(w49.ToString()) ,
                                                C50 = float.Parse(w50.ToString()) ,
                                                C51 = float.Parse(w51.ToString()) ,
                                                C52 = float.Parse(w52.ToString()) ,
                                                C53 = float.Parse(w53.ToString()) ,
                                            };
                                            db.OL_OT_Record_W.Add(emRecord);
                                            db.SaveChanges();
                                        } else {

                                            emRecord.C1 = float.Parse(w1.ToString());
                                            emRecord.C2 = float.Parse(w2.ToString());
                                            emRecord.C3 = float.Parse(w3.ToString());
                                            emRecord.C4 = float.Parse(w4.ToString());
                                            emRecord.C5 = float.Parse(w5.ToString());
                                            emRecord.C6 = float.Parse(w6.ToString());
                                            emRecord.C7 = float.Parse(w7.ToString());
                                            emRecord.C8 = float.Parse(w8.ToString());
                                            emRecord.C9 = float.Parse(w9.ToString());
                                            emRecord.C10 = float.Parse(w10.ToString());
                                            emRecord.C11 = float.Parse(w11.ToString());
                                            emRecord.C12 = float.Parse(w12.ToString());
                                            emRecord.C13 = float.Parse(w13.ToString());
                                            emRecord.C14 = float.Parse(w14.ToString());
                                            emRecord.C15 = float.Parse(w15.ToString());
                                            emRecord.C16 = float.Parse(w16.ToString());
                                            emRecord.C17 = float.Parse(w17.ToString());
                                            emRecord.C18 = float.Parse(w18.ToString());
                                            emRecord.C19 = float.Parse(w19.ToString());
                                            emRecord.C20 = float.Parse(w20.ToString());
                                            emRecord.C21 = float.Parse(w21.ToString());
                                            emRecord.C22 = float.Parse(w22.ToString());
                                            emRecord.C23 = float.Parse(w23.ToString());
                                            emRecord.C24 = float.Parse(w24.ToString());
                                            emRecord.C25 = float.Parse(w25.ToString());
                                            emRecord.C26 = float.Parse(w26.ToString());
                                            emRecord.C27 = float.Parse(w27.ToString());
                                            emRecord.C28 = float.Parse(w28.ToString());
                                            emRecord.C29 = float.Parse(w29.ToString());
                                            emRecord.C30 = float.Parse(w30.ToString());
                                            emRecord.C31 = float.Parse(w31.ToString());
                                            emRecord.C32 = float.Parse(w32.ToString());
                                            emRecord.C33 = float.Parse(w33.ToString());
                                            emRecord.C34 = float.Parse(w34.ToString());
                                            emRecord.C35 = float.Parse(w35.ToString());
                                            emRecord.C36 = float.Parse(w36.ToString());
                                            emRecord.C37 = float.Parse(w37.ToString());
                                            emRecord.C38 = float.Parse(w38.ToString());
                                            emRecord.C39 = float.Parse(w39.ToString());
                                            emRecord.C40 = float.Parse(w40.ToString());
                                            emRecord.C41 = float.Parse(w41.ToString());
                                            emRecord.C42 = float.Parse(w42.ToString());
                                            emRecord.C43 = float.Parse(w43.ToString());
                                            emRecord.C44 = float.Parse(w44.ToString());
                                            emRecord.C45 = float.Parse(w45.ToString());
                                            emRecord.C46 = float.Parse(w46.ToString());
                                            emRecord.C47 = float.Parse(w47.ToString());
                                            emRecord.C48 = float.Parse(w48.ToString());
                                            emRecord.C49 = float.Parse(w49.ToString());
                                            emRecord.C50 = float.Parse(w50.ToString());
                                            emRecord.C51 = float.Parse(w51.ToString());
                                            emRecord.C52 = float.Parse(w52.ToString());
                                            emRecord.C53 = float.Parse(w53.ToString());
                                            db.Entry(emRecord).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                        }
                                        mss = "Upload thành công!";
                                    }

                                }

                            }

                            TempData["msg"] = "<script> alert('" + mss + "')</script>";
                        }
                    } catch(Exception e) {
                        TempData["msg"] = "<script>alert('Error, need contact to IT. " + e.Message + ",  Row " + Convert.ToString(MesRow) + "')</script>";
                    }
                }

            } else {
                TempData["msg"] = "<script>alert('Tài khoản của bạn không thể upload');</script>";
            }


            return RedirectToAction("List_OT");
        }
        [HttpPost]
        public ActionResult Upload_Month() {
            if(kiemtra1() == 0) {

                if(Request != null) {
                    int MesRow = 0;
                    try {
                        HttpPostedFileBase file = Request.Files["UploadedFile1"];
                        if((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName)) {

                            var mss = "";
                            string fileName = file.FileName;
                            string fileContentType = file.ContentType;
                            byte[] fileBytes = new byte[file.ContentLength];
                            var data = file.InputStream.Read(fileBytes ,0 ,Convert.ToInt32(file.ContentLength));
                            using(var package = new ExcelPackage(file.InputStream)) {
                                var currentSheet = package.Workbook.Worksheets;
                                var workSheet = currentSheet.First();
                                var noOfCol = workSheet.Dimension.End.Column;
                                var noOfRow = workSheet.Dimension.End.Row;


                                for(var rowIterator = 3; rowIterator <= noOfRow; rowIterator++) {
                                    MesRow = rowIterator;
                                    if(workSheet.Cells[rowIterator ,1].Value == null )
                                        break;
                                    var id = workSheet.Cells[rowIterator ,1].Value ?? 0;
                                    var w1 = workSheet.Cells[rowIterator ,2].Value ?? 0;
                                    var w2 = workSheet.Cells[rowIterator ,3].Value ?? 0;
                                    var w3 = workSheet.Cells[rowIterator ,4].Value ?? 0;
                                    var w4 = workSheet.Cells[rowIterator ,5].Value ?? 0;
                                    var w5 = workSheet.Cells[rowIterator ,6].Value ?? 0;
                                    var w6 = workSheet.Cells[rowIterator ,7].Value ?? 0;
                                    var w7 = workSheet.Cells[rowIterator ,8].Value ?? 0;
                                    var w8 = workSheet.Cells[rowIterator ,9].Value ?? 0;
                                    var w9 = workSheet.Cells[rowIterator ,10].Value ?? 0;
                                    var w10 = workSheet.Cells[rowIterator ,11].Value ?? 0;
                                    var w11 = workSheet.Cells[rowIterator ,12].Value ?? 0;
                                    var w12 = workSheet.Cells[rowIterator ,13].Value ?? 0;



                                    var emRecord = db.OL_OT_Record_M.SingleOrDefault(t => t.EmpID == id.ToString().Trim());

                                    if(emRecord == null) {
                                        emRecord = new OL_OT_Record_M() {
                                            EmpID = id.ToString() ,
                                            C1 = float.Parse(w1.ToString()) ,
                                            C2 = float.Parse(w2.ToString()) ,
                                            C3 = float.Parse(w3.ToString()) ,
                                            C4 = float.Parse(w4.ToString()) ,
                                            C5 = float.Parse(w5.ToString()) ,
                                            C6 = float.Parse(w6.ToString()) ,
                                            C7 = float.Parse(w7.ToString()) ,
                                            C8 = float.Parse(w8.ToString()) ,
                                            C9 = float.Parse(w9.ToString()) ,
                                            C10 = float.Parse(w10.ToString()) ,
                                            C11 = float.Parse(w11.ToString()) ,
                                            C12 = float.Parse(w12.ToString()) ,

                                        };
                                        db.OL_OT_Record_M.Add(emRecord);
                                        db.SaveChanges();
                                    } else {

                                        emRecord.C1 = float.Parse(w1.ToString());
                                        emRecord.C2 = float.Parse(w2.ToString());
                                        emRecord.C3 = float.Parse(w3.ToString());
                                        emRecord.C4 = float.Parse(w4.ToString());
                                        emRecord.C5 = float.Parse(w5.ToString());
                                        emRecord.C6 = float.Parse(w6.ToString());
                                        emRecord.C7 = float.Parse(w7.ToString());
                                        emRecord.C8 = float.Parse(w8.ToString());
                                        emRecord.C9 = float.Parse(w9.ToString());
                                        emRecord.C10 = float.Parse(w10.ToString());
                                        emRecord.C11 = float.Parse(w11.ToString());
                                        emRecord.C12 = float.Parse(w12.ToString());

                                        db.Entry(emRecord).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                    mss = "Upload thành công!";


                                }

                            }

                            TempData["msg"] = "<script> alert('" + mss + "')</script>";
                        }
                    } catch(Exception e) {
                        TempData["msg"] = "<script>alert('Error, need contact to IT. " + e.Message + ",  Row " + Convert.ToString(MesRow) + "')</script>";
                    }
                }

            } else {
                TempData["msg"] = "<script>alert('Tài khoản của bạn không thể upload');</script>";
            }


            return RedirectToAction("List_OT");
        }

        public ActionResult Export(DateTime? ngaybatdau ,DateTime? ngayketthuc) {
            if(ngaybatdau != null && ngayketthuc != null) {
                var data = (from a in db.OL_OT_Details
                            join b in db.OL_OT_Item on a.OT_ID equals b.OT_ID
                            join g in db.OL_User_Approver on b.EmpID equals g.EmpID
                            join u in db.OL_User_Approver on a.ReqName equals u.UserCD
                            where a.AppStatus == 1 && a.EmpSubmit >= ngaybatdau & a.EmpSubmit <= DbFunctions.AddDays(ngayketthuc ,1)
                            select new {
                                EmpID = b.EmpID ,
                                EmpName = g.EmpName ,
                                EmpSubmit = a.EmpSubmit.ToString() ,
                                OTDate = b.OTDate.ToString() ,
                                OTTimeIn = b.OTTimeIn ,
                                OTDate1 = b.OTDate1.ToString() ,
                                OTTimeOut = b.OTTimeOut ,
                                OTCD = b.OTCD ,
                                OTNo = b.OTNo ?? 0 ,
                                cr = u.EmpID ,
                                VanToAdd = b.VanToAdd ,
                                Reason = b.Reason ,
                                Van = b.Van ,
                                Line = g.Line ,
                            }).ToList().Select(
                                    a => new OtExportRequest {
                                        EmpID = a.EmpID ,
                                        EmpName = a.EmpName ,
                                        EmpSubmit = a.EmpSubmit.ToString() ,
                                        OTDate = a.OTDate.ToString() ,
                                        OTTimeIn = Convert.ToDateTime(a.OTDate + " " + a.OTTimeIn) ,
                                        OTDate1 = a.OTDate1.ToString() ,
                                        OTTimeOut = Convert.ToDateTime(a.OTDate1 + " " + a.OTTimeOut) ,
                                        OTCD = a.OTCD ,
                                        OTNo = a.OTNo ,
                                        cr = a.cr ,
                                        VanToAdd = a.VanToAdd ,
                                        Reason = a.Reason ,
                                        Van = a.Van ,
                                        Line = a.Line ,
                                    });
                var ls = new List<OtExportRequest>();
                foreach(var variable in data) {
                    variable.EmpSubmit = Utilities.DateFormat(variable.EmpSubmit ,"dd/MM/yyyy HH:mm:ss");
                    variable.OTDate = Utilities.DateFormat(variable.OTDate ,"dd/MM/yyyy");
                    variable.OTDate1 = Utilities.DateFormat(variable.OTDate1 ,"dd/MM/yyyy");
                    ls.Add(variable);
                }

                var excel = new ExcelPackage();
                var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
                workSheet.Cells["A1:N1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                workSheet.Cells["A1:N1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#f2f2f2"));
                workSheet.Cells["A1:N1"].Style.Font.Bold = true;
                workSheet.Column(6).Style.Numberformat.Format = "hh:mm:ss";
                workSheet.Column(8).Style.Numberformat.Format = "hh:mm:ss";

                workSheet.Cells[1 ,1].Value = "Mã NV";
                workSheet.Cells[1 ,2].Value = "Họ tên";
                workSheet.Cells[1 ,3].Value = "Tổ";
                workSheet.Cells[1 ,4].Value = "Ngày tạo";
                workSheet.Cells[1 ,5].Value = "Ngày bắt đầu";
                workSheet.Cells[1 ,6].Value = "Giờ bắt đầu";
                workSheet.Cells[1 ,7].Value = "Ngày kết thúc";
                workSheet.Cells[1 ,8].Value = "Giờ kết thúc";
                workSheet.Cells[1 ,9].Value = "Loại OT";
                workSheet.Cells[1 ,10].Value = "Số giờ";
                workSheet.Cells[1 ,11].Value = "Lý do";
                workSheet.Cells[1 ,12].Value = "Có cần xe Công ty không";
                workSheet.Cells[1 ,13].Value = "Địa điểm xuống xe";
                workSheet.Cells[1 ,14].Value = "Người tạo";

                workSheet.Cells[2 ,1].LoadFromCollection(ls ,false);
                using(var col = workSheet.Cells[1 ,1 ,ls.Count() + 1 ,14]) {
                    col.AutoFitColumns();
                    col.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    col.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    col.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    col.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                using(var memoryStream = new MemoryStream()) {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition" ,"attachment;  filename=HR-OT-Request.xlsx");
                    excel.SaveAs(memoryStream);
                    memoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }

            return RedirectToAction("List_OT");

        }
        public ActionResult ExportPendingApprove(DateTime? ngaybatdau ,DateTime? ngayketthuc) {
            if(ngaybatdau != null && ngayketthuc != null) {


                var data = (from a in db.OL_OT_Details
                            join b in db.OL_OT_Item on a.OT_ID equals b.OT_ID
                            join g in db.OL_User_Approver on b.EmpID equals g.EmpID
                            join u in db.OL_User_Approver on a.ReqName equals u.UserCD
                            where a.AppStatus == -1 && a.EmpSubmit >= ngaybatdau && a.EmpSubmit <= DbFunctions.AddDays(ngayketthuc ,1)
                            select new {
                                EmpID = b.EmpID ,
                                EmpName = g.EmpName ,
                                EmpSubmit = a.EmpSubmit.ToString() ,
                                OTDate = b.OTDate.ToString() ,
                                OTTimeIn = b.OTTimeIn ,
                                OTDate1 = b.OTDate1.ToString() ,
                                OTTimeOut = b.OTTimeOut ,
                                OTCD = b.OTCD ,
                                OTNo = b.OTNo ?? 0 ,
                                ApproverName = g.ApproverName ,
                                ApproverEmail = g.ApproverEmail ,
                                status = "Pending" ,
                                cr = u.EmpID ,
                                VanToAdd = b.VanToAdd ,
                                Reason = b.Reason ,
                                Van = b.Van ,
                                Line = g.Line ,
                            }).ToList()
                                .Select(
                                    a => new OtExportRequestPending {
                                        EmpID = a.EmpID ,
                                        EmpName = a.EmpName ,
                                        EmpSubmit = a.EmpSubmit.ToString() ,
                                        OTDate = a.OTDate.ToString() ,
                                        OTTimeIn = Convert.ToDateTime(a.OTDate + " " + a.OTTimeIn) ,
                                        OTDate1 = a.OTDate1.ToString() ,
                                        OTTimeOut = Convert.ToDateTime(a.OTDate1 + " " + a.OTTimeOut) ,
                                        OTCD = a.OTCD ,
                                        OTNo = a.OTNo ,
                                        ApproverName = a.ApproverName ,
                                        ApproverEmail = a.ApproverEmail ,
                                        status = a.status ,
                                        cr = a.cr ,
                                        VanToAdd = a.VanToAdd ,
                                        Reason = a.Reason ,
                                        Van = a.Van ,
                                        Line = a.Line ,
                                    });

                var ls = new List<OtExportRequestPending>();
                foreach(var variable in data) {
                    variable.EmpSubmit = Utilities.DateFormat(variable.EmpSubmit ,"dd/MM/yyyy HH:mm:ss");
                    variable.OTDate = Utilities.DateFormat(variable.OTDate ,"dd/MM/yyyy");
                    variable.OTDate1 = Utilities.DateFormat(variable.OTDate1 ,"dd/MM/yyyy");
                    ls.Add(variable);
                }




                var excel = new ExcelPackage();
                var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
                workSheet.Cells["A1:Q1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                workSheet.Cells["A1:Q1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#f2f2f2"));
                workSheet.Cells["A1:Q1"].Style.Font.Bold = true;
                workSheet.Column(6).Style.Numberformat.Format = "hh:mm:ss";
                workSheet.Column(8).Style.Numberformat.Format = "hh:mm:ss";

                workSheet.Cells[1 ,1].Value = "Mã NV";
                workSheet.Cells[1 ,2].Value = "Họ tên";
                workSheet.Cells[1 ,3].Value = "Tổ";
                workSheet.Cells[1 ,4].Value = "Ngày tạo";
                workSheet.Cells[1 ,5].Value = "Ngày bắt đầu";
                workSheet.Cells[1 ,6].Value = "Giờ bắt đầu";
                workSheet.Cells[1 ,7].Value = "Ngày kết thúc";
                workSheet.Cells[1 ,8].Value = "Giờ kết thúc";
                workSheet.Cells[1 ,9].Value = "Loại OT";
                workSheet.Cells[1 ,10].Value = "Số giờ";
                workSheet.Cells[1 ,11].Value = "Lý do";
                workSheet.Cells[1 ,12].Value = "Có cần xe Công ty không";
                workSheet.Cells[1 ,13].Value = "Địa điểm xuống xe";

                workSheet.Cells[1 ,14].Value = "Người duyệt";
                workSheet.Cells[1 ,15].Value = "Email";
                workSheet.Cells[1 ,16].Value = "Trạng thái";
                workSheet.Cells[1 ,17].Value = "Người tạo";

                workSheet.Cells[2 ,1].LoadFromCollection(ls ,false);
                using(var col = workSheet.Cells[1 ,1 ,ls.Count() + 1 ,17]) {
                    col.AutoFitColumns();
                    col.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    col.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    col.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    col.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                using(var memoryStream = new MemoryStream()) {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition" ,"attachment;  filename=HR-OT-Pending-Request.xlsx");
                    excel.SaveAs(memoryStream);
                    memoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }

            }
            return RedirectToAction("List_OT");

        }

        [HttpPost]
        public ActionResult Upload_More(string section ,int dept ,string approver ,
           string approverMail ,string nameHR ,string mailHR ,string note) {
            if(Request != null) {
                var detail = new Models.OL_OT_Details();
                int MesRow = 0;
                try {

                    HttpPostedFileBase file = Request.Files["UploadedFile"];
                    if((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName)) {

                        string nameAndLocation = @"~\log\Upload\" + userLogin.Username + "-OT-" + DateTime.Now.Ticks + "-" + Path.GetFileName(file.FileName);
                        file.SaveAs(Server.MapPath(nameAndLocation));
                        var mss = "";
                        string fileName = file.FileName;
                        string fileContentType = file.ContentType;
                        byte[] fileBytes = new byte[file.ContentLength];
                        var data = file.InputStream.Read(fileBytes ,0 ,Convert.ToInt32(file.ContentLength));
                        using(var package = new ExcelPackage(file.InputStream)) {


                            var currentSheet = package.Workbook.Worksheets;
                            var workSheet = currentSheet.First();
                            var noOfCol = workSheet.Dimension.End.Column;
                            var noOfRow = workSheet.Dimension.End.Row;


                            if(workSheet.Cells[3 ,1].Value != null) {


                                // detail.OT_ID = ma1();
                                detail.ApproverName = approver;
                                detail.ApproverEmail = approverMail;
                                detail.Dept = dept;
                                detail.Section = section;
                                detail.ReqName = userLogin.Username;
                                detail.ReqEmail = userLogin.Email;
                                detail.AppStatus = -1;
                                detail.HRStatus = -2;
                                detail.HRName = nameHR;
                                detail.HREmail = mailHR;
                                detail.Notes = "";
                                detail.EmpSubmit = Utilities.GetDate_VietNam(DateTime.Now);
                                db.OL_OT_Details.Add(detail);
                                db.SaveChanges();
                                var oneLeave = new List<OL_OT_Item>();
                                var quagio = false;
                                string mss1 ="", mss2 ="", mss3 ="", mss4 = "", mss5 = "";
                                for(var rowIterator = 3; rowIterator <= noOfRow; rowIterator++) {
                                    if(workSheet.Cells[rowIterator ,1].Value == null )
                                        break;
                                    MesRow = rowIterator;

                                    var empid = workSheet.Cells[rowIterator ,1].Value ?? "";
                                    var otcd = workSheet.Cells[rowIterator ,3].Value ?? "";
                                    var date1 = workSheet.Cells[rowIterator ,4].Value ?? "";
                                    var timein = workSheet.Cells[rowIterator ,5].Value ?? "";
                                    var timeout = workSheet.Cells[rowIterator ,6].Value ?? "";
                                    var total = workSheet.Cells[rowIterator ,7].Value ?? "";
                                    var address1 = workSheet.Cells[rowIterator ,9].Value ?? "";
                                    var reason = workSheet.Cells[rowIterator ,8].Value ?? "";
                                    empid = Utilities.ValidEmpID(empid.ToString().Trim());

                                    var item1 = new OL_OT_Item();
                                    item1.OT_ID = detail.OT_ID;
                                    item1.OTCD = otcd.ToString().Trim().ToUpper();
                                    item1.Reason = reason.ToString().Trim();
                                    item1.OTDate = DateTime.Parse(date1.ToString().Trim());
                                    item1.OTDate1 = DateTime.Parse(date1.ToString().Trim());
                                    item1.OTTimeIn = DateTime.Parse(timein.ToString().Trim()).ToString(@"HH:mm:ss");
                                    item1.OTTimeOut = DateTime.Parse(timeout.ToString().Trim()).ToString(@"HH:mm:ss");
                                    Convert.ToDateTime("08/08/2020 " + item1.OTTimeIn);
                                    Convert.ToDateTime("08/08/2020 " + item1.OTTimeOut);

                                    item1.OTNo = double.Parse(total.ToString());
                                    item1.WWork = GetWeekOrderInYear(Utilities.GetDate_VietNam(DateTime.Parse(date1.ToString().Trim())))
                                        .ToString();
                                    item1.EmpID = empid.ToString().Trim();
                                    item1.Van = address1.ToString().Length > 5 ? 1 : 0;
                                    item1.VanToAdd = address1.ToString();
                                    oneLeave.Add(item1);
                                    if(!db.OL_OTCode.Any(a => a.OTCD == item1.OTCD)) {
                                        mss4 = mss4 + (mss4 != "" ? ", " : "") + item1.EmpID;
                                        quagio = true;
                                    } else if(!db.OL_User_Approver.Any(a => a.EmpID == item1.EmpID)) {
                                        mss5 = mss5 + (mss5 != "" ? ", " : "") + item1.EmpID;
                                        quagio = true;
                                    } else {

                                        var totalHoursTo = oneLeave.Where(a => a.EmpID == item1.EmpID).Sum(a => a.OTNo);
                                        //if(totalHoursTo <= 4)
                                        //{
                                        //var w = Week(int.Parse(item1.WWork) ,item1.EmpID) + totalHoursTo;
                                        //if(w <= 16) {
                                        //    var m = Month(DateTime.Parse(date1.ToString().Trim()) ,item1.EmpID) +
                                        //            totalHoursTo;
                                        //    if(m <= 40) {
                                        //        var y = Year(item1.EmpID) + totalHoursTo;
                                        //        if(y < 300) {

                                                    db.OL_OT_Item.Add(item1);
                                                    update_Week(int.Parse(item1.WWork) ,item1.EmpID ,double.Parse(item1.OTNo.ToString().Trim()));
                                                    update_Month(DateTime.Parse(item1.OTDate.ToString().Trim()) ,item1.EmpID ,double.Parse(item1.OTNo.ToString().Trim()));
                                        //        } else {
                                        //            quagio = true;
                                        //            mss1 = mss1 + (mss1 != "" ? ", " : "") + item1.EmpID + "(" + y + "h)";
                                        //        }
                                        //    } else {
                                        //        quagio = true;
                                        //        mss2 = mss2 + (mss2 != "" ? ", " : "") + item1.EmpID + "(" + m + "h)";


                                        //    }
                                        //} else {
                                        //    quagio = true;
                                        //    mss3 = mss3 + (mss3 != "" ? ", " : "") + item1.EmpID + "(" + w + "h)";

                                        //}

                                    }


                                }

                                try {

                                    if(quagio) {
                                        removeDetail(detail);
                                        var  ms1 = "► Số giờ trong năm vượt quá 300h/Number of hours in the year exceeds 300 hours: " + mss1 + "<br/><br/>";
                                        var  ms2 = "► Số giờ trong tháng vượt quá 40h/Number of hours in a month exceeds 40 hours: " + mss2 + "<br/>";
                                        var  ms4 = "► OTCode không đúng / OTCode is not correct: " + mss4 + "<br/>";
                                        var ms3 = "► Số giờ trong tuần vượt quá 16h/The number of hours per week exceeds 16 hours: " + mss3 + "<br/>";
                                        var ms5 = "► Mã nhân viên không đúng/Employee code is incorrect: " + mss5 + "<br/>";
                                        // var ms4 = "Số giờ trong ngày vượt quá 4h/Number of hours per day exceeds 4 hours: " + mss4 + "<br/>";
                                        TempData["msg"] = (mss3 != "" ? ms3 : "") + (mss2 != "" ? ms2 : "") + (mss1 != "" ? ms1 : "") + (mss4 != "" ? ms4 : "") + (mss5 != "" ? ms5 : "");
                                        return RedirectToAction("List_OT");
                                    } else {
                                        db.SaveChanges();
                                        mss = "thành công/Success.";
                                        Utilities.SendEmail("Phiếu làm thêm giờ #" + detail.OT_ID + "/Overtime request need your approval" ,userLogin.Email ,detail.ApproverEmail ,userLogin.Email ,"Dear " + detail.ApproverName + ",<br/><br/>Vui lòng phê duyệt phiếu làm thêm giờ #" + detail.OT_ID + ".<br/><span style='color:#0070c0;font-style: italic;'>Please approve or reject leave request #" + detail.OT_ID + ".</span> ");

                                    }
                                } catch(Exception e) {
                                    removeDetail(detail);
                                    TempData["msg"] = "<script>alert('Error,Please check the data and retry again. " + e.Message + ",  Row " + Convert.ToString(MesRow) + "')</script>";
                                    Utilities.WriteLogException(e ,"");
                                    return RedirectToAction("List_OT");
                                }





                            }
                        }

                        TempData["msg"] = "<script> alert('" + mss + "')</script>";

                    }
                } catch(Exception e) {
                    if(detail.OT_ID > 0) {

                        db = new MyContext();
                        db.OL_OT_Details.Remove(db.OL_OT_Details.Find(detail.OT_ID));
                        db.SaveChanges();
                    }
                    TempData["msg"] = "<script>alert('Error, Please check the data and retry again. " + e.Message + ",  Row " + Convert.ToString(MesRow) + "')</script>";
                    Utilities.WriteLogException(e ,"");

                }
            }

            return RedirectToAction("List_OT");
        }

        public void removeDetail(OL_OT_Details detail) {
            db = new MyContext();
            db.OL_OT_Details.Remove(db.OL_OT_Details.Find(detail.OT_ID));
            db.SaveChanges();
        }

        public double Week(int week ,string empid) {
            try {
                var emp = db.OL_OT_Record_W.Single(s => s.EmpID == empid);
                switch(week) {
                    case 1:
                        return double.Parse(emp.C1.ToString());
                        break;
                    case 2:
                        return double.Parse(emp.C2.ToString());
                        break;
                    case 3:
                        return double.Parse(emp.C3.ToString());
                        break;
                    case 4:
                        return double.Parse(emp.C4.ToString());
                        break;
                    case 5:
                        return double.Parse(emp.C5.ToString());
                        break;
                    case 6:
                        return double.Parse(emp.C6.ToString());
                        break;
                    case 7:
                        return double.Parse(emp.C7.ToString());
                        break;
                    case 8:
                        return double.Parse(emp.C8.ToString());
                        break;
                    case 9:
                        return double.Parse(emp.C9.ToString());
                        break;
                    case 10:
                        return double.Parse(emp.C10.ToString());
                        break;
                    case 11:
                        return double.Parse(emp.C11.ToString());
                        break;
                    case 12:
                        return double.Parse(emp.C12.ToString());
                        break;
                    case 13:
                        return double.Parse(emp.C13.ToString());
                        break;
                    case 14:
                        return double.Parse(emp.C14.ToString());
                        break;
                    case 15:
                        return double.Parse(emp.C15.ToString());
                        break;
                    case 16:
                        return double.Parse(emp.C16.ToString());
                        break;
                    case 17:
                        return double.Parse(emp.C17.ToString());
                        break;
                    case 18:
                        return double.Parse(emp.C18.ToString());
                        break;
                    case 19:
                        return double.Parse(emp.C19.ToString());
                        break;
                    case 20:
                        return double.Parse(emp.C20.ToString());
                        break;
                    case 21:
                        return double.Parse(emp.C21.ToString());
                        break;
                    case 22:
                        return double.Parse(emp.C22.ToString());
                        break;
                    case 23:
                        return double.Parse(emp.C23.ToString());
                        break;
                    case 24:
                        return double.Parse(emp.C24.ToString());
                        break;
                    case 25:
                        return double.Parse(emp.C25.ToString());
                        break;
                    case 26:
                        return double.Parse(emp.C26.ToString());
                        break;
                    case 27:
                        return double.Parse(emp.C27.ToString());
                        break;
                    case 28:
                        return double.Parse(emp.C28.ToString());
                        break;
                    case 29:
                        return double.Parse(emp.C29.ToString());
                        break;
                    case 30:
                        return double.Parse(emp.C30.ToString());
                        break;
                    case 31:
                        return double.Parse(emp.C31.ToString());
                        break;
                    case 32:
                        return double.Parse(emp.C32.ToString());
                        break;
                    case 33:
                        return double.Parse(emp.C33.ToString());
                        break;
                    case 34:
                        return double.Parse(emp.C34.ToString());
                        break;
                    case 35:
                        return double.Parse(emp.C35.ToString());
                        break;
                    case 36:
                        return double.Parse(emp.C36.ToString());
                        break;
                    case 37:
                        return double.Parse(emp.C37.ToString());
                        break;
                    case 38:
                        return double.Parse(emp.C38.ToString());
                        break;
                    case 39:
                        return double.Parse(emp.C39.ToString());
                        break;
                    case 40:
                        return double.Parse(emp.C40.ToString());
                        break;
                    case 41:
                        return double.Parse(emp.C41.ToString());
                        break;
                    case 42:
                        return double.Parse(emp.C42.ToString());
                        break;
                    case 43:
                        return double.Parse(emp.C43.ToString());
                        break;
                    case 44:
                        return double.Parse(emp.C44.ToString());
                        break;
                    case 45:
                        return double.Parse(emp.C45.ToString());
                        break;
                    case 46:
                        return double.Parse(emp.C46.ToString());
                        break;
                    case 47:
                        return double.Parse(emp.C47.ToString());
                        break;
                    case 48:
                        return double.Parse(emp.C48.ToString());
                        break;
                    case 49:
                        return double.Parse(emp.C49.ToString());
                        break;
                    case 50:
                        return double.Parse(emp.C50.ToString());
                        break;
                    case 51:
                        return double.Parse(emp.C51.ToString());
                        break;
                    case 52:
                        return double.Parse(emp.C52.ToString());
                        break;
                    case 53:
                        return double.Parse(emp.C53.ToString());
                        break;

                    default:
                        return 0;
                        break;

                }
            } catch {
                return 0;
            }
        }
        public void update_Week(int week ,string empid ,double sl) {

            var emp = db.OL_OT_Record_W.SingleOrDefault(s => s.EmpID == empid);
            if(emp == null) {
                var db1=new MyContext();
                db1.OL_OT_Record_W.Add(new OL_OT_Record_W {
                    EmpID = empid
                });
                db1.SaveChanges();
                emp = db.OL_OT_Record_W.SingleOrDefault(s => s.EmpID == empid);
            }

            switch(week) {
                case 1:
                    emp.C1 = emp.C1 + sl;
                    break;
                case 2:
                    emp.C2 = emp.C2 + sl;
                    break;
                case 3:
                    emp.C3 = emp.C3 + sl;
                    break;
                case 4:
                    emp.C4 = emp.C4 + sl;
                    break;
                case 5:
                    emp.C5 = emp.C5 + sl;
                    break;
                case 6:
                    emp.C6 = emp.C6 + sl;
                    break;
                case 7:
                    emp.C7 = emp.C7 + sl;
                    break;
                case 8:
                    emp.C8 = emp.C8 + sl;
                    break;
                case 9:
                    emp.C9 = emp.C9 + sl;
                    break;
                case 10:
                    emp.C10 = emp.C10 + sl;
                    break;
                case 11:
                    emp.C11 = emp.C11 + sl;
                    break;
                case 12:
                    emp.C12 = emp.C12 + sl;
                    break;
                case 13:
                    emp.C13 = emp.C13 + sl;
                    break;
                case 14:
                    emp.C14 = emp.C14 + sl;
                    break;
                case 15:
                    emp.C15 = emp.C15 + sl;
                    break;
                case 16:
                    emp.C16 = emp.C16 + sl;
                    break;
                case 17:
                    emp.C17 = emp.C17 + sl;
                    break;
                case 18:
                    emp.C18 = emp.C18 + sl;
                    break;
                case 19:
                    emp.C19 = emp.C19 + sl;
                    break;
                case 20:
                    emp.C20 = emp.C20 + sl;
                    break;
                case 21:
                    emp.C21 = emp.C21 + sl;
                    break;
                case 22:
                    emp.C22 = emp.C22 + sl;
                    break;
                case 23:
                    emp.C23 = emp.C23 + sl;
                    break;
                case 24:
                    emp.C24 = emp.C24 + sl;
                    break;
                case 25:
                    emp.C25 = emp.C25 + sl;
                    break;
                case 26:
                    emp.C26 = emp.C26 + sl;
                    break;
                case 27:
                    emp.C27 = emp.C27 + sl;
                    break;
                case 28:
                    emp.C28 = emp.C28 + sl;
                    break;
                case 29:
                    emp.C29 = emp.C29 + sl;
                    break;
                case 30:
                    emp.C30 = emp.C30 + sl;
                    break;
                case 31:
                    emp.C31 = emp.C31 + sl;
                    break;
                case 32:
                    emp.C32 = emp.C32 + sl;
                    break;
                case 33:
                    emp.C33 = emp.C33 + sl;
                    break;
                case 34:
                    emp.C34 = emp.C34 + sl;
                    break;
                case 35:
                    emp.C35 = emp.C35 + sl;
                    break;
                case 36:
                    emp.C36 = emp.C36 + sl;
                    break;
                case 37:
                    emp.C37 = emp.C37 + sl;
                    break;
                case 38:
                    emp.C38 = emp.C38 + sl;
                    break;
                case 39:
                    emp.C39 = emp.C39 + sl;
                    break;
                case 40:
                    emp.C40 = emp.C40 + sl;
                    break;
                case 41:
                    emp.C41 = emp.C41 + sl;
                    break;
                case 42:
                    emp.C42 = emp.C42 + sl;
                    break;
                case 43:
                    emp.C43 = emp.C43 + sl;
                    break;
                case 44:
                    emp.C44 = emp.C44 + sl;
                    break;
                case 45:
                    emp.C45 = emp.C45 + sl;
                    break;
                case 46:
                    emp.C46 = emp.C46 + sl;
                    break;
                case 47:
                    emp.C40 = emp.C40 + sl;
                    break;
                case 48:
                    emp.C48 = emp.C48 + sl;
                    break;
                case 49:
                    emp.C49 = emp.C49 + sl;
                    break;
                case 50:
                    emp.C50 = emp.C50 + sl;
                    break;
                case 51:
                    emp.C51 = emp.C51 + sl;
                    break;
                case 52:
                    emp.C52 = emp.C52 + sl;
                    break;
                case 53:
                    emp.C53 = emp.C53 + sl;
                    break;

            }


            db.Entry(emp).State = System.Data.Entity.EntityState.Modified;






        }
        public double Month(DateTime date ,string empid) {
            try {
                var emp = db.OL_OT_Record_M.SingleOrDefault(s => s.EmpID == empid);
                int month = date.Month;
                switch(month) {
                    case 1:
                        return double.Parse(emp.C1.ToString());
                        break;
                    case 2:
                        return double.Parse(emp.C2.ToString());
                        break;
                    case 3:
                        return double.Parse(emp.C3.ToString());
                        break;
                    case 4:
                        return double.Parse(emp.C4.ToString());
                        break;
                    case 5:
                        return double.Parse(emp.C5.ToString());
                        break;
                    case 6:
                        return double.Parse(emp.C6.ToString());
                        break;
                    case 7:
                        return double.Parse(emp.C7.ToString());
                        break;
                    case 8:
                        return double.Parse(emp.C8.ToString());
                        break;
                    case 9:
                        return double.Parse(emp.C9.ToString());
                        break;
                    case 10:
                        return double.Parse(emp.C10.ToString());
                        break;
                    case 11:
                        return double.Parse(emp.C11.ToString());
                        break;
                    case 12:
                        return double.Parse(emp.C12.ToString());
                        break;
                    default:
                        return 0;
                        break;
                }
            } catch(Exception e) {
                return 0;
            }
        }
        public double Year(string empid) {
            try {
                var emp = db.OL_OT_Record_M.SingleOrDefault(s => s.EmpID == empid);
                return emp.C1
                + emp.C2
                   + emp.C3
                   + emp.C4
                   + emp.C5
                   + emp.C6
                   + emp.C7
                   + emp.C8
                   + emp.C9
                   + emp.C10
            + emp.C11
            + emp.C12;

            } catch(Exception e) {
                return 0;
            }
        }
        public void update_Month(DateTime date ,string empid ,double sl) {

            var emp = db.OL_OT_Record_M.SingleOrDefault(s => s.EmpID == empid);
            int month = date.Month;
            if(emp == null) {
                var db1=new MyContext();
                db1.OL_OT_Record_M.Add(new OL_OT_Record_M {
                    EmpID = empid
                });
                db1.SaveChanges();
                emp = db.OL_OT_Record_M.SingleOrDefault(s => s.EmpID == empid);
            }
            switch(month) {
                case 1:
                    emp.C1 = emp.C1 + sl;
                    break;
                case 2:
                    emp.C2 = emp.C2 + sl;
                    break;
                case 3:
                    emp.C3 = emp.C3 + sl;
                    break;
                case 4:
                    emp.C4 = emp.C4 + sl;
                    break;
                case 5:
                    emp.C5 = emp.C5 + sl;
                    break;
                case 6:
                    emp.C6 = emp.C6 + sl;
                    break;
                case 7:
                    emp.C7 = emp.C7 + sl;
                    break;
                case 8:
                    emp.C8 = emp.C8 + sl;
                    break;
                case 9:
                    emp.C9 = emp.C9 + sl;
                    break;
                case 10:
                    emp.C10 = emp.C10 + sl;
                    break;
                case 11:
                    emp.C11 = emp.C11 + sl;
                    break;
                case 12:
                    emp.C12 = emp.C12 + sl;

                    break;

            }


            db.Entry(emp).State = System.Data.Entity.EntityState.Modified;



        }


    }

}

