using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using ProductionApp.Common;
using ProductionApp.Helpers;
using ProductionApp.Models;
using ProductionApp.Models.AutoKanban;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ProductionApp.Controllers.AutoKanban
{
    public class AutoKanbanController : BaseController
    {
        const string Kanban_Last_Upload = "KanbanLastUpload";
        // GET: AutoKanban
        public ActionResult Index(int groupId = 0)
        {
            string msg = "";
            try
            {
                ViewData["ListGroup"] = db.TBL_GROUP_MST.ToList();
                ViewData["ListDept"] = db.TBL_DEPARTMENT_MST.ToDictionary(x => x.NAME, x => x.DEPT_ID);

                var listKanban = db.TBL_KANBAN.ToList();
                var listCalledKanban = new List<TBL_KANBAN>();
                var listUncallKanban = new List<TBL_KANBAN>();
                if (groupId == 0 )
                {
                    listCalledKanban = listKanban.Where(x => x.CallDate != null)
                        .OrderByDescending(x => x.CallDate != null)
                        .ThenBy(x => x.CallDate != null ? x.CallDate : null)
                        .ThenBy(x => DateTime.Parse(x.SewDate).ToString("MM/dd/yyyy") == DateTime.Now.ToString("MM/dd/yyyy"))
                        .ThenByDescending(x => x.Priority)
                        .ToList();

                    listUncallKanban = listKanban.Where(x => string.IsNullOrEmpty(x.CallDate))
                        .OrderBy(x => DateTime.Parse(x.SewDate).ToString("MM/dd/yyyy") == DateTime.Now.ToString("MM/dd/yyyy"))
                        .ThenBy(x => DateTime.Parse(x.SewDate))
                        .ThenByDescending(x => x.Priority)
                        .ToList();
                }
                else
                {
                    listCalledKanban = listKanban.Where(x => x.Location == groupId)
                        .Where(x => x.CallDate != null)
                        .OrderByDescending(x => x.CallDate != null)
                        .ThenBy(x => x.CallDate != null ? x.CallDate : null)
                        .ThenBy(x => DateTime.Parse(x.SewDate).ToString("MM/dd/yyyy") == DateTime.Now.ToString("MM/dd/yyyy"))
                        .ThenByDescending(x => x.Priority)
                        .ToList();

                    listUncallKanban = listKanban.Where(x => x.Location == groupId)
                        .Where(x => string.IsNullOrEmpty(x.CallDate))
                        .OrderBy(x => DateTime.Parse(x.SewDate).ToString("MM/dd/yyyy") == DateTime.Now.ToString("MM/dd/yyyy"))
                        .ThenBy(x => DateTime.Parse(x.SewDate))
                        .ThenByDescending(x => x.Priority)
                        .ToList();
                }

                // Lọc theo warehouse or ccd
                if (userLogin.DeptID == 2) // CP
                {
                    listCalledKanban = listCalledKanban.Where(x => string.IsNullOrEmpty(x.CPSendDate)).ToList();
                }
                if (userLogin.DeptID == 26) // SP
                {
                    listCalledKanban = listCalledKanban.Where(x => string.IsNullOrEmpty(x.SPSendDate)).ToList();
                }

                ViewData["user"] = userLogin;
                var group = db.TBL_GROUP_MST.Where(x => x.GROUP_ID == groupId).FirstOrDefault();
                ViewData["tags"] = group != null ? group.GROUP_NAME : string.Empty;
                ViewData["lastUpload"] = db.TBL_SYSTEM.Where(x => x.id == Kanban_Last_Upload).FirstOrDefault();
                ViewData["listUncall"] = listUncallKanban;

                return View(listCalledKanban);
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
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
                            CacheHelper.Set("AutoKanbanFullPath", fullPath);
                            List<ExWorksheet> objSheets;
                            msg = ExcelHelper.GetAllWorksheets(fullPath, out objSheets);
                            if (msg.Length > 0) return Json(new { rs = false, msg = "Dung lượng file không vượt quá 5 Mb / The file size is not bigger than 5 Mb" });
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
            var listGroups = db.TBL_GROUP_MST.ToList();
            var listKanban = db.TBL_KANBAN.ToList();
            try
            {
                var fullPath = CacheHelper.Get("AutoKanbanFullPath");
                if (fullPath == null)
                    return Json(new { rs = false, msg = "Đã hết phiên làm việc. Vui lòng đăng nhập lại! /Expire working session. Please re-login!" });

                // List assets in Excel file
                DataTable dt;

                var sheet = selectedSheet == 0 ? 1 : selectedSheet;
                var headerRow = selectedHeader == 0 ? 1 : selectedHeader;
                msg = ExcelHelper.GetDataTableFromExcelFile(fullPath.ToString(), sheet, headerRow, out dt);
                if (msg.Length > 0)
                    return Json(new { rs = false, msg = "Dòng tiêu đề bạn chọn không có dữ liệu! /The header row is empty!" });

                var listDelete = listKanban.Where(x => string.IsNullOrEmpty(x.CallDate)).Where(x => x.UploadBy == userLogin.Username);
                db.TBL_KANBAN.RemoveRange(listDelete);
                db.SaveChanges();
                listKanban = db.TBL_KANBAN.ToList();

                var listKanbanInsert = new List<TBL_KANBAN>();
                if (dt != null && dt.Rows.Count > 0)
                {
                    Regex regex = new Regex(@"^\d{2}/\d{2}/\d{4}$", RegexOptions.IgnorePatternWhitespace);

                    foreach (DataRow row in dt.Rows)
                    {
                        // Check sewdate có đúng định dạng ngày tháng năm hay không?
                        Match match = regex.Match(row[4].ToString());
                        if (!match.Success)
                        {
                            msg = string.Format("Sai định dạng SewDate tại WO: {0}/ SewDate format is wrong at WO: {0} ", row[0]);
                            return Json(new { rs = false, msg = msg });
                        }

                        var GROUP_NAME = row[3] != null ? row[3].ToString() : string.Empty;
                        var isGroupExist = listGroups.FirstOrDefault(x => x.GROUP_NAME == GROUP_NAME);
                        if (isGroupExist == null)
                        {
                            msg = "Group " + GROUP_NAME + " không tồn tại. / Group " + GROUP_NAME + " not found ";
                            return Json(new { rs = false, msg = msg });
                        }

                        db.SaveChanges();
                        var kanbanItem = new TBL_KANBAN()
                        {
                            AsstWO = row[0] != null ? row[0].ToString() : string.Empty,
                            Qty = row[1] != null ? int.Parse(row[1].ToString()) : 0,
                            Priority = row[2] != null ? row[2].ToString() : string.Empty,
                            Location = isGroupExist.GROUP_ID,
                            //Location = group.GROUP_ID,
                            SewDate = row[4] != null ? row[4].ToString() : string.Empty,
                            WLChild = row[5] != null ? row[5].ToString() : string.Empty,
                            SellingStyle = row[6] != null ? row[6].ToString() : string.Empty,
                            Size = row[7] != null ? row[7].ToString() : string.Empty,
                            MnfStyle = row[8] != null ? row[8].ToString() : string.Empty,
                            MnfColor = row[9] != null ? row[9].ToString() : string.Empty,
                            UploadBy = userLogin.Username,
                            UploadDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture)
                        };

                        var isKanbanExist = listKanban.Where(x => x.WLChild == kanbanItem.WLChild).FirstOrDefault();
                        if (isKanbanExist == null)
                        {
                            db.TBL_KANBAN.Add(kanbanItem);
                        }
                        else
                        {
                            var cell5 = isKanbanExist.CallBy;
                            if (string.IsNullOrEmpty(cell5))
                            {
                                isKanbanExist.Qty = kanbanItem.Qty;
                                isKanbanExist.Priority = kanbanItem.Priority;
                                isKanbanExist.Location = kanbanItem.Location;
                                isKanbanExist.SewDate = kanbanItem.SewDate;
                                isKanbanExist.WLChild = kanbanItem.WLChild;
                                isKanbanExist.SellingStyle = kanbanItem.SellingStyle;
                                isKanbanExist.Size = kanbanItem.Size;
                                isKanbanExist.MnfColor = kanbanItem.MnfColor;
                                isKanbanExist.MnfStyle = kanbanItem.MnfStyle;
                                //isKanbanExist.UploadBy = kanbanItem.UploadBy;
                                //isKanbanExist.UploadDate = kanbanItem.UploadDate;
                            }
                        }
                        db.SaveChanges();
                    }
                }

                // Lưu giá trị last upload date vào bảng system
                var systemVar = db.TBL_SYSTEM.Where(x => x.id == Kanban_Last_Upload).FirstOrDefault();
                if (systemVar != null)
                {
                    systemVar.value = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                }
                else
                {
                    db.TBL_SYSTEM.Add(new TBL_SYSTEM() { id = Kanban_Last_Upload, value = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture) });
                }
                db.SaveChanges();

                var signalR = new KanbanModel()
                {
                    ActionType = 0,
                };
                GlobalHost.ConnectionManager.GetHubContext<SignalRConf>().Clients.Group("signalR").newMessageReceived(signalR);

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
                CacheHelper.Remove("AutoKanbanFullPath");
            }
        }

        /// <summary>
        /// Process actions from client
        /// </summary>
        /// <param name="assWo">assortment</param>
        /// <param name="action">specific action</param>
        /// <returns></returns>
        public ActionResult Action(string assWo, int action, int location, int actionTime, string cancelReason)
        {
            string msg = "";
            try
            {
                switch (action)
                {
                    case (int)EnumHelper.Enum_Action.Cancel: 
                        {
                            return Cancel(assWo, cancelReason);
                        }
                    case (int)EnumHelper.Enum_Action.Call:
                        {
                            return Call(assWo, location);
                        }
                    case (int)EnumHelper.Enum_Action.CPSend:
                        {
                            return CPSend(assWo, actionTime);
                        }
                    case (int)EnumHelper.Enum_Action.SPSend:
                        {
                            return SPSend(assWo, actionTime);
                        }
                    case (int)EnumHelper.Enum_Action.Complete:
                        {
                            return Complete(assWo);
                        }
                }

                msg = "Can not handle this error. Please contact to IT to know more =))";
                return Json(new { rs = false, msg = msg });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        private ActionResult Cancel(string assWo, string cancelReason)
        {
            string msg = string.Empty;
            try
            {
                var objKanban = db.TBL_KANBAN.Where(x => x.WLChild == assWo).FirstOrDefault();
                if (objKanban == null)
                {
                    msg = "Không tồn tại work order/ There is no exist work order.";
                    return Json(new { rs = false, msg = msg });
                }

                if (string.IsNullOrEmpty(objKanban.CallBy) && string.IsNullOrEmpty(objKanban.CallDate))
                {
                    msg = "Work order này chưa được call/ The work order has not been call.";
                    return Json(new { rs = false, msg = msg });
                }

                objKanban.CallBy = null;
                objKanban.CallDate = null;

                objKanban.CPSendBy = null;
                objKanban.CPSendDate = null;
                objKanban.CPTime = null;

                objKanban.SPSendBy = null;
                objKanban.SPSendDate = null;
                objKanban.SPTime = null;

                objKanban.CancelBy = userLogin.Username;
                objKanban.CancelDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                objKanban.CancelReason = cancelReason;

                // Save data to TBL_KANBAN_DATA
                var objKanbanData = new TBL_KANBAN_DATA()
                {
                    AsstWO = objKanban.AsstWO,
                    Qty = objKanban.Qty,
                    Priority = objKanban.Priority,
                    Location = objKanban.Location,
                    SewDate = objKanban.SewDate,
                    MnfColor = objKanban.MnfColor,
                    WLChild = objKanban.WLChild,
                    SellingStyle = objKanban.SellingStyle,
                    Size = objKanban.Size,
                    MnfStyle = objKanban.MnfStyle,
                    CallDate = objKanban.CallDate,
                    CallBy = objKanban.CallBy,
                    CPSendDate = objKanban.CPSendDate,
                    CPSendBy = objKanban.CPSendBy,
                    CPTime = objKanban.CPTime,
                    SPSendDate = objKanban.SPSendDate,
                    SPSendBy = objKanban.SPSendBy,
                    SPTime = objKanban.SPTime,
                    PrdComDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                    PrdComBy = userLogin.Username,
                    CancelDate = objKanban.CancelDate,
                    CancelBy = objKanban.CancelBy,
                    UploadDate = objKanban.UploadDate,
                    UploadBy = objKanban.UploadBy,
                    CancelReason = cancelReason
                };

                // Check cancel lần 2: Nếu đã cancel và được gọi lại sau đó lại cancel tiếp
                var objKBData = db.TBL_KANBAN_DATA.Where(x => x.WLChild == objKanbanData.WLChild).FirstOrDefault();
                if (objKBData != null)
                {
                    db.TBL_KANBAN_DATA.Remove(objKBData);
                }
                db.TBL_KANBAN_DATA.Add(objKanbanData);

                db.SaveChanges();

                var signalR = new KanbanModel() 
                { 
                    AssWo = objKanban.WLChild,
                    ActionType = (int)EnumHelper.Enum_Action.Cancel 
                };
                GlobalHost.ConnectionManager.GetHubContext<SignalRConf>().Clients.Group("signalR").newMessageReceived(signalR);
                msg = "Hủy work order thành công/ The work order has been canceled successfully.";

                return Json(new { rs = true, msg = msg });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        private ActionResult Call(string assWo, int location)
        {
            string msg = string.Empty;
            try
            {
                var newestCall = db.TBL_KANBAN.Where(x => !string.IsNullOrEmpty(x.CallBy)).SmartOrderByDescending(x => x.CallDate).FirstOrDefault();
                var objKanban = db.TBL_KANBAN.Where(x => x.WLChild == assWo).FirstOrDefault();
                if (objKanban == null)
                {
                    msg = "Không tồn tại work order/ There is no exist work order.";
                    return Json(new { rs = false, msg = msg });
                }

                if (!string.IsNullOrEmpty(objKanban.CallBy) && !string.IsNullOrEmpty(objKanban.CallDate))
                {
                    msg = "Work order này đã được call/ The work order has been called.";
                    return Json(new { rs = false, msg = msg });
                }

                objKanban.Location = location;
                objKanban.CallBy = userLogin.Username;
                objKanban.CallDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                db.SaveChanges();

                var signalR = new KanbanModel()
                {
                    AssWo = objKanban.WLChild,
                    ActionType = (int)EnumHelper.Enum_Action.Call,
                    CallDate = DateTime.Parse(objKanban.CallDate).ToString("dd/MM/yyyy HH:mm:ss"),
                    LocationId = objKanban.TBL_GROUP_MST.GROUP_ID,
                    LocationName = objKanban.TBL_GROUP_MST.GROUP_NAME,
                    NewestAssWo = newestCall != null ? newestCall.WLChild : ""
                };
                GlobalHost.ConnectionManager.GetHubContext<SignalRConf>().Clients.Group("signalR").newMessageReceived(signalR);

                msg = "Call work order thành công/ The work order has been called successfully.";
                return Json(new { rs = true, msg = msg});
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        private ActionResult CPSend(string assWo, int actionTime)
        {
            string msg = string.Empty;
            try
            {
                var objKanban = db.TBL_KANBAN.Where(x => x.WLChild == assWo).FirstOrDefault();
                if (objKanban == null)
                {
                    msg = "Không tồn tại work order/ There is no exist work order.";
                    return Json(new { rs = false, msg = msg });
                }

                if (string.IsNullOrEmpty(objKanban.CallBy) && string.IsNullOrEmpty(objKanban.CallDate))
                {
                    msg = "Work order này chưa được call/ The work order has not been call.";
                    return Json(new { rs = false, msg = msg });
                }

                if (!string.IsNullOrEmpty(objKanban.CPSendBy) && !string.IsNullOrEmpty(objKanban.CPSendDate))
                {
                    msg = "Work order này đã được CP gửi/ The work order has been sent by CP.";
                    return Json(new { rs = false, msg = msg });
                }

                objKanban.CPSendBy = userLogin.Username;
                objKanban.CPSendDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                objKanban.CPTime = actionTime;

                db.SaveChanges();

                var signalR = new KanbanModel()
                {
                    AssWo = objKanban.WLChild,
                    ActionType = (int)EnumHelper.Enum_Action.CPSend
                };
                GlobalHost.ConnectionManager.GetHubContext<SignalRConf>().Clients.Group("signalR").newMessageReceived(signalR);

                msg = "CP send thành công/ CP send successfully.";
                return Json(new { rs = true, msg = msg });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        private ActionResult SPSend(string assWo, int actionTime)
        {
            string msg = string.Empty;
            try
            {
                var objKanban = db.TBL_KANBAN.Where(x => x.WLChild == assWo).FirstOrDefault();
                if (objKanban == null)
                {
                    msg = "Không tồn tại work order/ There is no exist work order.";
                    return Json(new { rs = false, msg = msg });
                }

                if (string.IsNullOrEmpty(objKanban.CallBy) && string.IsNullOrEmpty(objKanban.CallDate))
                {
                    msg = "Work order này chưa được call/ The work order has not been call.";
                    return Json(new { rs = false, msg = msg });
                }

                if (!string.IsNullOrEmpty(objKanban.SPSendBy) && !string.IsNullOrEmpty(objKanban.SPSendDate))
                {
                    msg = "Work order này đã được SP gửi/ The work order has been sent by SP.";
                    return Json(new { rs = false, msg = msg });
                }

                objKanban.SPSendBy = userLogin.Username;
                objKanban.SPSendDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                objKanban.SPTime = actionTime;

                db.SaveChanges();

                var signalR = new KanbanModel()
                {
                    AssWo = objKanban.WLChild,
                    ActionType = (int)EnumHelper.Enum_Action.SPSend
                };
                GlobalHost.ConnectionManager.GetHubContext<SignalRConf>().Clients.Group("signalR").newMessageReceived(signalR);

                msg = "SP send thành công/ SP send successfully.";
                return Json(new { rs = true, msg = msg });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        private ActionResult Complete(string assWo)
        {
            string msg = string.Empty;
            try
            {
                var objKanban = db.TBL_KANBAN.Where(x => x.WLChild == assWo).FirstOrDefault();
                if (objKanban == null)
                {
                    msg = "Không tồn tại work order/ There is no exist work order.";
                    return Json(new { rs = false, msg = msg });
                }

                if (string.IsNullOrEmpty(objKanban.CallBy) && string.IsNullOrEmpty(objKanban.CallDate))
                {
                    msg = "Work order này chưa được call/ The work order has not been call.";
                    return Json(new { rs = false, msg = msg });
                }

                if (string.IsNullOrEmpty(objKanban.CPSendBy) && string.IsNullOrEmpty(objKanban.CPSendDate))
                {
                    msg = "Work order này chưa được CP gửi/ The work order has not been sent by CP.";
                    return Json(new { rs = false, msg = msg });
                }

                if (string.IsNullOrEmpty(objKanban.SPSendBy) && string.IsNullOrEmpty(objKanban.SPSendDate))
                {
                    msg = "Work order này chưa được SP gửi/ The work order has not been sent by SP.";
                    return Json(new { rs = false, msg = msg });
                }

                // Save data to TBL_KANBAN_DATA
                var objKanbanData = new TBL_KANBAN_DATA()
                {
                    AsstWO = objKanban.AsstWO,
                    Qty = objKanban.Qty,
                    Priority = objKanban.Priority,
                    Location = objKanban.Location,
                    SewDate = objKanban.SewDate,
                    MnfColor = objKanban.MnfColor,
                    WLChild = objKanban.WLChild,
                    SellingStyle = objKanban.SellingStyle,
                    Size = objKanban.Size,
                    MnfStyle = objKanban.MnfStyle,
                    CallDate = objKanban.CallDate,
                    CallBy = objKanban.CallBy,
                    CPSendDate = objKanban.CPSendDate,
                    CPSendBy = objKanban.CPSendBy,
                    CPTime = objKanban.CPTime,
                    SPSendDate = objKanban.SPSendDate,
                    SPSendBy = objKanban.SPSendBy,
                    SPTime = objKanban.SPTime,
                    PrdComDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                    PrdComBy = userLogin.Username,
                    CancelDate = objKanban.CancelDate,
                    CancelBy = objKanban.CancelBy,
                    CancelReason = objKanban.CancelReason,
                    UploadDate = objKanban.UploadDate,
                    UploadBy = objKanban.UploadBy
                };

                // Trường hợp record được thêm từ action Cancel được call lại. Check tồn tại nếu có thì xóa đi
                var existedAssWo = db.TBL_KANBAN_DATA.Where(x => x.WLChild == objKanbanData.WLChild).FirstOrDefault(); 
                if (existedAssWo != null)
                {
                    db.TBL_KANBAN_DATA.Remove(existedAssWo);
                }

                db.TBL_KANBAN_DATA.Add(objKanbanData);

                // Remove data from TBL_KANBAN
                db.TBL_KANBAN.Remove(objKanban);
                db.SaveChanges();

                var signalR = new KanbanModel()
                {
                    AssWo = objKanban.WLChild,
                    ActionType = (int)EnumHelper.Enum_Action.Complete
                };
                GlobalHost.ConnectionManager.GetHubContext<SignalRConf>().Clients.Group("signalR").newMessageReceived(signalR);

                msg = "Hoàn thành work order/ The work order has been completed.";
                return Json(new { rs = true, msg = msg });
            }
            catch (Exception ex)
            {
                Utilities.WriteLogException(ex, MethodBase.GetCurrentMethod().Name);
                msg = "Error occurred. Error details: " + ex.Message;
                return Json(new { rs = false, msg = msg });
            }
        }

        public ActionResult Report(string fromDate, string toDate, int location, int reportType)
        {
            string msg = "";
            try
            {
                var listGroup = db.TBL_GROUP_MST.ToList();
                dynamic result = null;

                if (reportType == 1)
                {
                    result = db.TBL_KANBAN.Where(x => x.CallDate != null).ToList();
                }

                if (reportType == 2)
                {
                    result = db.PROC_GET_AUTO_KANBAN(fromDate, toDate, location).ToList();
                }

                // Create header row
                DataTable dtReport = new DataTable();
                dtReport.Columns.Add("AssWO");
                dtReport.Columns.Add("Qty");
                dtReport.Columns.Add("Priority");
                dtReport.Columns.Add("Location");
                dtReport.Columns.Add("Sew Date");

                dtReport.Columns.Add("WLChild");
                dtReport.Columns.Add("SellingStyle");
                dtReport.Columns.Add("Size");
                dtReport.Columns.Add("MnfStyle");
                dtReport.Columns.Add("MnfColor");

                dtReport.Columns.Add("Call Date");
                dtReport.Columns.Add("Call User");
                dtReport.Columns.Add("CP Send Date");
                dtReport.Columns.Add("CP Send User");
                dtReport.Columns.Add("CP Time");
                dtReport.Columns.Add("SP Send Date");
                dtReport.Columns.Add("SP Send User");
                dtReport.Columns.Add("SP Time");
                dtReport.Columns.Add("PrdCom Date");
                dtReport.Columns.Add("PrdCom User");
                dtReport.Columns.Add("Cancel Date");
                dtReport.Columns.Add("Cancel User");
                dtReport.Columns.Add("Cancel Reason");
                dtReport.Columns.Add("Upload Date");
                dtReport.Columns.Add("Upload User");

                // Add data row
                foreach (var row in result)
                {
                    var group = listGroup.Where(x => x.GROUP_ID == row.Location).FirstOrDefault();
                    DataRow dr = dtReport.NewRow();
                    dr["AssWO"] = row.AsstWO;
                    dr["Qty"] = row.Qty;
                    dr["Priority"] = row.Priority;
                    dr["Location"] = group.GROUP_NAME;
                    dr["Sew Date"] = row.SewDate;

                    dr["WLChild"] = row.WLChild;
                    dr["SellingStyle"] = row.SellingStyle;
                    dr["Size"] = row.Size;
                    dr["MnfStyle"] = row.MnfStyle;
                    dr["MnfColor"] = row.MnfColor;

                    dr["Call Date"] = row.CallDate;
                    dr["Call User"] = row.CallBy;
                    dr["CP Send Date"] = row.CPSendDate;
                    dr["CP Send User"] = row.CPSendBy;
                    dr["CP Time"] = row.CPTime;
                    dr["SP Send Date"] = row.SPSendDate;
                    dr["SP Send User"] = row.SPSendBy;
                    dr["SP Time"] = row.SPTime;
                    dr["PrdCom Date"] = row.PrdComDate;
                    dr["PrdCom User"] = row.PrdComBy;
                    dr["Cancel Date"] = row.CancelDate;
                    dr["Cancel User"] = row.CancelBy;
                    dr["Cancel Reason"] = row.CancelReason;
                    dr["Upload Date"] = row.UploadDate;
                    dr["Upload User"] = row.UploadBy;

                    dtReport.Rows.Add(dr);
                }
                // FileName
                string strFileName = "AutoKanban_" + DateTime.Now.ToString("MM_dd_yyyy_HH_mm");
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

        public ActionResult GetListLocation()
        {
            string msg = string.Empty;
            try
            {
                var result = db.TBL_GROUP_MST
                            .Select(x => new
                            {
                                id = x.GROUP_ID,
                                name = x.GROUP_NAME,
                            }).ToList();

                return Json(new { rs = true, msg = msg, data = result });
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