using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Ajax.Utilities;
using Microsoft.VisualBasic.ApplicationServices;
using ProductionApp.Helpers;
using ProductionApp.Models;
using System.Data;
using ProductionApp.Common;

namespace ProductionApp.Controllers
{
    public  class BaseController:Controller
    {
        public MyContext db = new MyContext();
        private UserModels _user;
        public UserModels userLogin
        {
            get { return _user ?? (_user = GetCurrent()); }
            set { _user = value; }
        }

        protected override void Initialize(RequestContext requestContext)
        {

            Current.Controller = this; // allow code to easily find this controller
            ValidateRequest = false; // allow html/sql in form values - remember to validate!
            base.Initialize(requestContext);
            var tmp = userLogin;
        }

        public UserModels GetCurrent()
        {
            if (Session["SignedInUser"] != null)
            {
                return Session["SignedInUser"] as UserModels;
            }
            else if (Session["SignedInUser"] == null && Request.Cookies["SignedInUser"] != null)
            {
                var userName = Request.Cookies["SignedInUser"].Value;
                var user = db.TBL_USERS_MST.SingleOrDefault(a => a.USERNAME == userName && a.ACTIVATE == 1);
                if (user != null)
                {
                    var user_approver = db.OL_User_Approver.FirstOrDefault(x => x.UserCD == userName);
                    var tmp = new UserModels
                    {
                        Username = user.USERNAME,
                        Role = user.ROLE_ID.ToString(),
                        Fullname = user.FULLNAME,
                        PlantCode = user.TBL_WSHOP_MST.PLANT_ID ?? 0,
                        Workshop = user.WSHOP_ID.ToString(),
                        Email = user.EMAIL,
                        POSID = user.POSID ?? 0,
                        DeptID = user.DEPT ?? 0,
                        TBL_DEPARTMENT_MST = user.TBL_DEPARTMENT_MST,
                        EmpID = user_approver == null ? "" : user_approver.EmpID,
                        ApproverEmail = user_approver == null ? "" : user_approver.ApproverEmail,
                        ApproverName = user_approver == null ? "" : user_approver.ApproverName
                    };
                    Session["SignedInUser"] = tmp;
                    return tmp;
                }
                else
                {

                    Response.Cookies["SignedInUser"].Expires = DateTime.Now.AddDays(-1);
                }
            }

            return null;

        }

        public FileResult PushFile(DataTable dt, string fileName)
        {
            return File(GlobalFunction.Export2XLS(dt), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName + ".xlsx");
        }

        public void CalTaskNumber(int totalRow, ref int numberPerTask, ref int taskNumDefault)
        {
            if (totalRow <= taskNumDefault)
            {
                taskNumDefault = 1;
                numberPerTask = totalRow;
            }
            else
            {
                numberPerTask = totalRow / taskNumDefault;
                if (numberPerTask % taskNumDefault != 0)
                    numberPerTask++;
            }
        }
    }
}