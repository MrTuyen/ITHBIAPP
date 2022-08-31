using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;

namespace ProductionApp.Helpers
{
    static public class ExcelHelper
    {
        static public string GetDataTableFromExcelFile(string fullPath, int sheetIndex, int rowIndex, out DataTable dt)
        {
            string msg = "";
            dt = null;

            string extension = Path.GetExtension(fullPath).ToLower();
            if (extension == ".xls")
                msg = GetDataTableFromXls(fullPath, sheetIndex, rowIndex, out dt);
            else if (extension == ".xlsx")
                msg = GetDataTableFromXlsx(fullPath, sheetIndex, rowIndex, out dt);
            else
                return "Phần mở rộng của File không phải là Excel (.xls hoặc .xlsx)";
            if (msg.Length > 0)
                return msg;

            return msg;
        }

        static public string GetDataTableFromXls(string fullPath, int sheetIndex, int rowIndex, out DataTable dt)
        {
            string msg = "";
            dt = null;
            try
            {
                FileInfo fi = new FileInfo(fullPath);
                if (!fi.Exists) return "File không tồn tại: " + fullPath;

                HSSFWorkbook hssfwb;
                using (FileStream file = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                {
                    hssfwb = new HSSFWorkbook(file);
                }

                ISheet sheet = hssfwb.GetSheetAt(sheetIndex);
                dt = new DataTable(sheet.SheetName);

                int maxCols = 0;
                foreach (IRow row in sheet)
                {
                    if (row.LastCellNum > maxCols)
                    {
                        maxCols = row.LastCellNum;
                        for (int i = 0; i < maxCols; i++)
                            dt.Columns.Add(row.Cells[i].StringCellValue.Replace("\n", string.Empty));
                    }
                    break;
                }

                for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
                {
                    IRow row = sheet.GetRow(i);
                    if (row == null) continue;
                    rowIndex++;
                    DataRow dataRow = dt.NewRow();
                    foreach (var c in row.Cells)
                    {
                        switch (c.CellType)
                        {
                            case CellType.Blank: dataRow[c.ColumnIndex] = string.Empty; break;
                            case CellType.Boolean: dataRow[c.ColumnIndex] = c.BooleanCellValue; break;
                            case CellType.Error: dataRow[c.ColumnIndex] = c.ErrorCellValue; break;
                            case CellType.Formula: dataRow[c.ColumnIndex] = c; break;
                            case CellType.String: dataRow[c.ColumnIndex] = c.StringCellValue; break;
                            case CellType.Unknown: dataRow[c.ColumnIndex] = c; break;
                            case CellType.Numeric:
                                if (DateUtil.IsCellDateFormatted(c)) dataRow[c.ColumnIndex] = c.DateCellValue.ToString("dd/MM/yyyy");
                                else dataRow[c.ColumnIndex] = c.NumericCellValue.ToString();

                                break;
                        }
                    }

                    dt.Rows.Add(dataRow);
                }
            }
            catch (Exception ex)
            {
                dt = null;
                msg = ex.ToString() + " @Row: #" + rowIndex;
            }
            return msg;
        }

        static public string GetDataTableFromXlsx(string fullPath, int sheetIndex, int rowIndex, out DataTable dt)
        {
            string msg = "";
            dt = null;
            int rowNum = rowIndex + 1; // dòng xuất phát để lấy dữ liệu vào các cột = dòng tiêu đề + 1
            int colNum = 1; // cột bắt đầu lấy dữ liệu
            //int rowStart = 1; // dòng header chứa tên các cột dữ liệu
            try
            {
                FileInfo fi = new FileInfo(fullPath);
                if (!fi.Exists) return "File không tồn tại: " + fullPath;

                using (var pck = new ExcelPackage())
                {
                    using (var stream = File.OpenRead(fullPath))
                    {
                        pck.Load(stream);
                    }
                    var ws = pck.Workbook.Worksheets[sheetIndex];

                    dt = new DataTable();
                    for (colNum = 1; colNum <= ws.Dimension.End.Column; colNum++)
                    {
                        if (ws.Cells[rowIndex, colNum].Value == null || ws.Cells[rowIndex, colNum].Value.ToString().Trim() == "")
                            continue;

                        string colName = ws.Cells[rowIndex, colNum].Value.ToString().Replace("\n", string.Empty);
                        dt.Columns.Add(colName);
                    }

                    for (; rowNum <= ws.Dimension.End.Row; rowNum++)
                    {
                        var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                        DataRow row = dt.Rows.Add();
                        foreach (var cell in wsRow)
                        {
                            colNum = 1;

                            string format = cell.Style.Numberformat.Format;
                            object value = cell.Value;
                            if (value == null)
                            {
                                //ConfigHelper.Instance.WriteLog($"Lỗi đọc dữ liệu từ file Excel. cell.{cell.Columns.ToString()}. Value: {cell.Value}", string.Empty, MethodBase.GetCurrentMethod().Name, "GetDataTableFromXlsx");
                            }
                            if (format.Contains("yyyy") || format.Contains("yy"))
                            {
                                var v = cell.Value;
                                DateTime date = DateTime.MinValue;
                                if (v is double) date = DateTime.FromOADate((double)v);
                                else if (v is DateTime) date = (DateTime)v;
                                else
                                {
                                    try
                                    {
                                        date = DateTime.ParseExact(v.ToString().Trim(), "MM/dd/yyyy", CultureInfo.InvariantCulture);
                                    }
                                    catch { }
                                }
                                if (date != DateTime.MinValue) value = date.ToString("MM/dd/yyyy");
                            }
                            row[cell.Start.Column - 1] = value == null ? DBNull.Value : value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                dt = null;
                msg = ex.ToString() + " @Row-Col: #" + rowNum + "-" + colNum;
            }
            return msg;
        }

        static public string GetColumnNamesFromDatable(DataTable dt, out string[] arrCol)
        {
            string msg = string.Empty;
            arrCol = null;
            try
            {
                arrCol = dt.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray(); //.Replace("\n", "")
            }
            catch (Exception ex)
            {
                arrCol = null;
                msg = ex.ToString();
            }
            return msg;
        }

        static public string GetAllWorksheets(string fullPath, out List<ExWorksheet> objSheets)
        {
            string msg = string.Empty;
            objSheets = new List<ExWorksheet>();
            try
            {
                FileInfo fi = new FileInfo(fullPath);
                if (!fi.Exists) return "File không tồn tại: " + fullPath;

                string extension = Path.GetExtension(fullPath).ToLower();
                if (extension == ".xls")
                {
                    HSSFWorkbook hssfwb;
                    using (FileStream file = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                    {
                        hssfwb = new HSSFWorkbook(file);
                    }
                    for (int i = 0; i < hssfwb.NumberOfSheets; i++)
                    {
                        ExWorksheet obj = new ExWorksheet();
                        ISheet sheet = hssfwb.GetSheetAt(i);
                        obj.Index = i;
                        obj.SheetName = sheet.SheetName;
                        objSheets.Add(obj);
                    }
                }
                else if (extension == ".xlsx")
                {
                    using (var pck = new ExcelPackage())
                    {
                        using (var stream = File.OpenRead(fullPath))
                        {
                            pck.Load(stream);
                        }
                        var ws = pck.Workbook.Worksheets;
                        foreach (var item in ws)
                        {
                            ExWorksheet obj = new ExWorksheet();
                            obj.Index = item.Index;
                            obj.SheetName = item.Name;
                            objSheets.Add(obj);
                        }
                    }
                }
                else return "Phần mở rộng của File không phải là Excel (.xls hoặc .xlsx)";
                if (msg.Length > 0) return msg;
            }
            catch (Exception ex)
            {
                msg = ex.ToString();
            }
            return msg;
        }
    }
}
