using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProductionApp.Helpers;
using ProductionApp.Models;

namespace ProductionApp.Controllers {
    public class DisplayController:BaseController {
        // GET: Display
        public ActionResult Index() {

            var toDay = Utilities.GetDate_VietNam(DateTime.Today).Date;
            var group = db.TBL_WLOT_LOC.Where(w => w.ISSUE_DATE == toDay && w.QUANTITY > 0 && w.HOURS > 0).Select(w => w.TBL_GROUP_MST).Distinct().OrderBy(w => w.GROUP_NAME).ToList();
            return View(group);
        }

        public ActionResult OutputEff(string grID ,int index = 0) {
            ViewBag.targeteff = 0;
            ViewBag.resulteff = 0;
            ViewBag.targetOutput = 0;
            ViewBag.resultOutput = 0;
            ViewBag.GRIndex = "";
            try {
                DateTime dateNow =   Utilities.GetDate_VietNam(DateTime.Now);
                var toDay =   Utilities.GetDate_VietNam(DateTime.Today).Date;
                ViewBag.grID = grID;
                var lsID=grID.Split(',').Select(Int32.Parse).ToList();
                var ID = lsID[index];
                ViewBag.Index = index < lsID.Count - 1 ? index + 1 : 0;
                ViewBag.GRIndex = db.TBL_GROUP_MST.Find(ID).GROUP_NAME.Replace("Location ","");

                var start1 = dateNow.Hour < 14 ? 6 : 14;
                var SHIFT = dateNow.Hour < 14 ? 1 : 2;
                var nextHour = dateNow.Hour + dateNow.Minute * 1.0 / 60;
                double wtime = nextHour - start1;

                var timeShift = db.TBL_WLOT_LOC.FirstOrDefault(w => w.GROUP_ID == ID && w.ISSUE_DATE == toDay && w.SHIFT == SHIFT);
                if(timeShift != null && wtime > 0) {

                    var targetOutput = 0.0;
                    targetOutput = db.TBL_WLOT_LOC.Where(w => w.GROUP_ID == ID && w.ISSUE_DATE == toDay && w.SHIFT == SHIFT && w.HOURS > 0).Sum(a => a.QUANTITY / timeShift.HOURS * wtime) ?? 0;
                    var resultOutput = Math.Round(((
                        from l in db.TBL_CASE_LABEL
                        where DbFunctions.TruncateTime(l.TS_1) == toDay && l.GROUP_ID == ID && l.SHIFT == SHIFT
                        select l.QUANTITY).Sum() ?? 0) ,0);
                    ViewBag.targetOutput = Math.Round(targetOutput ,0);
                    ViewBag.resultOutput = resultOutput;
                }
                //eff
                if(timeShift != null && wtime > 0) {
                    wtime = wtime > timeShift.HOURS ? (double)timeShift.HOURS : wtime;
                    var hours = 0;
                    //var targetEff = db.TBL_Eff_Target.Where(w => w.GROUP_ID == ID && w.ISSUE_DATE == toDay && w.SHIFT == timeShift.SHIFT && w.HOURS > 0).Sum(a => a.QUANTITY / a.HOURS * wtime) ?? 0;
                    var targetEff = db.TBL_Eff_Target.Where(w => w.GROUP_ID == ID && w.ISSUE_DATE == toDay && w.SHIFT == timeShift.SHIFT && w.HOURS > 0).Sum(a => a.QUANTITY) ?? 0;
                    var wTime = (from l in db.Tbl_Working_Hour
                                 where DbFunctions.TruncateTime(l.DATEIN) == toDay && l.GROUPID == ID && l.SHIFT == timeShift.SHIFT
                                 select new {
                                     hour = l.QUANTITY
                                 }).Select(a => a.hour).Sum() / timeShift.HOURS * wtime;
                    var totalDz = (from l in db.TBL_CASE_LABEL
                                   from s in db.TBL_SAH_MST.Select(a => new { a.MnfStyle ,a.Size_Des ,a.SAH,a.Color }).Distinct()
                                   where (l.MnfStyle == s.MnfStyle || l.PkgStyle==s.MnfStyle)
                                   where l.SIZE == s.Size_Des
                                   where l.COLOR == s.Color
                                   where l.SHIFT == timeShift.SHIFT
                                   where DbFunctions.TruncateTime(l.TS_1) == toDay && l.GROUP_ID == ID
                                   select (l.QUANTITY / 12) * s.SAH).Sum();
                    ViewBag.targeteff = Math.Round(targetEff ,0);
                    ViewBag.resulteff = wTime > 0 ? Math.Round(totalDz / wTime * 100 ?? 0 ,0) : 0;
                }
            } catch(Exception e) {
                Utilities.WriteLogException(e);

                return RedirectToAction("Index");

            }
            return View();
        }
        // GET: Display
        public ActionResult DpmLeadtime(int grID) {
            ViewBag.grID = grID;
            return View();
        }
        // GET: Display
        public ActionResult WipMuv(int grID) {
            ViewBag.grID = grID;
            return View();
        }


    }
}