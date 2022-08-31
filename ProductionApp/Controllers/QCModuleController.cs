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
using System.Linq.Dynamic;

namespace ProductionApp.Controllers
{
    public class QCModuleController:BaseController
    {
        // GET: Report
        public ActionResult Index()
        {
            return View("Daily");
        }

        public ActionResult DefectScan()
        {
            ViewBag.business = db.TBL_BUSINESS_MST.ToList();
            return View();
        }

        public ActionResult AddDefect(FormCollection fc)
        {

            ViewBag.business = db.TBL_BUSINESS_MST.ToList();


            List<EndlineDefects> El_List = new List<EndlineDefects>();
            string QCID = fc["txtQCID"];
            //string WL = fc["txtWL"];
            string BIZ = fc["listBIZ"];
            string Line = fc["txtGroup"];
            string WLSample = fc["txtTotalWLSample"];

            for (int i = 1; i <= 20; i++)
            {
                string a = string.Concat("txtEmpID", i.ToString());
                string b = string.Concat("txtDefID", i.ToString());
                string c = string.Concat("txtProID", i.ToString());
                string d = string.Concat("txtQty", i.ToString());
                string e = string.Concat("txtRwk", i.ToString());
                if (fc[a].ToString().Trim() != "")
                {
                    EndlineDefects EL_Elem = new EndlineDefects();
                    EL_Elem.EmployeeID = Convert.ToInt32(fc[a]);
                    if (fc[b].ToString() != "")
                        EL_Elem.DefectID = (fc[b]).ToString();
                    if (fc[c].ToString() != "")
                        EL_Elem.ProcessID = Convert.ToInt32(fc[c]);
                    if (fc[d].ToString() != "")
                        EL_Elem.Quantity = Convert.ToInt32(fc[d]);
                    //EL_Elem.totalSamp = fc[e].ToString()==""?0: Convert.ToInt32(fc[e]);
                    EL_Elem.IsRework = fc[e];
                    EL_Elem.QCID = Convert.ToInt32(QCID);
                    //EL_Elem.LineID = Convert.ToInt32(Line);
                    EL_Elem.LineID = Line;
                    //EL_Elem.Worklot = WL;
                    EL_Elem.Business = BIZ;
                    El_List.Add(EL_Elem);

                    db.SaveChanges();

                }
                else
                { break; }

            }


            for (int i = 0; i < El_List.Count; i++)
            {
                TBL_QC_ENDLINE EndlineRecord = new TBL_QC_ENDLINE();
                EndlineRecord.WORKER_ID = El_List[i].EmployeeID;
                if (El_List[i].DefectID != null)
                    EndlineRecord.DEFECT_ID = El_List[i].DefectID;
                if (El_List[i].ProcessID != 0)
                    EndlineRecord.PROCESS_ID = El_List[i].ProcessID;
                if (El_List[i].Quantity != 0)
                    EndlineRecord.QUANTITY = El_List[i].Quantity;
                EndlineRecord.TOTAL_SAMPLE = El_List[i].totalSamp;
                EndlineRecord.QC_STAFF_ID = El_List[i].QCID;
                EndlineRecord.LINE_ID = El_List[i].LineID;
                //EndlineRecord.WL_ID = El_List[i].Worklot;
                EndlineRecord.TOTAL_SAMPLE = Convert.ToInt16(WLSample);

                EndlineRecord.BIZI_ID = El_List[i].Business;
                EndlineRecord.TS_1 = DateTime.Now;
                EndlineRecord.TS_1_USER = "Admin";
                EndlineRecord.OTFQ = (El_List[i].IsRework != null ? 1 : 0);
                db.TBL_QC_ENDLINE.Add(EndlineRecord);

                db.SaveChanges();
            }

            TBL_QC_ENDLINE_WL_SAMPLE WLSamplRecord = new TBL_QC_ENDLINE_WL_SAMPLE();
            WLSamplRecord.TOTAL_SAMPLE = Convert.ToInt16(WLSample);
            WLSamplRecord.BIZI_ID = BIZ;
            WLSamplRecord.LINE_ID = Line;
            WLSamplRecord.TS_1 = DateTime.Now;
            WLSamplRecord.TS_1_USER = "Admin";
            db.TBL_QC_ENDLINE_WL_SAMPLE.Add(WLSamplRecord);

            db.SaveChanges();

            return View("DefectScan");
        }
        [HttpPost]
        public ActionResult GetWLInformation(string WL)
        {
            var result = db.TBL_ASST_WL.Where(r => r.ASST_ID == WL).FirstOrDefault();

            return Json(result);
        }

        public ActionResult GetGroupInformation(string Gr)
        {
            var resultG = (from item in db.GetGroupByName(Gr) select item.GROUP_NAME).SingleOrDefault();
            return Json(resultG);
        }

        public ActionResult GetEmpInformation(int Emp)
        {
            var result = db.TBL_EMPLOYEE.Where(r => r.EMPLOYEE_ID == Emp).FirstOrDefault();
            return Json(result);
        }

        public ActionResult EndlineTotal(FormCollection fc)
        {
            if (fc["txtFrom"] == null) return View("EndlineTotal");
            DateTime from_date = Convert.ToDateTime(fc["txtFrom"]).Date;
            DateTime to_date = Convert.ToDateTime(fc["txtTo"]).Date;
            int from_week = GetWeekOfYear(from_date);
            int to_week = GetWeekOfYear(to_date);
            int from_year = from_date.Year;
            int to_year = to_date.Year;
            //var endline = db.TBL_QC_ENDLINE.Where(q => q.TS_1 >= from_date && q.TS_1 <= to_date).ToList();
            //var z = db.TBL_QC_ENDLINE_WL_SAMPLE.Where(q => q.TS_1 >= from_date && q.TS_1 <= to_date && q.BIZI_ID == "4").ToList();
            List<EndlineTotalView> EnlineTotal = db.Get_All_Business().Select(x => new EndlineTotalView()
            {
                Business = x.BIZ_NAME,

                //Total_Defect = Convert.ToInt64((from item in db.Get_QC_BIZ_Total_Defect_Sample(from_date, to_date, x.ID) select item.TOTAL_QTY).SingleOrDefault()),
                Total_Defect = db.TBL_QC_ENDLINE.Where(q => q.TS_1 >= from_date && q.TS_1 <= to_date && q.BIZI_ID == x.ID.ToString()).Sum(a => a.QUANTITY),

                //Total_Sample = Convert.ToInt64((from item in db.GetQcBizTotalSample(from_date, to_date, x.ID) select item.TOTAL_SAMPLE).SingleOrDefault()),
                Total_Sample = db.TBL_QC_ENDLINE_WL_SAMPLE.Where(q => q.TS_1 >= from_date && q.TS_1 <= to_date && q.BIZI_ID == x.ID.ToString()).Sum(p => p.TOTAL_SAMPLE),

                //OTFQ = Convert.ToInt64((from item in db.GetQCBizTotalOTFQ(from_date, to_date, x.ID) select item.TOTAL_QTY).SingleOrDefault()),
                OTFQ = db.TBL_QC_ENDLINE.Where(q => q.TS_1 >= from_date && q.TS_1 <= to_date && q.BIZI_ID == x.ID.ToString()).Sum(a => a.OTFQ),

                //target = Convert.ToDouble((from item in db.TBL_QC_BUSINESS_TARGET.Where(t => t.BIZ_ID == x.ID) select item.TARGET).SingleOrDefault()),
                target = (double)db.TBL_QC_BUSINESS_TARGET.SingleOrDefault(q => q.BIZ_ID == x.ID).TARGET,

            }).ToList();


            List<EndlineTOP5GroupView> Top5Group = db.GetQcAllDefectedGroup(from_date, to_date).Select(x => new EndlineTOP5GroupView()
            {

                Line_Name = Convert.ToString(x),
                Rate = Convert.ToDouble((from item in db.GetQCTotalDefectByGroup(from_date, to_date, x) select item.TOTAL_DEFECT).SingleOrDefault()) /
                Convert.ToDouble((from item in db.GetQcTotalSampleByGroup(from_date, to_date, x) select item.TOTAL_SAMPLE).SingleOrDefault())*10000,
                Top5_Defect = string.Join(", ", (from record in db.GetQCTop5DefectByGroup(from_date, to_date, x) select record.NAME)),
                Top5_Process = string.Join(", ", (from record in db.GetQcTop5ProcessByGroup(from_date, to_date, x) select record.NAME)),
                Audit_Score = Convert.ToDouble((from item in db.GetQCGroupAuditScore(from_week, to_week, to_year, x) select item.AVG_SCORE).SingleOrDefault())

            }).ToList();
            ViewBag.Top5Group = Top5Group;

            List<EndlineWarningDefectBiz> Warning = db.GetQcDefectBiz(from_date, to_date).Select(x => new EndlineWarningDefectBiz()
            {

                Defect_Name = x.DEFECT_NAME,
                Business = x.BIZ_NAME,
                Rate = (Convert.ToDouble(x.TOTAL_QUANTITY) / Convert.ToDouble((from item in db.Get_QC_BIZ_Total_Defect_Sample(from_date, to_date, x.ID) select item.TOTAL_SAMPLE).SingleOrDefault()) * 100),

            }).ToList();
            Warning.OrderBy(t => t.Business).ThenBy(t => t.Rate);
            ViewBag.Warning = Warning;


            return View("EndlineTotal", EnlineTotal);
        }

        public ActionResult DefectByEmployee()
        {
            IEnumerable<PROC_GET_ALL_WS_Result> AllWs = (from item in db.GetAllWS() select item);
            ViewBag.LstWS = new SelectList(AllWs, "WSHOP_ID", "NAME");
            return View("DefectByEmployee");
        }

        public JsonResult GetGroup(int ID)
        {
            db.Configuration.ProxyCreationEnabled = false;
            return Json(db.TBL_GROUP_MST.Where(p => p.WSHOP_ID == ID), JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetDefectByEmployee(FormCollection fc)
        {
            DateTime from_date = Convert.ToDateTime(fc["txtFrom"]).Date;
            DateTime to_date = Convert.ToDateTime(fc["txtTo"]).Date;
            int from_week = GetWeekOfYear(from_date);
            int to_week = GetWeekOfYear(to_date);
            int from_year = from_date.Year;
            int to_year = to_date.Year;

            string[] PGroup = (fc["LstGroup"]).ToString().Split(',');
            List<string> PLine = new List<string>();
            foreach (var item in PGroup)
            {
                PLine.Add(item.Substring(0, 3));
                PLine.Add(item.Substring(3, 3));
            }
            string AllLine = string.Join(",", PLine);

            IEnumerable<PROC_GET_ALL_WS_Result> AllWs = (from item in db.GetAllWS() select item);
            ViewBag.LstWS = new SelectList(AllWs, "WSHOP_ID", "NAME");

            List<DefectByEmployeeView> DefByEmp = db.GetQcAllEmployeeDefecs(from_date, to_date, AllLine).Select(x => new DefectByEmployeeView()
            {
                WorkerID = x.EMPLOYEE_ID,
                FullName = x.NAME,
                Group = x.Line,
                Defect_Qty = Convert.ToInt16(x.TOTAL_QUANTITY),
                Total_Sample = Convert.ToInt16(x.TOTAL_SAMPLE),
                Rate = Convert.ToDouble(x.TOTAL_QUANTITY) / Convert.ToDouble(x.TOTAL_SAMPLE),
                Rework_Qty = Convert.ToInt16((from item in db.GetQcEmployeeRework(from_date, to_date, AllLine, x.EMPLOYEE_ID) select item.TOTAL_QUANTITY).SingleOrDefault()),
                Rework_Samp = Convert.ToInt16((from item in db.GetQcEmployeeRework(from_date, to_date, AllLine, x.EMPLOYEE_ID) select item.TOTAL_SAMPLE).SingleOrDefault()),
                Rework_Rate = (from item in db.GetQcEmployeeRework(from_date, to_date, AllLine, x.EMPLOYEE_ID) select item.TOTAL_SAMPLE).SingleOrDefault() == null ? 0 : (Convert.ToDouble((from item in db.GetQcEmployeeRework(from_date, to_date, AllLine, x.EMPLOYEE_ID) select item.TOTAL_QUANTITY).SingleOrDefault()) /
                                Convert.ToDouble((from item in db.GetQcEmployeeRework(from_date, to_date, AllLine, x.EMPLOYEE_ID) select item.TOTAL_SAMPLE).SingleOrDefault())),
                PAudit_Qty = Convert.ToInt16((from item in db.GetQcEmployeePAudit(from_date, to_date, AllLine, x.EMPLOYEE_ID) select item.QUANTITY).SingleOrDefault()),
                Efficiency = Convert.ToDouble(from_year != to_year ? 0 : (from item in db.GetEmployeeDloEfficiency(from_week, to_week, to_year, x.EMPLOYEE_ID) select item.AVG_EFF).SingleOrDefault())
            }).ToList();

            List<DefectByEmployeeView> PAditNotInEndline = db.GetQcPAuditNotInEndline(from_date, to_date, AllLine).Select(x => new DefectByEmployeeView()
            {
                WorkerID = Convert.ToInt32(x.WORKER_ID),
                FullName = x.NAME,
                PAudit_Qty = Convert.ToInt16(x.QUANTITY),
                Group = x.LINE,
                Efficiency = Convert.ToDouble(from_year != to_year ? 0 : (from item in db.GetEmployeeDloEfficiency(from_week, to_week, to_year, x.WORKER_ID) select item.AVG_EFF).SingleOrDefault())
            }).ToList();
            DefByEmp.AddRange(PAditNotInEndline);

            //List<PROC_GET_QC_ALL_EMPLOYEE_DEFECS_Result> allEmpDef = (from item in db.GetQcAllEmployeeDefecs(from_date, to_date, AllLine) select item).ToList();
            return View("DefectByEmployee", DefByEmp);
        }

        public ActionResult ReworkScan()
        {
            return View("ReworkScan");
        }
        public ActionResult ProcessAuditScan()
        {
            return View("ProcessAuditScan");
        }
        public int GetWeekOfYear(DateTime dt)
        {
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            Calendar cal = dfi.Calendar;
            return cal.GetWeekOfYear(dt, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
        }
        public ActionResult AddPAudit(FormCollection fc)
        {
            string QCID = fc["txtQCID"];
            string Line = fc["txtGroup"];
            string Score = fc["txtScore"];

            // insert worker violation
            for (int i = 1; i <= 5; i++)
            {
                string a = string.Concat("txtEmpID", i.ToString());
                string b = string.Concat("txtDefID", i.ToString());
                string c = string.Concat("txtProID", i.ToString());
                string d = string.Concat("txtEmpVioDate", i.ToString());

                var WorkerID = fc[a];
                var DefectID = fc[b];
                var ProcessID = fc[c];
                var ViolationDate = fc[d];
                if (WorkerID != "" && DefectID != "" & ProcessID != "" & ViolationDate != "")
                {
                    TBL_QC_PAUDIT_REWORK PAditRecord = new TBL_QC_PAUDIT_REWORK();
                    PAditRecord.QC_STAFF_ID = Convert.ToInt32(QCID);
                    PAditRecord.LINE_ID = Convert.ToInt16(Line);
                    PAditRecord.WORKER_ID = Convert.ToInt32(WorkerID.ToString());
                    PAditRecord.DEFECT_ID = DefectID.ToString();
                    PAditRecord.PROCESS_ID = Convert.ToInt16(ProcessID.ToString());
                    PAditRecord.TYPE = 1;
                    PAditRecord.TS_1 = Convert.ToDateTime(ViolationDate);
                    PAditRecord.TS_1_USER = "Admin";
                    db.TBL_QC_PAUDIT_REWORK.Add(PAditRecord);
                }
            }

            for (int i = 1; i <= 7; i++)
            {
                string a = string.Concat("txtDate", i.ToString());
                string b = string.Concat("txtGreenLabel", i.ToString());
                string c = string.Concat("txtIRR", i.ToString());
                string d = string.Concat("txtFmlPkgs", i.ToString());
                string e = string.Concat("txtFmlCase", i.ToString());
                string f = string.Concat("txtFmlGarment", i.ToString());
                string g = string.Concat("txtLayout", i.ToString());
                string h = string.Concat("txtUsingTool", i.ToString());
                string y = string.Concat("txtUsingInfoTbl", i.ToString());
                string j = string.Concat("txtPrbdiscuss", i.ToString());
                string k = string.Concat("txtMetalScan", i.ToString());

                var Date = (fc[a] == "" ? "0" : fc[a]);
                var Greenlabel = (fc[b] == "" ? "0" : fc[b]);
                var IRR = (fc[c] == "" ? "0" : fc[c]);
                var FmlPkgs = (fc[d] == "" ? "0" : fc[d]);
                var FmlCase = (fc[e] == "" ? "0" : fc[e]);
                var FmlGarmt = (fc[f] == "" ? "0" : fc[f]);
                var Layout = (fc[g] == "" ? "0" : fc[g]);
                var UsingTool = (fc[h] == "" ? "0" : fc[h]);
                var UsingInforTbl = (fc[y] == "" ? "0" : fc[y]);
                var PrbDiscuss = (fc[j] == "" ? "0" : fc[j]);
                var MetalScan = (fc[k] == "" ? "0" : fc[k]);
                if (Date != "0")
                {
                    TBL_QC_PAUDIT_SCORE PAditScoreRecord = new TBL_QC_PAUDIT_SCORE();
                    PAditScoreRecord.QC_ID = Convert.ToInt32(QCID);
                    PAditScoreRecord.AUDIT_DATE = Convert.ToDateTime(Date);
                    PAditScoreRecord.GREEN_LABEL = Convert.ToDouble(Greenlabel);
                    PAditScoreRecord.IRR = Convert.ToDouble(IRR);
                    PAditScoreRecord.FML_PACKAGES = Convert.ToDouble(FmlPkgs);
                    PAditScoreRecord.FML_CASE = Convert.ToDouble(FmlCase);
                    PAditScoreRecord.FML_GARMENT = Convert.ToDouble(FmlGarmt);
                    PAditScoreRecord.LAYOUT = Convert.ToDouble(Layout);
                    PAditScoreRecord.USING_TOOL = Convert.ToDouble(UsingTool);
                    PAditScoreRecord.USING_INFO_TABLE = Convert.ToDouble(UsingInforTbl);
                    PAditScoreRecord.PROBLEM_DISCUSS = Convert.ToDouble(PrbDiscuss);
                    PAditScoreRecord.METAL_SCAN = Convert.ToDouble(MetalScan);
                    PAditScoreRecord.LINE_ID = Line;
                    PAditScoreRecord.WEEK = GetWeekOfYear(Convert.ToDateTime(Date));
                    PAditScoreRecord.YEAR = (Convert.ToDateTime(Date)).Year;
                    PAditScoreRecord.TS_1 = DateTime.Now;
                    PAditScoreRecord.TS_1_USER = "Admin";
                    PAditScoreRecord.SCORE = Convert.ToInt16(Convert.ToDouble(Greenlabel) + Convert.ToDouble(IRR) + Convert.ToDouble(FmlPkgs) +
                        Convert.ToDouble(FmlCase) + Convert.ToDouble(FmlGarmt) + Convert.ToDouble(Layout) + Convert.ToDouble(UsingTool) +
                        Convert.ToDouble(UsingInforTbl) + Convert.ToDouble(PrbDiscuss) + Convert.ToDouble(MetalScan));

                    db.TBL_QC_PAUDIT_SCORE.Add(PAditScoreRecord);
                }
            }

            db.SaveChanges();
            return View("ProcessAuditScan");
        }


        public ActionResult AddRework(FormCollection fc)
        {
            string QCID = fc["txtQCID"];
            string Line = fc["txtGroup"];
            string WL = fc["txtWL"];

            for (int i = 1; i <= 5; i++)
            {
                string a = string.Concat("txtEmpID", i.ToString());
                string b = string.Concat("txtDefID", i.ToString());
                string c = string.Concat("txtProID", i.ToString());
                string d = string.Concat("txtQty", i.ToString());
                string e = string.Concat("txtTotal", i.ToString());


                var WorkerID = fc[a];
                var DefectID = fc[b];
                var ProcessID = fc[c];
                var Quantity = fc[d];
                var Total = fc[e];
                if (WorkerID != "" && DefectID != "" & ProcessID != "" && Quantity != "" && Total != "")
                {
                    TBL_QC_PAUDIT_REWORK PAditRecord = new TBL_QC_PAUDIT_REWORK();
                    PAditRecord.QC_STAFF_ID = Convert.ToInt32(QCID);
                    PAditRecord.LINE_ID = Convert.ToInt16(Line);
                    PAditRecord.WL_ID = WL;
                    PAditRecord.WORKER_ID = Convert.ToInt32(WorkerID.ToString());
                    PAditRecord.DEFECT_ID = DefectID.ToString();
                    PAditRecord.PROCESS_ID = Convert.ToInt16(ProcessID.ToString());
                    PAditRecord.QUANTITY = Convert.ToInt16(Quantity);
                    PAditRecord.TOTAL_SAMPLE = Convert.ToInt16(Total);
                    PAditRecord.TYPE = 2;
                    PAditRecord.TS_1 = DateTime.Now;
                    PAditRecord.TS_1_USER = "Admin";
                    db.TBL_QC_PAUDIT_REWORK.Add(PAditRecord);
                }
            }

            db.SaveChanges();

            return View("ReworkScan");
        }

        public ActionResult DefectByWorkLot()
        {
            IEnumerable<PROC_GET_ALL_WS_Result> AllWs = (from item in db.GetAllWS() select item);
            ViewBag.LstWS = new SelectList(AllWs, "WSHOP_ID", "NAME");
            return View("DefectByWorkLot");
        }

        public ActionResult GetDefectByWorkLot(FormCollection fc)
        {
            DateTime from_date = Convert.ToDateTime(fc["txtFrom"]).Date;
            DateTime to_date = Convert.ToDateTime(fc["txtTo"]).Date;

            string[] PGroup = (fc["LstGroup"]).ToString().Split(',');
            List<string> PLine = new List<string>();
            foreach (var item in PGroup)
            {
                PLine.Add(item.Substring(0, 3));
                PLine.Add(item.Substring(3, 3));
            }
            string AllLine = string.Join(",", PLine);

            IEnumerable<PROC_GET_ALL_WS_Result> AllWs = (from item in db.GetAllWS() select item);
            ViewBag.LstWS = new SelectList(AllWs, "WSHOP_ID", "NAME");

            List<DefectByWorkLotView> DefByWL = db.GetQcAllDefectedWL(from_date, to_date, AllLine).Select(x => new DefectByWorkLotView()
            {
                WL = (x).ToString(),
                Selling_Style = (from item in db.GetQcWLDefects(x.ToString()) select item.STYLE).SingleOrDefault(),
                Group = (from item in db.GetQcWLDefects(x.ToString()) select item.LINE).SingleOrDefault(),
                Defect_ID = (from item in db.GetQcWLDefects(x.ToString()) select item.DEFECT_ID).SingleOrDefault(),
                Defect_Qty = Convert.ToInt64((from item in db.GetQcWLDefects(x.ToString()) select item.QUANTITY).SingleOrDefault()),
                Total_sample = Convert.ToInt64((from item in db.GetQcWLDefects(x.ToString()) select item.TOTAL_SAMPLE).SingleOrDefault()),
                Rate = Convert.ToDouble((from item in db.GetQcWLDefects(x.ToString()) select item.TOTAL_SAMPLE).SingleOrDefault()) == 0 ? 0 :
                        Convert.ToDouble((from item in db.GetQcWLDefects(x.ToString()) select item.QUANTITY).SingleOrDefault()) /
                        Convert.ToDouble((from item in db.GetQcWLDefects(x.ToString()) select item.TOTAL_SAMPLE).SingleOrDefault()),
                Rework_Qty = Convert.ToInt64((from item in db.GetQcWLRework(x.ToString()) select item.TOTAL_QUANTITY).SingleOrDefault()),
                Rework_Samp = Convert.ToInt64((from item in db.GetQcWLRework(x.ToString()) select item.TOTAL_SAMPLE).SingleOrDefault()),
                Rework_Rate = Convert.ToDouble((from item in db.GetQcWLRework(x.ToString()) select item.TOTAL_SAMPLE).SingleOrDefault()) == 0 ? 0 :
                              Convert.ToDouble((from item in db.GetQcWLRework(x.ToString()) select item.TOTAL_QUANTITY).SingleOrDefault()) /
                              Convert.ToDouble((from item in db.GetQcWLRework(x.ToString()) select item.TOTAL_SAMPLE).SingleOrDefault()),
                Date = (from item in db.GetQcWLDefects(x.ToString()) select item.DATE_1).SingleOrDefault(),

            }).ToList();
            return View("DefectByWorkLot", DefByWL);
        }

        public ActionResult Search(FormCollection fc)
        {

            if (fc["txtSearch"] == null) return View("Search");
            string WL = fc["txtSearch"];
            List<PROC_GET_QC_SEARCH_BY_WL_Result> ListSearch = (from item in db.GetQcSearchByWL(WL) select item).ToList();

            return View("Search", ListSearch);

        }

        public ActionResult ProcessAuditScore()
        {
            IEnumerable<PROC_GET_ALL_WS_Result> AllWs = (from item in db.GetAllWS() select item);
            ViewBag.LstWS = new SelectList(AllWs, "WSHOP_ID", "NAME");
            return View("ProcessAuditScore");
        }
        public ActionResult GetProcessAuditScore(FormCollection fc)
        {
            IEnumerable<PROC_GET_ALL_WS_Result> AllWs = (from item in db.GetAllWS() select item);
            ViewBag.LstWS = new SelectList(AllWs, "WSHOP_ID", "NAME");

            DateTime Date = Convert.ToDateTime(fc["txtDate"]);
            int WsID = Convert.ToInt16(fc["LstWS"]);

            List<PROC_GET_QC_PAUDIT_SCORE_BY_WS_DATE_Result> PauditRecord = (from item in db.GetQcPauditScoreByWsDate(Date, WsID) select item).ToList();

            List<PROC_GET_QC_PAUDIT_EMPLOYEE_BY_WS_DATE_Result> PauditEmpRecord = (from item in db.GetQcPauditEmployeeByWsDate(Date, WsID) select item).ToList();

            ViewBag.PauditEmpRecord = PauditEmpRecord;
            return View("ProcessAuditScore", PauditRecord);
        }
        #region Enline Edit
        public ActionResult EndlineEdit()
        {
            return View();
        }
        [HttpPost]
        public ActionResult GetTBL_QC_ENDLINE()
        {

            //jQuery DataTables Param
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            //Find paging info
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find order columns info
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault()
                                    + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            //find search columns info
            var contactName = Request.Form.GetValues("columns[6][search][value]").FirstOrDefault();
            //var country = Request.Form.GetValues("columns[3][search][value]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt16(start) : 0;
            int recordsTotal = 0;
            using (MyContext dc = new MyContext())
            {
                // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
                var v = (from a in dc.TBL_QC_ENDLINE select a);

                //SEARCHING...
                if (!string.IsNullOrEmpty(contactName))
                {
                    v = v.Where(a => a.TS_1.ToString().Contains(contactName));
                }
                //if (!string.IsNullOrEmpty(country))
                //{
                //    v = v.Where(a => a.LINE_ID.ToString().Contains(country));
                //}

                //SORTING...  (For sorting we need to add a reference System.Linq.Dynamic)
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    v = v.OrderBy(sortColumn + " " + sortColumnDir);
                }
 
                recordsTotal = v.Count();
                var data = v.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult Save(int id)
        {
            using (ProductionAppEntities dc = new ProductionAppEntities())
            {
                var v = dc.TBL_QC_ENDLINE.Where(a => a.ID == id).FirstOrDefault();
                return View(v);
            }
        }

        [HttpPost]
        public ActionResult Save(TBL_QC_ENDLINE emp)
        {
            bool status = false;
            if (ModelState.IsValid)
            {
                using (ProductionAppEntities dc = new ProductionAppEntities())
                {
                    if (emp.ID > 0)
                    {
                        //Edit 
                        var v = dc.TBL_QC_ENDLINE.Where(a => a.ID == emp.ID).FirstOrDefault();
                        if (v != null)
                        {
                            v.QC_STAFF_ID = emp.QC_STAFF_ID;
                            v.WORKER_ID = emp.WORKER_ID;
                            v.SUPRNT_ID = emp.SUPRNT_ID;
                            v.LINE_ID = emp.LINE_ID;
                            v.DEFECT_ID = emp.DEFECT_ID;
                            v.QUANTITY = emp.QUANTITY;
                            v.BIZI_ID = emp.BIZI_ID;
                            v.TS_1 = emp.TS_1;
                            //v.TS_2 = emp.TS_2;
                            v.COMMENT = emp.COMMENT;
                            v.PROCESS_ID = emp.PROCESS_ID;
                            v.TOTAL_SAMPLE = emp.TOTAL_SAMPLE;
                            v.TS_1_USER = emp.TS_1_USER;
                            //v.TS_2_USER = emp.TS_2_USER;
                            v.OTFQ = emp.OTFQ;
                        }
                    }
                    else
                    {
                        //Save
                        dc.TBL_QC_ENDLINE.Add(emp);
                    }
                    dc.SaveChanges();
                    status = true;
                }
            }
            return new JsonResult { Data = new { status = status } };
        }
        [HttpGet]
        public ActionResult Delete(int id)
        {
            using (ProductionAppEntities dc = new ProductionAppEntities())
            {
                var v = dc.TBL_QC_ENDLINE.Where(a => a.ID == id).FirstOrDefault();
                if (v != null)
                {
                    return View(v);
                }
                else
                {
                    return HttpNotFound();
                }
            }
        }

        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteEndline(int id)
        {
            bool status = false;
            using (ProductionAppEntities dc = new ProductionAppEntities())
            {
                var v = dc.TBL_QC_ENDLINE.Where(a => a.ID == id).FirstOrDefault();
                if (v != null)
                {
                    dc.TBL_QC_ENDLINE.Remove(v);
                    dc.SaveChanges();
                    status = true;
                }
            }
            return new JsonResult { Data = new { status = status } };
        }
        #endregion
    }
}
