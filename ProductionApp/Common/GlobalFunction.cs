using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ProductionApp.Common
{
    public class GlobalFunction
    {
        /// <summary>
        /// Return byte array
        /// </summary>
        /// <param name="dtData"></param>
        /// <returns></returns>
        public static byte[] Export2XLS(System.Data.DataTable dtData)
        {
            using (ExcelPackage pck = new ExcelPackage())
            {
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Sheet1");

                //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                ws.Cells["A1"].LoadFromDataTable(dtData, true);

                //Format the header for column 1-3
                using (ExcelRange rng = ws.Cells["A1:BZ1"])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;   //Set Pattern for the background to Solid
                    rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.WhiteSmoke);  //Set color to dark blue
                    rng.Style.Font.Color.SetColor(System.Drawing.Color.Black);
                }

                for (int i = 0; i < dtData.Columns.Count; i++)
                {
                    if (dtData.Columns[i].DataType == typeof(DateTime))
                    {
                        using (ExcelRange col = ws.Cells[2, i + 1, 2 + dtData.Rows.Count, i + 1])
                        {
                            //col.Style.Numberformat.Format = "MM/dd/yyyy HH:mm";
                            col.Style.Numberformat.Format = "dd/MM/yyyy HH:mm";
                            //col.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                        }
                    }
                    if (dtData.Columns[i].DataType == typeof(TimeSpan))
                    {
                        using (ExcelRange col = ws.Cells[2, i + 1, 2 + dtData.Rows.Count, i + 1])
                        {
                            col.Style.Numberformat.Format = "d.hh:mm";
                            col.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        }
                    }

                }
                return pck.GetAsByteArray();
            }
        }

        /// <summary>
        /// Save file
        /// </summary>
        /// <param name="dtData"></param>
        /// <returns></returns>
        public static void SaveDataTableToExcel(System.Data.DataTable dtData, string fileName)
        {
            using (ExcelPackage pck = new ExcelPackage())
            {
                //Create the worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Sheet1");

                //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                ws.Cells["A1"].LoadFromDataTable(dtData, true);

                //Format the header for column 1-3
                using (ExcelRange rng = ws.Cells["A1:BZ1"])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;   //Set Pattern for the background to Solid
                    rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.WhiteSmoke);  //Set color to dark blue
                    rng.Style.Font.Color.SetColor(System.Drawing.Color.Black);
                }

                for (int i = 0; i < dtData.Columns.Count; i++)
                {
                    if (dtData.Columns[i].DataType == typeof(DateTime))
                    {
                        using (ExcelRange col = ws.Cells[2, i + 1, 2 + dtData.Rows.Count, i + 1])
                        {
                            //col.Style.Numberformat.Format = "MM/dd/yyyy HH:mm";
                            col.Style.Numberformat.Format = "dd/MM/yyyy HH:mm";
                            //col.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                        }
                    }
                    if (dtData.Columns[i].DataType == typeof(TimeSpan))
                    {
                        using (ExcelRange col = ws.Cells[2, i + 1, 2 + dtData.Rows.Count, i + 1])
                        {
                            col.Style.Numberformat.Format = "d.hh:mm";
                            col.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        }
                    }

                }

                // Save file to physical disk
                using (MemoryStream ms = new MemoryStream())
                {
                    pck.SaveAs(ms);
                    ms.WriteTo(new FileStream(fileName, FileMode.Create));
                    ms.Close();
                }
            }
        }
    }
}