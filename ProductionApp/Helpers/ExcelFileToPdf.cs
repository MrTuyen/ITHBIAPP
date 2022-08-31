using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Castle.Core.Internal;
using OfficeOpenXml;
using PdfRpt.Core.Contracts;
using PdfRpt.FluentInterface;


namespace ProductionApp.Helpers
{

   

    public class ExcelFileReaderDataSource : IDataSource
    {
        private readonly string _filePath;
        private readonly int _worksheet;

        public ExcelFileReaderDataSource(string filePath, int worksheet)
        {
            _filePath = filePath;
            _worksheet = worksheet;
        }

        public IEnumerable<IList<CellData>> Rows()
        {
            var fileInfo = new FileInfo(_filePath);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException("{_filePath} file not found.");
            }

            using (var package = new ExcelPackage(fileInfo))
            {
                var worksheet = package.Workbook.Worksheets[_worksheet];
                var startCell = worksheet.Dimension.Start;
                var endCell = worksheet.Dimension.End;
               
                for (var row = startCell.Row + 1; row < endCell.Row + 1; row++)
                {
                    var i = 0;
                    var result = new List<CellData>();
                    for (var col = startCell.Column; col <= endCell.Column; col++)
                    {
                        if (worksheet.Cells[1, col].IsNullOrEmpty()) continue;
                        try
                        {
                            var pdfCellData = new CellData
                            {
                                PropertyName = worksheet.Cells[1, col].Value.ToString(),
                                PropertyValue = worksheet.Cells[row, col].Value,
                                PropertyIndex = i++
                            };
                            result.Add(pdfCellData);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                    yield return result;
                }

            }
        }
    }

    public static class ExcelFileUtils
    {
        public static IList<string> GetColumns(string filePath, int excelWorksheet)
        {
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException("{filePath} file not found.");
            }

            var columns = new List<string>();
            using (var package = new ExcelPackage(fileInfo))
            {
                var worksheet = package.Workbook.Worksheets[excelWorksheet];
                var startCell = worksheet.Dimension.Start;
                var endCell = worksheet.Dimension.End;
                for (var col = startCell.Column; col <= endCell.Column; col++)
                {
                    if (worksheet.Cells[1, col].IsNullOrEmpty()) continue;
                    try
                    {
                        var colHeader = worksheet.Cells[1, col].Value.ToString();
                        columns.Add(colHeader);
                    }
                    catch (Exception e)
                    {
                        continue;
                    }

                }

                   
            }
            return columns;
        }
    }

   
    public class ExcelFileToPdf
    {
        private PdfReport _PdfReport;
        public PdfReport ReadExcelFile(string filePath, int excelWorksheet)
        {
            _PdfReport= new PdfReport().DocumentPreferences(doc =>
                {
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
                })

                .MainTableTemplate(template => { template.BasicTemplate(BasicTemplate.ClassicTemplate); })
                .MainTablePreferences(table => {
                    table.ColumnsWidthsType(TableColumnWidthType.FitToContent);
                    table.MultipleColumnsPerPage(new MultipleColumnsPerPage {
                       // ColumnsGap = 7 ,
                       // ColumnsPerPage = 3 ,
                        ColumnsWidth = 750 ,
                        IsRightToLeft = false ,
                        TopMargin = 7
                    });

                })
                .MainTableDataSource(dataSource =>
                {
                    dataSource.CustomDataSource(() => new ExcelFileReaderDataSource(filePath, excelWorksheet));
                })
                .MainTableColumns(columns =>
                {

                    var order = 1;
                    foreach (var columnInfo in ExcelFileUtils.GetColumns(filePath, excelWorksheet))
                    {
                        columns.AddColumn(column =>
                        {
                            column.PropertyName(columnInfo);
                            column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                            column.IsVisible(true);
                            column.Order(order++);
                            //column.Width(1);
                            column.HeaderCell(columnInfo);
                        });
                    }
                })
                .MainTableEvents(events =>
                {
                    events.DataSourceIsEmpty(message: "There is no data available to display.");
                });
            //.Generate(data => data.AsPdfFile(TestUtils.GetOutputFileName()));
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