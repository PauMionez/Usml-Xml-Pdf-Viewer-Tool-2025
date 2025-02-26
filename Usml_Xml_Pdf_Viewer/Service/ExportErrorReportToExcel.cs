using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syncfusion.XlsIO;
using Usml_Xml_Pdf_Viewer.Model;

namespace Usml_Xml_Pdf_Viewer.Service
{
    internal class ExportErrorReportToExcel : Abstract.ViewModelBase
    {
        public async Task ExportErrorReportService(string xmlpath, ObservableCollection<ErrorReportModel> ErrorReportCollection)
        {
            try
            {
                // Get the directory and filename (without extension) of the input PDF
                string inputDirectory = Path.GetDirectoryName(xmlpath);
                string inputFileNameWithoutExt = Path.GetFileNameWithoutExtension(xmlpath);
                string datetimeMark = DateTime.Now.ToString("MM_dd_yyyy_HH_mm_ss");
                string outExcelFile = Path.Combine(inputDirectory, $"{inputFileNameWithoutExt}_Error_Report_{datetimeMark}.xlsx");

                #region Excel
                using (ExcelEngine excelEngine = new ExcelEngine())
                {
                    IApplication application = excelEngine.Excel;
                    IWorkbook wb = application.Workbooks.Create();
                    IWorksheet ws = wb.Worksheets[0];

                    #region Column headers

                    ws.Range[1, 1].Text = "Filename";
                    ws.Range[1, 2].Text = "Page Number";
                    ws.Range[1, 3].Text = "XML Line Number";
                    ws.Range[1, 4].Text = "Highlighted Text";
                    ws.Range[1, 5].Text = "Generic Errors";
                    ws.Range[1, 6].Text = "Remarks";


                    int lastCol = ws.UsedRange.LastColumn;

                    //Header excel design
                    ws.Range[1, 1, 1, lastCol].CellStyle.Font.Bold = true;
                    ws.Range[1, 1, 1, lastCol].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    ws.Range[1, 1, 1, lastCol].CellStyle.Font.Color = ExcelKnownColors.White;
                    ws.Range[1, 1, 1, lastCol].CellStyle.Color = System.Drawing.Color.FromArgb(42, 118, 189);
                    #endregion

                    #region Data

                    for (int row = 0; row < ErrorReportCollection.Count; row++)
                    {
                        ErrorReportModel record = ErrorReportCollection[row];
                        ws[row + 2, 1].Text = record.FileName;
                        ws[row + 2, 2].Number = record.VisiblePage;
                        ws[row + 2, 3].Number = record.LineNumberXML;
                        ws[row + 2, 4].Text = record.HighlightText;
                        ws[row + 2, 5].Text = record.Generic;
                        ws[row + 2, 6].Text = record.Remarks;

                        //center align the data
                        for (int column = 1; column <= 100; column++)
                        {
                            ws[1, column].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            ws[row + 2, column].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                        }
                    }
                    #endregion

                    ws.UsedRange.AutofitColumns();

                    await Task.Run(() =>
                    {
                        using (FileStream filestream = new FileStream(outExcelFile, FileMode.Create, FileAccess.ReadWrite))
                        {
                            wb.SaveAs(filestream);
                        }
                    });
                }

                #endregion

                Process.Start(outExcelFile);
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }

        }
    }
}
