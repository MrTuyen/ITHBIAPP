using System;
using System.Collections.Generic;
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

namespace ProductionApp.Controllers
{
    public class TrainingMaterialsController:BaseController
    {
        // GET: TrainingMaterials
        public ActionResult Index()
        {
            var ds = db.HR_Materials.ToList();
            return View(ds);
        }

        public ActionResult getAllRecodeByKey(int id)
        {
            bool proxyCreation = db.Configuration.ProxyCreationEnabled;
            try
            {
                //set ProxyCreation to false
                db.Configuration.ProxyCreationEnabled = false;
                var ds = db.HR_Materials.Where(s => s.id == id).ToList();
                return Json(ds, JsonRequestBehavior.AllowGet);
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
    }
}