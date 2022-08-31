using CrystalDecisions.CrystalReports.Engine;
using ProductionApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProductionApp.Helpers;

namespace ProductionApp.Controllers
{
    public class CTTicketOrderFabricController:BaseController
    {
        // GET: CTTicketOrderFabric

        public ActionResult Index()
        {
            //Select User, dang ky Session["Department"]
            var usrLogged = (UserModels)Session["SignedInUser"];
            TBL_USERS_MST userLoginNow = new TBL_USERS_MST();
            if (usrLogged != null)
            {
                userLoginNow = db.TBL_USERS_MST.Find(usrLogged.Username);
                if (userLoginNow == null)
                {
                    return RedirectToAction("NeedLogin", "Notification", "Scan");
                }
                else
                {
                    Session["Department"] = userLoginNow.DEPT;
                }
            }
            else
            {
                return RedirectToAction("NeedLogin", "Notification", "Scan");
            }
            return View();
        }

        public ActionResult ConfirmReceive(string ticketNumberScan)
        {
            if ((UserModels)Session["SignedInUser"] == null)
            {
                return RedirectToAction("NeedLogin", "Notification", "Scan");
            }
            try
            {
                // Update status of label
                int idTicket = 0;
                if (ticketNumberScan == null) ticketNumberScan = "0";
                else
                {
                    ticketNumberScan = ticketNumberScan.Trim();
                    if (ticketNumberScan.Length == 11)
                    {
                        idTicket = Utilities.ConvertStringToInt(ticketNumberScan.Substring(6,5));
                    }
                    else if (ticketNumberScan.Length <= 5 && ticketNumberScan.Length >= 1)
                    {
                        idTicket = Utilities.ConvertStringToInt(ticketNumberScan);
                    }
                    else
                    {
                        idTicket = 0;
                    }
                    if (idTicket > 0)
                    {
                        string department = (String)Session["Department"]; ;
                        string userLogged = "";
                        UserModels usrLogged = (UserModels)Session["SignedInUser"];
                        userLogged = usrLogged.Username;

                        var findTickerViaTicketnumber = (from c in db.TBL_CT_TICKET_ORDER_FABRIC
                                                         where c.ID_TICKET == idTicket
                                                         select c).SingleOrDefault();
                        if (findTickerViaTicketnumber != null)
                        {
                            var updateTicketStatus = db.TBL_CT_TICKET_ORDER_FABRIC.Find(findTickerViaTicketnumber.ID_TICKET);
                            if ((department.Trim().ToUpper() == "WH" || department.Trim().ToUpper() == "Warehouse") && updateTicketStatus.STATUS == "CREATED")
                            {
                                updateTicketStatus.STATUS = "WH PROCESSING";
                                updateTicketStatus.WH_USER_RECEIVE = userLogged.ToString();
                                updateTicketStatus.WH_DATE_RECEIVE = db.PROC_GET_DATE_NOW().FirstOrDefault();
                                db.SaveChanges();
                            }
                            if ((department.Trim().ToUpper() != "WH" && department.Trim().ToUpper() != "Warehouse") && updateTicketStatus.STATUS != "RECEIVED")
                            {
                                updateTicketStatus.STATUS = "RECEIVED";
                                updateTicketStatus.USER_RECEIVE = userLogged.ToString();
                                updateTicketStatus.DATE_RECEIVE_FABRIC = db.PROC_GET_DATE_NOW().FirstOrDefault();
                                db.SaveChanges();
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            { }
            //Querry
            List<PROC_GET_TICKET_PROCESS_BY_STATUS_1_Result> lstTicketView = new List<PROC_GET_TICKET_PROCESS_BY_STATUS_1_Result>();
            lstTicketView = (from item in db.PROC_GET_TICKET_PROCESS_BY_STATUS_1() select item).ToList();
            return PartialView("_ConfirmReceive", lstTicketView);
        }

        public ActionResult AddNewTicket()
        {
            
            if ((UserModels)Session["SignedInUser"] == null)
            {
                return RedirectToAction("NeedLogin", "Notification", "Scan");
            }
            string department = (String)Session["Department"]; ;
            if (department.Trim().ToUpper() == "WH" || department.Trim().ToUpper() == "Warehouse")
            {
                return Content("<script language='javascript' type='text/javascript'>alert('Bạn không có quyền tạo ticket. Click nút BACK để quay về');</script>");
            }
            return View();
        }

        [HttpPost]
        public ActionResult AddNewTicket(FormCollection fc)
        {
            List<TBL_CT_TICKET_ORDER_FABRIC_DETAIL> listValueInView = new List<TBL_CT_TICKET_ORDER_FABRIC_DETAIL>();
            for (int i = 1; i <= 10; i++)
            {
                string nameCodeFabric = "txtCodeFabric" + i.ToString();
                string nameDetail = "txtDetail" + i.ToString();
                string nameYards = "txtYards" + i.ToString();
                string nameWO = "txtWO" + i.ToString();
                string nameAssortment = "txtAssortment" + i.ToString();
                string nameNoteTTS = "txtNoteTTS" + i.ToString();

                TBL_CT_TICKET_ORDER_FABRIC_DETAIL valueView = new TBL_CT_TICKET_ORDER_FABRIC_DETAIL();
                valueView.ORDER_IN_LIST = i;
                valueView.CODE_FABRIC = fc[nameCodeFabric];
                valueView.NOTE_DETAIL = fc[nameDetail];
                valueView.NUMBER_REQUEST = Utilities.ConvertStringToDouble(fc[nameYards]);
                valueView.WO = fc[nameWO];
                valueView.ASSORTMENT = fc[nameAssortment];
                valueView.NOTE_TTS = fc[nameNoteTTS];
                if(!string.IsNullOrEmpty(valueView.CODE_FABRIC) && !string.IsNullOrEmpty(valueView.WO) && !string.IsNullOrEmpty(valueView.ASSORTMENT) && valueView.NUMBER_REQUEST > 0)
                {
                    listValueInView.Add(valueView);
                }
                else if (string.IsNullOrEmpty(valueView.CODE_FABRIC) && string.IsNullOrEmpty(valueView.WO) && string.IsNullOrEmpty(valueView.ASSORTMENT) && valueView.NUMBER_REQUEST == 0)
                {
                    //ignore
                }
                else
                {
                    //TempData["msg"] = "<script>alert('Change succesfully');</script>";
                    
                    //return JavaScript("alert('Dòng "+i.ToString()+ ": Bạn phải điền đủ ở cột: Mã vải + Số yards + WO + Assortment. Click nút back để nhập lại'); $('#txtCodeFabric"+i.ToString()+"').focus();");
                    return Content("<script language='javascript' type='text/javascript'>alert('Dòng " + i.ToString() + ": Bạn phải điền đủ ở cột: Mã vải + Số yards + WO + Assortment. Click nút back để nhập lại'); $('#txtCodeFabric" + i.ToString() + "').focus(); </script>");
                }
            }
            UserModels loginInfor = (UserModels)Session["SignedInUser"];
            DateTime dateToday = db.PROC_GET_DATE_NOW().FirstOrDefault().Value;
            TBL_CT_TICKET_ORDER_FABRIC ticketNew = new TBL_CT_TICKET_ORDER_FABRIC();
            ticketNew.TICKET_NUMBER = dateToday.ToString("yy") + "-" + dateToday.ToString("MM") + "-" + ticketNew.ID_TICKET.ToString("00000");
            ticketNew.DATE_CREATE = dateToday;
            ticketNew.USER_CREATE = loginInfor.Username;
            ticketNew.DATE_APPROVE = dateToday;
            ticketNew.USER_APPROVE = loginInfor.Username;
            ticketNew.STATUS = "CREATED";
            ticketNew.DATE_NEED_FABRIC = dateToday;
            ticketNew.DATE_RECEIVE_FABRIC = Convert.ToDateTime("01/01/1990");
            ticketNew.USER_RECEIVE = "";
            ticketNew.TYPE_OF_TICKET = "GET_MORE";
            ticketNew.WH_DATE_RECEIVE = Convert.ToDateTime("01/01/1990");
            ticketNew.WH_USER_RECEIVE = "";
            db.TBL_CT_TICKET_ORDER_FABRIC.Add(ticketNew);
            db.SaveChanges();
            var updateTicketNumber = db.TBL_CT_TICKET_ORDER_FABRIC.Find(ticketNew.ID_TICKET);
            updateTicketNumber.TICKET_NUMBER = dateToday.ToString("yy") + "-" + dateToday.ToString("MM") + "-" + ticketNew.ID_TICKET.ToString("00000");
            db.SaveChanges();
            //Them chi tiet
            foreach (var detailIn in listValueInView)
            {
                TBL_CT_TICKET_ORDER_FABRIC_DETAIL ticketDetailAdd = new TBL_CT_TICKET_ORDER_FABRIC_DETAIL();
                ticketDetailAdd = detailIn;
                ticketDetailAdd.ID_TICKET = ticketNew.ID_TICKET;
                db.TBL_CT_TICKET_ORDER_FABRIC_DETAIL.Add(ticketDetailAdd);
            }
            db.SaveChanges();
            
            return RedirectToAction("ViewDetail", "CTTicketOrderFabric", new {id = ticketNew.ID_TICKET });
        }

        public ActionResult ViewDetail(string id)
        {
            try
            {
                //UserModels usrLogged = (UserModels)Session["SignedInUser"];
                //TBL_USERS_MST userLoginNow = new TBL_USERS_MST();
                //if (usrLogged != null)
                //{
                //    userLoginNow = db.TBL_USERS_MST.Find(usrLogged.Username);
                //    if (userLoginNow == null)
                //    {
                //        return RedirectToAction("NeedLogin", "Notification", "Scan");
                //    }
                //}
                //else
                //{
                //    return RedirectToAction("NeedLogin", "Notification", "Scan");
                //}

                int idTicket = Utilities.ConvertStringToInt(id);
                if (idTicket > 0)
                {
                    var findTicket = db.TBL_CT_TICKET_ORDER_FABRIC.Find(idTicket);
                    var finddetailInTicket = (from c in db.TBL_CT_TICKET_ORDER_FABRIC_DETAIL
                                              where c.ID_TICKET == idTicket
                                              select c).ToList();
                    return View(finddetailInTicket);
                }
                else
                {
                    return RedirectToAction("Index", "CTTicketOrderFabric");
                }
            }
            catch
            {
                return RedirectToAction("Index", "CTTicketOrderFabric");
            }
        }

        [HttpPost]
        public ActionResult PrintTicket(FormCollection fcTicket)
        {
            int idTicket = Utilities.ConvertStringToInt(fcTicket["txtIDTicketPrint"]);
            List<DataTicketOrderFabricToPrint> listToPrint = new List<DataTicketOrderFabricToPrint>();
            if (idTicket > 0)
            {
                var findListDetail = (from c in db.TBL_CT_TICKET_ORDER_FABRIC_DETAIL
                                      where c.ID_TICKET == idTicket
                                      select c).ToList();
                var findInforTicket = db.TBL_CT_TICKET_ORDER_FABRIC.Find(idTicket);
                foreach(var ain in findListDetail)
                {
                    DataTicketOrderFabricToPrint addInforToPrint = new DataTicketOrderFabricToPrint();
                    addInforToPrint.ID_TICKET_DETAIL = ain.ID_TICKET_DETAIL;
                    addInforToPrint.ID_TICKET = ain.ID_TICKET.Value;
                    addInforToPrint.ORDER_IN_LIST = ain.ORDER_IN_LIST.Value;
                    addInforToPrint.CODE_FABRIC = ain.CODE_FABRIC;
                    addInforToPrint.NOTE_DETAIL = ain.NOTE_DETAIL;
                    addInforToPrint.NUMBER_REQUEST = ain.NUMBER_REQUEST.Value;
                    addInforToPrint.WO = ain.WO;
                    addInforToPrint.ASSORTMENT = ain.ASSORTMENT;
                    addInforToPrint.NOTE_TTS = ain.NOTE_TTS;
                    addInforToPrint.TICKET_NUMBER = findInforTicket.TICKET_NUMBER;
                    addInforToPrint.DATE_CREATE = findInforTicket.DATE_CREATE.Value;
                    listToPrint.Add(addInforToPrint);
                }

                ReportDocument reportPrint = new ReportDocument();
                reportPrint.Load(Path.Combine(Server.MapPath("~/CrystalReport"), "crptPrintTicketGetMoreFabric.rpt"));



                DataTable listSourceDataToPrint = new DataTable();
                listSourceDataToPrint.Columns.Add("ID_TICKET_DETAIL", typeof(int));
                listSourceDataToPrint.Columns.Add("ID_TICKET", typeof(int));
                listSourceDataToPrint.Columns.Add("ORDER_IN_LIST", typeof(int));

                listSourceDataToPrint.Columns.Add("CODE_FABRIC", typeof(string));
                listSourceDataToPrint.Columns.Add("NOTE_DETAIL", typeof(string));
                listSourceDataToPrint.Columns.Add("NUMBER_REQUEST", typeof(double));

                listSourceDataToPrint.Columns.Add("WO", typeof(string));
                listSourceDataToPrint.Columns.Add("ASSORTMENT", typeof(string));
                listSourceDataToPrint.Columns.Add("NOTE_TTS", typeof(string));
                listSourceDataToPrint.Columns.Add("TICKET_NUMBER", typeof(string));
                listSourceDataToPrint.Columns.Add("DATE_CREATE", typeof(DateTime));

                foreach(var ain in listToPrint)
                {
                    DataRow dr = listSourceDataToPrint.NewRow();
                    dr["ID_TICKET_DETAIL"] = ain.ID_TICKET_DETAIL;
                    dr["ID_TICKET"] = ain.ID_TICKET;
                    dr["ORDER_IN_LIST"] = ain.ORDER_IN_LIST;

                    dr["CODE_FABRIC"] = ain.CODE_FABRIC;
                    dr["NOTE_DETAIL"] = ain.NOTE_DETAIL;
                    dr["NUMBER_REQUEST"] = ain.NUMBER_REQUEST;

                    dr["WO"] = ain.WO;
                    dr["ASSORTMENT"] = ain.ASSORTMENT;
                    dr["NOTE_TTS"] = ain.NOTE_TTS;

                    dr["TICKET_NUMBER"] = ain.TICKET_NUMBER;
                    dr["DATE_CREATE"] = ain.DATE_CREATE;
                    listSourceDataToPrint.Rows.Add(dr);
                }

                reportPrint.SetDataSource(listSourceDataToPrint);
                //reportPrint.PrintOptions.PrinterName = GetDefaultPrinter();
                //reportPrint.PrintToPrinter(2,false,1,1);
                Response.Buffer = false;
                Response.ClearContent();
                Response.ClearHeaders();


                Stream stream = reportPrint.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", "TicketNeedPrint.pdf");
                //return RedirectToAction("ViewDetail", "CTTicketOrderFabric", new {id = idTicket});
            }
            return RedirectToAction("Index", "CTTicketOrderFabric");
        }
    }
}