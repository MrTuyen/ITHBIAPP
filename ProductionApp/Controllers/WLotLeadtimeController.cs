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

namespace ProductionApp.Controllers
{
    public class WLotLeadtimeController:BaseController
    {
        public ActionResult WLotLeadtime(FormCollection fc)
        {
            var fromDate = (fc["txtFrom"]);
            var toDate = (fc["txtTo"]);
            var wsID = (fc["listWC"]);
            List<PROC_GET_WL_LEADTIME_REPORT_BY_DATE_GROUPID_Result> WCRecord = new List<PROC_GET_WL_LEADTIME_REPORT_BY_DATE_GROUPID_Result>();
            if (fromDate != null && toDate != null && wsID != null)
            {
                WCRecord = (from item in db.GetWLLeadtimeReportByDateGroupID(Convert.ToDateTime(fromDate), Convert.ToDateTime(toDate), wsID.ToString()) select item).ToList();
            }
            IEnumerable<PROC_GET_ALL_GROUP_2_Result> lsApprovers = (from item in db.GetAllGroup2() select item);
            IEnumerable<PROC_GET_ALL_GROUP_2_Result> listWC = (from item in db.GetAllGroup2() select item);
            ViewBag.listWC = new SelectList(listWC, "GROUP_ID", "GROUP_NAME");
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.LstWCID = wsID;
            return View("WLotLeadtime", WCRecord);
        }
    }
}