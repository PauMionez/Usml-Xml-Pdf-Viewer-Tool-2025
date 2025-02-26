using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Wpf;
using Syncfusion.Windows.Shared;
using Usml_Xml_Pdf_Viewer.Model;

namespace Usml_Xml_Pdf_Viewer.Service
{
    internal class ErrorReportHighlightText : Abstract.ViewModelBase
    {
        public async Task<(int pageNumber, string highlightedText, string xmlTag, int xmlLineNumber)> HighlightErrorTextService(ChromiumWebBrowser CefBrowsers, string xmlPath, string currentpage)
        {
            if (CefBrowsers.CanExecuteJavascriptInMainFrame)
            {
                string script = @"
                        (() => {
                            const selection = window.getSelection();
                            if (!selection || selection.rangeCount === 0) return null;

                            const range = selection.getRangeAt(0);
                            const highlightedText = range.toString().trim();
                            if (!highlightedText) return null;

                            let node = range.startContainer;

                            //Find the closest visible page number
                            const pages = document.querySelectorAll('page');
                            let visiblePage = null;

                            for (const page of pages) {
                                const rect = page.getBoundingClientRect();
                                if (rect.top < window.innerHeight && rect.bottom > 0) {
                                    visiblePage = page;
                                    break;
                                }
                            }

                            const pageNumber = visiblePage ? visiblePage.textContent.trim() : null;

                            //Find the closest XML tag that contains the highlighted text
                            let xmlNode = node;
                            while (xmlNode && xmlNode.nodeType !== Node.ELEMENT_NODE) {
                                xmlNode = xmlNode.parentElement;
                            }

                            const xmlTag = xmlNode ? xmlNode.tagName : null;
                            const xmlText = xmlNode ? xmlNode.textContent.trim() : null;

                            return {
                                pageNumber: pageNumber,
                                highlightedText: highlightedText,
                                xmlTag: xmlTag
                            };
                        })();";

                var response = await CefBrowsers.EvaluateScriptAsync(script);

                if (response.Success && response.Result is IDictionary<string, object> result)
                {
                    int pageNumber = 0;

                    if (pageNumber != 0)
                    {
                        if (result.ContainsKey("pageNumber"))
                        {
                            pageNumber = int.Parse(result["pageNumber"].ToString());
                        }
                    }
                    else { 
                        int.TryParse(currentpage, out pageNumber); 
                    } 
                    


                    string highlightedText = result.ContainsKey("highlightedText") ? result["highlightedText"].ToString() : null;
                    string xmlTag = result.ContainsKey("xmlTag") ? result["xmlTag"].ToString() : null;
                    int xmlLineNumber = highlightedText != null ? FindOfficialTitleTagWithText(xmlPath, highlightedText, xmlTag) : 0;

                    //InformationMessage($"Page: {pageNumber}, Text: \"{highlightedText}\", XML Tag: {xmlTag}, XML Line: {xmlLineNumber}", "Highlight Info");

                    //ErrorReportCollection.Add(new ErrorReportModel
                    //{
                    //    VisiblePage = pageNumber,
                    //    HighlightText = highlightedText,
                    //    LineNumberXML = xmlLineNumber,
                    //});
                    return (pageNumber, highlightedText, xmlTag, xmlLineNumber);
                }
            }
            return (0, null, null, 0);
        }

        public int FindOfficialTitleTagWithText(string xmlFilePath, string highlightedText, string xmlTag)
        {
            try
            {
                var lines = File.ReadAllLines(xmlFilePath);

                for (int i = 0; i < lines.Length; i++)
                {
                    // Check if the line contains the <officialTitle> tag and highlighted text
                    if (lines[i].Contains($"<{xmlTag}") && lines[i].Contains(highlightedText))
                    {
                        return i + 1;
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                WarningMessage($"Error finding <{xmlTag}> tag with text: {ex.Message}");
            }
            return 0;
        }
    }
}
