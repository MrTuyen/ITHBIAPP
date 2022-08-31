using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionApp.Models
{
    public class UserModels
    {
        public string Username { set; get; }
        public string Email { set; get; }
        public string Role { set; get; }
        public string Fullname { set; get; }
        public int PlantCode { set; get; }
        public int POSID { set; get; }
        public string Workshop { set; get; }
        public int? DeptID { set; get; }
        public TBL_DEPARTMENT_MST TBL_DEPARTMENT_MST { set; get; }

        public string EmpID { set; get; }
        public string ApproverName { set; get; }
        public string ApproverEmail { set; get; }
    }
}