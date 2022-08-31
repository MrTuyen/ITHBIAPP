using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProductionApp.Helpers;
using ProductionApp.Models;

namespace ProductionApp.Controllers {
    public class LTSController:BaseController {
        #region LTS Online
        public ActionResult Index(int? groupId) {
            ViewBag.fgroupId = groupId;
            ViewBag.userLogin = userLogin;
            ViewBag.Group = db.TBL_WLOT_LOC.Where(w => w.ISSUE_DATE == DateTime.Today).Select(w => w.TBL_GROUP_MST).Where(a => a.Activate == 1).Distinct().OrderBy(a => a.GROUP_NAME).ToList();
            if(Request.Browser.Browser.ToUpper() == "IE" ||
               Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER") {
                ViewBag.mess = "<script>alert('Warning, Bạn không thể sử dụng bằng trình duyệt này!');</script>";
                return View();
            }
            if(groupId == null)
                return View();
            //2:Quản Đốc pendding| -2 quản đốc reject 
            //3:Trưởng BP pendding| -3 Trưởng BP reject
            //4:Kho tạm  pendding| -4 Kho tạm reject
            //5:Kế Toán  pendding| -5 Kế Toán reject
            //var statusApprove = new Dictionary<int?, string>();
            //statusApprove.Add(2, "Quản Đốc pendding");
            //statusApprove.Add(-2, "Quản đốc reject");
            //statusApprove.Add(3, "Trưởng BP pendding");
            //statusApprove.Add(-3, "Trưởng BP reject");
            //statusApprove.Add(4, "Kho tạm  pendding");
            //statusApprove.Add(-4, "Kho tạm reject");
            //statusApprove.Add(5, "Kế Toán  pendding");
            //statusApprove.Add(-5, "Kế Toán reject");

            var groupName = db.TBL_GROUP_MST.Single(a => a.GROUP_ID == groupId).GROUP_NAME;
            var list = (from cl in
                            from l in db.TBL_LTS_NONREC_LOT.Where(a => a.Loc_Name == groupName)
                            select new {
                                l.Comp_WL ,
                                l.Asst_SKU ,
                                l.Asst_WL ,
                                l.Selling_Style ,
                                l.Size ,
                                l.Loc_Name ,
                                l.Rec_Qty ,
                                l.Aging ,
                                l.Issued_date ,

                                FQ_Qty = (from c in db.TBL_CASE_LABEL
                                          where c.WLOT_ID == l.Asst_WL
                                          group c by new { c.QUANTITY ,c.LABEL_ID } into newC
                                          select newC).Sum(c => c.Key.QUANTITY)

                            }
                        group cl by new {
                            cl.Asst_SKU ,
                            cl.Asst_WL ,
                            cl.Selling_Style ,
                            cl.Size ,
                            cl.Loc_Name ,
                            cl.Aging ,
                            cl.FQ_Qty ,
                            cl.Issued_date ,
                        } into newCl

                        select new TBL_LTS_NONREC_LOT_DTO() {
                            Asst_SKU = newCl.Key.Asst_SKU ,
                            Asst_WL = newCl.Key.Asst_WL ,
                            Selling_Style = newCl.Key.Selling_Style ,
                            Size = newCl.Key.Size ,
                            Loc_Name = newCl.Key.Loc_Name ,
                            Rec_Qty = newCl.Sum(c => c.Rec_Qty) ,
                            Aging = newCl.Key.Aging ,
                            Issued_date = newCl.Key.Issued_date ,
                            FQ_Qty = newCl.Key.FQ_Qty ,
                            LTS = Math.Round((double)(newCl.Key.FQ_Qty / newCl.Sum(c => c.Rec_Qty) * 100 * 1.0) ,2) ,
                            // StatusApprove = db.TBL_LTS_Details.Where(a => a.Asst_WL == newCl.Key.Asst_WL).OrderByDescending(a => a.Submitted_date).FirstOrDefault().status ,
                            LtsDetails = db.TBL_LTS_Details.Where(a => a.Asst_WL == newCl.Key.Asst_WL).OrderByDescending(a => a.Submitted_date).FirstOrDefault() ,
                        }).OrderByDescending(a => new { a.LtsDetails.status ,a.Loc_Name }).ToList();

            ViewBag.me = db.TBL_LTS_APPROVE.FirstOrDefault(l => l.Mail.ToLower() == userLogin.Email.ToLower());
            return View(list.Where(c => (c.LtsDetails == null || c.LtsDetails.status < 6)).ToList());
        }
        public ActionResult LoadCompByAsst(string Asst_WL) {
            var list =( from l in db.TBL_LTS_NONREC_LOT
                       where l.Asst_WL == Asst_WL
                       select new TBL_LTS_NONREC_LOT_DTO() {
                           Comp_WL = l.Comp_WL ,
                           Asst_SKU = l.Asst_SKU ,
                           Asst_WL = l.Asst_WL ,
                           Selling_Style = l.Selling_Style ,
                           Size = l.Size ,
                           Loc_Name = l.Loc_Name ,
                           Rec_Qty = l.Rec_Qty ,
                           Aging = l.Aging ,
                           Issued_date = l.Issued_date ,
                           FQ_Qty = (from c in db.TBL_CASE_LABEL
                                     where c.WLOT_ID == l.Asst_WL
                                     group c by new { c.QUANTITY ,c.LABEL_ID }
                                         into newC
                                         select newC).Sum(c => c.Key.QUANTITY)
                       }).ToList();

            var LTS_NONREC = (from cl in
                                  from l in db.TBL_LTS_NONREC_LOT
                                  where l.Asst_WL == Asst_WL
                                  select new {
                                      l.Comp_WL ,
                                      l.Asst_SKU ,
                                      l.Asst_WL ,
                                      l.Selling_Style ,
                                      l.Size ,
                                      l.Loc_Name ,
                                      l.Rec_Qty ,
                                      l.Aging ,
                                      l.Issued_date ,
                                      FQ_Qty = (from c in db.TBL_CASE_LABEL
                                                where c.WLOT_ID == l.Asst_WL
                                                group c by new { c.QUANTITY ,c.LABEL_ID } into newC
                                                select newC).Sum(c => c.Key.QUANTITY)
                                  }
                              group cl by new {
                                  cl.Asst_SKU ,
                                  cl.Asst_WL ,
                                  cl.Selling_Style ,
                                  cl.Size ,
                                  cl.Loc_Name ,
                                  cl.Aging ,
                                  cl.Issued_date ,
                                  cl.FQ_Qty ,
                              } into newCl

                              select new TBL_LTS_NONREC_LOT_DTO() {
                                  Asst_SKU = newCl.Key.Asst_SKU ,
                                  Asst_WL = newCl.Key.Asst_WL ,
                                  Selling_Style = newCl.Key.Selling_Style ,
                                  Size = newCl.Key.Size ,
                                  Loc_Name = newCl.Key.Loc_Name ,
                                  Group = db.TBL_CASE_LABEL.FirstOrDefault(cd => cd.WLOT_ID == db.TBL_LTS_NONREC_LOT.FirstOrDefault(c => c.Asst_WL == newCl.Key.Asst_WL).Comp_WL).TBL_GROUP_MST.GROUP_NAME ,
                                  Rec_Qty = newCl.Sum(c => c.Rec_Qty) ,
                                  Aging = newCl.Key.Aging ,
                                  Issued_date = newCl.Key.Issued_date ,
                                  FQ_Qty = newCl.Key.FQ_Qty ,
                                  LTS = Math.Round((double)(newCl.Key.FQ_Qty / newCl.Sum(c => c.Rec_Qty) * 100 * 1.0) ,2)
                              }).FirstOrDefault();
            ViewBag.LTS_NONREC = LTS_NONREC;
            ViewBag.Approve = db.TBL_LTS_APPROVE.Where(l => l.MemberOf == "QD").OrderBy(l => l.Name).ToList();

            return PartialView("_LoadCompByAsst" ,list);
        }
        public JsonResult LoadRate(string Asst_WL ,double FQ_Qty ,double Rec_Qty ,string requestLTS) {

            //Comp_WL;Odd;IRR;ThrowOut;Sample;Rec;FQ|Comp_WL;Odd;IRR;ThrowOut;Sample;Rec;FQ
            var tmp = requestLTS.Split('|');
            foreach(var item in tmp) {
                var tmp1 = item.Split(';');
                FQ_Qty += Utilities.ValidDouble(tmp1[1]) + Utilities.ValidDouble(tmp1[2]) + Utilities.ValidDouble(tmp1[3]) + Utilities.ValidDouble(tmp1[4]);
            }
            return Json(Math.Round(FQ_Qty / Rec_Qty * 100 ,2) ,JsonRequestBehavior.AllowGet);
        }

        public JsonResult LtsConfirm(string Asst_WL ,double Rec_Qty ,double FQ_Qty ,string requestLTS) {
            try {
                var ltsNonrec = db.TBL_LTS_NONREC_LOT.FirstOrDefault(a => a.Asst_WL == Asst_WL);
                var rate = 100;
                var datenow = Utilities.GetDate_VietNam(DateTime.Now);
                var firstLabel = db.TBL_CASE_LABEL.FirstOrDefault(a => a.WLOT_ID == Asst_WL) ?? new TBL_CASE_LABEL();
                var ltsDetail = new TBL_LTS_Details() {
                    Asst_WL = Asst_WL ,
                    Group_Name = firstLabel.TBL_GROUP_MST != null ? firstLabel.TBL_GROUP_MST.GROUP_NAME : "" ,
                    Rec_Qty = Rec_Qty ,
                    FQ_Qty = FQ_Qty ,
                    status = 2 ,
                    Submitted_date = datenow ,
                    CreateBy = userLogin.Email ,
                };
                db.TBL_LTS_Details.Add(ltsDetail);
                db.SaveChanges();

                var tmp = requestLTS.Split('|');
                foreach(var item in tmp) {
                    var tmp1 = item.Split(';');
                    if(tmp[0] != "") {
                        if(ltsNonrec != null) {

                            var LtsItem= new TBL_LTS_Items() {
                                Comp_WL = tmp1[0] ,
                                LtsID = ltsDetail.ID ,

                                //Irr_Qty = Utilities.ValidDouble(tmp1[2]) ,
                                //Odd_Qty = Utilities.ValidDouble(tmp1[1]) ,
                                //Sample_Qty = Utilities.ValidDouble(tmp1[4]) ,
                                //ThrowOut_Qty = Utilities.ValidDouble(tmp1[3]) ,

                                Selling_Style = ltsNonrec.Selling_Style ,
                                Size = ltsNonrec.Size ,
                                PkgStyle = firstLabel.PkgStyle ?? "" ,
                                Packing_Color = "" ,
                            };
                            db.TBL_LTS_Items.Add(LtsItem);

                        }
                    }
                }
                db.SaveChanges();
                //var body = string.Format("Dear {0}, " +
                //                                        "<br/>Please follow up next steps for LTS#{1}(<a style='color:blue' href='{4}'>Click this link</a>)" +
                //                                        "<br/>- Requested on: {3}  " +
                //                                        "<br/>- Requested by: {2} " +
                //                                        "<br/>" ,
                //                   prod.Name ,
                //                   ltsDetail.ID ,
                //                   ((UserModels)Session["SignedInUser"]).Fullname ,
                //                   DateTime.Now ,
                //                   db.TBL_SYSTEM.First(a => a.id == "website").value + string.Format("scanCase/Ltsapprove?ltsid={0}" ,ltsDetail.ID)
                //                   );

                ////send mail
                //Utilities.SendEmail("HYC-LTS#" + ltsDetail.ID ,userLogin.Email ,prod.Mail ,userLogin.Email ,body);
                return Json("Thành công" ,JsonRequestBehavior.AllowGet);
            } catch(Exception e) {
                Utilities.WriteLogException(e ,"/Scancase/LtsSubmit");
                return Json("Lỗi hệ thống, Vui lòng liên hệ quản trị viên." ,JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult LtsSubmit(int Approve ,double Rec_Qty ,double FQ_Qty ,string Asst_WL ,string requestLTS) {
            try {
                var ltsNonrec = db.TBL_LTS_NONREC_LOT.FirstOrDefault(a => a.Asst_WL == Asst_WL);


                //Comp_WL;Odd;IRR;ThrowOut;Sample;FQ|Comp_WL;Odd;IRR;ThrowOut;Sample;FQ
                var tmp = requestLTS.Split('|');
                var rate = Math.Round((FQ_Qty / Rec_Qty * 100 * 1.0) ,2);
                var prod = db.TBL_LTS_APPROVE.First(l => l.id == Approve);
                var odd = db.TBL_LTS_APPROVE.First(l => l.MemberOf == "KhoTam");
                var ia = db.TBL_LTS_APPROVE.First(l => l.MemberOf == "Finance");
                var mgr = db.TBL_LTS_APPROVE.First(l => l.id == prod.Manager);
                var datenow = Utilities.GetDate_VietNam(DateTime.Now);
                var firstLabel = db.TBL_CASE_LABEL.FirstOrDefault(a => a.WLOT_ID == Asst_WL) ?? new TBL_CASE_LABEL();
                var ltsDetail = new TBL_LTS_Details() {
                    Asst_WL = Asst_WL ,
                    Group_Name = firstLabel.TBL_GROUP_MST != null ? firstLabel.TBL_GROUP_MST.GROUP_NAME : "" ,
                    Rec_Qty = Rec_Qty ,
                    FQ_Qty = FQ_Qty ,
                    //pending quandoc
                    status = 2 ,

                    Prod_PIC = Approve ,
                    Prod_Status = "Pending" ,
                    Prod_mail = prod.Mail ,

                    Odd_PIC = odd.id ,
                    Odd_Status = "Pending" ,
                    Odd_mail = odd.Mail ,

                    IA_PIC = ia.id ,
                    IA_Status = "Pending" ,
                    IA_mail = ia.Mail ,

                    PM_PIC = mgr.id ,
                    PM_Status = rate > 100 && rate <= 103 ? "None" : "Pending" ,
                    PM_mail = mgr.Mail ,

                    Submitted_date = datenow ,
                    CreateBy = userLogin.Email ,
                };
                db.TBL_LTS_Details.Add(ltsDetail);
                db.SaveChanges();


                foreach(var item in tmp) {
                    var tmp1 = item.Split(';');
                    if(tmp1[0] != "") {
                        if(ltsNonrec != null) {

                            var LtsItem= new TBL_LTS_Items() {
                                Comp_WL = tmp1[0] ,
                                LtsID = ltsDetail.ID ,

                                Irr_Qty = Utilities.ValidDouble(tmp1[2]) ,
                                Odd_Qty = Utilities.ValidDouble(tmp1[1]) ,
                                Sample_Qty = Utilities.ValidDouble(tmp1[4]) ,
                                ThrowOut_Qty = Utilities.ValidDouble(tmp1[3]) ,

                                Selling_Style = ltsNonrec.Selling_Style ,
                                Size = ltsNonrec.Size ,
                                PkgStyle = firstLabel.PkgStyle ?? "" ,
                                Packing_Color = "" ,
                            };
                            db.TBL_LTS_Items.Add(LtsItem);

                        }
                    }
                }
                db.SaveChanges();
                //var body = string.Format("Dear {0}, " +
                //                                        "<br/>Please follow up next steps for LTS#{1}(<a style='color:blue' href='{4}'>Click this link</a>)" +
                //                                        "<br/>- Requested on: {3}  " +
                //                                        "<br/>- Requested by: {2} " +
                //                                        "<br/>" ,
                //                   prod.Name ,
                //                   ltsDetail.ID ,
                //                   ((UserModels)Session["SignedInUser"]).Fullname ,
                //                   DateTime.Now ,
                //                   db.TBL_SYSTEM.First(a => a.id == "website").value + string.Format("scanCase/Ltsapprove?ltsid={0}" ,ltsDetail.ID)
                //                   );

                ////send mail
                //Utilities.SendEmail("HYC-LTS#" + ltsDetail.ID ,userLogin.Email ,prod.Mail ,userLogin.Email ,body);
                return Json("Thành công" ,JsonRequestBehavior.AllowGet);
            } catch(Exception e) {
                Utilities.WriteLogException(e ,"/Scancase/LtsSubmit");
                return Json("Lỗi hệ thống, Vui lòng liên hệ quản trị viên." ,JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult LtsApprove(int ltsid) {
            var LTS_Details = db.TBL_LTS_Details.SingleOrDefault(a => a.ID == ltsid) ?? new TBL_LTS_Details();

            if(userLogin == null) {
                return RedirectToAction("Login" ,"Account");
            }
            ViewBag.LTS_Details = LTS_Details;
            ViewBag.ltskey = userLogin.Email.ToLower();
            ViewBag.ltsid = ltsid;
            // ViewBag.Approve = db.TBL_LTS_APPROVE.Where(l => l.MemberOf == "QD").OrderBy(l => l.Name).ToList();
            return View(LTS_Details);
        }
        public JsonResult LtsApproveSubmit(int ltsNumber ,string ac ,int sta ,string ReasonReject) {
            try {
                var lts = db.TBL_LTS_Details.SingleOrDefault(l => l.ID == ltsNumber);

                var mailTitle = "";
                var mailBody = "";
                var mailTo = "";
                var mailCC = "";
                var prod = db.TBL_LTS_APPROVE.First(l => l.id == lts.Prod_PIC);
                var odd = db.TBL_LTS_APPROVE.First(l => l.MemberOf == "KhoTam");
                var ia = db.TBL_LTS_APPROVE.First(l => l.MemberOf == "Finance");
                var mgr = db.TBL_LTS_APPROVE.First(l => l.id == prod.Manager);


                //2:Quản Đốc pendding| -2 quản đốc reject 
                //3:Trưởng BP pendding| -3 Trưởng BP reject
                //4:Kho tạm  pendding| -4 Kho tạm reject
                //5:Kế Toán  pendding| -5 Kế Toán reject
                //6 done

                switch(sta) {//-2 quản đốc reject 
                    case -2: {
                            lts.Prod_Status = "Reject";
                            lts.Prod_Date = DateTime.Now;
                            lts.status = sta;
                            lts.ReasonReject = ReasonReject;
                            mailTo = lts.TBL_USERS_MST.EMAIL;
                            mailTitle = string.Format("LTS#{0} has been rejected" ,ltsNumber);
                            mailBody = string.Format("Dear {0}, " +
                                                     "<br/>Please follow LTS#{1}(<a style='color:blue' href='{4}'>Click this link</a>)" +
                                                     "<br/>- Requested on: {3}  " +
                                                     "<br/>- Requested by: {2} " +
                                                     "<br/>- Reject by: {5} " +
                                                     "<br/><b>*Reason</b>: {6} " +
                                                     "<br/>" ,
                                lts.TBL_USERS_MST.FULLNAME ,
                                ltsNumber ,
                                lts.TBL_USERS_MST.FULLNAME ,
                                lts.Submitted_date ,
                                db.TBL_SYSTEM.First(a => a.id == "website").value + string.Format("scanCase/Ltsapprove?ltsid={0}" ,ltsNumber) ,
                                prod.Name + "(" + prod.Mail + ")" ,
                                ReasonReject
                            );

                            break;
                        }
                    //quản đốc approve 
                    case 3: {

                            if(lts.PM_Status == "None") {
                                //rate > 100 && rate <= 103 
                                sta = 4;
                                lts.Prod_Status = "Approve";
                                lts.Prod_Date = DateTime.Now;
                                lts.status = sta;
                                mailTo = odd.Mail;
                                mailTitle = string.Format("LTS#{0} for your confirm" ,ltsNumber);
                                mailBody = string.Format("Dear {0}, " +
                                                         "<br/>Please follow up next steps for LTS#{1}(<a style='color:blue' href='{4}'>Click this link</a>)" +
                                                         "<br/>- Requested on: {3}  " +
                                                         "<br/>- Requested by: {2} " +
                                                         "<br/>- Approve by: {5} " +
                                                         "<br/>- Approve by: {6} " +
                                                         "<br/>" ,
                                    odd.Name ,
                                    ltsNumber ,
                                    lts.TBL_USERS_MST.FULLNAME ,
                                    lts.Submitted_date ,
                                    db.TBL_SYSTEM.First(a => a.id == "website").value + string.Format("scanCase/Ltsapprove?ltsid={0}" ,ltsNumber) ,
                                    prod.Name + "(" + prod.Mail + ")" ,
                                    mgr.Name + "(" + mgr.Mail + ")"
                                );

                            } else {
                                lts.Prod_Status = "Approve";
                                lts.Prod_Date = DateTime.Now;
                                lts.status = sta;
                                mailTo = mgr.Mail;
                                mailTitle = string.Format("LTS#{0} for your approval" ,ltsNumber);
                                mailBody = string.Format("Dear {0}, " +
                                                         "<br/>Please follow up next steps for LTS#{1}(<a style='color:blue' href='{4}'>Click this link</a>)" +
                                                         "<br/>- Requested on: {3}  " +
                                                         "<br/>- Requested by: {2} " +
                                                         "<br/>- Approve by: {5} " +
                                                         "<br/>" ,
                                    mgr.Name ,
                                    ltsNumber ,
                                    lts.TBL_USERS_MST.FULLNAME ,
                                    lts.Submitted_date ,
                                    db.TBL_SYSTEM.First(a => a.id == "website").value + string.Format("scanCase/Ltsapprove?ltsid={0}" ,ltsNumber) ,
                                    prod.Name + "(" + prod.Mail + ")"
                                    );

                            }


                            break;
                        }
                    // -3 Trưởng BP reject
                    case -3: {

                            lts.PM_Status = "Reject";
                            lts.PM_Date = DateTime.Now;
                            lts.status = sta;
                            lts.ReasonReject = ReasonReject;


                            mailTo = lts.TBL_USERS_MST.EMAIL;
                            mailTitle = string.Format("LTS#{0} has been rejected" ,ltsNumber);
                            mailBody = string.Format("Dear {0}, " +
                                                     "<br/>Please follow LTS#{1}(<a style='color:blue' href='{4}'>Click this link</a>)" +
                                                     "<br/>- Requested on: {3}  " +
                                                     "<br/>- Requested by: {2} " +
                                                     "<br/>- Approve by: {5} " +
                                                     "<br/>- Reject by: {6} " +
                                                     "<br/><b>*Reason</b>: {7} " +
                                                     "<br/>" ,
                                lts.TBL_USERS_MST.FULLNAME ,
                                ltsNumber ,
                                lts.TBL_USERS_MST.FULLNAME ,
                                lts.Submitted_date ,
                                db.TBL_SYSTEM.First(a => a.id == "website").value + string.Format("scanCase/Ltsapprove?ltsid={0}" ,ltsNumber) ,
                                prod.Name + "(" + prod.Mail + ")" ,
                                mgr.Name + "(" + mgr.Mail + ")" ,
                                ReasonReject
                            );

                            break;
                        }
                    // 4 Trưởng BP approve
                    case 4: {

                            lts.PM_Status = "Approve";
                            lts.PM_Date = DateTime.Now;
                            lts.status = sta;
                            mailTo = odd.Mail;
                            mailTitle = string.Format("LTS#{0} for your confirm" ,ltsNumber);
                            mailBody = string.Format("Dear {0}, " +
                                                     "<br/>Please follow up next steps for LTS#{1}(<a style='color:blue' href='{4}'>Click this link</a>)" +
                                                     "<br/>- Requested on: {3}  " +
                                                     "<br/>- Requested by: {2} " +
                                                     "<br/>- Approve by: {5} " +
                                                     "<br/>- Approve by: {6} " +
                                                     "<br/>" ,
                                odd.Name ,
                                ltsNumber ,
                               lts.TBL_USERS_MST.FULLNAME ,
                                lts.Submitted_date ,
                                db.TBL_SYSTEM.First(a => a.id == "website").value + string.Format("scanCase/Ltsapprove?ltsid={0}" ,ltsNumber) ,
                                prod.Name + "(" + prod.Mail + ")" ,
                                mgr.Name + "(" + mgr.Mail + ")"
                            );

                            break;
                        }
                    //-4 Kho tạm reject
                    case -4: {

                            lts.Odd_Status = "Reject";
                            lts.Odd_Date = DateTime.Now;
                            lts.status = sta;
                            lts.ReasonReject = ReasonReject;

                            mailTo = lts.TBL_USERS_MST.EMAIL;
                            mailTitle = string.Format("LTS#{0} has been rejected" ,ltsNumber);
                            mailBody = string.Format("Dear {0}, " +
                                                     "<br/>Please follow LTS#{1}(<a style='color:blue' href='{4}'>Click this link</a>)" +
                                                     "<br/>- Requested on: {3}  " +
                                                     "<br/>- Requested by: {2} " +
                                                     "<br/>- Approve by: {5} " +
                                                     "<br/>- Approve by: {6} " +
                                                     "<br/>- Reject by: {7} " +
                                                     "<br/><b>*Reason</b>: {8} " +
                                                     "<br/>" ,
                                lts.TBL_USERS_MST.FULLNAME ,
                                ltsNumber ,
                                lts.TBL_USERS_MST.FULLNAME ,
                                lts.Submitted_date ,
                                db.TBL_SYSTEM.First(a => a.id == "website").value + string.Format("scanCase/Ltsapprove?ltsid={0}" ,ltsNumber) ,
                                prod.Name + "(" + prod.Mail + ")" ,
                                mgr.Name + "(" + mgr.Mail + ")" ,
                                odd.Name + "(" + odd.Mail + ")" ,
                                ReasonReject
                            );
                            break;
                        }
                    //5 Kho tạm approve
                    case 5: {

                            lts.Odd_Status = "Confirm";
                            lts.Odd_Date = DateTime.Now;
                            lts.status = sta;


                            mailTo = ia.Mail;
                            mailTitle = string.Format("LTS#{0} for your process" ,ltsNumber);
                            mailBody = string.Format("Dear {0}, " +
                                                     "<br/>Please follow up next steps for LTS#{1}(<a style='color:blue' href='{4}'>Click this link</a>)" +
                                                     "<br/>- Requested on: {3}  " +
                                                     "<br/>- Requested by: {2} " +
                                                     "<br/>- Approve by: {5} " +
                                                     "<br/>- Approve by: {6} " +
                                                     "<br/>- Confirm by: {7} " +
                                                     "<br/>" ,
                                ia.Name ,
                                ltsNumber ,
                                lts.TBL_USERS_MST.FULLNAME ,
                                lts.Submitted_date ,
                                db.TBL_SYSTEM.First(a => a.id == "website").value + string.Format("scanCase/Ltsapprove?ltsid={0}" ,ltsNumber) ,
                                prod.Name + "(" + prod.Mail + ")" ,
                                mgr.Name + "(" + mgr.Mail + ")" ,
                                odd.Name + "(" + odd.Mail + ")"
                            );
                            break;
                        }
                    //-5 Kế Toán reject
                    case -5: {

                            lts.IA_Status = "Reject";
                            lts.IA_Date = DateTime.Now;
                            lts.status = sta;
                            lts.ReasonReject = ReasonReject;

                            mailTo = lts.TBL_USERS_MST.EMAIL;
                            mailTitle = string.Format("LTS#{0} has been rejected" ,ltsNumber);
                            mailBody = string.Format("Dear {0}, " +
                                                     "<br/>Please follow LTS#{1}(<a style='color:blue' href='{4}'>Click this link</a>)" +
                                                     "<br/>- Requested on: {3}  " +
                                                     "<br/>- Requested by: {2} " +
                                                     "<br/>- Approve by: {5} " +
                                                     "<br/>- Approve by: {6} " +
                                                     "<br/>- Confirm by: {7} " +
                                                     "<br/>- Reject by: {8} " +
                                                     "<br/><b>*Reason</b>: {9} " +
                                                     "<br/>" ,
                                lts.TBL_USERS_MST.FULLNAME ,
                                ltsNumber ,
                                lts.TBL_USERS_MST.FULLNAME ,
                                lts.Submitted_date ,
                                db.TBL_SYSTEM.First(a => a.id == "website").value + string.Format("scanCase/Ltsapprove?ltsid={0}" ,ltsNumber) ,
                                prod.Name + "(" + prod.Mail + ")" ,
                                mgr.Name + "(" + mgr.Mail + ")" ,
                                odd.Name + "(" + odd.Mail + ")" ,
                                ia.Name + "(" + ia.Mail + ")" ,
                                ReasonReject
                            );
                            break;
                        }
                    //6 Kế Toán approve
                    case 6: {

                            lts.IA_Status = "Process";
                            lts.IA_Date = DateTime.Now;
                            lts.status = sta;




                            mailTo = lts.TBL_USERS_MST.EMAIL;
                            mailTitle = string.Format("LTS#{0} is process" ,ltsNumber);
                            mailBody = string.Format("Dear {0}, " +
                                                     "<br/>Please follow LTS#{1}(<a style='color:blue' href='{4}'>Click this link</a>)" +
                                                     "<br/>- Requested on: {3}  " +
                                                     "<br/>- Requested by: {2} " +
                                                     "<br/>- Approve by: {5} " +
                                                     "<br/>- Approve by: {6} " +
                                                     "<br/>- Confirm by: {7} " +
                                                     "<br/>- Process by: {8} " +
                                                     "<br/>" ,
                                lts.TBL_USERS_MST.FULLNAME ,
                               ltsNumber ,
                                lts.TBL_USERS_MST.FULLNAME ,
                                lts.Submitted_date ,
                                db.TBL_SYSTEM.First(a => a.id == "website").value + string.Format("scanCase/Ltsapprove?ltsid={0}" ,ltsNumber) ,
                                prod.Name + "(" + prod.Mail + ")" ,
                                mgr.Name + "(" + mgr.Mail + ")" ,
                                odd.Name + "(" + odd.Mail + ")" ,
                                ia.Name + "(" + ia.Mail + ")"
                            );
                            break;
                        }

                }

                db.SaveChanges();
              //  Utilities.SendEmail(mailTitle ,userLogin.Email ,mailTo ,mailCC ,mailBody);
                return Json("Thành công" ,JsonRequestBehavior.AllowGet);
            } catch(Exception e) {
                Utilities.WriteLogException(e ,"/Scancase/LtsApproveSubmit");
                return Json("Lỗi hệ thống, Vui lòng liên hệ quản trị viên." ,JsonRequestBehavior.AllowGet);
            }
        }
        //private String RenderLtsNumber() {
        //    try {
        //        var pallet = db.TBL_SYSTEM.Single(s => s.id == "LtsOnline");
        //        pallet.value = (int.Parse(pallet.value) + 1).ToString();
        //        db.SaveChanges();
        //        return DateTime.Now.Year.ToString().Substring(2) + "000000".Substring(pallet.value.Length) + pallet.value;
        //    } catch(Exception e) {
        //        Utilities.WriteLogException(e ,"/Scancase/RenderLtsNumber");
        //        throw;
        //    }
        //}

        #endregion
    }
}