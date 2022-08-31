//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;
//using ProductionApp.Models;
//using System.IO;

//namespace ProductionApp.Controllers
//{

//    public class CreateECNController : Controller
//    {
//        ECNEntities db = new ECNEntities();
//        // GET: CreateECN
//        //public ActionResult Index()
//        //{
//        //    ECNDetails oneECN = new ECNDetails();

//        //    //get list ECN Types
//        //    IEnumerable<PROC_GET_LIST_ECN_TYPES_Result> ECNTypes = (from item in db.GetListECNTypes() select item);
//        //    ViewBag.ecntypes = new SelectList(ECNTypes, "ID", "TYPE_NAME"); 

//        //    //get list Products
//        //    IEnumerable<PROC_GET_LIST_PROD_Result> products = (from item in db.GetListProducts() select item);
//        //    ViewBag.AffectedProducts = new SelectList(products, "ID", "PROD_NAME");

//        //    //get list material change type
//        //    IEnumerable<PROC_GET_MAT_CHANGE_TYPE_Result> changeTypes = (from item in db.GetListMaterialChangeTypes() select item);
//        //    ViewBag.MaterialChangeType = new SelectList(changeTypes, "ID", "DESC");

//        //    //get list phases
//        //    List<TBL_AFFECTED_PHASES> phases = (from item in db.TBL_AFFECTED_PHASES select item).ToList();
//        //    oneECN.lsPhases = phases;

//        //    //get list docs
//        //    List<TBL_AFFECTED_DOCS> docs = (from item in db.TBL_AFFECTED_DOCS select item).ToList();
//        //    oneECN.lsDocs = docs;

//        //    //get list SP
//        //    List<TBL_SPLEVEL_MST> sp = (from item in db.TBL_SPLEVEL_MST select item).ToList();
//        //    oneECN.lsSP = sp;

//        //    //get list material infor
//        //    string ecnNO = GenerateECN();
//        //    List<PROC_GET_ECN_MATERIAL_INFOR_Result> mats = (from item in db.GetECNMaterialInfor(ecnNO) select item).ToList();
//        //    oneECN.lsMaterialInfor = mats;

//        //    oneECN.ecnNo = ecnNO;

//        //    return View(oneECN);
//        //}

//        public ActionResult Index()
//        {
//            ViewECNModel oneECN = new ViewECNModel();
//            oneECN.MaterialInforModel = new ListMaterialInfor();
//            oneECN.ECNInforModel = new ECNDetails();

//            //get list ECN Types
//            IEnumerable<PROC_GET_LIST_ECN_TYPES_Result> ECNTypes = (from item in db.GetListECNTypes() select item);
//            ViewBag.ecntypes = new SelectList(ECNTypes, "ID", "TYPE_NAME");

//            //get list Products
//            IEnumerable<PROC_GET_LIST_PROD_Result> products = (from item in db.GetListProducts() select item);
//            ViewBag.AffectedProducts = new SelectList(products, "ID", "PROD_NAME");

//            //get list material change type
//            IEnumerable<PROC_GET_MAT_CHANGE_TYPE_Result> changeTypes = (from item in db.GetListMaterialChangeTypes() select item);
//            ViewBag.MaterialChangeType = new SelectList(changeTypes, "ID", "DESC");

//            //get list phases
//            List<TBL_AFFECTED_PHASES> phases = (from item in db.TBL_AFFECTED_PHASES select item).ToList();
//            oneECN.ECNInforModel.lsPhases = phases;

//            //get list docs
//            List<TBL_AFFECTED_DOCS> docs = (from item in db.TBL_AFFECTED_DOCS select item).ToList();
//            oneECN.ECNInforModel.lsDocs = docs;

//            //get list SP
//            List<TBL_SPLEVEL_MST> sp = (from item in db.TBL_SPLEVEL_MST select item).ToList();
//            oneECN.ECNInforModel.lsSP = sp;

//            //get ECNNo
//            string ecnNO = GenerateECN();

//            //get list material infor            
//            List<PROC_GET_ECN_MATERIAL_INFOR_Result> mats = (from item in db.GetECNMaterialInfor(ecnNO) select item).ToList();
//            oneECN.MaterialInforModel.lsMaterialInfor = mats;

//            //get list approvers
//            IEnumerable<PROC_GET_LIST_APPROVERS_Result> approvers = (from item in db.GetListApprovers() select item);
//            ViewBag.lsApprovers = new SelectList(approvers, "person", "fullname");

//            oneECN.ECNInforModel.ecnNo = ecnNO;

//            return View(oneECN);
//        }

//        private void SendEmailToApprovers(string ecn, string persons)
//        {
//                List<string> list = persons.Split(',').ToList();
//                List<string> listEmailAppr = new List<string>();
//                List<string> listUserNameAppr = new List<string>();

//                //get list email of approvers
//                for (int j = 0; j < list.Count(); j++)
//                {
//                    listUserNameAppr.Add(list[j].Split(';')[0]);
//                    listEmailAppr.Add(list[j].Split(';')[1]);
//                }


//                //add data into TBL_ECN_APPR_HST
//                for (int i = 0; i < listUserNameAppr.Count(); i++)
//                {
//                    TBL_ECN_APPR_HST newApproval = new TBL_ECN_APPR_HST();
//                    newApproval.ECN = ecn;
//                    newApproval.USERNAME = listUserNameAppr[i];
//                    newApproval.ISAPPROVED = "0";
//                    db.TBL_ECN_APPR_HST.Add(newApproval);
//                    db.SaveChanges();
//                }

//                //send email to list approvers
//                string subject = "NEW ECN - NO REPLY EMAIL";
//                string content = "<!DOCTYPE html> <html> <body><p> Click Here To Check:<a href = " + "ECN/ViewDistribute/" + ecn + ">ECN " + ecn + "</a> </p></body></ html >";
//                SendMailExtension.SendMail(listEmailAppr, subject, content);
//        }

//        [HttpPost]
//        public ActionResult AddNew(string ecn, string pdm, string inNO, int types, string title, string period, DateTime planImp, string desc, string affProd,string phases,string docs,string sp, string persons)
//        {
//            try
//            {
//                if (ModelState.IsValid)
//                {
//                    //add new record into TBL_ECN
//                    TBL_ECN newECN = new TBL_ECN();
//                    newECN.ECN_NO = ecn;
//                    newECN.PDM_NO = pdm;
//                    newECN.IN_NO = inNO;
//                    newECN.ECN_TYPE = types;
//                    newECN.TITLE = title;
//                    newECN.ECN_PERIOD = period;
//                    newECN.PLAN_IMP = planImp;
//                    newECN.DESC = desc;
//                    newECN.CREATED_BY = GetUsersLoggingIn().UserName;
//                    newECN.CREATED_TS = DateTime.Now;
//                    db.TBL_ECN.Add(newECN);
//                    db.SaveChanges();

//                    //add affected Phases into TBL_ECN_PHASE
//                    AddAffectedPhases(ecn, phases);

//                    //ADD AFFECTED DOCS INTO TBL_ECN_DOCS
//                    AddAffectedDocs(ecn, docs);

//                    //ADD AFFECTED PRODUCTS INTO TBL_ECN_PRODUCT
//                    AddAffectedProd(ecn, affProd);

//                    //ADD AFFECTED SALEPACK LEVEL INTO TBL_ECN_SPLEVEL
//                    AddAffectedSP(ecn, sp);

//                    //send email
//                    SendEmailToApprovers(ecn, persons);

//                    return Json("New ECN has been created successfully. ", JsonRequestBehavior.AllowGet);
//                }
//                else {
//                    return Json("Unable to save changes. ", JsonRequestBehavior.AllowGet);
//                }
//            }
//            catch (Exception ex)
//            {
//                return Json(ex.Message, JsonRequestBehavior.AllowGet);
//            }
//        }

//        private void AddAffectedSP(string ecn, string sp)
//        {
//            string[] list = sp.Split(',');
//            for (int i = 0; i < list.Count(); i++)
//            {
//                TBL_ECN_SPLEVEL newRecord = new TBL_ECN_SPLEVEL();
//                newRecord.ECN_NO = ecn;
//                newRecord.AFF_SPLEVEL_ID = Int32.Parse(list[i]);
//                db.TBL_ECN_SPLEVEL.Add(newRecord);
//                db.SaveChanges();
//            }
             
//        }

//        private void AddAffectedProd(string ecn, string affProd)
//        {
//            string[] list = affProd.Split(',');
//            for (int i = 0; i < list.Count(); i++)
//            {
//                TBL_ECN_PRODUCT newRecord = new TBL_ECN_PRODUCT();
//                newRecord.ECN_NO = ecn;
//                newRecord.AFF_PROD_ID = Int32.Parse(list[i]);
//                db.TBL_ECN_PRODUCT.Add(newRecord);
//                db.SaveChanges();
//            }
//        }

//        private void AddAffectedDocs(string ecn, string docs)
//        {
//            string[] list = docs.Split(',');
//            for (int i = 0; i < list.Count(); i++)
//            {
//                TBL_ECN_DOCS newRecord = new TBL_ECN_DOCS();
//                newRecord.ECN_NO = ecn;
//                newRecord.AFF_DOCS_ID = Int32.Parse(list[i]);
//                db.TBL_ECN_DOCS.Add(newRecord);
//                db.SaveChanges();
//            }
//        }

//        private void AddAffectedPhases(string ecn, string phases)
//        {
//            string[] list = phases.Split(',');
//            for (int i = 0; i < list.Count(); i++)
//            {
//                TBL_ECN_PHASE newRecord = new TBL_ECN_PHASE();
//                newRecord.ECN_NO = ecn;
//                newRecord.AFF_PHASE_ID = Int32.Parse(list[i]);
//                db.TBL_ECN_PHASE.Add(newRecord);
//                db.SaveChanges();
//            }
//        }

//        [HttpPost]
//        public ActionResult AddMatChangeType(string ecn, int type, string matCode, string matName, decimal currAmount, decimal changedAmount, string filePath)
//        {
//            try
//            {
//                if (ModelState.IsValid)
//                {
//                    //save files onto server, path is: /App_Data/SemiProdFile
//                    var fileName = Path.GetFileName(filePath);
//                    var path = Path.Combine(Server.MapPath("~/App_Data/SemiProdFile"), fileName);          


//                    //add new record into TBL_ECN_MAT
//                    TBL_ECN_MAT newRecord = new TBL_ECN_MAT();
//                    newRecord.ECN_NO = ecn;
//                    newRecord.MAT_CHANGE_TYPE = type;
//                    newRecord.MAT_CODE = matCode;
//                    newRecord.MAT_NAME = matName;
//                    newRecord.CURR_AMOUNT = currAmount;
//                    newRecord.CHANGE_AMOUNT = changedAmount;
//                    newRecord.UPDATE_USER = GetUsersLoggingIn().UserName;
//                    newRecord.UPDATE_TS = DateTime.Now;
//                    newRecord.MAT_INFOR_FILE = path;
//                    db.TBL_ECN_MAT.Add(newRecord);
//                    db.SaveChanges();

//                    return Json("Changes been saved successfully. ", JsonRequestBehavior.AllowGet);
//                }
//                else {
//                    return Json("Unable to save changes. ", JsonRequestBehavior.AllowGet);
//                }
//            }
//            catch (Exception ex)
//            {
//                return Json(ex.Message, JsonRequestBehavior.AllowGet);
//            }
//        }

//        public ActionResult ShowMaterialChange(string ecn)
//        {
//            List<PROC_GET_ECN_MATERIAL_INFOR_Result> dbList = GetListMaterialChange(ecn);
//            ViewECNModel ls = new ViewECNModel();
//            ls.MaterialInforModel = new ListMaterialInfor();
//            ls.MaterialInforModel.lsMaterialInfor = dbList;

//            //get list material change type
//            IEnumerable<PROC_GET_MAT_CHANGE_TYPE_Result> changeTypes = (from item in db.GetListMaterialChangeTypes() select item);
//            ViewBag.MaterialChangeType = new SelectList(changeTypes, "ID", "DESC");

//            return PartialView("ViewMaterialChange", ls);
//        }

//        //[ChildActionOnly]
//        //public ActionResult ViewMaterialChange(string ecn)
//        //{
//        //    List<PROC_GET_ECN_MATERIAL_INFOR_Result> dbList = GetListMaterialChange(ecn);
//        //    //ECNDetails ls = new ECNDetails();
//        //    ViewECNModel ls = new ViewECNModel();
//        //    ls.MaterialInforModel = new ListMaterialInfor();
//        //    ls.MaterialInforModel.lsMaterialInfor = dbList;

//        //    //get list material change type
//        //    IEnumerable<PROC_GET_MAT_CHANGE_TYPE_Result> changeTypes = (from item in db.GetListMaterialChangeTypes() select item);
//        //    ViewBag.MaterialChangeType = new SelectList(changeTypes, "ID", "DESC");

//        //    return PartialView(ls);
//        //}

//        public string GenerateECN()
//        {
//            string lid = "";
//            decimal nextValue = db.GetNextECNNO().Single().Value;
//            string year = DateTime.Now.Year.ToString().Substring(2, 2);
//            lid = "HEN" + year + nextValue.ToString("00000");
//            return lid;
//        }

//        public Users GetUsersLoggingIn()
//        {
//            //var user = Session["user"] as Users;
//            Users user = new Users();
//            user.UserName = "l19nguye";
//            return user;
//        }

//        public List<PROC_GET_ECN_MATERIAL_INFOR_Result> GetListMaterialChange(string ecn)
//        {
//            List<PROC_GET_ECN_MATERIAL_INFOR_Result> lsMaterialInfor = (from item in db.GetECNMaterialInfor(ecn) select item).ToList();
//            return lsMaterialInfor;
//        }
//    }
//}