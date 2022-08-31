using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProductionApp.Models;

namespace ProductionApp.Controllers
{
    public class ApproveController:BaseController
    {
        // GET: Approve
        public ActionResult Index()
        {
            return View();
        }
        
    }
}