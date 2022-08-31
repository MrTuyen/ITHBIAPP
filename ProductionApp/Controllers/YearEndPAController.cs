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
using OfficeOpenXml;
using System.Diagnostics;

namespace ProductionApp.Controllers
{
    public class YearEndPAController:BaseController
    {
        // GET: QuaterlyKPI
        public ActionResult Index()
        {
            return View(db.TBL_DEPARTMENT_MST.ToList());
        }
    }
}