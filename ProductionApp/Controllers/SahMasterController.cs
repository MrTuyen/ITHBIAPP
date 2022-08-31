using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml;
using ProductionApp.Helpers;
using ProductionApp.Models;

namespace ProductionApp.Controllers
{
    public class SahMasterController:BaseController
    {
        // GET: MasterData
        public ActionResult Index()
        {
            Session["UploadSahMaster"] = "Done";
            return View("UploadSahMaster");
        }

        public ActionResult UploadSahMaster()
        {
            Session["UploadSahMaster"] = "Uploading...";
            if (Request != null)
            {
                int MesRow = 0;
                try
                {

                    HttpPostedFileBase file = Request.Files["UploadedFile"];
                    if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                    {
                        var mss = "";
                        string fileName = file.FileName;
                        string fileContentType = file.ContentType;
                        byte[] fileBytes = new byte[file.ContentLength];
                        var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                        var list = new List<TBL_SAH_MST>();
                      
                        using (var package = new ExcelPackage(file.InputStream))
                        {
                            var currentSheet = package.Workbook.Worksheets;
                            var workSheet = currentSheet.First();
                            var noOfCol = workSheet.Dimension.End.Column;
                            var noOfRow = workSheet.Dimension.End.Row;
                          
                            for (var rowIterator = 3; rowIterator <= noOfRow; rowIterator++)
                            {
                                MesRow = rowIterator;
                                var Pkg_Style = workSheet.Cells[rowIterator, 1].Value;
                                var Sel_Style = "";
                                var Mnf_Style = workSheet.Cells[rowIterator, 2].Value;
                                var status = workSheet.Cells[rowIterator, 3].Value;
                                var sah = workSheet.Cells[rowIterator, 4].Value;
                                var color = workSheet.Cells[rowIterator, 5].Value;
                                var sizeCD = workSheet.Cells[rowIterator, 6].Value;
                                var sizeDs = workSheet.Cells[rowIterator, 7].Value;

                                if (Pkg_Style == null || Pkg_Style.ToString() == "") break;

                                //var SAH_Record = db.TBL_SAH_MST.SingleOrDefault(t => t.MnfStyle == Mnf_Style.ToString() && t.SizeCD == (string)sizeCD);
                                //if (SAH_Record != null)
                                //{
                                //    db.TBL_SAH_MST.Remove(SAH_Record);
                                //}

                                var oneRecord = new TBL_SAH_MST
                                {
                                    SAH = Convert.ToDouble(sah),
                                    Sel_Style = Sel_Style.ToString(),
                                    MnfStyle = Mnf_Style.ToString(),
                                    SizeCD = sizeCD.ToString(),
                                    Size_Des = sizeDs.ToString(),
                                    Color = color.ToString(),
                                    Pkg_Style = Pkg_Style.ToString(),
                                    Status = status.ToString(),
                                    TS_USER = userLogin.Username ,
                                    TS = DateTime.Now
                                };
                                list.Add(oneRecord);
                                //db.TBL_SAH_MST.Add(oneRecord);
                                //var result = db.SaveChanges();
                                // if (result == ErrorCode.EntityValidation)
                                //{
                                //    mss += "</br>Kiểm tra lại dữ liệu,  dòng " + Convert.ToString(MesRow);
                                //}
                            }

                        }
                        db.Database.ExecuteSqlCommand("delete from TBL_SAH_MST ");
                        var table = ToDataTable(list);
                        var conString = ConfigurationManager.ConnectionStrings["ProductionAppEntities"].ConnectionString;
                        if(conString.ToLower().StartsWith("metadata=")) {
                            System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder efBuilder = new System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder(conString);
                            conString = efBuilder.ProviderConnectionString;
                        }
                        using(SqlConnection con = new SqlConnection(conString)) {
                            using(SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con)) {
                                //Set the database table name.
                                sqlBulkCopy.DestinationTableName = "TBL_SAH_MST";
                                //[OPTIONAL]: Map the Excel columns with that of the database table
                                sqlBulkCopy.ColumnMappings.Add("SAH" ,"SAH");
                                sqlBulkCopy.ColumnMappings.Add("Sel_Style" ,"Sel_Style");
                                sqlBulkCopy.ColumnMappings.Add("MnfStyle" ,"MnfStyle");
                                sqlBulkCopy.ColumnMappings.Add("SizeCD" ,"SizeCD");
                                sqlBulkCopy.ColumnMappings.Add("Size_Des" ,"Size_Des");
                                sqlBulkCopy.ColumnMappings.Add("Color" ,"Color");
                                sqlBulkCopy.ColumnMappings.Add("Pkg_Style" ,"Pkg_Style");
                                sqlBulkCopy.ColumnMappings.Add("Status" ,"Status");
                                sqlBulkCopy.ColumnMappings.Add("TS_USER" ,"TS_USER");
                                sqlBulkCopy.ColumnMappings.Add("TS" ,"TS");
                                con.Open();
                                sqlBulkCopy.BulkCopyTimeout = 0;
                                sqlBulkCopy.WriteToServer(table);
                                con.Close();
                            }

                        }


                        Session["UploadSahMaster"] = "Done";
                        ViewBag.Status = mss == "" ? "Successful" : mss;
                    }
                }

                catch (Exception e)
                {
                    ViewBag.Status = "Error, need contact to IT. " + e.Message + ",  Row " + Convert.ToString(MesRow);
                    Session["UploadSahMaster"] = "Done";
                    Utilities.WriteLogException(e, "UploadSahMaster");
                }
            }
            return View("UploadSahMaster");
        }
        public JsonResult Status(String size)
        {
            return Json(Session["UploadSahMaster"], JsonRequestBehavior.AllowGet);

        }
        public DataTable ToDataTable<T>(IList<T> data) {
            var properties =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach(PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name ,Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach(T item in data) {
                DataRow row = table.NewRow();
                foreach(PropertyDescriptor prop in properties) {
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                }
                table.Rows.Add(row);
            }
            return table;

        }
    }
}