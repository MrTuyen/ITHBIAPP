using OfficeOpenXml;
using ProductionApp.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using ProductionApp.Models;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;
using ProductionApp.Models.ITAsset;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using ProductionApp.Common;

namespace ProductionApp.Controllers.Assets
{
    public class ITAssetsController : BaseController
    {
        private readonly string CountsheetID = "CountsheetCache";
        // GET: ITAssets
        public ActionResult CheckingAssets(string strSearch)
        {
            strSearch = string.IsNullOrEmpty(strSearch) ? strSearch : strSearch.Trim().ToLower();
            var listData = new List<IT_Fixed_Asset>();
            if (string.IsNullOrEmpty(strSearch))
            {
                listData = db.IT_Fixed_Asset.Where(x => string.IsNullOrEmpty(x.STATUS) && string.IsNullOrEmpty(x.COUNTSHEET)).OrderBy(x => x.TAG).ToList();
            }
            else
            {
                listData = db.IT_Fixed_Asset.Where(x => string.IsNullOrEmpty(x.STATUS) && string.IsNullOrEmpty(x.COUNTSHEET))
                    .Where(x => (x.TAG.Trim().ToLower().Contains(strSearch) ||
                        x.SERIAL.Trim().ToLower().Contains(strSearch) ||
                        x.TBL_DIVISION_MST.NAME.Trim().ToLower().Contains(strSearch) ||
                        x.TBL_DEPARTMENT_MST.NAME.Trim().ToLower().Contains(strSearch) ||
                        x.USER.Trim().ToLower().Contains(strSearch)))
                        .OrderBy(x => x.TAG).ToList();
            }

            ViewData["ListDivision"] = db.TBL_DIVISION_MST.ToList();
            ViewData["ListDepartment"] = db.TBL_DEPARTMENT_MST.ToList();
            ViewData["ListModel"] = db.IT_PC_Model_MST.ToList();
            CacheHelper.Set("ExportData", listData); // cache data for exporting to excel

            return View(listData);
        }

        /// <summary>
        /// Filter asset by division and department
        /// </summary>
        /// <param name="formSearch"></param>
        /// <returns></returns>
        //public ActionResult FilterByDivAndDept(FormITAssetSearch formSearch)
        //{
        //    var listData = new List<IT_Fixed_Asset>();
        //    if (formSearch.DEPT == 0 && formSearch.DIVISION == 0)
        //    {
        //        listData = db.IT_Fixed_Asset.Where(x => x.STATUS == "" || x.STATUS == null).ToList();
        //    }
        //    else if (formSearch.DIVISION > 0 && formSearch.DEPT == 0)
        //    {
        //        listData = db.IT_Fixed_Asset.Where(x => x.STATUS == "" || x.STATUS == null)
        //            .Where(x => (x.DIVISION == formSearch.DIVISION)).ToList();
        //    }
        //    else if (formSearch.DIVISION == 0 && formSearch.DEPT > 0)
        //    {
        //        listData = db.IT_Fixed_Asset.Where(x => x.STATUS == "" || x.STATUS == null)
        //            .Where(x => (x.DEPT == formSearch.DEPT)).ToList();
        //    }
        //    else
        //    {
        //        listData = db.IT_Fixed_Asset.Where(x => x.STATUS == "" || x.STATUS == null)
        //            .Where(x => (x.DIVISION == formSearch.DIVISION && x.DEPT == formSearch.DEPT)).ToList();
        //    }

        //    ViewData["ListDivision"] = db.TBL_DIVISION_MST.ToList();
        //    ViewData["ListDepartment"] = db.TBL_DEPARTMENT_MST.ToList();
        //    ViewData["ListModel"] = db.IT_PC_Model_MST.ToList();
        //    ViewData["ExportFlag"] = true;
        //    CacheHelper.Set("ExportData", listData); // cache data for exporting to excel

        //    return View("CheckingAssets", listData);
        //}

        public ActionResult FilterByDivAndDept(FormITAssetSearch formSearch)
        {
            var listModel = db.IT_PC_Model_MST.ToList();
            var listDiv = db.TBL_DIVISION_MST.ToList();
            var listDept = db.TBL_DEPARTMENT_MST.ToList();

            var listData = new ConcurrentBag<IT_Fixed_Asset>();
            var tempData = db.PROC_GET_ITASSET(formSearch.YEAR, formSearch.DEPT, formSearch.DIVISION, formSearch.TAG, formSearch.SERIAL, formSearch.USER).OrderBy(x => x.TAG).ToList();

            int taskNum = 20;
            int qtyperTask = 0;
            CalTaskNumber(tempData.Count, ref qtyperTask, ref taskNum);
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < taskNum; i++)
            {
                int tempI = i;
                var items = tempData.Skip(tempI * qtyperTask).Take(qtyperTask).ToList();
                var task = Task.Run(() =>
                {
                    foreach (var x in items)
                    {
                        var model = listModel.Where(d => d.ID == x.MODEL).FirstOrDefault();
                        var div = listDiv.Where(d => d.ID == x.DIVISION).FirstOrDefault();
                        var dept = listDept.Where(d => d.DEPT_ID == x.DEPT).FirstOrDefault();

                        var item = new IT_Fixed_Asset()
                        {
                            ID = x.ID,
                            TAG = x.TAG,
                            SERIAL = x.SERIAL,
                            IT_PC_Model_MST = model,
                            MODEL = x.MODEL,
                            PUR_DATE = x.PUR_DATE,
                            WARRANTY = x.WARRANTY,
                            TBL_DIVISION_MST = div,
                            DIVISION = x.DIVISION,
                            TBL_DEPARTMENT_MST = dept,
                            DEPT = x.DEPT,
                            USER = x.USER,
                            NOTES = x.NOTES,
                            STATUS = x.STATUS,
                            COUNTSHEET = x.COUNTSHEET
                        };
                        listData.Add(item);
                    }
                });
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());

            ViewData["ListDivision"] = db.TBL_DIVISION_MST.ToList();
            ViewData["ListDepartment"] = db.TBL_DEPARTMENT_MST.ToList();
            ViewData["ListModel"] = db.IT_PC_Model_MST.ToList();
            ViewData["ExportFlag"] = true;
            CacheHelper.Set("ExportData", listData.ToList()); // cache data for exporting to excel

            return View("CheckingAssets", listData.ToList());
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
                            CacheHelper.Set("FullPath", fullPath);
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
                var listAssets = db.IT_Fixed_Asset.ToList(); // List assets in database
                var listModels = db.IT_PC_Model_MST.ToList(); // List models in database
                var listDepartments = db.TBL_DEPARTMENT_MST.ToList(); // List department in database
                var listDivisions = db.TBL_DIVISION_MST.ToList(); // List divisions in database
                var listErrorAssets = new List<Error_Asset>(); // List exist assets if tag and serial has existed

                var fullPath = CacheHelper.Get("FullPath");
                if (fullPath == null)
                    return Json(new { rs = false, msg = "Đã hết phiên làm việc. Vui lòng đăng nhập lại! /Expire working session. Please re-login!" });

                // List assets in Excel file
                DataTable dt;

                var sheet = selectedSheet == 0 ? 1 : selectedSheet;
                var headerRow = selectedHeader == 0 ? 1 : selectedHeader;
                msg = ExcelHelper.GetDataTableFromExcelFile(fullPath.ToString(), sheet, headerRow, out dt);
                if (msg.Length > 0)
                    return Json(new { rs = false, msg = "Dòng tiêu đề bạn chọn không có dữ liệu! /The header row is empty!" });

                var listInsertAssets = new List<IT_Fixed_Asset>();
                var listInsertModels = new List<IT_PC_Model_MST>();
                var listInsertDepartments = new List<TBL_DEPARTMENT_MST>();
                var listInsertDivisions = new List<TBL_DIVISION_MST>();

                if (dt != null && dt.Rows.Count > 0)
                {
                    var asset = new IT_Fixed_Asset();
                    var model = new IT_PC_Model_MST();
                    var division = new TBL_DIVISION_MST();
                    var dept = new TBL_DEPARTMENT_MST();

                    foreach (DataRow row in dt.Rows)
                    {
                        var tag = row[1] != null ? row[1].ToString() : string.Empty;
                        var serial = row[2] != null ? row[2].ToString() : string.Empty;
                        // Process components data
                        // PC model
                        model = new IT_PC_Model_MST()
                        {
                            NAME = row[3] != null ? row[3].ToString() : string.Empty
                        };
                        var objModel = listModels.Where(x => x.NAME == model.NAME).FirstOrDefault();
                        if (objModel == null)
                        {
                            db.IT_PC_Model_MST.Add(model);
                            listModels.Add(model);
                        }

                        // Divison
                        division = new TBL_DIVISION_MST()
                        {
                            NAME = row[6] != null ? row[6].ToString() : string.Empty
                        };
                        var objDivision = listDivisions.Where(x => x.NAME == division.NAME).FirstOrDefault();
                        if (objDivision == null)
                        {
                            //db.TBL_DIVISION_MST.Add(division);
                            //listDivisions.Add(division);

                            var objError = new Error_Asset()
                            {
                                Tag = tag,
                                Serial = serial,
                                Reason = "Division does not exitst in database"
                            };
                            listErrorAssets.Add(objError);

                            continue;
                        }

                        // Department
                        dept = new TBL_DEPARTMENT_MST()
                        {
                            NAME = row[7] != null ? row[7].ToString() : string.Empty
                        };
                        var objDept = listDepartments.Where(x => x.NAME == dept.NAME).FirstOrDefault();
                        if (objDept == null)
                        {
                            //db.TBL_DEPARTMENT_MST.Add(dept);
                            //listDepartments.Add(dept);

                            var objError = new Error_Asset()
                            {
                                Tag = tag,
                                Serial = serial,
                                Reason = "Department does not exitst in database"
                            };
                            listErrorAssets.Add(objError);

                            continue;
                        }

                        // Save all components in order to return component's id
                        db.SaveChanges();

                        // Process main data
                        asset = new IT_Fixed_Asset()
                        {
                            TAG = tag,
                            SERIAL = serial,
                            MODEL = db.IT_PC_Model_MST.Where(x => x.NAME == model.NAME).FirstOrDefault().ID,
                            PUR_DATE = row[4] != null ? row[4].ToString() : string.Empty,
                            WARRANTY = row[5] != null ? row[5].ToString() : string.Empty,
                            DIVISION = objDivision.ID,
                            DEPT = objDept.DEPT_ID,
                            USER = row[8] != null ? row[8].ToString() : string.Empty,
                            NOTES = row[9] != null ? row[9].ToString() : string.Empty,
                            STATUS = row[10] != null ? row[10].ToString() : string.Empty,
                            COUNTSHEET = row[11] != null ? row[11].ToString() : string.Empty,
                        };

                        // Check exist => if exist then add to listExistedAssets
                        var isAssetExist = listAssets.Where(x => x.TAG == (string.IsNullOrEmpty(asset.TAG) ? "" : asset.TAG) && x.SERIAL == (string.IsNullOrEmpty(asset.SERIAL) ? "" : asset.SERIAL)).FirstOrDefault();
                        if (isAssetExist != null)
                        {
                            var objError = new Error_Asset()
                            {
                                Tag = asset.TAG,
                                Serial = asset.SERIAL,
                                Reason = "Duplicate tag and serial"
                            };
                            listErrorAssets.Add(objError);
                        }
                        else
                        {
                            listAssets.Add(asset);
                            listInsertAssets.Add(asset);
                        }
                    }

                    db.IT_Fixed_Asset.AddRange(listInsertAssets);
                    db.SaveChanges();
                }

                // Write error file
                if (listErrorAssets != null && listErrorAssets.Count > 0)
                {
                    //string fileErrorPath = HttpContext.Server.MapPath("~/log/errorImport.txt");
                    //string content = string.Empty;
                    //foreach (var item in listErrorAssets)
                    //    content += "TAG: " + item.Tag + " - SERIAL: " + item.Serial + " - Reason: " + item.Reason + Environment.NewLine;
                    //System.IO.File.WriteAllText(fileErrorPath, content);

                    //return Json(new { rs = true, msg = msg, file = "/log/errorImport.txt" });

                    DataTable dtReport = new DataTable();
                    dtReport.Columns.Add("Tag");
                    dtReport.Columns.Add("Serial");
                    dtReport.Columns.Add("Reason");

                    // Add data row
                    foreach (var row in listErrorAssets)
                    {
                        DataRow dr = dtReport.NewRow();
                        dr["Tag"] = row.Tag;
                        dr["Serial"] = row.Serial;
                        dr["Reason"] = row.Reason;
                        dtReport.Rows.Add(dr);
                    }
                    // FileName
                    GlobalFunction.SaveDataTableToExcel(dtReport, HttpContext.Server.MapPath("~/log/errorImport.xlsx"));
                    return Json(new { rs = true, msg = msg, file = "/log/errorImport.xlsx" });
                }

                return Json(new { rs = true, msg = msg });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        //public ActionResult ReadExcelFile()
        //{
        //    string msg = "File uploaded succesfully!";
        //    try
        //    {
        //        var listAssets = db.IT_Fixed_Asset.ToList(); // List assets in database
        //        var listModels = db.IT_PC_Model_MST.ToList(); // List models in database
        //        var listDepartments = db.TBL_DEPARTMENT_MST.ToList(); // List department in database
        //        var listDivisions = db.TBL_DIVISION_MST.ToList(); // List divisions in database
        //        var listErrorAssets = new List<Error_Asset>(); // List exist assets if tag and serial has existed

        //        var fullPath = CacheHelper.Get("FullPath");
        //        if (fullPath == null)
        //            return Json(new { rs = false, msg = "Đã hết phiên làm việc. Vui lòng đăng nhập lại! /Expire working session. Please re-login!" });

        //        // List assets in Excel file
        //        DataTable dt;

        //        var selectedSheet = 1;
        //        var headerRow = 1;
        //        msg = ExcelHelper.GetDataTableFromExcelFile(fullPath.ToString(), selectedSheet, headerRow, out dt);
        //        if (msg.Length > 0)
        //            return Json(new { rs = false, msg = "Dòng tiêu đề bạn chọn không có dữ liệu! /The header row is empty!" });

        //        var listInsertAssets = new List<IT_Fixed_Asset>();
        //        var listInsertModels = new List<IT_PC_Model_MST>();
        //        var listInsertDepartments = new List<TBL_DEPARTMENT_MST>();
        //        var listInsertDivisions = new List<TBL_DIVISION_MST>();

        //        if (dt != null && dt.Rows.Count > 0)
        //        {
        //            var asset = new IT_Fixed_Asset();
        //            var model = new IT_PC_Model_MST();
        //            var division = new TBL_DIVISION_MST();
        //            var dept = new TBL_DEPARTMENT_MST();

        //            foreach (DataRow row in dt.Rows)
        //            {
        //                // Process components data
        //                // PC model
        //                model = new IT_PC_Model_MST()
        //                {
        //                    NAME = row[3] != null ? row[3].ToString() : string.Empty
        //                };
        //                var objModel = listModels.Where(x => x.NAME == model.NAME).FirstOrDefault();
        //                if (objModel == null)
        //                {
        //                    db.IT_PC_Model_MST.Add(model);
        //                    listModels.Add(model);
        //                }

        //                // Divison
        //                division = new TBL_DIVISION_MST()
        //                {
        //                    NAME = row[6] != null ? row[6].ToString() : string.Empty
        //                };
        //                var objDivision = listDivisions.Where(x => x.NAME == division.NAME).FirstOrDefault();
        //                if (objDivision == null)
        //                {
        //                    //db.TBL_DIVISION_MST.Add(division);
        //                    //listDivisions.Add(division);
        //                }

        //                // Department
        //                dept = new TBL_DEPARTMENT_MST()
        //                {
        //                    NAME = row[7] != null ? row[7].ToString() : string.Empty
        //                };
        //                var objDept = listDepartments.Where(x => x.NAME == dept.NAME).FirstOrDefault();
        //                if (objDept == null)
        //                {
        //                    db.TBL_DEPARTMENT_MST.Add(dept);
        //                    listDepartments.Add(dept);
        //                }

        //                // Save all components in order to return component's id
        //                db.SaveChanges();

        //                // Process main data
        //                asset = new IT_Fixed_Asset()
        //                {
        //                    TAG = row[1] != null ? row[1].ToString() : string.Empty,
        //                    SERIAL = row[2] != null ? row[2].ToString() : string.Empty,
        //                    MODEL = db.IT_PC_Model_MST.Where(x => x.NAME == model.NAME).FirstOrDefault().ID,
        //                    PUR_DATE = row[4] != null ? row[4].ToString() : string.Empty,
        //                    WARRANTY = row[5] != null ? row[5].ToString() : string.Empty,
        //                    DIVISION = db.TBL_DIVISION_MST.Where(x => x.NAME == division.NAME).FirstOrDefault().ID,
        //                    DEPT = db.TBL_DEPARTMENT_MST.Where(x => x.NAME == dept.NAME).FirstOrDefault().DEPT_ID,
        //                    USER = row[8] != null ? row[8].ToString() : string.Empty,
        //                    NOTES = row[9] != null ? row[9].ToString() : string.Empty,
        //                    STATUS = row[10] != null ? row[10].ToString() : string.Empty,
        //                    COUNTSHEET = row[11] != null ? row[11].ToString() : string.Empty,
        //                };

        //                // Check exist => if exist then add to listExistedAssets
        //                var isAssetExist = listAssets.Where(x => x.TAG == (string.IsNullOrEmpty(asset.TAG) ? "" : asset.TAG) && x.SERIAL == (string.IsNullOrEmpty(asset.SERIAL) ? "" : asset.SERIAL)).FirstOrDefault();
        //                if (isAssetExist != null)
        //                {
        //                    var objError = new Error_Asset()
        //                    {
        //                        Tag = asset.TAG,
        //                        Serial = asset.SERIAL,
        //                        Reason = "Duplicate tag and serial"
        //                    };
        //                    listErrorAssets.Add(objError);
        //                }
        //                else
        //                {
        //                    listAssets.Add(asset);
        //                    listInsertAssets.Add(asset);
        //                }
        //            }

        //            db.IT_Fixed_Asset.AddRange(listInsertAssets);
        //            db.SaveChanges();
        //        }

        //        if (listErrorAssets != null && listErrorAssets.Count > 0)
        //        {
        //            string fileErrorPath = HttpContext.Server.MapPath("~/log/errorImport.txt");
        //            string content = string.Empty;
        //            foreach (var item in listErrorAssets)
        //                content += "TAG: " + item.Tag + " - SERIAL: " + item.Serial + " - Reason: " + item.Reason + Environment.NewLine;
        //            System.IO.File.WriteAllText(fileErrorPath, content);

        //            return Json(new { rs = true, msg = msg, file = "/log/errorImport.txt" });
        //        }

        //        return Json(new { rs = true, msg = msg });
        //    }
        //    catch (Exception ex)
        //    {
        //        Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
        //        msg = "Error occurred. Error details: " + ex.Message;
        //        return Json(new { rs = false, msg = msg });
        //    }
        //}

        //public ActionResult ReadExcelFile()
        //{
        //    string msg = "";
        //    try
        //    {
        //        var fullPath = CacheHelper.Get("FullPath");
        //        if (fullPath == null)
        //            return Json(new { rs = false, msg = "Đã hết phiên làm việc. Vui lòng đăng nhập lại! /Expire working session. Please re-login!" });

        //        // List assets in Excel file
        //        DataTable dt;

        //        var selectedSheet = 1;
        //        var headerRow = 1;
        //        msg = ExcelHelper.GetDataTableFromExcelFile(fullPath.ToString(), selectedSheet, headerRow, out dt);
        //        if (msg.Length > 0)
        //            return Json(new { rs = false, msg = "Dòng tiêu đề bạn chọn không có dữ liệu! /The header row is empty!" });

        //        List<IT_Fixed_Asset> listAssets = new List<IT_Fixed_Asset>();
        //        if (dt != null && dt.Rows.Count > 0)
        //        {
        //            IT_Fixed_Asset asset = new IT_Fixed_Asset();
        //            IT_PC_Model_MST model = new IT_PC_Model_MST();
        //            TBL_DIVISION_MST division = new TBL_DIVISION_MST();
        //            TBL_DEPARTMENT_MST dept = new TBL_DEPARTMENT_MST();
        //            foreach (DataRow row in dt.Rows)
        //            {
        //                // Process components data
        //                // PC model
        //                model = new IT_PC_Model_MST()
        //                {
        //                    NAME = row[3] != null ? row[3].ToString() : string.Empty
        //                };
        //                var objModel = db.IT_PC_Model_MST.Where(x => x.NAME == model.NAME).FirstOrDefault();
        //                if (objModel == null)
        //                {
        //                    db.IT_PC_Model_MST.Add(model);
        //                }

        //                // Divison
        //                division = new TBL_DIVISION_MST()
        //                {
        //                    NAME = row[6] != null ? row[6].ToString() : string.Empty
        //                };
        //                var objDivision = db.TBL_DIVISION_MST.Where(x => x.NAME == division.NAME).FirstOrDefault();
        //                if (objDivision == null)
        //                {
        //                    db.TBL_DIVISION_MST.Add(division);
        //                }

        //                // Department
        //                dept = new TBL_DEPARTMENT_MST()
        //                {
        //                    NAME = row[7] != null ? row[7].ToString() : string.Empty
        //                };
        //                var objDept = db.TBL_DEPARTMENT_MST.Where(x => x.NAME == dept.NAME).FirstOrDefault();
        //                if (objDept == null)
        //                {
        //                    db.TBL_DEPARTMENT_MST.Add(dept);
        //                }

        //                // Save all components in order to return their id
        //                db.SaveChanges();

        //                // Process main data
        //                asset = new IT_Fixed_Asset()
        //                {
        //                    TAG = row[1] != null ? row[1].ToString() : string.Empty,
        //                    SERIAL = row[2] != null ? row[2].ToString() : string.Empty,
        //                    MODEL = db.IT_PC_Model_MST.Where(x => x.NAME == model.NAME).FirstOrDefault().ID,
        //                    PUR_DATE = row[4] != null ? row[4].ToString() : string.Empty,
        //                    WARRANTY = row[5] != null ? row[5].ToString() : string.Empty,
        //                    DIVISION = db.TBL_DIVISION_MST.Where(x => x.NAME == division.NAME).FirstOrDefault().ID,
        //                    DEPT = db.TBL_DEPARTMENT_MST.Where(x => x.NAME == dept.NAME).FirstOrDefault().DEPT_ID,
        //                    USER = row[8] != null ? row[8].ToString() : string.Empty,
        //                    NOTES = row[9] != null ? row[9].ToString() : string.Empty,
        //                    STATUS = row[10] != null ? row[10].ToString() : string.Empty,
        //                    COUNTSHEET = row[11] != null ? row[11].ToString() : string.Empty,
        //                };
        //                listAssets.Add(asset);
        //            }
        //            db.IT_Fixed_Asset.AddRange(listAssets);
        //            db.SaveChanges();
        //        }

        //        // Return message for client
        //        msg = "File uploaded succesfully!";
        //        return Json(new { rs = true, msg = msg });
        //    }
        //    catch (Exception ex)
        //    {
        //        Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
        //        msg = "Error occurred. Error details: " + ex.Message;
        //        return Json(new { rs = false, msg = msg });
        //    }
        //}

        /// <summary>
        /// Add new PC
        /// </summary>
        /// <returns></returns>
        public ActionResult AddNewPC(IT_Fixed_Asset data)
        {
            string msg = "";
            try
            {
                var isExist = db.IT_Fixed_Asset.Where(x => x.TAG == (string.IsNullOrEmpty(data.TAG) ? "" : data.TAG) && x.SERIAL == (string.IsNullOrEmpty(data.SERIAL) ? "" : data.SERIAL)).FirstOrDefault();
                if (isExist != null)
                {
                    msg = "Đã tồn tại tài sản có tag: "+ data.TAG + " và serial: " +data.SERIAL+ ". /Existed asset with tag: "+ data.TAG + " và serial: " +data.SERIAL;
                    return Json(new { rs = false, msg = msg });
                }
                db.IT_Fixed_Asset.Add(data);
                var isSuccess = db.SaveChanges();
                if (isSuccess < 0)
                {
                    msg = "Thêm tài sản không thành công. /Add new asset failed.";
                    return Json(new { rs = true, msg = msg });
                }
                msg = "Thêm tài sản thành công. /Add new asset successfully.";
                return Json(new { rs = true, msg = msg, data = new { ID = data.ID } });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        /// <summary>
        /// Get asset's info by ID
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ActionResult GetAssetById(IT_Fixed_Asset data)
        {
            string msg = "";
            try
            {
                var objAsset = db.IT_Fixed_Asset.Where(x => x.ID == data.ID)
                    .Select(
                    x => new
                    {
                        ID = x.ID,
                        TAG = x.TAG,
                        SERIAL = x.SERIAL,
                        MODEL = x.MODEL,
                        PUR_DATE = x.PUR_DATE,
                        WARRANTY = x.WARRANTY,
                        DIVISION = x.DIVISION,
                        DEPT = x.DEPT,
                        USER = x.USER,
                        NOTES = x.NOTES,
                        STATUS = x.STATUS,
                        COUNTSHEET = x.COUNTSHEET,
                        NOTE = x.NOTES
                    })
                    .FirstOrDefault();
                if (objAsset == null)
                {
                    msg = "Không tồn tại tài sản. / Asset does not exits.";
                    return Json(new { rs = false, msg = msg });
                }
                return Json(new { rs = true, msg = msg, data = objAsset }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        /// <summary>
        /// Get asset's info by Tag
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ActionResult GetAssetByTag(IT_Fixed_Asset data)
        {
            string msg = "";
            try
            {
                // Get countsheet
                var countSheet = GetCountsheet();

                // Get asset's info
                var objAsset = db.IT_Fixed_Asset.Where(x => x.TAG == data.TAG)
                    .Select(
                    x => new
                    {
                        ID = x.ID,
                        TAG = x.TAG,
                        SERIAL = x.SERIAL,
                        MODEL = x.MODEL,
                        PUR_DATE = x.PUR_DATE,
                        WARRANTY = x.WARRANTY,
                        DIVISION = x.DIVISION,
                        DEPT = x.DEPT,
                        USER = x.USER,
                        NOTES = x.NOTES,
                        STATUS = x.STATUS,
                        COUNTSHEET = string.IsNullOrEmpty(x.COUNTSHEET) ? (countSheet.FirstNumber.ToString() + "-" + countSheet.SecondNumber.ToString()) : x.COUNTSHEET,
                        NOTE = x.NOTES,
                    })
                    .FirstOrDefault();
                if (objAsset == null)
                {
                    msg = "Không tồn tại tài sản. / Asset does not exits.";
                    return Json(new { rs = false, msg = msg });
                }
                return Json(new { rs = true, msg = msg, data = objAsset }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        /// <summary>
        /// Update asset
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ActionResult UpdateAsset(IT_Fixed_Asset data)
        {
            string msg = "";
            try
            {
                var objAsset = db.IT_Fixed_Asset.Where(x => x.ID == data.ID).FirstOrDefault();
                if (objAsset == null)
                {
                    msg = "Không tồn tại tài sản. / Asset does not exits.";
                    return Json(new { rs = false, msg = msg });
                }

                // Update asset
                db.Entry(objAsset).CurrentValues.SetValues(data);

                // Update current countsheet value to TBL_System
                if (!string.IsNullOrEmpty(data.COUNTSHEET))
                {
                    var objCS = db.TBL_SYSTEM.Where(x => x.id == CountsheetID).FirstOrDefault();
                    objCS.value = data.COUNTSHEET;
                    //db.Entry(objCS).CurrentValues.SetValues(new TBL_SYSTEM() { id = objCS.id, value = data.COUNTSHEET });
                }

                var isSuccess = db.SaveChanges();
                if (isSuccess < 0)
                {
                    msg = "Cập nhật tài sản không thành công. /Update asset failed.";
                    return Json(new { rs = false, msg = msg });
                }
                msg = "Cập nhật tài sản thành công. /Update asset successfully.";
                return Json(new { rs = true, msg = msg, data = new { ID = data.ID, ISSCAN = string.IsNullOrEmpty(data.COUNTSHEET) ? false : true } }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        /// <summary>
        /// add new model
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ActionResult AddNewModel(IT_PC_Model_MST data)
        {
            string msg = "";
            try
            {
                var objModel = db.IT_PC_Model_MST.Where(x => x.NAME == data.NAME).FirstOrDefault();
                if (objModel != null)
                {
                    msg = "Đã tồn tại model. / Modal existed.";
                    return Json(new { rs = false, msg = msg });
                }

                db.IT_PC_Model_MST.Add(data);
                var isSuccess = db.SaveChanges();
                if (isSuccess < 0)
                {
                    msg = "Thêm model không thành công. /Add new model failed.";
                    return Json(new { rs = false, msg = msg });
                }
                msg = "Thêm model thành công. /Add new model successfully.";
                return Json(new { rs = true, msg = msg, data = data });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        /// <summary>
        /// add new division
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ActionResult AddNewDivision(TBL_DIVISION_MST data)
        {
            string msg = "";
            try
            {
                var objModel = db.TBL_DIVISION_MST.Where(x => x.NAME == data.NAME).FirstOrDefault();
                if (objModel != null)
                {
                    msg = "Đã tồn tại division. / Division existed.";
                    return Json(new { rs = false, msg = msg });
                }

                db.TBL_DIVISION_MST.Add(data);
                var isSuccess = db.SaveChanges();
                if (isSuccess < 0)
                {
                    msg = "Thêm division không thành công. /Add new division failed.";
                    return Json(new { rs = false, msg = msg });
                }
                msg = "Thêm division thành công. /Add new division successfully.";
                return Json(new { rs = true, msg = msg, data = data });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        /// <summary>
        /// add new model
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ActionResult AddNewDepartment(TBL_DEPARTMENT_MST data)
        {
            string msg = "";
            try
            {
                var objModel = db.TBL_DEPARTMENT_MST.Where(x => x.NAME == data.NAME).FirstOrDefault();
                if (objModel != null)
                {
                    msg = "Đã tồn tại department. / Department existed.";
                    return Json(new { rs = false, msg = msg });
                }

                db.TBL_DEPARTMENT_MST.Add(data);
                var isSuccess = db.SaveChanges();
                if (isSuccess < 0)
                {
                    msg = "Thêm department không thành công. /Add new department failed.";
                    return Json(new { rs = false, msg = msg });
                }
                msg = "Thêm department thành công. /Add new department successfully.";
                return Json(new { rs = true, msg = msg, data = data });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        /// <summary>
        /// Export data to excel from filter
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportExcel()
        {
            string msg = "";
            try
            {
                var result = new List<IT_Fixed_Asset>();
                var cacheData = CacheHelper.Get("ExportData"); // get data from cache for exporting to excel
                if (cacheData == null)
                    return Json(new { rs = false, msg = "Đã hết phiên làm việc. Vui lòng đăng nhập lại! /Expire working session. Please re-login!" });
                result = (List<IT_Fixed_Asset>)cacheData;

                // Create header row
                DataTable dtReport = new DataTable();
                dtReport.Columns.Add("ID");
                dtReport.Columns.Add("Tag#");
                dtReport.Columns.Add("Serial#");
                dtReport.Columns.Add("Model");
                dtReport.Columns.Add("Pur Date");
                dtReport.Columns.Add("Warranty");
                dtReport.Columns.Add("Division");
                dtReport.Columns.Add("Department");
                dtReport.Columns.Add("User");
                dtReport.Columns.Add("Notes");
                dtReport.Columns.Add("Status");
                dtReport.Columns.Add("CountSheet");

                // Add data row
                foreach (var row in result)
                {
                    DataRow dr = dtReport.NewRow();
                    dr["ID"] = row.ID;
                    dr["Tag#"] = row.TAG;
                    dr["Serial#"] = row.SERIAL;
                    dr["Model"] = row.MODEL;
                    dr["Pur Date"] = row.PUR_DATE;
                    dr["Warranty"] = row.WARRANTY;
                    dr["Division"] = row.TBL_DIVISION_MST.NAME;
                    dr["Department"] = row.TBL_DEPARTMENT_MST.NAME;
                    dr["User"] = row.USER;
                    dr["Notes"] = row.NOTES;
                    dr["Status"] = row.STATUS;
                    dr["CountSheet"] = row.COUNTSHEET;
                    dtReport.Rows.Add(dr);
                }
                // FileName
                string strFileName = "IT_Assets_" + DateTime.Now.ToString("dd_MM_yyyy_HH_mm");
                return PushFile(dtReport, strFileName);
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        /// <summary>
        /// Backup data to backup table then update all assets's status to blank
        /// </summary>
        /// <returns></returns>
        public ActionResult Backup()
        {
            string msg = "";
            var listRawData = db.IT_Fixed_Asset;
            try
            {
                if (listRawData.Where(x => string.IsNullOrEmpty(x.STATUS)).Count() > 0)
                {
                    msg = "Vẫn còn dữ liệu chưa được kiểm kê. Vui lòng kiểm tra lại. / There is still exist no inventoried data. Please check again.";
                    return Json(new { rs = false, msg = msg });
                }
                var listBackupData = listRawData.ToList()
                    .Select(x =>
                        new IT_Fixed_Asset_BK()
                        {
                            TAG = x.TAG,
                            SERIAL = x.SERIAL,
                            MODEL = x.MODEL,
                            PUR_DATE = x.PUR_DATE,
                            WARRANTY = x.WARRANTY,
                            DIVISION = x.DIVISION,
                            DEPT = x.DEPT,
                            USER = x.USER,
                            NOTES = x.NOTES,
                            STATUS = x.STATUS,
                            COUNTSHEET = x.COUNTSHEET
                        }
                    ).ToList();

                // Insert data to backup table after inventorying
                db.IT_Fixed_Asset_BK.AddRange(listBackupData);

                // Update all assets after inventorying
                listRawData.ToList().ForEach(x =>
                {
                    x.STATUS = "";
                    x.COUNTSHEET = "";
                });

                // Update current countsheet value in TBL_System to 1-0
                var objCS = db.TBL_SYSTEM.Where(x => x.id == CountsheetID).FirstOrDefault();
                objCS.value = "1-0";

                var isSuccess = db.SaveChanges();
                if (isSuccess < 0)
                {
                    msg = "Backup không thành công. /Backup failed.";
                    return Json(new { rs = false, msg = msg });
                }
                msg = "Backup thành công. /Backup successfully.";
                return Json(new { rs = true, msg = msg });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        /// <summary>
        /// Get current value of countsheet cache being stored in TBL_System - id: CountsheetCache
        /// </summary>
        /// <returns></returns>
        public CountsheetModel GetCountsheet()
        {
            string msg = "";
            try
            {
                // get markup countsheet from TBL_System - id: CountsheetCache
                var objCS = db.TBL_SYSTEM.Where(x => x.id == "CountsheetCache").FirstOrDefault();
                if (objCS == null)
                {
                    return new CountsheetModel();
                }
                var temp = objCS.value.Split('-');
                var countSheet = new CountsheetModel()
                {
                    FirstNumber = int.Parse(temp[0]),
                    SecondNumber = int.Parse(temp[1])
                };

                if (countSheet.SecondNumber == 10)
                {
                    countSheet.FirstNumber++;
                    countSheet.SecondNumber = 1;
                }
                else
                {
                    countSheet.SecondNumber++;
                }
                return countSheet;
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return new CountsheetModel();
            }
        }

        /// <summary>
        /// Print countsheet follow finance format
        /// </summary>
        /// <returns></returns>
        public ActionResult Print()
        {
            string msg = "";
            try
            {
                var result = db.IT_Fixed_Asset.Where(x => (!string.IsNullOrEmpty(x.COUNTSHEET) && !string.IsNullOrEmpty(x.STATUS))).ToList();
                if (result == null || result.Count == 0)
                {
                    msg = "Chưa có tài sản được kiểm kê. / There are no assets inventoried.";
                    return Json(new { rs = false, msg = msg });
                }

                // Create header row
                DataTable dtReport = new DataTable();
                dtReport.Columns.Add("Line Ref");
                dtReport.Columns.Add("Lawson Asset Number ( Số tài sản hệ thống)");
                dtReport.Columns.Add("Asset Description ( Miêu tả tài sản )");
                dtReport.Columns.Add("Asset Type ( Loại tài sản )");
                dtReport.Columns.Add("Sub Type");
                dtReport.Columns.Add("Tag Number ( Số Tag ) ");
                dtReport.Columns.Add("Serial Number ( Số Serial )");
                dtReport.Columns.Add("Model ( Mã )");
                dtReport.Columns.Add("Found In Area?   Check yes ( Tài sản tìm thấy ghi dấu) X");
                dtReport.Columns.Add("Count Team Observation or Comments ( Ghi chú )");

                // Add data row
                foreach (var row in result)
                {
                    DataRow dr = dtReport.NewRow();
                    dr["Line Ref"] = "";
                    dr["Lawson Asset Number ( Số tài sản hệ thống)"] = "";
                    dr["Asset Description ( Miêu tả tài sản )"] = "";
                    dr["Asset Type ( Loại tài sản )"] = "";
                    dr["Sub Type"] = "";
                    dr["Tag Number ( Số Tag ) "] = row.TAG;
                    dr["Serial Number ( Số Serial )"] = row.SERIAL;
                    dr["Model ( Mã )"] = row.IT_PC_Model_MST.NAME;
                    dr["Found In Area?   Check yes ( Tài sản tìm thấy ghi dấu) X"] = row.COUNTSHEET;
                    dr["Count Team Observation or Comments ( Ghi chú )"] = row.USER;
                    dtReport.Rows.Add(dr);
                }
                // FileName
                string strFileName = "Countsheet_" + DateTime.Now.ToString("dd_MM_yyyy_HH_mm");
                Response.AppendHeader("Set-Cookie", "fileDownload=true; path=/");
                return PushFile(dtReport, strFileName);
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }
    }
}