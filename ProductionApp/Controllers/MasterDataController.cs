using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml;
using ProductionApp.Models;
using System.Globalization;


namespace ProductionApp.Controllers
{
    public class MasterDataController:BaseController
    {
      
        // GET: MasterData
        public ActionResult Index()
        {
            if ((UserModels)Session["SignedInUser"] == null)
            {
                return RedirectToAction("NeedLogin", "Notification");
            }
            return View("UploadUnasCase");
        }

        public ActionResult Upload(FormCollection formCollection)
        {
            if (Request != null)
            {
                HttpPostedFileBase file = Request.Files["UploadedFile"];
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string fileName = file.FileName;
                    string fileContentType = file.ContentType;
                    byte[] fileBytes = new byte[file.ContentLength];
                    var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                    using (var package = new ExcelPackage(file.InputStream))
                    {
                        var currentSheet = package.Workbook.Worksheets;
                        var workSheet = currentSheet.First();
                        var noOfCol = workSheet.Dimension.End.Column;
                        var noOfRow = workSheet.Dimension.End.Row;

                        for (int rowIterator = 8; rowIterator <= noOfRow; rowIterator++)
                        {
                            TBL_RAW_DATA casetmp = new TBL_RAW_DATA();
                            casetmp.LABEL_ID = workSheet.Cells[rowIterator, 5].Value.ToString().Trim();
                            casetmp.WLOT = workSheet.Cells[rowIterator, 4].Value.ToString().Trim();
                            casetmp.PkgStyle = workSheet.Cells[rowIterator, 6].Value.ToString().Trim();
                            casetmp.COLOR = workSheet.Cells[rowIterator, 7].Value.ToString().Trim();
                            casetmp.SIZE = workSheet.Cells[rowIterator, 8].Value.ToString().Trim();
                            casetmp.LABEL_TYPE = workSheet.Cells[rowIterator, 10].Value.ToString().Trim();
                            casetmp.QUANTITY = (Convert.ToDouble(workSheet.Cells[rowIterator, 11].Value));

                            db.TBL_RAW_DATA.Add(casetmp);
                        }
                        db.SaveChanges();
                        InsertWLot();
                        InsertCaseLabel();
                    }
                }
            }

            return View("Index");
        }

        public void InsertWLot()
        {
            List<PROC_GET_ALL_RAW_WLOT_Result> AllWlot = (from item in db.PROC_GET_ALL_RAW_WLOT() select item).ToList();
            if (AllWlot.Count > 0)
            {
                foreach (var item in AllWlot)
                {
                    if (!checkWlotExist(item.WLOT.Trim()))
                    {
                        TBL_WORK_LOT wlotRecord = new TBL_WORK_LOT();
                        wlotRecord.WLOT_ID = item.WLOT.Trim();
                        wlotRecord.TOTAL_DZ = item.TOTAL_DZ;
                        db.TBL_WORK_LOT.Add(wlotRecord);
                        db.SaveChanges();
                    }
                }

            }

        }
        public Boolean checkWlotExist(string wlot)
        {
            string tmp = "";
            tmp = (from item in db.TBL_WORK_LOT.Where(t => t.WLOT_ID.Equals(wlot)) select item.WLOT_ID).SingleOrDefault();
            if (tmp != null)
                return true;
            return false;
        }

        public void InsertCaseLabel()
        {
            List<PROC_GET_ALL_RAW_LABEL_Result> AllCase = (from item in db.GetAllRawLabel() select item).ToList();
            if (AllCase.Count > 0)
            {
                foreach (var item in AllCase)
                {
                    if (!checkCaseExist(item.LABEL_ID.Trim()))
                    {
                        TBL_CASE_LABEL CaseRecord = new TBL_CASE_LABEL();
                        CaseRecord.WLOT_ID = item.WLOT.Trim();
                        CaseRecord.LABEL_ID = item.LABEL_ID;
                        CaseRecord.QUANTITY = item.QUANTITY;
                        CaseRecord.PkgStyle = item.STYLE;
                        CaseRecord.COLOR = item.COLOR;
                        CaseRecord.SIZE = item.SIZE;
                        CaseRecord.TS_1 = DateTime.Now;
                        CaseRecord.TS_1_USER = ((UserModels)Session["SignedInUser"] == null ? null : ((UserModels)Session["SignedInUser"]).Username.ToString());
                        CaseRecord.STATUS = 0;
                        CaseRecord.PLANT_CODE = item.Plant_Code;
                        db.TBL_CASE_LABEL.Add(CaseRecord);
                        db.SaveChanges();
                    }
                    else if (!CheckCaseWLExit(item.LABEL_ID, item.WLOT))
                    {
                        // CHECK
                        if (IsMultiWL(item.LABEL_ID, item.WLOT))
                        {
                            TBL_CASE_MULTI_WLOT MWL = new TBL_CASE_MULTI_WLOT();
                            MWL.LABEL_ID = item.LABEL_ID;
                            MWL.WLOT_ID = item.WLOT;
                            MWL.TS_1_USER = ((UserModels)Session["SignedInUser"] == null ? null : ((UserModels)Session["SignedInUser"]).Username.ToString());
                            MWL.TS_1 = DateTime.Now;
                            db.TBL_CASE_MULTI_WLOT.Add(MWL);
                            db.SaveChanges();
                        }
                    }
                }

            }

        }
        public Boolean checkCaseExist(string label)
        {
            string tmp = "";
            tmp = (from item in db.TBL_CASE_LABEL.Where(t => t.LABEL_ID.Equals(label)) select item.LABEL_ID).SingleOrDefault();
            if (tmp != null)
                return true;
            return false;
        }

        public Boolean IsMultiWL(string label, string WL)
        {
            string tmp = "";
            tmp = (from item in db.TBL_CASE_MULTI_WLOT.Where(t => t.LABEL_ID.Equals(label) & t.WLOT_ID.Equals(WL)) select item.LABEL_ID).SingleOrDefault();
            if (tmp == null)
                return true;
            return false;
        }

        public Boolean CheckCaseWLExit(string label, string WL)
        {
            string tmp = "";
            tmp = (from item in db.TBL_CASE_LABEL.Where(t => t.LABEL_ID.Equals(label) & t.WLOT_ID.Equals(WL)) select item.LABEL_ID).SingleOrDefault();
            if (tmp != null)
                return true;
            return false;
        }

        public ActionResult UploadUnAssignCase()
        {
            if (Request != null)
            {
                try
                {
                    HttpPostedFileBase file = Request.Files["UploadedFile"];
                    if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                    {
                        string fileName = file.FileName;
                        string fileContentType = file.ContentType;
                        byte[] fileBytes = new byte[file.ContentLength];
                        var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                        using (var package = new ExcelPackage(file.InputStream))
                        {
                            var currentSheet = package.Workbook.Worksheets;
                            var workSheet = currentSheet.First();
                            var noOfCol = workSheet.Dimension.End.Column;
                            var noOfRow = workSheet.Dimension.End.Row;

                            for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                            {
                                if (workSheet.Cells[rowIterator, 1].Value == null || workSheet.Cells[rowIterator, 1].Value.ToString() == "")
                                {
                                    break;
                                }
                                else
                                {
                                    if (!checkCaseExist(workSheet.Cells[rowIterator, 1].Value.ToString().Trim()))
                                    {
                                        string qty = workSheet.Cells[rowIterator, 4].Value.ToString();
                                        string[] arrTmp = qty.Split('/');

                                        TBL_CASE_LABEL casetmp = new TBL_CASE_LABEL();
                                        casetmp.LABEL_ID = workSheet.Cells[rowIterator, 1].Value.ToString().Trim();
                                        casetmp.TYPE = workSheet.Cells[rowIterator, 2].Value.ToString().Trim();
                                        casetmp.QUANTITY = Convert.ToDouble(arrTmp[0]) + (Convert.ToDouble(arrTmp[1]) / 12);
                                        casetmp.STATUS = 0;
                                        casetmp.TS_1_USER = ((UserModels)Session["SignedInUser"] == null ? null : ((UserModels)Session["SignedInUser"]).Username.ToString());
                                        casetmp.TS_1 = DateTime.Now;
                                        db.TBL_CASE_LABEL.Add(casetmp);
                                        db.SaveChanges();
                                    }
                                }
                            }

                        }
                        ViewBag.Status = "Upload Sucessful.";
                    }

                }
                catch (Exception e)
                {
                    ViewBag.Status = e.Message;
                }
            }

            return View("UploadUnAssignCase");
        }

        public ActionResult UploadUnasCase()
        {
            if (Request != null)
            {
                //try
                //{
                HttpPostedFileBase file = Request.Files["UploadedFile"];
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string fileName = file.FileName;
                    string fileContentType = file.ContentType;
                    byte[] fileBytes = new byte[file.ContentLength];
                    var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                    using (var package = new ExcelPackage(file.InputStream))
                    {
                        var currentSheet = package.Workbook.Worksheets;
                        var workSheet = currentSheet.First();
                        var noOfCol = workSheet.Dimension.End.Column;
                        var noOfRow = workSheet.Dimension.End.Row;

                        for (int rowIterator = 6; rowIterator <= noOfRow; rowIterator++)
                        {
                            string WLotTmp = (workSheet.Cells[rowIterator, 10].Value == null ? "OddLot" : workSheet.Cells[rowIterator, 10].Value.ToString().Trim());
                            var labelTyle = workSheet.Cells[rowIterator, 3].Value.ToString().Trim();
                            if (workSheet.Cells[rowIterator, 2].Value.ToString().Length == 9 && labelTyle != "MR" && labelTyle != "ID")
                            {
                                if (!checkCaseExist(workSheet.Cells[rowIterator, 2].Value.ToString().Trim()))
                                {
                                    string sku = workSheet.Cells[rowIterator, 13].Value.ToString().Trim();

                                    string xsku = sku.Replace("  ", " ");
                                    sku = xsku;
                                    string[] arrTmp = sku.Split(' ');

                                    TBL_CASE_LABEL casetmp = new TBL_CASE_LABEL();
                                    casetmp.LABEL_ID = workSheet.Cells[rowIterator, 2].Value.ToString().Trim();
                                    casetmp.TYPE = workSheet.Cells[rowIterator, 3].Value.ToString().Trim();
                                    casetmp.QUANTITY = Convert.ToDouble(workSheet.Cells[rowIterator, 4].Value.ToString().Trim()); //+ (Convert.ToDouble(arrTmp[1]) / 12);
                                    casetmp.TS_1_USER = ((UserModels)Session["SignedInUser"] == null ? null : ((UserModels)Session["SignedInUser"]).Username.ToString());
                                    casetmp.TS_1 = DateTime.Now;
                                    casetmp.PLANT_CODE = Convert.ToInt16(workSheet.Cells[rowIterator, 1].Value.ToString().Trim());
                                    casetmp.PkgStyle = arrTmp[0].ToString();
                                    casetmp.COLOR = arrTmp[1].ToString();
                                    casetmp.SIZE = arrTmp[3].ToString();
                                    casetmp.WLOT_ID = WLotTmp;
                                    casetmp.STATUS = 0;
                                    db.TBL_CASE_LABEL.Add(casetmp);
                                    db.SaveChanges();
                                }
                                else if (!CheckCaseWLExit(workSheet.Cells[rowIterator, 2].Value.ToString().Trim(), WLotTmp))
                                {
                                    if (IsMultiWL(workSheet.Cells[rowIterator, 2].Value.ToString().Trim(), WLotTmp))
                                    {
                                        TBL_CASE_MULTI_WLOT CMWL = new TBL_CASE_MULTI_WLOT();
                                        CMWL.LABEL_ID = workSheet.Cells[rowIterator, 2].Value.ToString().Trim();
                                        CMWL.WLOT_ID = WLotTmp;
                                        CMWL.TS_1_USER = ((UserModels)Session["SignedInUser"] == null ? null : ((UserModels)Session["SignedInUser"]).Username.ToString());
                                        CMWL.TS_1 = DateTime.Now;
                                        db.TBL_CASE_MULTI_WLOT.Add(CMWL);
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }
                    }
                    ViewBag.Status = "Upload Sucessful.";
                }

                //}
                //    catch (Exception e)
                //{
                //ViewBag.Status = e.InnerException;
                //}
            }

            return View("UploadUnasCase");
        }

        public ActionResult UploadProdPlan()
        {
            if (Request != null)
            {
                try
                {
                    HttpPostedFileBase file = Request.Files["UploadedFile"];
                    if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                    {
                        string fileName = file.FileName;
                        string fileContentType = file.ContentType;
                        byte[] fileBytes = new byte[file.ContentLength];
                        var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                        using (var package = new ExcelPackage(file.InputStream))
                        {
                            var currentSheet = package.Workbook.Worksheets;
                            var workSheet = currentSheet.First();
                            var noOfCol = workSheet.Dimension.End.Column;
                            var noOfRow = workSheet.Dimension.End.Row;
                            string labor, sah;
                            string style;
                            string pl_date, target, group;

                            for (int rowIterator = 3; rowIterator <= noOfRow; rowIterator++)
                            {
                                labor = (workSheet.Cells[rowIterator, 8].Value == null ? null : workSheet.Cells[rowIterator, 8].Value.ToString());
                                for (int i = 10; i <= 30; i += 4)
                                {
                                    pl_date = (workSheet.Cells[1, i].Value == null ? null : workSheet.Cells[1, i].Value.ToString());
                                    sah = (workSheet.Cells[rowIterator, i + 1].Value == null ? null : workSheet.Cells[rowIterator, i + 1].Value.ToString());
                                    style = (workSheet.Cells[rowIterator, i - 1].Value == null ? null : workSheet.Cells[rowIterator, i - 1].Value.ToString());
                                    target = (workSheet.Cells[rowIterator, i + 2].Value == null ? null : workSheet.Cells[rowIterator, i + 2].Value.ToString());
                                    group = (workSheet.Cells[rowIterator, 2].Value == null ? null : workSheet.Cells[rowIterator, 2].Value.ToString());
                                    if (pl_date != null && sah != null && style != null && target != null && sah != "#REF!" && style != "#REF!" && target != "#REF!")
                                    {
                                        TBL_PROD_PLAN tmp = new TBL_PROD_PLAN();
                                        tmp.PLAN_DATE = Convert.ToDateTime(pl_date);
                                        tmp.SAH = Convert.ToDouble(sah);
                                        tmp.STYLE = style;
                                        tmp.TARGET_QTY = Convert.ToDouble(target);
                                        TBL_GROUP_MST Group_record = db.TBL_GROUP_MST.SingleOrDefault(t => t.GROUP_NAME.Equals(group));
                                        tmp.GROUP_ID = (Group_record == null ? 0 : Group_record.GROUP_ID);
                                        tmp.LABOR = Convert.ToDouble(labor);
                                        tmp.TS_1 = DateTime.Now;
                                        tmp.TS_1_USER = "Admin";
                                        db.TBL_PROD_PLAN.Add(tmp);
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }
                        ViewBag.Status = "Upload Sucessful.";
                    }

                }
                catch (Exception e)
                {
                    ViewBag.Status = e.InnerException;
                }
            }

            return View("UploadProdPlan");
        }


        public ActionResult UploadEmployee()
        {
            //if ((UserModels)Session["SignedInUser"] == null)
            //{
            //    return RedirectToAction("NeedLogin", "Notification");
            //}
            UserModels usr = (UserModels)Session["SignedInUser"];

            if (Request != null)
            {
                int MesRow = 0;
                try
                {
                    HttpPostedFileBase file = Request.Files["UploadedFile"];
                    if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                    {
                        string fileName = file.FileName;
                        string fileContentType = file.ContentType;
                        byte[] fileBytes = new byte[file.ContentLength];
                        var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                        using (var package = new ExcelPackage(file.InputStream))
                        {
                            var currentSheet = package.Workbook.Worksheets;
                            var workSheet = currentSheet.First();
                            var noOfCol = workSheet.Dimension.End.Column;
                            var noOfRow = workSheet.Dimension.End.Row;
                            for (int rowIterator = 3; rowIterator <= noOfRow; rowIterator++)
                            {
                                MesRow = rowIterator;
                                if (workSheet.Cells[rowIterator, 1].Value != null)
                                {
                                    var employeeId = Convert.ToInt64(workSheet.Cells[rowIterator, 1].Value);
                                    var name = workSheet.Cells[rowIterator, 2].Value;
                                    var dept = int.Parse(workSheet.Cells[rowIterator, 3].Value.ToString());
                                    var pos = "";
                                    var onbDate = "";
                                    var status = "";
                                    var visa = "";


                                    var tmpE = db.TBL_EMPLOYEE.FirstOrDefault(t => t.EMPLOYEE_ID == employeeId);
                                    if (tmpE == null)
                                    {
                                        var deptRecord = db.TBL_DEPARTMENT_MST.FirstOrDefault(t => t.DEPT_ID == dept);
                                        var empRecord = new TBL_EMPLOYEE
                                        {
                                            EMPLOYEE_ID = employeeId,
                                            NAME = name.ToString(),
                                            DEPARTMENT_ID = deptRecord.DEPT_ID,
                                            Position = pos.ToString(),
                                            Status = status.ToString(),
                                            VISA = visa == null ? "" : visa.ToString()
                                        };

                                        //EmpRecord.ONBOARD_DATE = Convert.ToDateTime(OnbDate.ToString());
                                        db.TBL_EMPLOYEE.Add(empRecord);
                                        db.SaveChanges();

                                    }
                                }
                                else
                                {
                                    break;
                                }


                            }
                            //DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
                            //DateTime date1 = DateTime.Now;
                            //Calendar cal = dfi.Calendar;
                            //int a = cal.GetWeekOfYear(date1, dfi.CalendarWeekRule,
                            //                  dfi.FirstDayOfWeek);

                        }
                        ViewBag.Status = "Upload Sucessful.";
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Status = "Dữ liệu sai tại dòng:  " + ",  Row " + Convert.ToString(MesRow) + e.Message;
                }
            }
            return View("UploadEmployee");
        }
    }
}