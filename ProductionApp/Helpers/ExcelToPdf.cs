using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using OfficeOpenXml;
using PdfRpt.Core.Contracts;
using PdfRpt.Core.Helper;
using PdfRpt.Core.PdfTable;
using PdfRpt.FluentInterface;
using ProductionApp.Models;

namespace ProductionApp.Helpers {




    public class ExcelDataReaderDataSource:IDataSource {
        //private readonly string _filePath;
        private readonly  ExcelPackage package;

        public ExcelDataReaderDataSource(ExcelPackage excel) {
            package = excel;

        }

        public IEnumerable<IList<CellData>> Rows() {


            var worksheet = package.Workbook.Worksheets[1];
            var startCell = worksheet.Dimension.Start;
            var endCell = worksheet.Dimension.End;

            for(var row = startCell.Row + 1; row < endCell.Row + 1; row++) {
                var i = 0;
                var result = new List<CellData>();
                for(var col = startCell.Column; col <= endCell.Column; col++) {
                    var pdfCellData = new CellData {
                        PropertyName = worksheet.Cells[1 ,col].Value.ToString() ,
                        PropertyValue = worksheet.Cells[row ,col].Value ,
                        PropertyIndex = i++
                    };
                    result.Add(pdfCellData);
                }
                yield return result;

            }
        }
    }

    public static class ExcelUtils {
        public static IList<string> GetColumns(ExcelPackage package) {
            var columns = new List<string>();

            var worksheet = package.Workbook.Worksheets[1];
            var startCell = worksheet.Dimension.Start;
            var endCell = worksheet.Dimension.End;

            for(int col = startCell.Column; col <= endCell.Column; col++) {
                var colHeader = worksheet.Cells[1 ,col].Value.ToString();
                columns.Add(colHeader);

            }
            return columns;
        }
    }

    public class ExcelToPdf {
        private PdfReport _PdfReport;
        public PdfReport CreateExcelToPdf(ExcelPackage excel ,UserModels user) {
            _PdfReport = new PdfReport().DocumentPreferences(doc => {
                doc.RunDirection(PdfRunDirection.LeftToRight);
                doc.Orientation(PageOrientation.Landscape);
                doc.PageSize(PdfPageSize.A4);
                doc.DocumentMetadata(new DocumentMetadata {
                    Author = "hihoang" ,
                    Application = "PdfRpt" ,
                    Keywords = "report" ,
                    Subject = "export Rpt" ,
                    Title = "report"
                });
                doc.Compression(new CompressionSettings {
                    EnableCompression = true ,
                    EnableFullCompression = true
                });
            }).PagesFooter(footer => {
                footer.DefaultFooter(user.Fullname + "-" + DateTime.Now.ToString("MM/dd/yyyy"));
            })

                .MainTableTemplate(template => { template.BasicTemplate(BasicTemplate.ClassicTemplate); })
                .MainTablePreferences(table => {
                    table.ColumnsWidthsType(TableColumnWidthType.FitToContent);
                    table.MultipleColumnsPerPage(new MultipleColumnsPerPage {
                        ColumnsGap = 7 ,
                        ColumnsPerPage = 3 ,
                        ColumnsWidth = 750 ,
                        IsRightToLeft = false ,
                        TopMargin = 7
                    });

                })
                .MainTableDataSource(dataSource => {
                    dataSource.CustomDataSource(() => new ExcelDataReaderDataSource(excel));
                })
                .MainTableColumns(columns => {
                    columns.AddColumn(column => {
                        column.PropertyName("rowNo");
                        column.IsRowNumber(true);
                        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                        column.IsVisible(true);
                        column.Order(0);
                        // column.Width(20);
                        column.HeaderCell("No.");
                    });

                    var order = 1;
                    foreach(var columnInfo in ExcelUtils.GetColumns(excel)) {
                        columns.AddColumn(column => {
                            column.PropertyName(columnInfo);
                            column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                            column.IsVisible(true);
                            column.Order(order++);
                            // column.Width(20);
                            column.HeaderCell(columnInfo);
                        });
                    }
                })
                .MainTableEvents(events => {
                    events.DataSourceIsEmpty(message :"There is no data available to display.");
                }).Export(export => {
                    export.ToXml();
                    export.ToExcel();
                });


            return _PdfReport;
        }

        public void SaveToLocalServer(string filename) {

            Random r = new Random();
            int rInt = r.Next(0 ,99999999);
            var path = Path.Combine(Path.Combine(HttpContext.Current.Server.MapPath(@"~\log\upload")) ,
                filename + "_" + rInt + ".pdf");

            _PdfReport.Generate(data => data.AsPdfFile(path));
        }



        public byte[] SaveToClient() {
            var pdfReportData=  _PdfReport.Generate(data => data.AsPdfStream(new MemoryStream()));

            return ((MemoryStream)pdfReportData.PdfStreamOutput).ToArray();

        }
    }
}