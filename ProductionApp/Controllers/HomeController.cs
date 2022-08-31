using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProductionApp.Models;

namespace ProductionApp.Controllers {
    public class HomeController:BaseController {
        public ActionResult Index() {
            return View();
        }

        public ActionResult About() {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact() {
            ViewBag.Message = "Your contact page.";

            return View();
        }


        public ActionResult EndlineEdit() {
            return View();
        }
        public ActionResult GetTBL_QC_ENDLINE() {
            using(ProductionAppEntities dc = new ProductionAppEntities()) {
                var endline = dc.TBL_QC_ENDLINE.OrderBy(a => a.TS_1).ToList();
                return Json(new { data = endline } ,JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult Save(int id) {
            using(ProductionAppEntities dc = new ProductionAppEntities()) {
                var v = dc.TBL_QC_ENDLINE.Where(a => a.ID == id).FirstOrDefault();
                return View(v);
            }
        }

        [HttpPost]
        public ActionResult Save(TBL_QC_ENDLINE emp) {
            bool status = false;
            if(ModelState.IsValid) {
                using(ProductionAppEntities dc = new ProductionAppEntities()) {
                    if(emp.ID > 0) {
                        //Edit 
                        var v = dc.TBL_QC_ENDLINE.Where(a => a.ID == emp.ID).FirstOrDefault();
                        if(v != null) {
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
                    } else {
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
        public ActionResult Delete(int id) {
            using(ProductionAppEntities dc = new ProductionAppEntities()) {
                var v = dc.TBL_QC_ENDLINE.Where(a => a.ID == id).FirstOrDefault();
                if(v != null) {
                    return View(v);
                } else {
                    return HttpNotFound();
                }
            }
        }

        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteEndline(int id) {
            bool status = false;
            using(ProductionAppEntities dc = new ProductionAppEntities()) {
                var v = dc.TBL_QC_ENDLINE.Where(a => a.ID == id).FirstOrDefault();
                if(v != null) {
                    dc.TBL_QC_ENDLINE.Remove(v);
                    dc.SaveChanges();
                    status = true;
                }
            }
            return new JsonResult { Data = new { status = status } };
        }
    }
}