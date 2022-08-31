using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProductionApp.Models;
using System.Collections;
using System.Globalization;
using System.Net;
using OfficeOpenXml;
using System.IO;
using System.Data;
using System.Data.Entity;
using System.Web.UI;
using System.Web.UI.WebControls;
using Castle.Core.Internal;
using OfficeOpenXml.Style;
using ProductionApp.Helpers;

namespace ProductionApp.Controllers {
    public class Leave_OTController:BaseController {
        // GET: Leave_OT
        public ActionResult Index() {
            if(userLogin == null) {
                return RedirectToAction("NeedLogin" ,"Notification");
            }

            return RedirectToAction("List_Leave");
        }

        public ActionResult List_Leave() {


            if(userLogin != null) {
                if(userLogin.Username.ToLower() == "admin") {
                    ViewBag.list = db.OL_LeaveDetails.OrderByDescending(s => s.LeaveID);
                    ViewBag.per = 3;
                } else {
                    var hr_team = db.TBL_SYSTEM.Where(s => s.value.ToLower() == userLogin.Email.ToLower() & s.value3 == "HR_CB").ToList();
                    if(hr_team.Count > 0) {
                        ViewBag.list = db.OL_LeaveDetails.Where(s => s.AppStatus != -3 & s.HRStatus != 2).OrderByDescending(s => s.LeaveID);
                        ViewBag.per = 2;
                    } else {
                        ViewBag.list = db.OL_LeaveDetails.Where(s => (s.ApproverEmail.ToLower() == userLogin.Email.ToLower() || s.ReqEmail.ToLower() == userLogin.Email.ToLower())).OrderByDescending(s => s.LeaveID).Take(100);
                        ViewBag.per = 1;
                    }
                }

            } else {
                ViewBag.list = "";
            }



            return View();
        }

        public ActionResult Report(string empID ,DateTime? date) {
            if(empID.IsNullOrEmpty())
                return View();
            var ls = db.OL_Leave_Item.Where(a => a.EmpID == empID.Trim() && a.OL_LeaveDetails.AppStatus == 1);
            if(date != null)
                ls = ls.Where(a => a.FromDate == date);

            var hr_team = db.TBL_SYSTEM.Where(s => s.value.ToLower() == userLogin.Email.ToLower() & s.value3 == "HR_CB").ToList();
            // HR==fail
            if(hr_team.Count <= 0) {
                ls = ls.Where(a => a.OL_LeaveDetails.Dept == userLogin.DeptID);
            }
            return View(ls.OrderBy(a => a.FromDate).ToList());
        }

        public ActionResult Leave() {
            ViewBag.leave_code = db.OL_LeaveCode.ToList();
            ViewBag.group = db.TBL_GROUP_MST.ToList();

            try {
                var empID = db.OL_User_Approver.Single(a => a.UserCD == userLogin.Username).EmpID;
                var balance = db.OL_LeaveBalance.SingleOrDefault(s => s.EmpID == empID) ?? new OL_LeaveBalance();
                ViewData["balance"] = balance;
            } catch(Exception e) {
                ViewData["balance"] = new OL_LeaveBalance();
                TempData["msg"] = "<script>alert('Balance not found, Please contact to HR team!');</script>";
            }

            var user1 = db.TBL_USERS_MST.Single(s => s.USERNAME == userLogin.Username);
            ViewData["user"] = user1;
            var dept = db.TBL_DEPARTMENT_MST.Single(s => s.DEPT_ID == user1.DEPT);
            ViewData["dept"] = dept;
            try {
                ViewData["app"] = db.OL_User_Approver.Single(s => s.UserCD == user1.USERNAME);
            } catch(Exception e) {
                ViewData["app"] = new OL_User_Approver();
                TempData["msg"] = "<script>alert('Your info not exist, Please contact to HR team!');</script>";
            }
            ViewBag.user_list = db.OL_User_Approver.Where(s => s.Section == dept.DEPT_ID).ToList();
            ViewBag.hr = db.TBL_SYSTEM.Where(s => s.value3 == "HR_CB").ToList();

            return View();
        }
        public JsonResult User_Load(string id) {
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try {
                //set ProxyCreation to false
                db.Configuration.ProxyCreationEnabled = false;
                var ds = db.OL_LeaveBalance.FirstOrDefault(s => s.EmpID == id);
                return Json(ds ,JsonRequestBehavior.AllowGet);
            } catch(Exception ex) {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(ex.Message);
            } finally {
                //restore ProxyCreation to its original state
                db.Configuration.ProxyCreationEnabled = proxyCreation;
            };
        }
        public JsonResult load_cb(string id) {
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try {
                //set ProxyCreation to false
                db.Configuration.ProxyCreationEnabled = false;
                var ds = db.TBL_SYSTEM.FirstOrDefault(s => s.id == id);
                return Json(ds ,JsonRequestBehavior.AllowGet);
            } catch(Exception ex) {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(ex.Message);
            } finally {
                //restore ProxyCreation to its original state
                db.Configuration.ProxyCreationEnabled = proxyCreation;
            };
        }
        public ActionResult Add_new(string leaveID ,float total ,float used ,string fromdate ,string todate) {

            if(Session["one_leave"] == null) // Nếu giỏ hàng chưa được khởi tạo
            {
                Session["one_leave"] = new List<One_Leave>();  // Khởi tạo Session["giohang"] là 1 List<CartItem>
            }

            string ms = "";
            if(leaveID == "" || fromdate == "" || todate == "") {
                ms = "Thiếu thông tin/Lack of information";
            } else {
                try {

                    var emp = db.OL_User_Approver.SingleOrDefault(s => s.UserCD == userLogin.Username);
                    if(emp == null)
                        ms = "Employee not found, Please contact to HR team!";
                    else {
                        var name = db.OL_LeaveCode.Single(s => s.LeaveCD == leaveID);
                        var oneLeave = Session["one_leave"] as List<One_Leave>;
                        //var one = oneLeave.FirstOrDefault(s => s.leaveID == leaveID);
                        var leaveBalance = db.OL_LeaveBalance.Single(s => s.EmpID == emp.EmpID);
                        if(leaveBalance == null)
                            ms = "Balance not found, Please contact to HR team!";
                        else {
                            var total1 = oneLeave.Sum(a => a.total_date);
                            var total_date = Utilities.DateDiff(DateTime.Parse(fromdate) ,DateTime.Parse(todate)) + 1;

                            if(total1 + leaveBalance.UsedLeave + total_date > leaveBalance.TotalLeave && leaveID == "AL2") {
                                ms = "Đã hết số ngày nghỉ phép(AL2)/The number of days off has expired(AL2)";
                            } else {
                                var newItem = new One_Leave() {
                                    leaveID = leaveID ,
                                    leaveName = name.LeaveName ,
                                    total = total ,
                                    used = used ,
                                    remai = total - used ,
                                    fromdate = fromdate ,
                                    todate = todate ,
                                    total_date = (int)total_date

                                };
                                oneLeave.Add(newItem);


                            }
                        }
                    }
                } catch(Exception e) {
                    Utilities.WriteLogException(e ,"Leave/Add_new");
                    ms = "System error, Please retry or contact to IT team!";
                }
            }
            return Json(ms ,JsonRequestBehavior.AllowGet);
        }
        public ActionResult Add_half(string leaveID ,float total ,float used ,string one_date ,string dauca ,float total_date_more) {
            string ms = "";
            if(Session["half"] == null) // Nếu giỏ hàng chưa được khởi tạo
            {
                Session["half"] = new List<Half_One>();  // Khởi tạo Session["giohang"] là 1 List<CartItem>
            }
            if(leaveID == "" || one_date == "") {
                ms = "Thiếu thông tin/Lack of information";
            } else {
                try {

                    var emp = db.OL_User_Approver.SingleOrDefault(s => s.UserCD == userLogin.Username);
                    if(emp == null)
                        ms = "Employee not found, Please contact to HR team!";
                    else {
                        var name = db.OL_LeaveCode.Single(s => s.LeaveCD == leaveID);
                        var oneLeave = Session["half"] as List<Half_One>;
                        // var one = oneLeave.FirstOrDefault(s => s.leaveID == leaveID);
                        var leaveBalance = db.OL_LeaveBalance.SingleOrDefault(s => s.EmpID == emp.EmpID);
                        if(leaveBalance == null)
                            ms = "Balance not found, Please contact to HR team!";
                        else {
                            var total1 = oneLeave.Sum(a => a.total_date);

                            if(total1 + leaveBalance.UsedLeave + 0.5 > leaveBalance.TotalLeave && leaveID == "AL2") {
                                ms = "Đã hết số ngày nghỉ phép(AL2)/The number of days off has expired(AL2)";
                            } else {


                                var newItem = new Half_One() {
                                    leaveID = leaveID ,
                                    leaveName = name.LeaveName ,
                                    total = total ,
                                    used = used ,
                                    remai = total - used ,
                                    date = one_date ,
                                    dauca = dauca ,
                                    total_date = total_date_more


                                };
                                oneLeave.Add(newItem);


                            }
                        }
                    }
                } catch(Exception e) {
                    Utilities.WriteLogException(e ,"Leave/Add_half");
                    ms = "System error, Please retry or contact to IT team!";
                }
            }

            return Json(ms ,JsonRequestBehavior.AllowGet);
        }
        public ActionResult Add_Morefull(string LeaveCD ,float total_more ,float used_more ,string more_fromdate ,
            string more_todate ,string EmpName) {
            string ms = "";

            if(Session["more_full"] == null) // Nếu giỏ hàng chưa được khởi tạo
            {
                Session["more_full"] = new List<More_Full>();  // Khởi tạo Session["giohang"] là 1 List<CartItem>
            }
            if(LeaveCD == "" || more_fromdate == "" || more_todate == "") {
                ms = "Thiếu thông tin/Lack of information";
            } else {
                try {
                    var name = db.OL_LeaveCode.Single(s => s.LeaveCD == LeaveCD);
                    var oneLeave = Session["more_full"] as List<More_Full>;

                    var emp = db.OL_User_Approver.SingleOrDefault(s => s.EmpID == EmpName);
                    if(emp == null)
                        ms = "Employee not found, Please contact to HR team!";
                    else {
                        var total = db.OL_LeaveBalance.SingleOrDefault(s => s.EmpID == emp.EmpID);
                        if(total == null)
                            ms = "Balance not found, Please contact to HR team!";
                        else {
                            var total1 = oneLeave.Where(a => a.empID == EmpName).Sum(a => a.total_date);
                            var total_date=   Utilities.DateDiff(DateTime.Parse(more_fromdate) ,DateTime.Parse(more_todate)) + 1;
                            if(total1 + total.UsedLeave + total_date > total.TotalLeave && LeaveCD == "AL2") {
                                ms = "Đã hết số ngày nghỉ phép(AL2)/The number of days off has expired(AL2)";
                            } else {
                                var newItem = new More_Full() {
                                    leaveID = LeaveCD ,
                                    leaveName = name.LeaveName ,
                                    total = total_more ,
                                    used = used_more ,
                                    EmpName = emp.EmpName ,
                                    empID = EmpName ,
                                    EmpEmail = emp.EmpEmail ,
                                    remai = total_more - used_more ,
                                    fromdate = more_fromdate ,
                                    todate = more_todate ,
                                    total_date = (int)total_date
                                };
                                oneLeave.Add(newItem);
                                //ms = "OK";

                            }
                        }
                    }
                } catch(Exception e) {
                    Utilities.WriteLogException(e ,"Leave/Add_Morefull");
                    ms = "System error, Please retry or contact to IT team!";
                }
            }

            return Json(ms ,JsonRequestBehavior.AllowGet);
        }
        public ActionResult Add_MoreHalf(string LeaveCD ,float total_more ,float used_more ,string more_date ,string more_dauca ,float more_total ,string EmpName) {
            string ms = "";

            if(Session["more_half"] == null) // Nếu giỏ hàng chưa được khởi tạo
            {
                Session["more_half"] = new List<More_Half>();  // Khởi tạo Session["giohang"] là 1 List<CartItem>
            }
            if(LeaveCD == "" || more_date == "") {
                ms = "Thiếu thông tin/Lack of information";
            } else {
                try {


                    var name = db.OL_LeaveCode.Single(s => s.LeaveCD == LeaveCD);
                    var oneLeave = Session["more_half"] as List<More_Half>;

                    var emp = db.OL_User_Approver.SingleOrDefault(s => s.EmpID == EmpName);
                    if(emp == null)
                        ms = "Employee not found, Please contact to HR team!";
                    else {
                        var total = db.OL_LeaveBalance.SingleOrDefault(s => s.EmpID == emp.EmpID);
                        if(total == null)
                            ms = "Balance not found, Please contact to HR team!";
                        else {
                            var total1 = oneLeave.Where(a => a.EmpID == EmpName).Sum(a => a.used);
                            if(total1 + total.UsedLeave + 0.5 > total.TotalLeave && LeaveCD == "AL2") {
                                ms = "Đã hết số ngày nghỉ phép(AL2)/The number of days off has expired(AL2)";
                            } else {
                                var newItem = new More_Half() {
                                    leaveID = LeaveCD ,
                                    leaveName = name.LeaveName ,
                                    total = total_more ,
                                    used = used_more ,
                                    EmpID = EmpName ,
                                    EmpName = emp.EmpName ,
                                    EmpEmail = emp.EmpEmail ,
                                    remai = total_more - used_more ,
                                    date = more_date ,
                                    dauca = more_dauca ,
                                    total_date = more_total
                                };
                                oneLeave.Add(newItem);
                                //   ms = "Đăng ký thành công!";


                            }
                        }
                    }
                } catch(Exception e) {
                    Utilities.WriteLogException(e ,"Leave/Add_MoreHalf");
                    ms = "System error, Please retry or contact to IT team!";
                }
            }

            return Json(ms ,JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Delete(string id) {
            var one = Session["one_leave"] as List<One_Leave>;
            var itemXoa = one.FirstOrDefault(m => m.leaveID == id);
            if(itemXoa != null) {
                one.Remove(itemXoa);
            }
            return Json("" ,JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Delete1(string id) {
            var one = Session["half"] as List<Half_One>;
            var itemXoa = one.FirstOrDefault(m => m.leaveID == id);
            if(itemXoa != null) {
                one.Remove(itemXoa);
            }
            return Json("" ,JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Delete_half(string id) {
            var one = Session["more_half"] as List<More_Half>;
            var itemXoa = one.FirstOrDefault(m => m.leaveID == id);
            if(itemXoa != null) {
                one.Remove(itemXoa);
            }
            return Json("" ,JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Delete_full(string id) {
            var one = Session["more_full"] as List<More_Full>;
            var itemXoa = one.FirstOrDefault(m => m.leaveID == id);
            if(itemXoa != null) {
                one.Remove(itemXoa);
            }
            return Json("" ,JsonRequestBehavior.AllowGet);
        }
        public ActionResult Load() {

            List<One_Leave> List = null;
            if(Session["one_leave"] != null) {
                List = (List<One_Leave>)Session["one_leave"];

            }

            return Json(List ,JsonRequestBehavior.AllowGet);

        }
        public ActionResult Load1() {

            List<Half_One> List = null;
            if(Session["half"] != null) {
                List = (List<Half_One>)Session["half"];

            }

            return Json(List ,JsonRequestBehavior.AllowGet);

        }
        public ActionResult Load_morefull() {

            List<More_Full> List = null;
            if(Session["more_full"] != null) {
                List = (List<More_Full>)Session["more_full"];

            }

            return Json(List ,JsonRequestBehavior.AllowGet);

        }
        public ActionResult Load_morehalf() {

            List<More_Half> List = null;
            if(Session["more_half"] != null) {
                List = (List<More_Half>)Session["more_half"];

            }

            return Json(List ,JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult One_full(string section ,string approver ,
            string approverMail ,string nameHR ,string mailHR ,string note) {
            string ms = "";
            try {

                var emp = db.OL_User_Approver.Single(a => a.UserCD == userLogin.Username);

                var item = Session["one_leave"] as List<One_Leave>;

                if(item != null) {
                    var detail = new OL_LeaveDetails {
                        ApproverName = approver ,
                        ApproverEmail = approverMail ,
                        Dept = userLogin.DeptID ,
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

                    db.OL_LeaveDetails.Add(detail);
                    db.SaveChanges();

                    for(int i = 0; i < item.Count; i++) {

                        var item1 = new OL_Leave_Item {
                            LeaveID = detail.LeaveID ,
                            LeaveCD = item[i].leaveID ,
                            LeaveNo = item[i].total_date ,
                            Reason = note ,
                            FromDate = DateTime.Parse(item[i].fromdate) ,
                            ToDate = DateTime.Parse(item[i].todate)
                        };

                        item1.EmpID = emp.EmpID;
                        db.OL_Leave_Item.Add(item1);

                        var code = db.OL_LeaveCode.Single(s => s.LeaveCD == item1.LeaveCD);
                        if(code.LeaveCD == "AL2") {
                            var used = db.OL_LeaveBalance.SingleOrDefault(s => s.EmpID == item1.EmpID);
                            used.UsedLeave = used.UsedLeave + item[i].total_date;
                            db.Entry(used).State = System.Data.Entity.EntityState.Modified;

                        }


                    }
                    if(db.SaveChanges() > 0) {
                        Session.Remove("one_leave");
                        Session.Remove("more_half");
                        Session.Remove("more_full");
                        Session.Remove("half");
                        ms = "Thành công/success !";
                        Utilities.SendEmail("Yêu cầu phê duyệt nghỉ số #" + detail.LeaveID + " / Leave request need your approval" ,userLogin.Email ,detail.ApproverEmail ,detail.ReqEmail ,"Dear " + detail.ApproverName + ",<br/><br/>Vui lòng phê duyệt đề nghị nghỉ #" + detail.LeaveID + ".<br/><span style='color:#0070c0;font-style: italic;'>  Please approve or reject leave request #" + detail.LeaveID + ".</span>");
                    } else {
                        db = new MyContext();
                        db.OL_LeaveDetails.Remove(db.OL_LeaveDetails.Find(detail.LeaveID));
                        db.SaveChanges();
                        ms = "Fail, Please retry or contact to HR team!";
                    }


                }
            } catch(Exception e) {
                Utilities.WriteLogException(e ,"Leave/One_full");
                ms = "System error, Please retry or contact to IT team!";
            }

            return Json(ms ,JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public ActionResult One_half(string section ,string approver ,
            string approverMail ,string nameHR ,string mailHR ,string note) {
            string ms1 = "";
            try {

                var emp = db.OL_User_Approver.Single(a => a.UserCD == userLogin.Username);

                var item = Session["half"] as List<Half_One>;
                if(item != null) {
                    var detail = new OL_LeaveDetails {
                        ApproverName = approver ,
                        ApproverEmail = approverMail ,
                        Dept = userLogin.DeptID ,
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

                    db.OL_LeaveDetails.Add(detail);
                    db.SaveChanges();

                    for(int i = 0; i < item.Count; i++) {
                        var item1 = new OL_Leave_Item {
                            LeaveID = detail.LeaveID ,
                            LeaveCD = item[i].leaveID ,
                            LeaveNo = item[i].total_date ,
                            Reason = note ,
                            FromDate = DateTime.Parse(item[i].date) ,
                            ToDate = DateTime.Parse(item[i].date) ,
                            LeaveInMorning = item[i].dauca
                        };


                        item1.EmpID = emp.EmpID;
                        db.OL_Leave_Item.Add(item1);
                        if(item1.LeaveNo == 0.5) {
                            var code = db.OL_LeaveCode.Single(s => s.LeaveCD == item1.LeaveCD);
                            if(code.LeaveCD == "AL2") {
                                var used = db.OL_LeaveBalance.SingleOrDefault(s => s.EmpID == item1.EmpID);

                                used.UsedLeave = used.UsedLeave + item[i].total_date;
                                db.Entry(used).State = System.Data.Entity.EntityState.Modified;

                            }

                        }


                    }

                    if(db.SaveChanges() > 0) {

                        ms1 = "Thành công/success !";
                        Utilities.SendEmail("Yêu cầu phê duyệt nghỉ số #" + detail.LeaveID + " / Leave request need your approval" ,userLogin.Email ,detail.ApproverEmail ,detail.ReqEmail ,"Dear " + detail.ApproverName + ",<br/><br/>Vui lòng phê duyệt đề nghị nghỉ #" + detail.LeaveID + ".<br/><span style='color:#0070c0;font-style: italic;'>Please approve or reject leave request #" + detail.LeaveID + ".<br/>");
                        Session.Remove("one_leave");
                        Session.Remove("more_half");
                        Session.Remove("more_full");
                        Session.Remove("half");
                    } else {
                        db = new MyContext();
                        db.OL_LeaveDetails.Remove(db.OL_LeaveDetails.Find(detail.LeaveID));
                        db.SaveChanges();
                        ms1 = "Fail, Please retry or contact to HR team!";
                    }
                }

            } catch(Exception e) {
                Utilities.WriteLogException(e ,"Leave/One_half");
                ms1 = "System error, Please retry or contact to IT team!";
            }

            return Json(ms1 ,JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public ActionResult More_full(string section ,string approver ,string approverMail ,string nameHR ,string mailHR ,string note) {
            string ms = "";
            try {

                var item = Session["more_full"] as List<More_Full>;
                bool balence = true;
                string Id = "";
                if(item != null) {


                    var detail = new OL_LeaveDetails {
                        ApproverName = approver ,
                        ApproverEmail = approverMail ,
                        Dept = userLogin.DeptID ,
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
                    //detail.LeaveID = ma1();
                    db.OL_LeaveDetails.Add(detail);
                    db.SaveChanges();


                    foreach(var t in item) {
                        var item1 = new OL_Leave_Item {
                            LeaveID = detail.LeaveID ,
                            LeaveCD = t.leaveID ,
                            LeaveNo = t.total_date ,
                            Reason = note ,
                            FromDate = DateTime.Parse(t.fromdate) ,
                            ToDate = DateTime.Parse(t.todate) ,
                            EmpID = t.empID
                        };
                        db.OL_Leave_Item.Add(item1);

                        var code = db.OL_LeaveCode.Single(s => s.LeaveCD == item1.LeaveCD);
                        if(code.LeaveCD == "AL2") {
                            var used = db.OL_LeaveBalance.SingleOrDefault(s => s.EmpID == item1.EmpID);

                            used.UsedLeave = used.UsedLeave + t.total_date;
                            db.Entry(used).State = System.Data.Entity.EntityState.Modified;
                            //db.SaveChanges();

                        }


                    }
                    if(db.SaveChanges() > 0) {
                        ms = "Thành công/success !";
                        Utilities.SendEmail("Yêu cầu phê duyệt nghỉ số #" + detail.LeaveID + " / Leave request need your approval" ,userLogin.Email ,detail.ApproverEmail ,detail.ReqEmail ,"Dear " + detail.ApproverName + ",<br/><br/>Vui lòng phê duyệt đề nghị nghỉ #" + detail.LeaveID + ".<br/><span style='color:#0070c0;font-style: italic;'>Please approve or reject leave request #" + detail.LeaveID + ".<br/>");

                        Session.Remove("one_leave");
                        Session.Remove("more_half");
                        Session.Remove("more_full");
                        Session.Remove("half");
                    } else {
                        db = new MyContext();
                        db.OL_LeaveDetails.Remove(db.OL_LeaveDetails.Find(detail.LeaveID));
                        db.SaveChanges();
                        ms = "Fail, Please retry or contact to HR team!";
                    }

                }


            } catch(Exception e) {
                Utilities.WriteLogException(e ,"Leave/Add_Morefull");
                ms = "System error, Please retry or contact to IT team!";
            }

            return Json(ms ,JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public ActionResult More_half(string section ,string approver ,
           string approverMail ,string nameHR ,string mailHR ,string note) {
            string ms1 = "";
            try {

                var item = Session["more_half"] as List<More_Half>;
                bool balance = true;
                string Id = "";
                if(item != null) {


                    var detail = new Models.OL_LeaveDetails {
                        ApproverName = approver ,
                        ApproverEmail = approverMail ,
                        HRName = nameHR ,
                        HREmail = mailHR ,
                        Notes = note ,
                        Dept = userLogin.DeptID ,
                        Section = section ,
                        ReqName = userLogin.Username ,
                        ReqEmail = userLogin.Email ,
                        AppStatus = -1 ,
                        HRStatus = -2 ,
                        EmpSubmit = Utilities.GetDate_VietNam(DateTime.Now)
                    };
                    // detail.LeaveID = ma1();

                    db.OL_LeaveDetails.Add(detail);
                    db.SaveChanges();



                    for(int i = 0; i < item.Count; i++) {
                        var item1 = new OL_Leave_Item {
                            LeaveID = detail.LeaveID ,
                            LeaveCD = item[i].leaveID ,
                            LeaveNo = item[i].total_date ,
                            Reason = note ,
                            FromDate = DateTime.Parse(item[i].date) ,
                            ToDate = DateTime.Parse(item[i].date) ,
                            LeaveInMorning = item[i].dauca ,
                            EmpID = item[i].EmpID
                        };

                        db.OL_Leave_Item.Add(item1);

                        var code = db.OL_LeaveCode.Single(s => s.LeaveCD == item1.LeaveCD);
                        if(code.LeaveCD == "AL2") {
                            var used = db.OL_LeaveBalance.Single(s => s.EmpID == item1.EmpID);
                            used.UsedLeave = used.UsedLeave + item[i].total_date;
                            db.Entry(used).State = System.Data.Entity.EntityState.Modified;
                        }
                    }

                    if(db.SaveChanges() > 0) {
                        ms1 = "Thành công/success !";
                        Utilities.SendEmail("Yêu cầu phê duyệt nghỉ số #" + detail.LeaveID + " / Leave request need your approval" ,userLogin.Email ,detail.ApproverEmail ,detail.ReqEmail ,"Dear " + detail.ApproverName + ",<br/><br/>Vui lòng phê duyệt đề nghị nghỉ #" + detail.LeaveID + ".<br/><span style='color:#0070c0;font-style: italic;'>Please approve or reject leave request #" + detail.LeaveID + ".</span>");
                        Session.Remove("one_leave");
                        Session.Remove("more_half");
                        Session.Remove("more_full");
                        Session.Remove("half");
                    } else {
                        db = new MyContext();
                        db.OL_LeaveDetails.Remove(db.OL_LeaveDetails.Find(detail.LeaveID));
                        db.SaveChanges();
                        ms1 = "Fail, Please retry or contact to HR team!";
                    }

                }
            } catch(Exception e) {
                Utilities.WriteLogException(e ,"Leave/More_half");
                ms1 = "System error, Please retry or contact to IT team!";
            }


            return Json(ms1 ,JsonRequestBehavior.AllowGet);

        }
        public ActionResult Edit_Leave(int id) {
            var request = db.OL_LeaveDetails.Find(id);
            int? dept1 = request.Dept;
            ViewData["dept"] = db.TBL_DEPARTMENT_MST.Single(s => s.DEPT_ID == dept1);
            ViewData["item"] = db.OL_Leave_Item.Where(s => s.LeaveID == id).OrderBy(a => a.ID).ToList();
            ViewData["leave"] = db.OL_LeaveDetails.Find(id);
            ViewData["manager"] = db.OL_User_Approver.First(s => s.UserCD == request.ReqName);
            ViewData["hrteam"] = db.TBL_SYSTEM.Single(s => s.value == request.HREmail && s.value3 == "HR_CB");
            return View();
        }

        [HttpPost]
        public ActionResult Approve(int id ,string dept) {
            var request = db.OL_LeaveDetails.Find(id);

            var user1 = db.OL_User_Approver.SingleOrDefault(s => s.EmpID == dept);
            if(user1 != null && user1.ApproverEmail.ToLower() == userLogin.Email.ToLower()) {
                try {
                    request.AppStatus = 1;
                    request.AppSubmitted = Utilities.GetDate_VietNam(DateTime.Now);
                    db.Entry(request).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    Utilities.SendEmail("Đề nghị nghỉ #" + request.LeaveID + " đã được duyệt / Leave request #" + request.LeaveID + " has been approved." ,request.ApproverEmail ,request.HREmail ,request.ReqEmail ,"Hi HR teams,<br/><br/>Đề nghị nghỉ #" + request.LeaveID + " đã được phê duyệt, vui lòng xử lý.<br/><span style='color:#0070c0;font-style: italic;'>Request #" + request.LeaveID + " has been approved. Please process this request.</span>");

                    TempData["msg"] = "<script>alert('Thành công/Success');</script>";

                } catch(Exception e) {
                    Utilities.WriteLogException(e);
                    TempData["msg"] = "<script>alert('Error, need contact to IT')</script>";
                }
            } else {
                TempData["msg"] = "<script>alert('Bạn không thể phê duyệt/You cannot approve');</script>";
            }
            return RedirectToAction("List_Leave");
        }

        // UserModels user;
        [HttpPost]

        public ActionResult HRteam_Approve(int id ,string hrteam) {
            var request = db.OL_LeaveDetails.Find(id);


            var hr = db.TBL_SYSTEM.SingleOrDefault(s => s.value.ToLower() == userLogin.Email.ToLower() & s.value3 == "HR_CB");
            if(hr != null) {
                if(request.AppStatus == 1) {
                    try {
                        request.HRStatus = 2;
                        request.HRSubmit = Utilities.GetDate_VietNam(DateTime.Now);
                        db.Entry(request).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        Utilities.SendEmail("Đề nghị nghỉ #" + request.LeaveID + " đã xử lý / Leave request #" + request.LeaveID + " has been processed." ,request.HREmail ,request.ReqEmail ,"" ,"Hi " + request.TBL_USERS_MST.FULLNAME + ",<br/><br/>Đề nghị nghỉ #" + request.LeaveID + " đã được xử lý.<br/><span style='color:#0070c0;font-style: italic;'>Request #" + request.LeaveID + " has been processed.</span>");
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

            return RedirectToAction("List_Leave");
        }

        [HttpPost]
        public ActionResult SendMail(int id ,string body ,string dept1) {
            try {

                var request = db.OL_LeaveDetails.Find(id);

                var user1 = db.OL_User_Approver.SingleOrDefault(s => s.EmpID == dept1);
                if(user1 != null && user1.ApproverEmail.ToLower() == userLogin.Email.ToLower() || userLogin.Username.ToLower() == "admin") {

                    request.AppStatus = -3;
                    db.Entry(request).State = System.Data.Entity.EntityState.Modified;


                    foreach(var code in request.OL_Leave_Item) {
                        if(code.LeaveCD == "AL2") {
                            var used = db.OL_LeaveBalance.SingleOrDefault(s => s.EmpID == code.EmpID);

                            used.UsedLeave = used.UsedLeave - code.LeaveNo;
                            db.Entry(used).State = System.Data.Entity.EntityState.Modified;

                        }
                    }

                    db.SaveChanges();
                    Utilities.SendEmail("Yêu cầu nghỉ số #" + request.LeaveID + " đã bị từ chối/ Leave request has been rejected" ,request.ApproverEmail ,request.ReqEmail ,"" ,"Dear " + request.TBL_USERS_MST.FULLNAME + ",<br/><br/>Đề nghị nghỉ #" + request.LeaveID + " đã bị từ chối.<br/><span style='color:#0070c0;font-style: italic;'>Leave request #" + request.LeaveID + " has been rejected.</span> <br/><br/>" + body);
                    TempData["msg"] = "<script>alert('Thành công/Success');</script>";

                } else {
                    TempData["msg"] = "<script>alert('Bạn không thể phê duyệt/You cannot approve');</script>";
                }
            } catch(Exception e) {
                Utilities.WriteLogException(e ,"Leave/SendMail");
                TempData["msg"] = "<script>alert('Thất bại/Fail');</script>";
            }
            return RedirectToAction("List_Leave");
        }
        int kiemtra1() {
            int i = 1;

            if(userLogin != null) {
                var manager1 = db.TBL_USERS_MST.Single(s => s.USERNAME == userLogin.Username);
                var loc1 = db.TBL_SYSTEM.FirstOrDefault(a => a.value.ToLower() == manager1.EMAIL.ToLower() & a.value3 == "HR_CB");
                if(loc1 != null)
                    i = 0;
                //

            }
            return i;
        }
        [HttpPost]
        public ActionResult Upload_LeaveBal() {
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
                                    string id = workSheet.Cells[rowIterator ,1].Value.ToString();
                                    var total = workSheet.Cells[rowIterator ,2].Value.ToString();
                                    var used = workSheet.Cells[rowIterator ,3].Value.ToString();
                                    id = Utilities.ValidEmpID(id.Trim());
                                    var emRecord = db.OL_LeaveBalance.SingleOrDefault(s => s.EmpID == id.ToString());

                                    if(emRecord == null) {
                                        emRecord = new OL_LeaveBalance() {
                                            EmpID = id ,
                                            TotalLeave = float.Parse(total.Trim()) ,
                                            UsedLeave = float.Parse(used.Trim())
                                        };
                                        db.OL_LeaveBalance.Add(emRecord);
                                        db.SaveChanges();
                                    } else {

                                        emRecord.TotalLeave = float.Parse(total.Trim());
                                        emRecord.UsedLeave = float.Parse(used.Trim());
                                        db.Entry(emRecord).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                    mss = "Upload thành công!";


                                }

                            }

                            TempData["msg"] = "<script> alert('" + mss + "')</script>";
                        }
                    } catch(Exception e) {
                        Utilities.WriteLogException(e ,"Leave/Upload_LeaveBal");
                        TempData["msg"] = "<script> alert('Error, need contact to IT. " + e.Message + ",  Row " + Convert.ToString(MesRow) + "')</script>";
                    }
                }

            } else {
                TempData["msg"] = "<script>alert('Tài khoản của bạn không thể upload');</script>";
            }


            return RedirectToAction("List_Leave");
        }
        [HttpPost]
        public ActionResult Upload_Emp() {

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
                                    var empid = workSheet.Cells[rowIterator ,1].Value ?? "";
                                    var usercd = workSheet.Cells[rowIterator ,2].Value ?? "";
                                    var empname = workSheet.Cells[rowIterator ,3].Value ?? "";
                                    var empemail = workSheet.Cells[rowIterator ,4].Value ?? "";
                                    var section = workSheet.Cells[rowIterator ,5].Value ?? "";
                                    var dept = workSheet.Cells[rowIterator ,6].Value ?? "";
                                    var app = workSheet.Cells[rowIterator ,7].Value ?? "";
                                    var appemail = workSheet.Cells[rowIterator ,8].Value ?? "";
                                    var line = workSheet.Cells[rowIterator ,9].Value ?? "";

                                    var emRecord = db.OL_User_Approver.SingleOrDefault(t => t.EmpID == empid.ToString().Trim());
                                    empid = Utilities.ValidEmpID(empid.ToString().Trim());

                                    if(emRecord == null) {
                                        emRecord = new OL_User_Approver() {
                                            EmpID = empid.ToString().Trim() ,
                                            UserCD = usercd.ToString().Trim().ToLower() ,
                                            EmpName = empname.ToString().Trim() ,
                                            EmpEmail = empemail.ToString().Trim() ,
                                            Section = int.Parse(section.ToString().Trim()) ,
                                            Dept = dept.ToString().Trim() ,
                                            ApproverName = app.ToString().Trim() ,
                                            Line = line.ToString().Trim() ,
                                            ApproverEmail = appemail.ToString().Trim()
                                        };
                                        db.OL_User_Approver.Add(emRecord);
                                        if(!db.OL_LeaveBalance.Any(a => a.EmpID == empid.ToString().Trim())) {
                                            var leaveBalance= new OL_LeaveBalance() {
                                                EmpID = empid.ToString().Trim() ,
                                                TotalLeave = 0 ,
                                                UsedLeave = 0
                                            };
                                            db.OL_LeaveBalance.Add(leaveBalance);
                                        }



                                    } else {
                                        emRecord.UserCD = usercd.ToString().Trim().ToLower();
                                        emRecord.EmpName = empname.ToString().Trim();
                                        emRecord.EmpEmail = empemail.ToString().Trim();
                                        emRecord.Section = int.Parse(section.ToString().Trim());
                                        emRecord.Dept = dept.ToString().Trim();
                                        emRecord.ApproverName = app.ToString().Trim();
                                        emRecord.ApproverEmail = appemail.ToString().Trim();
                                        emRecord.Line = line.ToString().Trim();
                                        db.Entry(emRecord).State = System.Data.Entity.EntityState.Modified;

                                    }
                                    if(usercd.ToString().Trim().Length > 0) {
                                        var tblUser =db.TBL_USERS_MST.SingleOrDefault(a => a.USERNAME.ToLower() == usercd.ToString().Trim().ToLower());
                                        if(tblUser == null) {
                                            tblUser = new TBL_USERS_MST {
                                                USERNAME = usercd.ToString().Trim().ToLower() ,
                                                FULLNAME = empname.ToString().Trim() ,
                                                EMAIL = empemail.ToString().Trim() ,
                                                WSHOP_ID = 1 ,
                                                ACTIVATE = 1 ,
                                                ROLE_ID = 5 ,
                                                Password = "123" ,
                                                DEPT = int.Parse(section.ToString().Trim()) ,
                                                POSID = 4
                                            };
                                            db.TBL_USERS_MST.Add(tblUser);
                                        } else {
                                            tblUser.FULLNAME = empname.ToString().Trim();
                                            tblUser.EMAIL = empemail.ToString().Trim();
                                            tblUser.DEPT = int.Parse(section.ToString().Trim());
                                            db.Entry(tblUser).State = System.Data.Entity.EntityState.Modified;

                                        }
                                        var cates = db.TBL_CATEGORIES.Where(a => a.AccessDefault.Contains("HRCB"));
                                        foreach(var item in cates) {
                                            var tmp = db.TBL_PERMISSION.SingleOrDefault(a => a.USERNAME == tblUser.USERNAME && a.MO_ID == item.CA_ID);
                                            if(tmp == null) {

                                                db.TBL_PERMISSION.Add(new TBL_PERMISSION {
                                                    USERNAME = tblUser.USERNAME ,
                                                    MO_ID = item.CA_ID
                                                });
                                            }
                                        }
                                    }
                                    db.SaveChanges();
                                }
                                mss = "Upload thành công!";

                            }

                            TempData["msg"] = "<script> alert('" + mss + "')</script>";
                        }
                    } catch(Exception e) {
                        Utilities.WriteLogException(e ,"Leave/Upload_Emp");
                        TempData["msg"] = "<script> alert('Error, need contact to IT. " + e.Message + ",  Row " + Convert.ToString(MesRow) + "')</script>";
                    }
                }
            } else {
                TempData["msg"] = "<script>alert('Tài khoản của bạn không thể upload');</script>";
            }


            return RedirectToAction("List_Leave");
        }
        [HttpPost]
        public ActionResult Export(DateTime? ngaybatdau ,DateTime? ngayketthuc) {
            if(ngaybatdau != null && ngayketthuc != null) {
                var data = (from a in db.OL_LeaveDetails
                            join b in db.OL_Leave_Item on a.LeaveID equals b.LeaveID
                            join g in db.OL_User_Approver on b.EmpID equals g.EmpID
                            join u in db.OL_User_Approver on a.ReqName equals u.UserCD
                            where a.AppStatus == 1 && a.EmpSubmit >= ngaybatdau && a.EmpSubmit <= DbFunctions.AddDays(ngayketthuc ,1)
                            select new LeaveExportRequest {
                                EmpID = g.EmpID ,
                                EmpName = g.EmpName ,
                                LeaveCD = b.LeaveCD ,
                                EmpSubmit = a.EmpSubmit.ToString() ,
                                FromDate = b.FromDate ,
                                ToDate = b.ToDate ,
                                LeaveInMorning = b.LeaveInMorning ,
                                Note = b.Reason ,
                                LeaveNo = b.LeaveNo ?? 0 ,
                                cr = u.EmpID
                            }).ToList();
                var ls = new List<LeaveExportRequest>();
                foreach(var variable in data) {
                    variable.EmpSubmit = Utilities.DateFormat(variable.EmpSubmit ,"dd/MM/yyyy HH:mm:ss");
                    ls.Add(variable);
                }

                var excel = new ExcelPackage();
                var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
                workSheet.Cells["A1:I1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                workSheet.Cells["A1:I1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#f2f2f2"));
                workSheet.Cells["A1:I1"].Style.Font.Bold = true;
                workSheet.Column(5).Style.Numberformat.Format = "dd/MM/yyyy";
                workSheet.Column(6).Style.Numberformat.Format = "dd/MM/yyyy";

                workSheet.Cells[1 ,1].Value = "Mã NV";
                workSheet.Cells[1 ,2].Value = "Họ tên";
                workSheet.Cells[1 ,3].Value = "Hình thức";
                workSheet.Cells[1 ,4].Value = "Ngày tạo";
                workSheet.Cells[1 ,5].Value = "Từ ngày";
                workSheet.Cells[1 ,6].Value = "Đến ngày";
                workSheet.Cells[1 ,7].Value = "Ca sáng";
                workSheet.Cells[1 ,8].Value = "Số ngày";
                workSheet.Cells[1 ,9].Value = "Lý do";
                workSheet.Cells[1 ,10].Value = "Người tạo";

                workSheet.Cells[2 ,1].LoadFromCollection(ls ,false);
                using(var col = workSheet.Cells[1 ,1 ,ls.Count() + 1 ,10]) {
                    col.AutoFitColumns();
                    col.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    col.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    col.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    col.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                using(var memoryStream = new MemoryStream()) {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition" ,"attachment;  filename=HR-Leave-Request.xlsx");
                    excel.SaveAs(memoryStream);
                    memoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }

            return RedirectToAction("List_Leave");

        }
        public ActionResult ExportPendingApprove(DateTime? ngaybatdau ,DateTime? ngayketthuc) {
            if(ngaybatdau != null && ngayketthuc != null) {
                var data = (from a in db.OL_LeaveDetails
                            join b in db.OL_Leave_Item on a.LeaveID equals b.LeaveID
                            join g in db.OL_User_Approver on b.EmpID equals g.EmpID
                            join u in db.OL_User_Approver on a.ReqName equals u.UserCD
                            where a.AppStatus == -1 & a.EmpSubmit >= ngaybatdau & a.EmpSubmit <= DbFunctions.AddDays(ngayketthuc ,1)
                            select new LeaveExportRequestPending {
                                EmpID = g.EmpID ,
                                EmpName = g.EmpName ,
                                LeaveCD = b.LeaveCD ,
                                EmpSubmit = a.EmpSubmit.ToString() ,
                                FromDate = b.FromDate ,
                                ToDate = b.ToDate ,
                                LeaveInMorning = b.LeaveInMorning ,
                                LeaveNo = b.LeaveNo ?? 0 ,
                                ApproverName = g.ApproverName ,
                                ApproverEmail = g.ApproverEmail ,
                                status = "Pending" ,
                                Note = b.Reason ,
                                cr = u.EmpID

                            }).ToList();
                var ls = new List<LeaveExportRequestPending>();
                foreach(var variable in data) {
                    variable.EmpSubmit = Utilities.DateFormat(variable.EmpSubmit ,"dd/MM/yyyy HH:mm:ss");
                    ls.Add(variable);
                }

                var excel = new ExcelPackage();
                var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
                workSheet.Cells["A1:L1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                workSheet.Cells["A1:L1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#f2f2f2"));
                workSheet.Cells["A1:L1"].Style.Font.Bold = true;
                workSheet.Column(5).Style.Numberformat.Format = "dd/MM/yyyy";
                workSheet.Column(6).Style.Numberformat.Format = "dd/MM/yyyy";


                workSheet.Cells[1 ,1].Value = "Mã NV";
                workSheet.Cells[1 ,2].Value = "Họ tên";
                workSheet.Cells[1 ,3].Value = "Hình thức";
                workSheet.Cells[1 ,4].Value = "Ngày tạo";
                workSheet.Cells[1 ,5].Value = "Từ ngày";
                workSheet.Cells[1 ,6].Value = "Đến ngày";
                workSheet.Cells[1 ,7].Value = "Ca sáng";
                workSheet.Cells[1 ,8].Value = "Số ngày";
                workSheet.Cells[1 ,9].Value = "Lý do";
                workSheet.Cells[1 ,10].Value = "Người duyệt";
                workSheet.Cells[1 ,11].Value = "Email";
                workSheet.Cells[1 ,12].Value = "Trạng thái";
                workSheet.Cells[1 ,13].Value = "Người tạo";

                workSheet.Cells[2 ,1].LoadFromCollection(ls ,false);
                using(var col = workSheet.Cells[1 ,1 ,ls.Count() + 1 ,13]) {
                    col.AutoFitColumns();
                    col.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    col.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    col.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    col.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                using(var memoryStream = new MemoryStream()) {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition" ,"attachment;  filename=HR-Leave-Request-Pending.xlsx");
                    excel.SaveAs(memoryStream);
                    memoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }

            return RedirectToAction("List_Leave");

        }

        [HttpPost]
        public ActionResult Upload_More(string section ,string approver ,
           string approverMail ,string nameHR ,string mailHR ,string note) {
            if(Request != null) {
                int MesRow = 0;
                var detail = new OL_LeaveDetails();
                try {
                    HttpPostedFileBase file = Request.Files["UploadedFile"];
                    if((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName)) {

                        string nameAndLocation = @"~\log\Upload\" + userLogin.Username + "-Leave-" + DateTime.Now.Ticks + "-" + Path.GetFileName(file.FileName);
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
                            bool balance = true;
                            bool balance1 = true;
                            string Id = "";
                            string Id1 = "";
                            string mss3 = "";
                            for(var rowIterator = 3; rowIterator <= noOfRow; rowIterator++) {
                                if(workSheet.Cells[rowIterator ,1].Value == null )
                                    break;
                                string empid = workSheet.Cells[rowIterator ,1].Value.ToString();
                                var leavecd = workSheet.Cells[rowIterator ,3].Value ?? "";
                                empid = Utilities.ValidEmpID(empid.ToString().Trim());
                                if(!db.OL_User_Approver.Any(a => a.EmpID == empid)) {
                                    mss3 = mss3 + (mss3 != "" ? ", " : "") + empid;
                                    balance = false;
                                    continue;
                                }


                                var ExistBalance = db.OL_LeaveBalance.SingleOrDefault(s => s.EmpID == empid);
                                if(leavecd.ToString().ToUpper().Trim() == "AL2") {
                                    var fromdate = workSheet.Cells[rowIterator ,4].Value ?? "";
                                    var todate = workSheet.Cells[rowIterator ,5].Value ?? "";
                                    var total_date = Utilities.DateDiff(DateTime.Parse(fromdate.ToString().Trim()) ,
                                                         DateTime.Parse(todate.ToString().Trim())) + 1;
                                    if(ExistBalance.UsedLeave + total_date > ExistBalance.TotalLeave) {
                                        balance = false;
                                        Id1 = Id1 + (Id1 != "" ? ", " : "") +
                                              workSheet.Cells[rowIterator ,1].Value.ToString();
                                    }
                                }



                            }

                            if(!balance) {
                                mss = "► Thông tin phép không có, vui lòng liên hệ HR team/Leave info not found, please contact to HR team: " + Id + "<br/>";
                                var ms3 = "► Mã nhân viên không đúng/Employee code is incorrect: " + mss3 + "<br/>";
                                var mss1 = "► Hết số ngày nghỉ phép(AL2)/The number of days off has expired(AL2): " + Id1 + "<br/>";
                                TempData["msg"] = (Id1 != "" ? mss1 : "") + (Id != "" ? mss : "") + (mss3 != "" ? ms3 : "");
                            } else {

                                detail = new OL_LeaveDetails {
                                    ApproverName = approver ,
                                    ApproverEmail = approverMail ,
                                    Dept = userLogin.DeptID ,
                                    Section = section ,
                                    ReqName = userLogin.Username ,
                                    ReqEmail = userLogin.Email ,
                                    AppStatus = -1 ,
                                    HRStatus = -2 ,
                                    HRName = nameHR ,
                                    HREmail = mailHR ,
                                    Notes = "" ,
                                    EmpSubmit = Utilities.GetDate_VietNam(DateTime.Now)
                                };
                                // detail.LeaveID = ma1();
                                db.OL_LeaveDetails.Add(detail);
                                db.SaveChanges();
                                for(var rowIterator = 3; rowIterator <= noOfRow; rowIterator++) {
                                    MesRow = rowIterator;
                                    if(workSheet.Cells[rowIterator ,1].Value == null )
                                        break;

                                    var empid = workSheet.Cells[rowIterator ,1].Value ?? "";
                                    var leavecd = workSheet.Cells[rowIterator ,3].Value ?? "";
                                    var fromdate = workSheet.Cells[rowIterator ,4].Value ?? "";
                                    var todate = workSheet.Cells[rowIterator ,5].Value ?? "";
                                    var leaveno = workSheet.Cells[rowIterator ,6].Value ?? "";
                                    var leavemorn =  workSheet.Cells[rowIterator ,7].Value ?? "";
                                    var reason = workSheet.Cells[rowIterator ,8].Value ?? "";
                                    empid = Utilities.ValidEmpID(empid.ToString().Trim());
                                    var item1 = new OL_Leave_Item {
                                        LeaveID = detail.LeaveID ,
                                        LeaveCD = leavecd.ToString().Trim().ToUpper() ,
                                        Reason = reason.ToString() ,
                                        LeaveNo = double.Parse(leaveno.ToString().Trim()) ,
                                        FromDate = DateTime.Parse(fromdate.ToString().Trim()) ,
                                        ToDate = DateTime.Parse(todate.ToString().Trim()) ,
                                        LeaveInMorning = leavemorn.ToString().Trim() ,
                                        EmpID = empid.ToString().Trim()
                                    };
                                    db.OL_Leave_Item.Add(item1);
                                    var code = db.OL_LeaveCode.Single(s => s.LeaveCD == item1.LeaveCD);

                                    if(code.LeaveCD == "AL2") {
                                        var used = db.OL_LeaveBalance.Single(s => s.EmpID == item1.EmpID);
                                        used.UsedLeave = used.UsedLeave + item1.LeaveNo;
                                        db.Entry(used).State = System.Data.Entity.EntityState.Modified;

                                    }


                                }

                                try {
                                    db.SaveChanges();
                                    TempData["msg"] = "<script> alert('Thành công/Success')</script>";
                                    Utilities.SendEmail(
                                        "Yêu cầu phê duyệt nghỉ số #" + detail.LeaveID +
                                        " / Leave request need your approval" ,userLogin.Email ,detail.ApproverEmail ,
                                        detail.ReqEmail ,
                                        "Dear " + detail.ApproverName + ",<br/><br/>Vui lòng phê duyệt đề nghị nghỉ #" +
                                        detail.LeaveID + ".<br/><span style='color:#0070c0;font-style: italic;'>Please approve or reject leave request #" + detail.LeaveID +
                                        ".</span>");
                                } catch(Exception e) {

                                    if(detail.LeaveID > 0) {
                                        db = new MyContext();

                                        db.OL_LeaveDetails.Remove(db.OL_LeaveDetails.Find(detail.LeaveID));
                                        db.SaveChanges();
                                    }

                                    Utilities.WriteLogException(e ,"Leave/Upload_More");
                                    TempData["msg"] = "<script>alert('Error, Please check the data and retry again. " + e.Message + ",  Row " + Convert.ToString(MesRow) + "')</script>";
                                }

                            }
                        }
                    }
                } catch(Exception e) {
                    db = new MyContext();
                    if(detail.LeaveID > 0) {

                        db.OL_LeaveDetails.Remove(db.OL_LeaveDetails.Find(detail.LeaveID));
                        db.SaveChanges();
                    }
                    Utilities.WriteLogException(e ,"Leave/Upload_More");
                    TempData["msg"] = "<script>alert('Error, Please check the data and retry again. " + e.Message + ",  Row " + Convert.ToString(MesRow) + "')</script>";
                }
            }

            return RedirectToAction("List_Leave");
        }

        //bool ExistBalance(string EmpID)
        //{
        //    if(db.OL_LeaveBalance.SingleOrDefault(s => s.EmpID == EmpID) == null)
        //        return false;
        //    return true;
        //}
    }
}