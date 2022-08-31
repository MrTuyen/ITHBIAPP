using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web.Mvc;
using OfficeOpenXml;
using ProductionApp.Helpers;
using ProductionApp.Models;

namespace ProductionApp.Controllers {
    public class MUVController:BaseController {
        // GET: MUV
        public ActionResult Index(FormCollection fr) {
            ViewBag.Group = db.TBL_GROUP_MST.Where(a => a.Activate == 1).Distinct().OrderBy(a => a.GROUP_NAME).ToList();

            if(fr["week"] == null || fr["GROUP_ID"] == null)
                return View();


            var week = int.Parse(fr["week"]);
            string fweek = fr["week"];
            var groupNames = fr["GROUP_ID"].Split(',');
            var data = //BOM   & case
                (from j in
                     (
                       from b in
                           (from b1 in db.MUV_BOM
                            from b2 in db.MUV_Actual.Select(a1 => new { a1.Week ,a1.Item ,a1.AsstWL }).Distinct()
                            where b2.AsstWL.Substring(2 ,6) == b1.AsstWO
                            where b2.Week == week
                            where "00 01 02 03 04 05 06 07 08 09".Contains( b2.AsstWL.Substring(8 ,2)) 
                            //.Where(b21 => b21.Week == week).Select(b21 => b21.AsstWL).Distinct()
                            select new {
                                b2.Week ,                                                        
                                b2.AsstWL ,
                                b1.Item ,
                                b1.MUV_ITEMS.MUV_GROUP_ITEM.NAME ,
                                b1.AsstWO ,
                                Quantity = b1.Quantity * b1.MUV_ITEMS.Cost
                            }
                            )
                       join a in db.MUV_Actual.Where(c => c.Week == week).Select(y => new {
                           y.Item ,
                           y.MUV_ITEMS.MUV_GROUP_ITEM.NAME ,
                           y.AsstWL ,
                           Quantity = y.Quantity * y.MUV_ITEMS.Cost
                       }).Where(a => a.Quantity != null)
                           on new { Item = b.Item.Substring(0 ,6) ,AsstWL = b.AsstWO } equals new { Item = a.Item.Substring(0 ,6) ,AsstWL = a.AsstWL.Substring(2 ,6) } into joined
                       from j in joined.DefaultIfEmpty()
                       where j == null
                       select b)
                 from c in db.TBL_CASE_LABEL
                     .Where(c => groupNames.Any(g => g.Contains(c.TBL_GROUP_MST.GROUP_NAME))).Select(d =>
                         new { d.WLOT_ID ,d.TBL_GROUP_MST ,d.WK ,d.SIZE ,d.MnfStyle ,d.PkgStyle ,d.COLOR }).Distinct()
                 where c.WLOT_ID.Substring(2 ,6) == j.AsstWO
                 select new MuExport() {
                     GroupName = c.TBL_GROUP_MST.GROUP_NAME ,
                     asstWO = j.AsstWL.Substring(2 ,6) ,
                     Document = j.AsstWL ,
                     Item = j.Item.Substring(0 ,6) ,
                     ItemGroup = j.NAME ,
                     Week = fweek ,
                     Size = c.SIZE ,
                     MnfStyle = c.MnfStyle ,
                     PkgStyle = c.PkgStyle ,
                     Color = c.COLOR ,
                     Actual = 0 ,
                     Standard = j.Quantity ,
                     Var = 0 - ("00 01 02 03 04 05 06 07 08 09".Contains(j.AsstWL.Substring(8 ,2)) ? j.Quantity : 0) ,
                     Type = c.WLOT_ID.Substring(8 ,2) ,
                 }
                )
                .Concat

                (
                //Actual & case
                    from j in
                        (from a in db.MUV_Actual.Where(c => c.Week == week).Select(y => new {
                            y.Item ,
                            y.Week ,
                            y.MUV_ITEMS.MUV_GROUP_ITEM.NAME ,
                            y.AsstWL ,
                            Quantity = y.Quantity * y.MUV_ITEMS.Cost
                        })
                         join b in db.MUV_BOM.Where(c => c.Week == week).Select(y => new {
                             y.Item ,
                             y.MUV_ITEMS.MUV_GROUP_ITEM.NAME ,
                             y.AsstWO ,
                             Quantity = y.Quantity * y.MUV_ITEMS.Cost
                         }).Where(a => a.Quantity != null)
                             on new { Item = a.Item.Substring(0 ,6) ,AsstWL = a.AsstWL.Substring(2 ,6) } equals new { Item = b.Item.Substring(0 ,6) ,AsstWL = b.AsstWO } into joined
                         from j in joined.DefaultIfEmpty()
                         where j == null
                         select a)
                    from c in db.TBL_CASE_LABEL
                        .Where(c => groupNames.Any(g => g.Contains(c.TBL_GROUP_MST.GROUP_NAME))).Select(d =>
                            new { d.WLOT_ID ,d.TBL_GROUP_MST ,d.WK ,d.SIZE ,d.MnfStyle ,d.PkgStyle ,d.COLOR }).Distinct()
                    where c.WLOT_ID.Substring(2 ,6) == j.AsstWL.Substring(2 ,6)
                    select new MuExport() {
                        GroupName = c.TBL_GROUP_MST.GROUP_NAME ,
                        asstWO = j.AsstWL.Substring(2 ,6) ,
                        Document = j.AsstWL ,
                        Item = j.Item.Substring(0 ,6) ,
                        ItemGroup = j.NAME ,
                        Week = j.Week.ToString() ,
                        Size = c.SIZE ,
                        MnfStyle = c.MnfStyle ,
                        PkgStyle = c.PkgStyle ,
                        Color = c.COLOR ,
                        Actual = j.Quantity ?? 0 ,
                        Standard = 0 ,
                        Var = ("00 01 02 03 04 05 06 07 08 09".Contains(j.AsstWL.Substring(8 ,2)) ? j.Quantity ?? 0 : 0) ,
                        Type = j.AsstWL.Substring(8 ,2) ,
                    }
                )
                .Concat(
                //actual not case
                    from j in
                        (from a in db.MUV_Actual.Where(y => y.Week == week).Select(y => new {
                            y.Item ,
                            y.Week ,
                            y.MUV_ITEMS.MUV_GROUP_ITEM.NAME ,
                            y.AsstWL ,
                            Quantity = y.Quantity * y.MUV_ITEMS.Cost
                        })
                         join b in db.TBL_CASE_LABEL.Select(y => new { y.WLOT_ID }).Distinct()
                             on new { AsstWL = a.AsstWL.Substring(2 ,6) } equals new { AsstWL = b.WLOT_ID.Substring(2 ,6) } into joined
                         from j in joined.DefaultIfEmpty()
                         where j == null
                         select a)
                    select new MuExport() {
                        GroupName = "Other" ,
                        asstWO = j.AsstWL.Substring(2 ,6) ,
                        Document = j.AsstWL ,
                        Item = j.Item.Substring(0 ,6) ,
                        ItemGroup = j.NAME ,
                        Week = j.Week.ToString() ,
                        Size = "" ,
                        MnfStyle = "" ,
                        PkgStyle = "" ,
                        Color = "" ,
                        Actual = j.Quantity ?? 0 ,
                        Standard = 0 ,
                        Var = ("00 01 02 03 04 05 06 07 08 09".Contains(j.AsstWL.Substring(8 ,2)) ? j.Quantity ?? 0 : 0) ,
                        Type = j.AsstWL.Substring(8 ,2) ,
                    }
                )
                //actual + standard & case
                .Concat(
                    (from a in db.MUV_Actual.Where(c => c.Week == week).Select(y => new {
                        y.Week ,
                        y.Item ,
                        y.MUV_ITEMS.MUV_GROUP_ITEM.NAME ,
                        y.AsstWL ,
                        Quantity = y.Quantity * y.MUV_ITEMS.Cost
                    })
                     from b in db.MUV_BOM.Where(c => c.Week == week).Select(y => new {
                         y.Item ,
                         y.MUV_ITEMS.MUV_GROUP_ITEM.NAME ,
                         y.AsstWO ,
                         Quantity = y.Quantity * y.MUV_ITEMS.Cost
                     })
                     from c in db.TBL_CASE_LABEL
                         .Where(c => groupNames.Any(g => g.Contains(c.TBL_GROUP_MST.GROUP_NAME))).Select(d =>
                             new { d.WLOT_ID ,d.TBL_GROUP_MST ,d.WK ,d.SIZE ,d.MnfStyle ,d.PkgStyle ,d.COLOR })
                         .Distinct()
                     where c.WLOT_ID.Substring(2 ,6) == a.AsstWL.Substring(2 ,6)
                           && a.AsstWL.Substring(2 ,6) == b.AsstWO
                           && a.Item.Substring(0 ,6) == b.Item.Substring(0 ,6)
                     select new MuExport() {
                         GroupName = c.TBL_GROUP_MST.GROUP_NAME ,
                         asstWO = a.AsstWL.Substring(2 ,6) ,
                         Document = a.AsstWL ,
                         Item = a.Item.Substring(0 ,6) ,
                         ItemGroup = a.NAME ,
                         Week = a.Week.ToString() ,
                         Size = c.SIZE ,
                         MnfStyle = c.MnfStyle ,
                         PkgStyle = c.PkgStyle ,
                         Color = c.COLOR ,
                         Actual = a.Quantity ?? 0 ,
                         Standard = "00 01 02 03 04 05 06 07 08 09".Contains(a.AsstWL.Substring(8 ,2))
                             ? b.Quantity ?? 0
                             : 0 ,
                         Var = ("00 01 02 03 04 05 06 07 08 09".Contains(a.AsstWL.Substring(8 ,2)) ? a.Quantity - b.Quantity : 0) ,
                         Type = a.AsstWL.Substring(8 ,2) ,
                     }));

            if(fr["hanhdong"] != "Export") {
                var list = data.Distinct().GroupBy(x => new {
                    x.ItemGroup ,
                    x.GroupName ,
                    x.Week

                }).Select(newA => new MuView() {
                    Item = newA.Key.ItemGroup ,
                    GroupName = newA.Key.GroupName ,
                    Week = newA.Key.Week ,
                    DM = newA.Sum(c => (c.Actual - c.Standard)) ?? 0 ,
                    Standard = newA.Sum(c => c.Standard) ?? 0 ,
                    Actual = newA.Sum(c => c.Actual) ?? 0
                })
                    .Concat(from a in db.MUV_Actual
                            where a.Week == week - 1 && (a.AsstWL.Length != 10)
                            group a by new {
                                a.MUV_ITEMS.MUV_GROUP_ITEM ,
                                a.Week
                            }
                                into newA
                                select new MuView() {
                                    Item = newA.Key.MUV_GROUP_ITEM.NAME ,
                                    GroupName = "Other" ,
                                    Week = newA.Key.Week.ToString() ,
                                    DM =
                                        newA.Sum(c => (c.Quantity < 0 ? c.Quantity * -1 : c.Quantity) * c.MUV_ITEMS.Cost) ?? 0 ,
                                    Standard = 0 ,
                                    Actual =
                                        newA.Sum(c => (c.Quantity < 0 ? c.Quantity * -1 : c.Quantity) * c.MUV_ITEMS.Cost) ?? 0
                                }).Where(e => !db.MUV_BOM.Any(m => m.Item == e.Item)).OrderBy(a => new { a.GroupName ,a.Item })
                    .ToList();




                ViewBag.fgroupId = fr["GROUP_ID"];
                ViewBag.week = week;
                return View(list);

            }


            var excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Week #" + week);


            var muExports = data.Distinct()
                .OrderBy(a => new { a.GroupName ,a.asstWO ,a.Item }).ToList().GroupBy(c => new {
                    c.asstWO ,
                    c.Document ,
                    c.ItemGroup ,
                    c.Item ,
                    c.Week ,
                    c.Size ,
                    c.MnfStyle ,
                    c.PkgStyle ,
                    c.Color ,
                    //c.Actual ,
                    //c.Standard ,
                    //c.Var ,
                    c.Type ,
                })
                .Select(c => new {
                    GroupName = string.Join(", " ,c.Select(x => x.GroupName).Distinct()) ,
                    asstWO = c.Key.asstWO ,
                    Document = c.Key.Document ,
                    ItemGroup = c.Key.ItemGroup ,
                    Item = c.Key.Item ,
                    Week = c.Key.Week ,
                    Size = c.Key.Size ,
                    MnfStyle = c.Key.MnfStyle ,
                    PkgStyle = c.Key.PkgStyle ,
                    Color = c.Key.Color ,
                    Standard = c.Sum(a => a.Standard) ,
                    Actual = c.Sum(a => a.Actual) ,
                    Var = c.Sum(a => a.Var) ,
                    Type = ValidType(c.Key.Type) ,

                });

            //  muExports.ForEach(m => { m.Type = ValidType(m.Type); });
            workSheet.Cells[1 ,1].LoadFromCollection(muExports ,true);

            using(var col = workSheet.Cells[1 ,1 ,muExports.Count() + 1 ,13]) {
                col.AutoFitColumns();
            }

            workSheet.Cells["A1:N1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            workSheet.Cells["A1:N1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightSkyBlue);
            workSheet.Cells["A1:N1"].Style.Font.Bold = true;
            workSheet.Cells["A1:N1"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            workSheet.Cells["A1:N1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            var start = workSheet.Dimension.Start;
            var end = workSheet.Dimension.End;
            var xanh = System.Drawing.Color.CadetBlue;
            var cam = System.Drawing.Color.DarkOrange;
            var dotham = System.Drawing.Color.Crimson;
            for(int row = start.Row + 1; row <= end.Row; row++) {

                var color = dotham;
                //if(double.Parse(workSheet.Cells[row ,12].Text) == 0)
                //    color = xanh;
                //else 
                if(double.Parse(workSheet.Cells[row ,12].Text.Replace("-" ,"")) -
                    double.Parse(workSheet.Cells[row ,11].Text) <= 0)
                    color = xanh;
                var index = string.Format("K{0}:M{0}" ,row);
                workSheet.Cells[index].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                workSheet.Cells[index].Style.Fill.BackgroundColor.SetColor(color);
            }

            using(var memoryStream = new MemoryStream()) {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition" ,"attachment;  filename=MU-Week #" + week + ".xlsx");
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }

            return View();
        }

        // UPLOAD BOM AND ACTUAL 
        string ValidType(string Type) {

            if(Type == "")
                return "";
            if("00 01 02 03 04 05 06 07 08 09".Contains(Type))
                return "Cấp lần đầu";
            if("10 11 12 13 14 15 16 17 18 19".Contains(Type))
                return "Cấp bù";
            if("20 21 22 23 24 25 26 27 28 29".Contains(Type))
                return "Cấp thêm";
            if("30 31 32 33 34 35 36 37 38 39".Contains(Type))
                return "Trả hàng";
            //if("00 01 02 03 04 05 06 07 08 09".Contains(Type))
            //    return "Cấp lần đầu: " + Type;
            //if("10 11 12 13 14 15 16 17 18 19".Contains(Type))
            //    return "Cấp bù: " + Type;
            //if("20 21 22 23 24 25 26 27 28 29".Contains(Type))
            //    return "Cấp thêm: " + Type;
            //if("30 31 32 33 34 35 36 37 38 39".Contains(Type))
            //    return "Trả hàng: " + Type;
            return "";
        }

        public JsonResult UploadMuv(FormCollection fr) {
            var rdoModule = fr["RdoModule"];

            var mesRow = 0;
            var mss = "";
            try {
                Session["UploadMuv"] = "Hệ thống đang xử lý";
                var file = Request.Files["UploadedFile"];
                if((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName)) {
                    var fileName = file.FileName;
                    var fileContentType = file.ContentType;
                    var fileBytes = new byte[file.ContentLength];
                    var data = file.InputStream.Read(fileBytes ,0 ,Convert.ToInt32(file.ContentLength));
                    using(var package = new ExcelPackage(file.InputStream)) {
                        var currentSheet = package.Workbook.Worksheets;
                        var workSheet = currentSheet.First();
                        var noOfCol = workSheet.Dimension.End.Column;
                        var noOfRow = workSheet.Dimension.End.Row;

                        switch(rdoModule) {
                            case "Actual": {

                                    var fWeek = Utilities.GetIso8601WeekOfYear(Convert.ToDateTime(workSheet.Cells[3 ,1].Value.ToString()));
                                    var listActual = new List<MUV_Actual>();
                                    //var caseLabels = db.TBL_CASE_LABEL;
                                    for(var rowIterator = 3; rowIterator <= noOfRow; rowIterator++) {
                                        if(workSheet.Cells[rowIterator ,1].Value == null)
                                            break;
                                        mesRow = rowIterator;

                                        var issueDate = Convert.ToDateTime(workSheet.Cells[rowIterator ,1].Value.ToString());
                                        var Week = Utilities.GetIso8601WeekOfYear(issueDate);
                                        var asstWl = workSheet.Cells[rowIterator ,2].Value.ToString().Trim();
                                        var item = workSheet.Cells[rowIterator ,3].Value.ToString().Trim();
                                        var quantity = workSheet.Cells[rowIterator ,4].Value.ToString().Trim();
                                        var cost = workSheet.Cells[rowIterator ,5].Value.ToString().Trim();
                                        var record = new MUV_Actual {
                                            IssueDate = issueDate ,
                                            AsstWL = asstWl ,
                                            Item = item ,
                                            Week = Week ,
                                            Quantity = quantity == "" ? 0 : double.Parse(quantity) * -1 ,
                                            Cost = cost == "" ? 0 : double.Parse(cost) ,
                                        };

                                        fWeek = (int)(fWeek > record.Week ? record.Week : fWeek);
                                        listActual.Add(record);
                                    }
                                    db.Database.ExecuteSqlCommand("delete from MUV_Actual where Week >= " + fWeek + " or Week < " + fWeek + " - 4");
                                    var table = ToDataTable(listActual);

                                    var conString = ConfigurationManager.ConnectionStrings["ProductionAppEntities"].ConnectionString;
                                    if(conString.ToLower().StartsWith("metadata=")) {
                                        System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder efBuilder = new System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder(conString);
                                        conString = efBuilder.ProviderConnectionString;
                                    }
                                    using(SqlConnection con = new SqlConnection(conString)) {
                                        using(SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con)) {
                                            //Set the database table name.
                                            sqlBulkCopy.DestinationTableName = "MUV_Actual";
                                            //[OPTIONAL]: Map the Excel columns with that of the database table
                                            sqlBulkCopy.ColumnMappings.Add("IssueDate" ,"IssueDate");
                                            sqlBulkCopy.ColumnMappings.Add("AsstWL" ,"AsstWL");
                                            sqlBulkCopy.ColumnMappings.Add("Item" ,"Item");
                                            sqlBulkCopy.ColumnMappings.Add("Week" ,"Week");
                                            sqlBulkCopy.ColumnMappings.Add("GroupName" ,"GroupName");
                                            sqlBulkCopy.ColumnMappings.Add("Quantity" ,"Quantity");
                                            sqlBulkCopy.ColumnMappings.Add("Cost" ,"Cost");
                                            con.Open();
                                            sqlBulkCopy.BulkCopyTimeout = 0;
                                            sqlBulkCopy.WriteToServer(table);
                                            con.Close();
                                        }

                                    }

                                    Session["UploadMuv"] = "NONE";
                                    mss += "Success!";
                                    break;
                                }

                            case "Bom": {
                                    var fWeek = int.Parse(workSheet.Cells[3 ,9].Value.ToString().Trim());

                                    var listBom = new List<MUV_BOM>();
                                    for(var rowIterator = 3; rowIterator <= noOfRow; rowIterator++) {
                                        if(workSheet.Cells[rowIterator ,1].Value == null)
                                            break;
                                        mesRow = rowIterator;
                                        var compWo = workSheet.Cells[rowIterator ,1].Value == null ? "" : workSheet.Cells[rowIterator ,1].Value.ToString().Trim();
                                        var asstWo = workSheet.Cells[rowIterator ,2].Value == null ? "" : workSheet.Cells[rowIterator ,2].Value.ToString().Trim();
                                        var mnfStyle = workSheet.Cells[rowIterator ,3].Value == null ? "" : workSheet.Cells[rowIterator ,3].Value.ToString().Trim();
                                        var selStyle =workSheet.Cells[rowIterator ,4].Value == null ? "" : workSheet.Cells[rowIterator ,4].Value.ToString().Trim();
                                        var pkgStyle = workSheet.Cells[rowIterator ,5].Value == null ? "" : workSheet.Cells[rowIterator ,5].Value.ToString().Trim();
                                        var color = workSheet.Cells[rowIterator ,6].Value == null ? "" : workSheet.Cells[rowIterator ,6].Value.ToString().Trim();
                                        var wc =  workSheet.Cells[rowIterator ,7].Value == null ? "" : workSheet.Cells[rowIterator ,7].Value.ToString().Trim();
                                        var size = workSheet.Cells[rowIterator ,8].Value == null ? "" : workSheet.Cells[rowIterator ,8].Value.ToString().Trim();
                                        var week = workSheet.Cells[rowIterator ,9].Value == null ? "" : workSheet.Cells[rowIterator ,9].Value.ToString().Trim();
                                        var quantity =workSheet.Cells[rowIterator ,11].Value == null ? "" : workSheet.Cells[rowIterator ,11].Value.ToString().Trim();
                                        var item = workSheet.Cells[rowIterator ,10].Value == null ? "" : workSheet.Cells[rowIterator ,10].Value.ToString().Trim();

                                        var record = new MUV_BOM() {
                                            Item = item ,
                                            Color = color ,
                                            SelStyle = selStyle ,
                                            PkgStyle = pkgStyle ,
                                            MnfStyle = mnfStyle ,
                                            Quantity = float.Parse(quantity) ,
                                            Size = size ,
                                            AsstWO = "000000".Substring(asstWo.Length) + asstWo ,
                                            CompWO = "000000".Substring(compWo.Length) + compWo ,
                                            WC = wc ,
                                            Week = int.Parse(week)

                                        };
                                        fWeek = (int)(fWeek > record.Week ? record.Week : fWeek);
                                        listBom.Add(record);
                                    }

                                    db.Database.ExecuteSqlCommand("delete from MUV_BOM where Week >= " + fWeek + " or Week < " + fWeek + " - 4");
                                    var table = ToDataTable(listBom);

                                    var conString = ConfigurationManager.ConnectionStrings["ProductionAppEntities"].ConnectionString;
                                    if(conString.ToLower().StartsWith("metadata=")) {
                                        System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder efBuilder = new System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder(conString);
                                        conString = efBuilder.ProviderConnectionString;
                                    }
                                    using(SqlConnection con = new SqlConnection(conString)) {
                                        using(SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con)) {
                                            //Set the database table name.
                                            sqlBulkCopy.DestinationTableName = "MUV_BOM";
                                            //[OPTIONAL]: Map the Excel columns with that of the database table
                                            sqlBulkCopy.ColumnMappings.Add("Item" ,"Item");
                                            sqlBulkCopy.ColumnMappings.Add("Color" ,"Color");
                                            sqlBulkCopy.ColumnMappings.Add("SelStyle" ,"SelStyle");
                                            sqlBulkCopy.ColumnMappings.Add("PkgStyle" ,"PkgStyle");
                                            sqlBulkCopy.ColumnMappings.Add("MnfStyle" ,"MnfStyle");
                                            sqlBulkCopy.ColumnMappings.Add("Quantity" ,"Quantity");
                                            sqlBulkCopy.ColumnMappings.Add("Size" ,"Size");
                                            sqlBulkCopy.ColumnMappings.Add("AsstWO" ,"AsstWO");
                                            sqlBulkCopy.ColumnMappings.Add("CompWO" ,"CompWO");
                                            sqlBulkCopy.ColumnMappings.Add("WC" ,"WC");
                                            sqlBulkCopy.ColumnMappings.Add("Week" ,"Week");
                                            con.Open();
                                            sqlBulkCopy.BulkCopyTimeout = 0;
                                            sqlBulkCopy.WriteToServer(table);
                                            con.Close();
                                        }

                                    }


                                    Session["UploadMuv"] = "NONE";
                                    mss += "Success!";
                                    break;
                                }

                            default:
                                mss += "Vui lòng chọn loại tài liệu";
                                break;
                        }
                    }
                }
            } catch(Exception e) {
                mss += "need contact to IT. " + e.Message + ",  row " + Convert.ToString(mesRow);
                Utilities.WriteLogException(e ,ViewBag.Status);
                Session["UploadMuv"] = "NONE";
            }
            Session["UploadMuv"] = "NONE";
            return Json(mss ,JsonRequestBehavior.AllowGet);
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
        public JsonResult Status() {
            return Json(Session["UploadMuv"] ?? "NONE" ,JsonRequestBehavior.AllowGet);

        }

        public ActionResult Items(FormCollection fr ,string editID ,string deleteID) {
            var muvGroupItem = db.MUV_GROUP_ITEM.SingleOrDefault(a => a.ID == editID) ?? new MUV_GROUP_ITEM();
            try {
                if(deleteID != null) {
                    db.MUV_GROUP_ITEM.RemoveRange(db.MUV_GROUP_ITEM.Where(a => a.ID == deleteID));
                    var result = db.SaveChanges();
                    switch(result) {
                        case -2:
                            ViewBag.mss = "Cannot Delete ! / Không thể xóa";
                            break;
                        default:
                            return RedirectToAction("Items");
                    }


                }
                if(HttpContext.Request.HttpMethod == HttpMethod.Post.Method) {

                    muvGroupItem.ID = fr["ID"];
                    muvGroupItem.NAME = fr["NAME"];
                    if(editID == null)
                        db.MUV_GROUP_ITEM.Add(muvGroupItem);
                    var result = db.SaveChanges();
                    if(result > 0) {
                        ViewBag.mss = "Success!";
                        muvGroupItem = new MUV_GROUP_ITEM();
                        if(editID != null) {
                            return RedirectToAction("Items");
                        }
                    } else
                        switch(result) {
                            case -1:
                                ViewBag.mss = "ID exist! / Mã đã tồn tại";
                                break;
                            case -3:
                                ViewBag.mss = "Data invalid! / Dữ liệu không đúng";
                                break;
                        }
                }
            } catch(Exception e) {
                ViewBag.mss = "System error, Please contact to Admin! /Lỗi hệ thống, Vui lòng liên hệ quản trị viên";
                Utilities.WriteLogException(e ,ViewBag.Status);
            }
            ViewBag.listGroupItem = db.MUV_GROUP_ITEM.OrderBy(a => a.NAME).ToList();
            return View(muvGroupItem);
        }

        public JsonResult UploadItems(FormCollection fr) {
            var rdoModule = fr["RdoModule"];

            var mesRow = 0;
            var mss = "";
            try {
                Session["Uploaditems"] = "Hệ thống đang xử lý";
                var file = Request.Files["UploadedFile"];
                if((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName)) {
                    var fileName = file.FileName;
                    var fileContentType = file.ContentType;
                    var fileBytes = new byte[file.ContentLength];
                    var data = file.InputStream.Read(fileBytes ,0 ,Convert.ToInt32(file.ContentLength));
                    using(var package = new ExcelPackage(file.InputStream)) {
                        var currentSheet = package.Workbook.Worksheets;
                        var workSheet = currentSheet.First();
                        var noOfCol = workSheet.Dimension.End.Column;
                        var noOfRow = workSheet.Dimension.End.Row;
                        var groupItems = db.MUV_GROUP_ITEM;
                        var table = new DataTable();
                        table.Columns.AddRange(new DataColumn[3] { new DataColumn("ItemID", typeof(string)),
                            new DataColumn("GroupID", typeof(string)),
                            new DataColumn("Cost",typeof(float)) });
                        var lsgrItems= new List<MUV_GROUP_ITEM>();
                        for(var rowIterator = 3; rowIterator <= noOfRow; rowIterator++) {
                            if(workSheet.Cells[rowIterator ,1].Value == null)
                                break;
                            mesRow = rowIterator;

                            var itemId = workSheet.Cells[rowIterator ,1].Value.ToString().Trim();
                            var cost = workSheet.Cells[rowIterator ,2].Value.ToString().Trim();
                            var groupId = workSheet.Cells[rowIterator ,3].Value.ToString().Trim();
                            var groupName = workSheet.Cells[rowIterator ,4].Value.ToString().Trim();
                            // var groupItem = groupItems.FirstOrDefault(a => a.ID == groupId);
                            if(!lsgrItems.Any(a => a.ID == groupId))
                                lsgrItems.Add(new MUV_GROUP_ITEM() { ID = groupId ,NAME = groupName });
                            table.Rows.Add(itemId ,groupId ,float.Parse(cost));
                        }



                        var conString = ConfigurationManager.ConnectionStrings["ProductionAppEntities"].ConnectionString;
                        if(conString.ToLower().StartsWith("metadata=")) {
                            System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder efBuilder = new System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder(conString);
                            conString = efBuilder.ProviderConnectionString;
                        }
                        //upload group
                        var gRtable = new DataTable();
                        gRtable.Columns.AddRange(new DataColumn[2] { new DataColumn("ID", typeof(string)),
                            new DataColumn("NAME", typeof(string)) });

                        var lsGRs = lsgrItems.Distinct().ToList();
                        foreach(var muvGroupItem in lsGRs) {
                            gRtable.Rows.Add(muvGroupItem.ID ,muvGroupItem.NAME);
                        }
                        using(var con = new SqlConnection(conString)) {
                            using(var cmd = new SqlCommand("Proc_Update_MU_GROUPITEMS")) {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Connection = con;
                                cmd.Parameters.AddWithValue("@tblCustomers" ,gRtable);
                                con.Open();
                                cmd.ExecuteNonQuery();
                                con.Close();
                            }
                        }


                        //upload Item
                        using(var con = new SqlConnection(conString)) {
                            using(var cmd = new SqlCommand("Proc_Update_MU_ITEMS")) {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Connection = con;
                                cmd.Parameters.AddWithValue("@tblCustomers" ,table);
                                con.Open();
                                cmd.ExecuteNonQuery();
                                con.Close();
                            }
                        }



                        Session["Uploaditems"] = "NONE";
                        mss += "Success!";
                    }
                }
            } catch(Exception e) {
                mss += "need contact to IT. " + e.Message + ",  row " + Convert.ToString(mesRow);
                Utilities.WriteLogException(e ,ViewBag.Status);
                Session["Uploaditems"] = "NONE";
            }
            Session["Uploaditems"] = "NONE";
            return Json(mss ,JsonRequestBehavior.AllowGet);
        }
        public JsonResult StatusItems() {
            return Json(Session["UploadItems"] ?? "NONE" ,JsonRequestBehavior.AllowGet);

        }
        public JsonResult LoadWeekActual() {
            var actuals = db.MUV_Actual.Where(t =>
                t.GroupName == "Not Found");

            foreach(var item in actuals) {
                var caseLabel = db.TBL_CASE_LABEL.FirstOrDefault(a => a.WLOT_ID == item.AsstWL);

                if(caseLabel != null)
                    item.GroupName = caseLabel.TBL_GROUP_MST.GROUP_NAME;
            }
            db.SaveChanges();
            return Json("Success!" ,JsonRequestBehavior.AllowGet);

        }
    }
}