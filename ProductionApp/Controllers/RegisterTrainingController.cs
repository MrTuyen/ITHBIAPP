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
using ProductionApp.Controllers;
using System.Data;
using System.Net;
using System.Net.Mail;
using ProductionApp.Helpers;

namespace ProductionApp.Controllers
{
    public class RegisterTrainingController:BaseController
    {

        public ActionResult Index()
        {
            dynamic mymodel = new System.Dynamic.ExpandoObject();
            mymodel.buss = db.TBL_DEPARTMENT_MST.OrderBy(a => a.NAME).ToList();
            mymodel.systems = db.TBL_SYSTEM.Where(a => a.value3 == "L&D_TEAM").ToList();
            mymodel.planList = db.HR_TRAINING_REG.OrderBy(a => a.DeptID).ThenByDescending(a => a.id).ToList();
            return View(mymodel);
        }

        [HttpPost]
        public ActionResult AddTraining(int phongban, string ldteam, string chienluoc1, string chienluoc2, string chienluoc3, string chienluoc4, string chienluoc5, string chienluoc6, string goiy1, string goiy2, string goiy3, string goiy4, string goiy5, string goiy6)
        {
            var kq = "";

            if (chienluoc1 != "" && goiy1 != "" && chienluoc2 != "" && goiy2 != "" && chienluoc3 != "" && goiy3 != "" && chienluoc4 != "" && goiy4 != "" && chienluoc5 != "" && goiy5 != "" && chienluoc6 != "" && goiy6 != "")
            {
                var rg = new HR_TRAINING_REG
                {
                    DeptID = phongban,
                    AssignTo = ldteam,
                    Strategic = chienluoc1,
                    StrategicSuggestion = goiy1,
                    Problems = chienluoc2,
                    ProblemsSuggestion = goiy2,
                    JobEvolution = chienluoc3,
                    JobEvolutionSuggestion = goiy3,
                    CulturalChanges = chienluoc4,
                    CulturalChangesSuggestion = goiy4,
                    StaffNeeds = chienluoc5,
                    StaffNeedsSuggestion = goiy5,
                    StaffNeedsKey = chienluoc6,
                    StaffNeedsKeySuggestion = goiy6,
                    CreateDate = Utilities.GetDate_VietNam(DateTime.Now) ,
                    LastUpdateBy = userLogin.Username ,
                    UpdateDate = Utilities.GetDate_VietNam(DateTime.Now)

                };

                db.HR_TRAINING_REG.Add(rg);
                if (db.SaveChanges() > 0)
                {
                    SendEmail(ldteam, "Hi Team, <br/> Phòng " + db.TBL_DEPARTMENT_MST.SingleOrDefault(a => a.DEPT_ID == phongban).NAME + " vừa gửi khảo sát đào tạo.");
                    kq = "Add successfully!";
                }
            }
            return Json(new { msg = kq }, JsonRequestBehavior.AllowGet);
            //  return RedirectToAction("Student");
        }

        public JsonResult getAll()
        {
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                //set ProxyCreation to false
                db.Configuration.ProxyCreationEnabled = false;
                var data = db.HR_TRAINING_REG.ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
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

        public ActionResult SendEmail(string mailTo, string body)
        {
            Utilities.SendEmail("[L&D] Register training request", db.TBL_SYSTEM.Single(a => a.id == "hycmail").value, mailTo, "", body);
            var kq = "Sent email successfully!";
            return Json(new { msg = kq }, JsonRequestBehavior.AllowGet);
        }


    }
}