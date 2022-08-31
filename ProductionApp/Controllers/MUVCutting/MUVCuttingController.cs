using Newtonsoft.Json;
using ProductionApp.Helpers;
using ProductionApp.Models;
using ProductionApp.Models.MUVCutting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ProductionApp.Controllers.MUVCutting
{
    public class MUVCuttingController : BaseController
    {
        public static List<MUV_CT_Goal> muGoal = new List<MUV_CT_Goal>();
        public static List<TBL_PLANT_MST> plants = new List<TBL_PLANT_MST>();

        // GET: MUVCutting
        public ActionResult Index()
        {
            ViewData["User"] = userLogin;
            ViewData["Plant"] = db.TBL_PLANT_MST.ToList();
            ViewData["WC"] = db.MUV_CT_SellingWC.Select(x => x.Group).Distinct().ToList();

            muGoal = db.MUV_CT_Goal.ToList();
            plants = db.TBL_PLANT_MST.ToList();
            return View();
        }

        /// <summary>
        /// Upload file excel from client
        /// </summary>
        /// <returns></returns>
        public ActionResult UploadFile()
        {
            string msg = "";
            // Checking no of files injected in Request object  
            if (Request.Files.Count > 0)
            {
                try
                {
                    foreach (string file in Request.Files)
                    {
                        var fileContent = Request.Files[file];
                        if (fileContent != null && fileContent.ContentLength > 0)
                        {
                            string path = Server.MapPath("~/Uploads/");
                            string extension = Path.GetExtension(path + fileContent.FileName);
                            if (extension != ".xls" && extension != ".xlsx") return Json(new { rs = false, msg = "File không đúng định dạng, Hỗ trợ định dạng .xls và .xlsx." });

                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }

                            foreach (string key in Request.Files)
                            {
                                HttpPostedFileBase postedFile = Request.Files[key];
                                postedFile.SaveAs(path + fileContent.FileName);
                            }
                            string fullPath = path + fileContent.FileName;
                            CacheHelper.Set("MUVCutting", fullPath);
                            List<ExWorksheet> objSheets;
                            msg = ExcelHelper.GetAllWorksheets(fullPath, out objSheets);
                            if (msg.Length > 0) return Json(new { rs = false, msg = msg });
                            return Json(new { rs = true, fileName = fileContent.FileName, listSheet = JsonConvert.SerializeObject(objSheets), msg = "Tệp dữ liệu đã tải lên thành công." }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    // Returns message that successfully uploaded  
                    msg = "File Uploaded Successfully!";
                    return Json(new { rs = true, msg = msg });
                }
                catch (Exception ex)
                {
                    Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                    msg = "Error occurred. Error details: " + ex.Message;
                    return Json(new { rs = false, msg = msg });
                }
            }
            else
            {
                msg = "No files selected.";
                return Json(new { rs = false, msg = msg });
            }
        }

        /// <summary>
        /// Read data from cache file
        /// </summary>
        /// <returns></returns>
        /// 
        public ActionResult ReadExcelFile(int selectedSheet, int selectedHeader)
        {
            string msg = "File uploaded succesfully!";
            try
            {
                var fullPath = CacheHelper.Get("MUVCutting");
                if (fullPath == null)
                    return Json(new { rs = false, msg = "Đã hết phiên làm việc. Vui lòng đăng nhập lại! /Expire working session. Please re-login!" });

                // List assets in Excel file
                DataTable dt;

                var sheet = selectedSheet == 0 ? 1 : selectedSheet;
                var headerRow = selectedHeader == 0 ? 1 : selectedHeader;
                msg = ExcelHelper.GetDataTableFromExcelFile(fullPath.ToString(), sheet, headerRow, out dt);
                if (msg.Length > 0)
                    return Json(new { rs = false, msg = "Dòng tiêu đề bạn chọn không có dữ liệu! /The header row is empty!" });

                List<MUV_CT_SellingWC> listMUV = new List<MUV_CT_SellingWC>();
                if (dt != null && dt.Rows.Count > 0)
                {
                    MUV_CT_SellingWC muv = null;
                    foreach (DataRow row in dt.Rows)
                    {
                        muv = new MUV_CT_SellingWC()
                        {
                            MFG = row[0] == null ? "" : row[0].ToString(),
                            Group = row[1] == null ? "" : row[1].ToString(),
                            UserUpdate = row[2] == null ? "" : row[2].ToString(),
                            UploadDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture)
                        };

                        listMUV.Add(muv);
                    }

                    db.MUV_CT_SellingWC.AddRange(listMUV);
                    db.SaveChanges();
                }

                msg = "Tải lên file thành công. / Upload file successfully.";
                return Json(new { rs = true, msg = msg });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
            finally
            {
                CacheHelper.Remove("MUVCutting");
            }
        }

        /// <summary>
        /// Upload file excel from client
        /// </summary>
        /// <returns></returns>
        public ActionResult UploadFileTarget()
        {
            string msg = "";
            // Checking no of files injected in Request object  
            if (Request.Files.Count > 0)
            {
                try
                {
                    foreach (string file in Request.Files)
                    {
                        var fileContent = Request.Files[file];
                        if (fileContent != null && fileContent.ContentLength > 0)
                        {
                            string path = Server.MapPath("~/Uploads/");
                            string extension = Path.GetExtension(path + fileContent.FileName);
                            if (extension != ".xls" && extension != ".xlsx") return Json(new { rs = false, msg = "File không đúng định dạng, Hỗ trợ định dạng .xls và .xlsx." });

                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }

                            foreach (string key in Request.Files)
                            {
                                HttpPostedFileBase postedFile = Request.Files[key];
                                postedFile.SaveAs(path + fileContent.FileName);
                            }
                            string fullPath = path + fileContent.FileName;
                            CacheHelper.Set("MUVCuttingTarget", fullPath);
                            List<ExWorksheet> objSheets;
                            msg = ExcelHelper.GetAllWorksheets(fullPath, out objSheets);
                            if (msg.Length > 0) return Json(new { rs = false, msg = msg });
                            return Json(new { rs = true, fileName = fileContent.FileName, listSheet = JsonConvert.SerializeObject(objSheets), msg = "Tệp dữ liệu đã tải lên thành công." }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    // Returns message that successfully uploaded  
                    msg = "File Uploaded Successfully!";
                    return Json(new { rs = true, msg = msg });
                }
                catch (Exception ex)
                {
                    Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                    msg = "Error occurred. Error details: " + ex.Message;
                    return Json(new { rs = false, msg = msg });
                }
            }
            else
            {
                msg = "No files selected.";
                return Json(new { rs = false, msg = msg });
            }
        }

        /// <summary>
        /// Read data from cache file
        /// </summary>
        /// <returns></returns>
        /// 
        public ActionResult ReadExcelFileTarget(int selectedSheet, int selectedHeader)
        {
            string msg = "File uploaded succesfully!";
            try
            {
                var fullPath = CacheHelper.Get("MUVCuttingTarget");
                if (fullPath == null)
                    return Json(new { rs = false, msg = "Đã hết phiên làm việc. Vui lòng đăng nhập lại! /Expire working session. Please re-login!" });

                // List assets in Excel file
                DataTable dt;

                var sheet = selectedSheet == 0 ? 1 : selectedSheet;
                var headerRow = selectedHeader == 0 ? 1 : selectedHeader;
                msg = ExcelHelper.GetDataTableFromExcelFile(fullPath.ToString(), sheet, headerRow, out dt);
                if (msg.Length > 0)
                    return Json(new { rs = false, msg = "Dòng tiêu đề bạn chọn không có dữ liệu! /The header row is empty!" });

                List<MUV_CT_Goal> listMUV = new List<MUV_CT_Goal>();
                if (dt != null && dt.Rows.Count > 0)
                {
                    MUV_CT_Goal muv = null;
                    foreach (DataRow row in dt.Rows)
                    {
                        muv = new MUV_CT_Goal()
                        {
                            Plant = row[0] == null ? "" : row[0].ToString(),
                            Month = row[1] == null ? "" : row[1].ToString(),
                            WK = row[2] == null ? "" : row[2].ToString(),
                            OPC = row[3] == null ? 0 : double.Parse(row[3].ToString()),
                            Stretch = row[4] == null ? 0 : double.Parse(row[4].ToString()),
                        };

                        listMUV.Add(muv);
                    }

                    db.MUV_CT_Goal.AddRange(listMUV);
                    db.SaveChanges();
                }

                msg = "Tải lên file thành công. / Upload file successfully.";
                return Json(new { rs = true, msg = msg });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
            finally
            {
                CacheHelper.Remove("MUVCuttingTarget");
            }
        }

        public ActionResult Submit(string plant, string date, List<string> wc, int viewType)
        {
            string msg = "";
            try
            {
                switch (viewType)
                {
                    case (int)EnumHelper.View_Type.Weekly:
                        {
                            return Weekly(plant, date, wc, viewType);
                        }
                    case (int)EnumHelper.View_Type.Daily:
                        {
                            return Daily(plant, date, wc, viewType);
                        }
                    case (int)EnumHelper.View_Type.Style:
                        {
                            return Style(plant, date, wc, viewType);
                        }
                    case (int)EnumHelper.View_Type.Fabric:
                        {
                            return Fabric(plant, date, wc, viewType);
                        }
                }
                return Json(new { rs = false, msg = msg });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        public ActionResult Weekly(string plant, string date, List<string> wc, int viewType)
        {
            string msg = "";
            try
            {
                var week = "W" + Utilities.GetIso8601WeekOfYear(Convert.ToDateTime(date)).ToString();
                Dictionary<string, List<PROC_GET_MUV_Cutting_Result>> dicWcMuv = new Dictionary<string, List<PROC_GET_MUV_Cutting_Result>>();
                decimal? sum = 0;
                // Calculate MU
                foreach (var item in wc)
                {
                    var result = GetMUV(plant, date, item, viewType);
                    dicWcMuv.Add(item, result);
                    sum += Utilities.NullSafeDecimal(result[0].Amount, 0);
                }

                // Get target
                double? opc = 0;
                double? stretch = 0;
                if (string.IsNullOrEmpty(plant))
                {
                    //plant = "95,96";
                    plant = string.Join(",", plants.Select(x => x.PLANT_ID));
                }
                foreach (var item in plant.Split(','))
                {
                    var goal = muGoal.Where(x => x.WK == week && x.Plant == item).ToList();
                    if (goal != null && goal.Count > 0)
                    {
                        opc += goal.FirstOrDefault().OPC;
                        stretch += goal.FirstOrDefault().Stretch;
                    }
                }

                CacheHelper.Set("MUVWeekly", dicWcMuv);
                CacheHelper.Set("Time", date);

                return Json(new { rs = true, msg = msg, data = dicWcMuv.ToList(), sum = sum, goal = new { OPC = opc, Stretch = stretch } });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        public ActionResult Daily(string plant, string date, List<string> wc, int viewType)
        {
            string msg = "";
            try
            {
                var week = "W" + Utilities.GetIso8601WeekOfYear(Convert.ToDateTime(date)).ToString();
                Dictionary<string, List<PROC_GET_MUV_Cutting_Result>> dicWcMuv = new Dictionary<string, List<PROC_GET_MUV_Cutting_Result>>();
                decimal? sum = 0;
                // Calculate MU
                foreach (var item in wc)
                {
                    var result = GetMUV(plant, date, item, viewType);
                    dicWcMuv.Add(item, result);
                    sum += Utilities.NullSafeDecimal(result[1].Amount, 0);
                }

                // Get target
                double? opc = 0;
                double? stretch = 0;
                if (string.IsNullOrEmpty(plant))
                {
                    //plant = "95,96";
                    plant = string.Join(",", plants.Select(x => x.PLANT_ID));
                }
                foreach (var item in plant.Split(','))
                {
                    var goal = muGoal.Where(x => x.WK == week && x.Plant == item).ToList();
                    if (goal != null && goal.Count > 0)
                    {
                        opc += goal.FirstOrDefault().OPC;
                        stretch += goal.FirstOrDefault().Stretch;
                    }
                }

                CacheHelper.Set("MUVDaily", dicWcMuv);
                CacheHelper.Set("Time", date);

                return Json(new { rs = true, msg = msg, data = dicWcMuv.ToList(), sum = sum, goal = new { OPC = opc, Stretch = stretch } });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        public ActionResult Style(string plant, string date, List<string> wc, int viewType)
        {
            string msg = "";
            try
            {
                var week = "W" + Utilities.GetIso8601WeekOfYear(Convert.ToDateTime(date)).ToString();
                Dictionary<string, object> dicGoal = new Dictionary<string, object>();
                Dictionary<string, decimal> dicSum = new Dictionary<string, decimal>();
                Dictionary<string, Dictionary<string, List<MUVStyleResponse>>> dicWcMuvStyle = new Dictionary<string, Dictionary<string, List<MUVStyleResponse>>>();
                string wcString = string.Join(",", wc);
                if (string.IsNullOrEmpty(plant))
                {
                    foreach (var item in plants)
	                   {
                        var plantId = item.PLANT_ID.ToString();
		                      var goal = muGoal.Where(x => x.WK == week && x.Plant == plantId).ToList();
                        var plant95 = GetMUVStyle(plantId, date, wcString, viewType);
                        dicSum.Add(plantId, plant95.ElementAt(1).Value.Sum(x => x.Amount));
                        dicGoal.Add(plantId, goal);
                        dicWcMuvStyle.Add(plantId, plant95);

	                   }

                    //// Plant 95
                    //var goal = muGoal.Where(x => x.WK == week && x.Plant == "95").ToList();
                    //var plant95 = GetMUVStyle("95", date, wcString, viewType);
                    //dicSum.Add("95", plant95.ElementAt(1).Value.Sum(x => x.Amount));
                    //dicGoal.Add("95", goal);
                    //dicWcMuvStyle.Add("95", plant95);

                    //// Plant 96
                    //goal = muGoal.Where(x => x.WK == week && x.Plant == "96").ToList();
                    //var plant96 = GetMUVStyle("96", date, wcString, viewType);
                    //dicSum.Add("96", plant96.ElementAt(1).Value.Sum(x => x.Amount));
                    //dicGoal.Add("96", goal);
                    //dicWcMuvStyle.Add("96", plant96);
                }
                else
                {
                    var goal = muGoal.Where(x => x.WK == week && x.Plant == plant).ToList();
                    var result = GetMUVStyle(plant, date, wcString, viewType);
                    dicSum.Add(plant, result.ElementAt(1).Value.Sum(x => x.Amount));
                    dicGoal.Add(plant, goal);
                    dicWcMuvStyle.Add(plant, result);
                }

                CacheHelper.Set("MUVStyle", dicWcMuvStyle);
                CacheHelper.Set("Time", date);

                return Json(new { rs = true, msg = msg, data = dicWcMuvStyle.ToList(), sum = dicSum.ToList(), goal = dicGoal.ToList() });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        public ActionResult Fabric(string plant, string date, List<string> wc, int viewType)
        {
            string msg = "";
            try
            {
                var week = "W" + Utilities.GetIso8601WeekOfYear(Convert.ToDateTime(date)).ToString();
                Dictionary<string, object> dicGoal = new Dictionary<string, object>();
                Dictionary<string, decimal> dicSum = new Dictionary<string, decimal>();
                Dictionary<string, Dictionary<string, List<MUVFabricResponse>>> dicWcMuvFabric = new Dictionary<string, Dictionary<string, List<MUVFabricResponse>>>();
                string wcString = string.Join(",", wc);
                if (string.IsNullOrEmpty(plant))
                {
                    foreach (var item in plants)
                    {
                        var plantId = item.PLANT_ID.ToString();
		                      var goal = muGoal.Where(x => x.WK == week && x.Plant == plantId).ToList();
                        var plant95 = GetMUVFabric(plantId, date, wcString, viewType);

                        dicSum.Add(plantId, plant95.ElementAt(1).Value.Sum(x => x.Amount));
                        dicGoal.Add(plantId, goal);
                        dicWcMuvFabric.Add(plantId, plant95);
                    }
                    //// Plant 95
                    //var goal = muGoal.Where(x => x.WK == week && x.Plant == "95").ToList();
                    //var plant95 = GetMUVFabric("95", date, wcString, viewType);

                    //dicSum.Add("95", plant95.ElementAt(1).Value.Sum(x => x.Amount));
                    //dicGoal.Add("95", goal);
                    //dicWcMuvFabric.Add("95", plant95);

                    //// Plant 96
                    //goal = muGoal.Where(x => x.WK == week && x.Plant == "96").ToList();
                    //var plant96 = GetMUVFabric("96", date, wcString, viewType);

                    //dicSum.Add("96", plant96.ElementAt(1).Value.Sum(x => x.Amount));
                    //dicGoal.Add("96", goal);
                    //dicWcMuvFabric.Add("96", plant96);
                }
                else
                {
                    var goal = muGoal.Where(x => x.WK == week && x.Plant == plant).ToList();
                    var result = GetMUVFabric(plant, date, wcString, viewType);
                    dicSum.Add(plant, result.ElementAt(1).Value.Sum(x => x.Amount));
                    dicGoal.Add(plant, goal);
                    dicWcMuvFabric.Add(plant, result);
                }

                CacheHelper.Set("MUVFabric", dicWcMuvFabric);
                CacheHelper.Set("Time", date);

                return Json(new { rs = true, msg = msg, data = dicWcMuvFabric.ToList(), sum = dicSum.ToList(), goal = dicGoal.ToList() });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        public List<PROC_GET_MUV_Cutting_Result> GetMUV(string plant, string date, string wc, int viewType)
        {
            string msg = "";
            try
            {
                List<PROC_GET_MUV_Cutting_Result> listMUVResponse = new List<PROC_GET_MUV_Cutting_Result>();

                //DateTime pickDate = DateTime.ParseExact(date, "MM/dd/yyyy 00:00:00", CultureInfo.InvariantCulture);
                DateTime pickDate = DateTime.Parse(date);

                switch (viewType)
                {
                    case (int)EnumHelper.View_Type.Weekly:
                        {
                            DateTime firstDayOfWeek = pickDate.AddDays(-(int)pickDate.DayOfWeek);
                            DateTime lastDayOfWeek = firstDayOfWeek.AddDays(6);
                            //var muv1 = db.P   ROC_GET_MUV_Cutting(plant, firstDayOfWeek.ToString("MM/dd/yyyy"), lastDayOfWeek.ToString("MM/dd/yyyy"), wc);
                            var muv1 = db.PROC_GET_MUV_Cutting(plant, firstDayOfWeek.ToString("MM/dd/yyyy"), pickDate.ToString("MM/dd/yyyy"), wc);
                            listMUVResponse.AddRange(muv1);

                            DateTime firstDayOfMonth = new DateTime(pickDate.Year, pickDate.Month, 1);
                            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                            //var muv2 = db.PROC_GET_MUV_Cutting(plant, firstDayOfMonth.ToString("MM/dd/yyyy"), lastDayOfMonth.ToString("MM/dd/yyyy"), wc);
                            var muv2 = db.PROC_GET_MUV_Cutting(plant, firstDayOfMonth.ToString("MM/dd/yyyy"), pickDate.ToString("MM/dd/yyyy"), wc);
                            listMUVResponse.AddRange(muv2);

                            return listMUVResponse;
                        }

                    case (int)EnumHelper.View_Type.Daily:
                        {
                            var muv1 = db.PROC_GET_MUV_Cutting(plant, pickDate.ToString("MM/dd/yyyy"), pickDate.ToString("MM/dd/yyyy"), wc);
                            listMUVResponse.AddRange(muv1);

                            DateTime firstDayOfWeek = pickDate.AddDays(-(int)pickDate.DayOfWeek);
                            DateTime lastDayOfWeek = firstDayOfWeek.AddDays(6);
                            //var muv2 = db.PROC_GET_MUV_Cutting(plant, firstDayOfWeek.ToString("MM/dd/yyyy"), lastDayOfWeek.ToString("MM/dd/yyyy"), wc);
                            var muv2 = db.PROC_GET_MUV_Cutting(plant, firstDayOfWeek.ToString("MM/dd/yyyy"), pickDate.ToString("MM/dd/yyyy"), wc);
                            listMUVResponse.AddRange(muv2);

                            return listMUVResponse;
                        }
                }

                return listMUVResponse;
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return new List<PROC_GET_MUV_Cutting_Result>();
            }
        }

        public Dictionary<string, List<MUVFabricResponse>> GetMUVFabric(string plant, string date, string wc, int viewType)
        {
            string msg = "";
            try
            {
                Dictionary<string, List<MUVFabricResponse>> listMUVResponse = new Dictionary<string, List<MUVFabricResponse>>();

                //DateTime pickDate = DateTime.ParseExact(date, "MM/dd/yyyy 00:00:00", CultureInfo.InvariantCulture);
                DateTime pickDate = DateTime.Parse(date);

                var muv1 = GetFabric(plant, pickDate.ToString("MM/dd/yyyy"), pickDate.ToString("MM/dd/yyyy"), wc);
                listMUVResponse.Add("day", muv1);

                DateTime firstDayOfWeek = pickDate.AddDays(-(int)pickDate.DayOfWeek);
                DateTime lastDayOfWeek = firstDayOfWeek.AddDays(6);
                //var muv2 = GetMuvFabric(plant, firstDayOfWeek.ToString("MM/dd/yyyy"), lastDayOfWeek.ToString("MM/dd/yyyy"), wc);
                var muv2 = GetFabric(plant, firstDayOfWeek.ToString("MM/dd/yyyy"), pickDate.ToString("MM/dd/yyyy"), wc);
                listMUVResponse.Add("week", muv2);

                return listMUVResponse;
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return new Dictionary<string, List<MUVFabricResponse>>();
            }
        }

        public Dictionary<string, List<MUVStyleResponse>> GetMUVStyle(string plant, string date, string wc, int viewType)
        {
            string msg = "";
            try
            {
                Dictionary<string, List<MUVStyleResponse>> listMUVResponse = new Dictionary<string, List<MUVStyleResponse>>();

                //DateTime pickDate = DateTime.ParseExact(date, "MM/dd/yyyy 00:00:00", CultureInfo.InvariantCulture);
                DateTime pickDate = DateTime.Parse(date);

                var muv1 = GetStyle(plant, pickDate.ToString("MM/dd/yyyy"), pickDate.ToString("MM/dd/yyyy"), wc);
                listMUVResponse.Add("day", muv1);

                DateTime firstDayOfWeek = pickDate.AddDays(-(int)pickDate.DayOfWeek);
                DateTime lastDayOfWeek = firstDayOfWeek.AddDays(6);
                //var muv2 = GetMuvStyle(plant, firstDayOfWeek.ToString("MM/dd/yyyy"), lastDayOfWeek.ToString("MM/dd/yyyy"), wc);
                var muv2 = GetStyle(plant, firstDayOfWeek.ToString("MM/dd/yyyy"), pickDate.ToString("MM/dd/yyyy"), wc);
                listMUVResponse.Add("week", muv2);

                return listMUVResponse;
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return new Dictionary<string, List<MUVStyleResponse>>();
            }
        }

        public List<MUVFabricResponse> GetFabric(string plant, string fromDate, string toDate, string wc)
        {
            // Danh sách records lấy ra trong bảng ACI theo ngày, plan, wc
            var aciRecords = db.PROC_GET_MUV_Cutting_Fabric(plant, fromDate, toDate, wc).ToList();
            // Danh sách những records trong muv1 có LW_ACCOUNT == "512215"
            var list512215 = aciRecords.Where(x => x.LW_ACCOUNT == "512215").ToList();
            // Danh sách những records trong muv1 có LW_ACCOUNT == "512216"
            var list512216 = aciRecords.Where(x => x.LW_ACCOUNT == "512216").ToList();

            // Qty mã vải trong bảng ACI phục vụ việc tính mu trong bảng BOM với những mã vải có bộ ba style, color, size
            Dictionary<string, decimal> dic216 = new Dictionary<string, decimal>(); 
            foreach (var item in list512216)
            {
                var key = item.STYLE + "," + item.COLOR + "," + item.SIZE_CD + "," + item.GENERIC_ORDER_NO;
                if (dic216.ContainsKey(key))
                {
                    var newValue = dic216.Where(x => x.Key == key).FirstOrDefault().Value + decimal.Parse(item.INVENTORY_TRANS_QTY);
                    dic216.Remove(key);
                    dic216.Add(key, newValue);
                }
                else
                {
                    dic216.Add(key, decimal.Parse(item.INVENTORY_TRANS_QTY));
                }
            }

            // Nếu không có những bản ghi có LW_Account = 512216 thì return luôn => không có dữ liệu để tính toán tiếp
            if (dic216.Count <= 0)
            {
                return new List<MUVFabricResponse>();
            }

            // Lọc bớt data trong bảng BOM theo giá trị cần lấy từ bảng ACI với các bản ghi có trường LW_ACCOUNT = 512216
            var style216 = list512216.Select(a => a.STYLE).ToList();
            var color216 = list512216.Select(a => a.COLOR).ToList();
            var size216 = list512216.Select(a => a.SIZE_CD).ToList();
            var bomRecords = db.MUV_CT_BOM.Where(x => style216.Contains(x.STYLE_CD))
                            .Where(x => color216.Contains(x.COLOR_CD))
                            .Where(x => size216.Contains(x.SIZE_CD)).ToList();


            // Tính MU của từng mã vải trong bảng ACI
            Dictionary<string, decimal> dicMuACI = new Dictionary<string, decimal>();
            foreach (var item in dic216)
            {
                var item216 = item.Key.Split(',');
                var style = item216[0];
                var color = item216[1];
                var size = item216[2];
                var wo = item216[3];

                var listFabric = list512215.Where(x => x.GENERIC_ORDER_NO == wo).ToList();
                foreach (var itemFabric in listFabric)
                {
                    var key = item.Key + "," + itemFabric.STYLE;

                    if (dicMuACI.ContainsKey(key))
                    {
                        var newValue = dicMuACI.Where(x => x.Key == key).FirstOrDefault().Value + decimal.Parse(itemFabric.INVENTORY_TRANS_AMT);
                        dicMuACI.Remove(key);
                        dicMuACI.Add(key, newValue);
                    }
                    else
                    {
                        dicMuACI.Add(key, decimal.Parse(itemFabric.INVENTORY_TRANS_AMT));
                    }
                }
            }

            // Tính MU của từng mã vải trong bảng BOM
            Dictionary<string, decimal> dicMuBOM = new Dictionary<string, decimal>(); // chưa nhân với số lượng trong bảng ACI
            foreach (var item in dic216)
            {
                var item216 = item.Key.Split(',');
                var style = item216[0];
                var color = item216[1];
                var size = item216[2];
                var wo = item216[3];

                var listFabric = bomRecords.Where(x => x.STYLE_CD == style && x.COLOR_CD == color && x.SIZE_CD == size).ToList();

                foreach (var itemFabric in listFabric)
                {
                    var key = item.Key + "," + itemFabric.COMP_STYLE_CD;
                    decimal mu = (item.Value / 12M) * decimal.Parse(itemFabric.REQD_QTY) * decimal.Parse(itemFabric.TOTAL_COST_AMT) * (1 + decimal.Parse(itemFabric.CURR_YIELD_LOSS_PCT));

                    if (dicMuBOM.ContainsKey(key))
                    {
                        var newValue = dicMuBOM.Where(x => x.Key == key).FirstOrDefault().Value + mu;
                        dicMuBOM.Remove(key);
                        dicMuBOM.Add(key, newValue);
                    }
                    else
                    {
                        dicMuBOM.Add(key, mu);
                    }
                }
            }

            // Tính MU cuối cùng bằng MU trong ACI trừ đi MU trong BOM
            Dictionary<string, decimal> dicFinalMU = new Dictionary<string, decimal>();
            Dictionary<string, decimal> dicFinalQty = new Dictionary<string, decimal>();
            foreach (var item in dicMuACI)
            {
                var key = item.Key;
                var muBomItem = dicMuBOM.FirstOrDefault(x => x.Key == key).Value;

                dicFinalMU.Add(key, item.Value - muBomItem);

                var tempKey = key.Split(',');
                var keyQty = tempKey[0] + "," + tempKey[1] + "," + tempKey[2] + "," + tempKey[3];
                var tempQty = dic216.FirstOrDefault(x => x.Key == keyQty).Value;
                dicFinalQty.Add(key, tempQty);
            }

            // MU
            Dictionary<string, decimal> dicMU = new Dictionary<string, decimal>();
            foreach (var item in dicFinalMU)
            {
                var key = item.Key.Split(',')[4];

                if (dicMU.ContainsKey(key))
                {
                    var newValue = dicMU.Where(x => x.Key == key).FirstOrDefault().Value + item.Value;
                    dicMU.Remove(key);
                    dicMU.Add(key, newValue);
                }
                else
                {
                    dicMU.Add(key, item.Value);
                }

            }

            // QTY
            Dictionary<string, decimal> dicQTY = new Dictionary<string, decimal>();
            foreach (var item in dicFinalQty)
            {
                var key = item.Key.Split(',')[4];

                if (dicQTY.ContainsKey(key))
                {
                    var newValue = dicQTY.Where(x => x.Key == key).FirstOrDefault().Value + item.Value;
                    dicQTY.Remove(key);
                    dicQTY.Add(key, newValue);
                }
                else
                {
                    dicQTY.Add(key, item.Value);
                }
            }

            // Tổng số lượng dozen
            List<MUVFabricResponse> listMuvStyle = new List<MUVFabricResponse>();

            foreach (var item in dicQTY)
            {
                var dz = item.Value / 12;
                var amount = dicMU.FirstOrDefault(x => x.Key == item.Key).Value;
                MUVFabricResponse mu = new MUVFabricResponse()
                {
                    Fabric = item.Key,
                    Dz = dz,
                    Amount = Math.Round(amount, 2),
                    RunRate = Math.Round(amount / dz, 2)
                };

                listMuvStyle.Add(mu);
            }

            return listMuvStyle;
        }

        public List<MUVStyleResponse> GetStyle(string plant, string fromDate, string toDate, string wc)
        {
            var ttsOrder = db.TBL_TTS_ORDER_ST.ToList();
            // Danh sách records lấy ra trong bảng ACI theo ngày, plan, wc
            var aciRecords = db.PROC_GET_MUV_Cutting_Fabric(plant, fromDate, toDate, wc).ToList();

            // Danh sách những records trong muv1 có LW_ACCOUNT == "512216"
            var listAciRecords512216 = aciRecords.Where(x => x.LW_ACCOUNT == "512216").ToList();

            // Danh sách những records trong muv1 có LW_ACCOUNT == "512215"
            var listAciRecords512215 = aciRecords.Where(x => x.LW_ACCOUNT == "512215").ToList();

            // Danh sách WO 
            var listWo = aciRecords.DistinctBy(x => x.GENERIC_ORDER_NO).Select(x => x.GENERIC_ORDER_NO).ToList();

            var listMnfStyles = ttsOrder.Where(x => listWo.Contains(x.WO)).Select(x => new { x.MfgStyle, x.WO }).Distinct().ToList();

            Dictionary<string, decimal> dicQty216 = new Dictionary<string, decimal>(); // Số lượng mã vải trong bảng ACI
            Dictionary<string, decimal> dicAmount216 = new Dictionary<string, decimal>(); // Amount mã vải trong bảng ACI
            Dictionary<string, decimal> dicAmount215 = new Dictionary<string, decimal>(); // Amount mã vải trong bảng ACI

            foreach (var item in listMnfStyles)
            {
                var mnfStyle = item.MfgStyle;
                var wo = item.WO;

                // Danh sách MNF style và Qty 216
                var qty = listAciRecords512216.Where(x => x.GENERIC_ORDER_NO == wo).Sum(x => decimal.Parse(x.INVENTORY_TRANS_QTY));
                if (dicQty216.ContainsKey(mnfStyle))
                {
                    var newValue = dicQty216.Where(x => x.Key == mnfStyle).FirstOrDefault().Value + qty;
                    dicQty216.Remove(mnfStyle);
                    dicQty216.Add(mnfStyle, newValue);
                }
                else
                {
                    dicQty216.Add(mnfStyle, qty);
                }

                // Danh sách MNF style và Amount 216
                var amount216 = listAciRecords512216.Where(x => x.GENERIC_ORDER_NO == wo).Sum(x => decimal.Parse(x.INVENTORY_TRANS_AMT));
                if (dicAmount216.ContainsKey(mnfStyle))
                {
                    var newValue = dicAmount216.Where(x => x.Key == mnfStyle).FirstOrDefault().Value + amount216;
                    dicAmount216.Remove(mnfStyle);
                    dicAmount216.Add(mnfStyle, newValue);
                }
                else
                {
                    dicAmount216.Add(mnfStyle, amount216);
                }

                // Danh sách MNF style và Amount 215
                var amount215 = listAciRecords512215.Where(x => x.GENERIC_ORDER_NO == wo).Sum(x => decimal.Parse(x.INVENTORY_TRANS_AMT));
                if (dicAmount215.ContainsKey(mnfStyle))
                {
                    var newValue = dicAmount215.Where(x => x.Key == mnfStyle).FirstOrDefault().Value + amount215;
                    dicAmount215.Remove(mnfStyle);
                    dicAmount215.Add(mnfStyle, newValue);
                }
                else
                {
                    dicAmount215.Add(mnfStyle, amount215);
                }

            }


            // Nếu không có những bản ghi có LW_Account = 512216 thì return luôn => không có dữ liệu để tính toán tiếp
            if (dicQty216.Count <= 0)
            {
                return new List<MUVStyleResponse>();
            }

            //
            List<MUVStyleResponse> listMuvStyle = new List<MUVStyleResponse>();
            foreach (var item in dicQty216)
            {
                var dz = item.Value / 12;
                var amount = dicAmount215.Where(x => x.Key == item.Key).FirstOrDefault().Value - dicAmount216.Where(x => x.Key == item.Key).FirstOrDefault().Value;
                MUVStyleResponse mu = new MUVStyleResponse()
                {
                    Style = item.Key,
                    Dz = dz,
                    Amount = Math.Round(amount, 2),
                    RunRate = Math.Round(amount / dz, 2)
                };

                listMuvStyle.Add(mu);
            }
            return listMuvStyle;
        }

        public ActionResult Report(int viewType)
        {
            string msg = "";
            try
            {
                string date = CacheHelper.GetCheckExist("Time").ToString();
                if (string.IsNullOrEmpty(date))
                {
                    return Json(new { rs = false, msg = "Bạn chưa submit để lấy kết quả trả ra báo cáo/ You has not submit to save result" });
                }
                var week = "W" + Utilities.GetIso8601WeekOfYear(Convert.ToDateTime(date)).ToString();
                string strFileName = "MUV_Cutting_Weekly_" + week;

                // Create header row
                DataTable dtReport = new DataTable();
                dtReport.Columns.Add("WL");
                dtReport.Columns.Add("Selling Style");
                dtReport.Columns.Add("Mnf Style");
                dtReport.Columns.Add("WC");
                dtReport.Columns.Add("Style/ Fabric");
                dtReport.Columns.Add("Qty");
                dtReport.Columns.Add("MU");
                dtReport.Columns.Add("RunRate");

                switch (viewType)
                {
                    case (int)EnumHelper.View_Type.Weekly:
                        {
                            var weekly = CacheHelper.GetCheckExist("MUVWeekly");
                            if (string.IsNullOrEmpty(weekly.ToString()))
                            {
                                return Json(new { rs = false, msg = "Bạn chưa submit để lấy kết quả trả ra báo cáo/ You has not submit to save result" });
                            }

                            // Add data row
                            foreach (var row in (Dictionary<string, List<PROC_GET_MUV_Cutting_Result>>)weekly)
                            {
                                DataRow dr = dtReport.NewRow();
                                dr["WL"] = "";
                                dr["Selling Style"] = "";
                                dr["Mnf Style"] = "";
                                dr["WC"] = row.Key;
                                dr["Style/ Fabric"] = "";
                                dr["Qty"] = row.Value[0].Dz;
                                dr["MU"] = row.Value[0].Amount;
                                dr["RunRate"] = row.Value[0].RunRate;

                                dtReport.Rows.Add(dr);
                            }

                        } break;
                    case (int)EnumHelper.View_Type.Daily:
                        {
                            strFileName = "MUV_Cutting_Daily_" + DateTime.Parse(date).ToString("MM_dd_yyyy");
                            var daily = CacheHelper.GetCheckExist("MUVDaily");
                            if (string.IsNullOrEmpty(daily.ToString()))
                            {
                                return Json(new { rs = false, msg = "Bạn chưa submit để lấy kết quả trả ra báo cáo/ You has not submit to save result" });
                            }

                            // Add data row
                            foreach (var row in (Dictionary<string, List<PROC_GET_MUV_Cutting_Result>>)daily)
                            {
                                DataRow dr = dtReport.NewRow();
                                dr["WL"] = "";
                                dr["Selling Style"] = "";
                                dr["Mnf Style"] = "";
                                dr["WC"] = row.Key;
                                dr["Style/ Fabric"] = "";
                                dr["Qty"] = row.Value[0].Dz;
                                dr["MU"] = row.Value[0].Amount;
                                dr["RunRate"] = row.Value[0].RunRate;

                                dtReport.Rows.Add(dr);
                            }
                        } break;
                    case (int)EnumHelper.View_Type.Style:
                        {
                            strFileName = "MUV_Cutting_Style_" + DateTime.Parse(date).ToString("MM_dd_yyyy");
                            var style = CacheHelper.GetCheckExist("MUVStyle");
                            if (string.IsNullOrEmpty(style.ToString()))
                            {
                                return Json(new { rs = false, msg = "Bạn chưa submit để lấy kết quả trả ra báo cáo/ You has not submit to save result" });
                            }

                            // Add data row
                            foreach (var item in (Dictionary<string, Dictionary<string, List<MUVStyleResponse>>>)style)
                            {
                                var listItem = item.Value.ElementAt(0).Value;
                                foreach (var row in listItem)
                                {
                                    DataRow dr = dtReport.NewRow();
                                    dr["WL"] = "";
                                    dr["Selling Style"] = "";
                                    dr["Mnf Style"] = "";
                                    dr["WC"] = "";
                                    dr["Style/ Fabric"] = row.Style;
                                    dr["Qty"] = row.Dz;
                                    dr["MU"] = row.Amount;
                                    dr["RunRate"] = row.RunRate;

                                    dtReport.Rows.Add(dr);
                                }
                            }

                        } break;
                    case (int)EnumHelper.View_Type.Fabric:
                        {
                            strFileName = "MUV_Cutting_Fabric_" + DateTime.Parse(date).ToString("MM_dd_yyyy");
                            var fabric = CacheHelper.GetCheckExist("MUVFabric");
                            if (string.IsNullOrEmpty(fabric.ToString()))
                            {
                                return Json(new { rs = false, msg = "Bạn chưa submit để lấy kết quả trả ra báo cáo/ You has not submit to save result" });
                            }

                            // Add data row
                            foreach (var item in (Dictionary<string, Dictionary<string, List<MUVFabricResponse>>>)fabric)
                            {
                                var listItem = item.Value.ElementAt(0).Value;
                                foreach (var row in listItem)
                                {
                                    DataRow dr = dtReport.NewRow();
                                    dr["WL"] = "";
                                    dr["Selling Style"] = "";
                                    dr["Mnf Style"] = "";
                                    dr["WC"] = "";
                                    dr["Style/ Fabric"] = row.Fabric;
                                    dr["Qty"] = row.Dz;
                                    dr["MU"] = row.Amount;
                                    dr["RunRate"] = row.RunRate;

                                    dtReport.Rows.Add(dr);
                                }
                            }

                        } break;
                }

                // FileName
                Response.AppendHeader("Set-Cookie", "fileDownload=true; path=/");
                return PushFile(dtReport, strFileName);
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "MUVCutting Report error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
            //finally
            //{
            //    CacheHelper.Remove("Time");
            //}
        }
    }
}