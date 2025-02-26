using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using ICSharpCode.AvalonEdit;
using System.Windows.Controls;
using System.Windows;
using Syncfusion.DocIO.DLS;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using Usml_Xml_Pdf_Viewer.Model;

namespace Usml_Xml_Pdf_Viewer.Service
{
    internal class GetPDFContent : Abstract.ViewModelBase
    {
        public string TopLeftRightText(IReadOnlyList<UglyToad.PdfPig.DocumentLayoutAnalysis.TextBlock> blocks, UglyToad.PdfPig.Content.Page pageText)
        {
            try
            {
                var topBlocks = blocks.Where(b => b.BoundingBox.Bottom > (pageText.Height * 2 / 3));
                //var topBlocks = blocks.OrderByDescending(b => b.BoundingBox.Top).Take(1);
                string topBlockText = string.Join(" ", topBlocks.Select(b => b.Text));
                return $"{topBlockText}".Trim();
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
                return null;
            }
        }

        public string BottomLeftRightText(IReadOnlyList<UglyToad.PdfPig.DocumentLayoutAnalysis.TextBlock> blocks, UglyToad.PdfPig.Content.Page pageText)
        {
            try
            {

                //var bottomBlocks = blocks.Where(b => b.BoundingBox.Top < (pageText.Height / 3));
                var bottomBlocks = blocks.OrderBy(b => b.BoundingBox.Bottom).Take(1);

                string bottomBlockText = string.Join(" ", bottomBlocks.Select(b => b.Text));
                return $"{bottomBlockText}".Trim();

                //// Define the bottom 10% of the page height as the area of interest
                //double bottomThreshold = pageText.Height * 0.1;

                //var bottomBlocks = blocks
                //    .Where(b => b.BoundingBox.Bottom <= bottomThreshold)  // Select blocks in the bottom 10%
                //    .OrderBy(b => b.BoundingBox.Bottom); // Sort to maintain order

                //var bottomBlocks = blocks.Where(b => b.BoundingBox.Bottom < (pageText.Height * 0.16)).OrderBy(b => b.BoundingBox.Bottom); 

                //var bottomBlocks = blocks.OrderBy(b => b.BoundingBox.Bottom).Take(1);

            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
                return null;
            }
        }

        public int FindPDFPageByContent(ObservableCollection<DocumentModel> DocumentCollection, string pageNumber, bool checkUpperOnly, bool checkBottomOnly, bool bothtopbottom)
        {
            try
            {
                DocumentModel topfoundPage = DocumentCollection.FirstOrDefault(doc => doc.TopContent.Contains(pageNumber) == true);
                DocumentModel bottomfoundPage = DocumentCollection.FirstOrDefault(doc => doc.BottomContent.Contains(pageNumber) == true);
                //DocumentModel bothtopbottomPage = DocumentCollection.FirstOrDefault(doc => doc.TopContent.Contains(pageNumber) == true || doc.BottomContent.Contains(pageNumber) == true);


                string pattern = $@"\b{Regex.Escape(pageNumber)}\b";
                DocumentModel bothtopbottomPage = DocumentCollection.FirstOrDefault(doc => Regex.IsMatch(doc.TopContent, pattern, RegexOptions.IgnoreCase) || Regex.IsMatch(doc.BottomContent, pattern, RegexOptions.IgnoreCase));


                if (topfoundPage == null && bottomfoundPage == null && bothtopbottomPage == null)
                {
                    //Console.WriteLine("Page number not found in the document collection.");
                    return 0;
                }

                if (checkUpperOnly && topfoundPage != null)
                {
                    return topfoundPage.pdfPage;
                }
                else if (checkBottomOnly && bottomfoundPage != null)
                {
                    return bottomfoundPage.pdfPage;
                }
                else if (bothtopbottom && bothtopbottomPage != null)
                {
                    return bothtopbottomPage.pdfPage;
                }

                return 0;
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
                return 0;
            }

        }



    }
}
