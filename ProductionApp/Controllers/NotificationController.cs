using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProductionApp.Models;

namespace ProductionApp.Controllers
{
    public class NotificationController:BaseController
    {
        // GET: Notification
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult NeedLogin(string module)
        {
            NotificationModel not = new NotificationModel();
            not.content = "You need login to use this " + module + " function";
            
            return View("index", not);
        }

        public ActionResult NonPermission(string module)
        {
            NotificationModel not = new NotificationModel();
            not.content = "You do not have permission on this " + module + " function";

            return View("NonPermission", not);
        }
    }
}