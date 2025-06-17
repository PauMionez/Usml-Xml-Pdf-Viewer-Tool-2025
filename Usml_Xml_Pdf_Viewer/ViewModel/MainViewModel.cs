using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CefSharp;
using CefSharp.Wpf;
using ControlzEx.Standard;
using DevExpress.Mvvm;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Utils;
using Syncfusion.Windows.PdfViewer;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using Usml_Xml_Pdf_Viewer.Model;
using Usml_Xml_Pdf_Viewer.Service;
using System.Diagnostics;
using HandyControl.Controls;
using HandyControl.Data;

namespace Usml_Xml_Pdf_Viewer.ViewModel
{
    internal class MainViewModel : Abstract.ViewModelBase
    {

        #region Commands
        public AsyncCommand<DocumentViewer> SelectPDFCommand { get; private set; }
        public AsyncCommand<TextEditor> AvalonTextEditor_LoadedCommand { get; private set; }
        public DevExpress.Mvvm.DelegateCommand<PdfViewerControl> CurrentPageChanged { get; private set; }

        public DevExpress.Mvvm.DelegateCommand XMLViewerMouseHoverCommand { get; private set; }
        public DevExpress.Mvvm.DelegateCommand PDFViewerMouseHoverCommand { get; private set; }
        public DevExpress.Mvvm.DelegateCommand CSSViewerMouseHoverCommand { get; private set; }
        public DevExpress.Mvvm.DelegateCommand<ChromiumWebBrowser> LoadXmlCSSWebCommand { get; private set; }
        public DevExpress.Mvvm.DelegateCommand SearchCssButtonCommand { get; private set; }
        public DevExpress.Mvvm.DelegateCommand SearchXmlButtonCommand { get; private set; }
        public DevExpress.Mvvm.DelegateCommand SaveUpdateXmlFileCommand { get; private set; }
        public DevExpress.Mvvm.AsyncCommand HighlightErrorTextCommand { get; private set; }
        public DevExpress.Mvvm.AsyncCommand ExportErrorReportCommand { get; private set; }
        public DevExpress.Mvvm.AsyncCommand<MouseWheelEventArgs> ZoomBrowserViewerCommand { get; private set; }
        public DevExpress.Mvvm.AsyncCommand<MouseWheelEventArgs> ZoomXmlTextViewerCommand { get; private set; }
        public DevExpress.Mvvm.AsyncCommand RemoveXMLHeaderTagCommand { get; private set; }
        public DevExpress.Mvvm.DelegateCommand<string> SaveErrorRemarkCommand { get; private set; }
        public DevExpress.Mvvm.DelegateCommand UploadGenericErrorFileCommand { get; private set; }
        public DevExpress.Mvvm.DelegateCommand CloseOrResetAllCommand { get; private set; }
        public DevExpress.Mvvm.DelegateCommand ScreenShotCommand { get; private set; }
        //public DevExpress.Mvvm.DelegateCommand SpellCheckerCommand { get; private set; }
        public DevExpress.Mvvm.DelegateCommand RemoveSpellcheckerTagsCommand { get; private set; }
        public DevExpress.Mvvm.DelegateCommand AddIdOnSidenoteCommand { get; private set; }
        public DevExpress.Mvvm.DelegateCommand LinkSidenoteCommand { get; private set; }
        public DevExpress.Mvvm.DelegateCommand AddPlaceholderTagSCommand { get; private set; }
        public DevExpress.Mvvm.DelegateCommand ShortcutTagCommand { get; private set; }

        #endregion



        public MainViewModel()
        {
            #region Initial Commands
            SelectPDFCommand = new AsyncCommand<DocumentViewer>(SelectPDFFile);
            AvalonTextEditor_LoadedCommand = new AsyncCommand<TextEditor>(AvalonTextEditor_Loaded);

            CurrentPageChanged = new DevExpress.Mvvm.DelegateCommand<PdfViewerControl>(PDFPageScroll);
            XMLViewerMouseHoverCommand = new DevExpress.Mvvm.DelegateCommand(OnXMLViewerMouseHover);
            PDFViewerMouseHoverCommand = new DevExpress.Mvvm.DelegateCommand(OnPDFViewerMouseHover);
            CSSViewerMouseHoverCommand = new DevExpress.Mvvm.DelegateCommand(OnCssViewerMouseHover);
            LoadXmlCSSWebCommand = new DevExpress.Mvvm.DelegateCommand<ChromiumWebBrowser>(LoadWebBrowserViewer);
            SearchCssButtonCommand = new DevExpress.Mvvm.DelegateCommand(SearchTextInCss);
            SearchXmlButtonCommand = new DevExpress.Mvvm.DelegateCommand(SearchTextInXml);
            SaveUpdateXmlFileCommand = new DevExpress.Mvvm.DelegateCommand(SaveUpdateXmlFile);
            SaveErrorRemarkCommand = new DevExpress.Mvvm.DelegateCommand<string>(SaveErrorRemark);
            UploadGenericErrorFileCommand = new DevExpress.Mvvm.DelegateCommand(UploadGenericErrorFile);
            CloseOrResetAllCommand = new DevExpress.Mvvm.DelegateCommand(CloseOrResetAll);
            ScreenShotCommand = new DevExpress.Mvvm.DelegateCommand(ScreenShot);


            //SpellCheckerCommand = new DevExpress.Mvvm.DelegateCommand(SpellChecker);
            RemoveSpellcheckerTagsCommand = new DevExpress.Mvvm.DelegateCommand(RemoveSpellcheckerTags);
            AddIdOnSidenoteCommand = new DevExpress.Mvvm.DelegateCommand(AddIdOnSidenote);
            LinkSidenoteCommand = new DevExpress.Mvvm.DelegateCommand(LinkSidenote);
            AddPlaceholderTagSCommand = new DevExpress.Mvvm.DelegateCommand(AddPlaceholderTagS);
            ShortcutTagCommand = new DevExpress.Mvvm.DelegateCommand(ShortcutTag);

            HighlightErrorTextCommand = new DevExpress.Mvvm.AsyncCommand(HighlightErrorText);
            ExportErrorReportCommand = new DevExpress.Mvvm.AsyncCommand(ExportErrorReport);
            ZoomBrowserViewerCommand = new DevExpress.Mvvm.AsyncCommand<MouseWheelEventArgs>(ZoomBrowserViewer);
            ZoomXmlTextViewerCommand = new DevExpress.Mvvm.AsyncCommand<MouseWheelEventArgs>(ZoomXmlTextViewer);
            RemoveXMLHeaderTagCommand = new DevExpress.Mvvm.AsyncCommand(RemoveXMLHeaderTag);
            #endregion


            DocumentCollection = new ObservableCollection<DocumentModel>();
            XmlTagsCollection = new ObservableCollection<xmlTagModel>();
            ErrorReportCollection = new ObservableCollection<ErrorReportModel>();
            SidenoteCollection = new ObservableCollection<SidenoteModel>();
            GenericErrorList = new ObservableCollection<string>();
            LoadingStatusPdf = Visibility.Hidden;
            IsErrorInputGrid = Visibility.Hidden;
            XmlTextFontSize = 12;
            IsXmlViewerChecked = true;
        }

        #region Properties

        private ObservableCollection<DocumentModel> _DocumentCollection;
        public ObservableCollection<DocumentModel> DocumentCollection
        {
            get { return _DocumentCollection; }
            set { _DocumentCollection = value; OnPropertyChanged(); }
        }

        private ObservableCollection<xmlTagModel> _xmlTagsCollection;
        public ObservableCollection<xmlTagModel> XmlTagsCollection
        {
            get { return _xmlTagsCollection; }
            set { _xmlTagsCollection = value; OnPropertyChanged(); }
        }

        private ObservableCollection<ErrorReportModel> _errorReportCollection;
        public ObservableCollection<ErrorReportModel> ErrorReportCollection
        {
            get { return _errorReportCollection; }
            set { _errorReportCollection = value; OnPropertyChanged(); }
        }

        private ObservableCollection<string> _genericErrorList;
        public ObservableCollection<string> GenericErrorList
        {
            get { return _genericErrorList; }
            set { _genericErrorList = value; OnPropertyChanged(); }
        }

        private ObservableCollection<SidenoteModel> _sidenoteCollection;
        public ObservableCollection<SidenoteModel> SidenoteCollection
        {
            get { return _sidenoteCollection; }
            set { _sidenoteCollection = value; OnPropertyChanged(); }
        }

        private string _SelectedPDFSourceStream;
        public string SelectedPDFSourceStream
        {
            get { return _SelectedPDFSourceStream; }
            set { _SelectedPDFSourceStream = value; OnPropertyChanged(); }
        }

        private Visibility _loadingStatusPdf;
        public Visibility LoadingStatusPdf
        {
            get { return _loadingStatusPdf; }
            set { _loadingStatusPdf = value; OnPropertyChanged(); }
        }

        private TextDocument _CodingTxtDocument;
        public TextDocument CodingTxtDocument
        {
            get { return _CodingTxtDocument; }
            set { _CodingTxtDocument = value; OnPropertyChanged(); }
        }

        private DocumentModel _SelectedDocumentItem;
        public DocumentModel SelectedDocumentItem
        {
            get { return _SelectedDocumentItem; }
            set { _SelectedDocumentItem = value; OnPropertyChanged(); }
        }

        private int _currentPDFPage;
        public int CurrentPDFPage
        {
            get { return _currentPDFPage; }
            set { _currentPDFPage = value; OnPropertyChanged(); }
        }

        private CurrentPageChangedEventHandler _pdfCurrenpageChanged;
        public CurrentPageChangedEventHandler PdfCurrenpageChanged
        {
            get { return _pdfCurrenpageChanged; }
            set { _pdfCurrenpageChanged = value; OnPropertyChanged(); }
        }

        private bool _isCheckboxXMLscroll;
        public bool IsCheckboxXMLscroll
        {
            get { return _isCheckboxXMLscroll; }
            set
            {
                if (_isCheckboxXMLscroll != value)
                {
                    _isCheckboxXMLscroll = value;
                    if (_isCheckboxXMLscroll)
                    {
                        IsCheckboxPDFscroll = false;
                    }
                    OnPropertyChanged();
                }
            }
        }

        private bool _isCheckboxPDFscroll;
        public bool IsCheckboxPDFscroll
        {
            get { return _isCheckboxPDFscroll; }
            set
            {
                if (_isCheckboxPDFscroll != value)
                {
                    _isCheckboxPDFscroll = value;
                    if (_isCheckboxPDFscroll)
                    {
                        IsCheckboxXMLscroll = false;
                    }
                    OnPropertyChanged();
                }
            }
        }


        private string _searchCssTextBox;
        public string SearchCssTextBox
        {
            get { return _searchCssTextBox; }
            set { _searchCssTextBox = value; OnPropertyChanged(); }
        }

        private string _searchXmlTextBox;
        public string SearchXmlTextBox
        {
            get { return _searchXmlTextBox; }
            set { _searchXmlTextBox = value; OnPropertyChanged(); }
        }


        private bool _isXmlViewerChecked;
        public bool IsXmlViewerChecked
        {
            get { return _isXmlViewerChecked; }
            set
            {
                _isXmlViewerChecked = value;
                OnPropertyChanged(nameof(IsXmlViewerChecked));
                OnPropertyChanged(nameof(XmlEditorVisibility));
                OnPropertyChanged(nameof(BrowserGridRowSpan)); // Notify UI of change }
            }
        }

        private string _errorHighlightInfo;
        public string ErrorHighlightInfo
        {
            get { return _errorHighlightInfo; }
            set { _errorHighlightInfo = value; OnPropertyChanged(); }
        }

        private bool _isPopUpErrorInput;
        public bool IsPopUpErrorInput
        {
            get { return _isPopUpErrorInput; }
            set { _isPopUpErrorInput = value; OnPropertyChanged(); }
        }

        private string _genericErrorInput;
        public string GenericErrorInput
        {
            get { return _genericErrorInput; }
            set { _genericErrorInput = value; OnPropertyChanged(); }
        }

        private string _remarksErrorInput;
        public string RemarksErrorInput
        {
            get { return _remarksErrorInput; }
            set { _remarksErrorInput = value; OnPropertyChanged(); }
        }

        private Visibility _isErrorInputGrid;
        public Visibility IsErrorInputGrid
        {
            get { return _isErrorInputGrid; }
            set { _isErrorInputGrid = value; OnPropertyChanged(); }
        }

        private string _selectedGenericError;
        public string SelectedGenericError
        {
            get { return _selectedGenericError; }
            set { _selectedGenericError = value; OnPropertyChanged(); }
        }

        private double _xmlTextFontSize;

        public double XmlTextFontSize
        {
            get { return _xmlTextFontSize; }
            set { _xmlTextFontSize = value; OnPropertyChanged(); }
        }



        //private int _browserGridRowSpan;

        //public int BrowserGridRowSpan
        //{
        //    get { return _browserGridRowSpan; }
        //    set { _browserGridRowSpan = value; OnPropertyChanged(); }
        //}

        private bool _isMouseEnterChecked;
        public bool IsMouseEnterChecked
        {
            get { return _isMouseEnterChecked; }
            set { _isMouseEnterChecked = value; OnPropertyChanged(); }
        }

        #endregion


        #region Fields
        public DocumentViewer documentViewerInteropControl = null;
        public TextEditor CodingTextControl = null;
        //public System.Windows.Controls.WebBrowser XmlCssViewer;
        //public string GlobalPDFFilePath;
        public bool UpperPage;
        public bool BottomPage;
        public bool BothTopBottom;
        public string lastDetectedPage;
        public string LastDetectedPageTag;
        public string GlobalXmlFilePath;
        private int lastSearchIndex = -1;
        //private int nextSidenoteIndex = 1; // Start from S1
        private ChromiumWebBrowser CefBrowsers;
        private PdfViewerControl PDFViewer;
        private string GenericDefaultListPath = @"Asset\Generic Errors.txt";
        private string SpellingCheckerCssPath = @"Asset\spellchecker.css";
        private string UsmlCssPath = @"Asset\uslm.css";

        public Visibility XmlEditorVisibility => IsXmlViewerChecked ? Visibility.Visible : Visibility.Hidden;
        public int BrowserGridRowSpan => IsXmlViewerChecked ? 1 : 3;
        public bool IsCssViewer = false;

        public int highlightPageNumber;
        public string highlightBrowserText;
        public string highlightXmlTag;
        public int highlightXmlLineNumber;
        public int PDFScrollLastPages;
        #endregion


        //Load browser control
        private void LoadWebBrowserViewer(ChromiumWebBrowser XmlViewer)
        {
            CefBrowsers = XmlViewer;
        }

        /// <summary>
        /// Load the document into the control. Set the control as a global variable.
        /// </summary>
        /// <param name="documentViewerXamlControl"></param>
        /// <returns></returns>/
        private async Task SelectPDFFile(DocumentViewer documentViewerXamlControl)
        {
            try
            {

                // Set the control global variable
                documentViewerInteropControl = documentViewerXamlControl;
                CodingTxtDocument = new TextDocument();

                LoadGenericErrorList(GenericDefaultListPath);

                //Get file path
                string xmlFilePath = GetFilePath(@"Select docx files (*.xml)", "*.xml", "Open multiple xml documents");
                if (xmlFilePath == null) return;

                string pdfFilePath = GetFilePath(@"Select pdf files (*.pdf)", "*.pdf", "Open multiple pdf documents");
                if (pdfFilePath == null) return;


                string textContent = "";

                // Load xml file in textviewer
                using (FileStream fs = new FileStream(xmlFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    using (StreamReader reader = FileReader.OpenStream(fs, Encoding.UTF8))
                    {
                        textContent = await reader.ReadToEndAsync();
                        CodingTxtDocument = new TextDocument(textContent);
                    }
                }


                await Task.Run(async () =>
                {
                    //Read and Get pdf contents and save in collection
                    await GetPDFTopBottomContent(pdfFilePath);

                    //Get all page tags and save in collection
                    await ExtractPageTags(textContent);
                });


                await Task.Run(() =>
                {
                    SelectedPDFSourceStream = pdfFilePath;
                });


                //Load input xml to Browser Viewer
                GlobalXmlFilePath = xmlFilePath;
                await InsertXMLHeader(GlobalXmlFilePath);
                CefBrowsers.Address = new Uri(GlobalXmlFilePath).AbsoluteUri;



                //Browser Viewer Scroll 
                CefBrowsers.JavascriptMessageReceived += OnJavaScriptMessageReceived;
                await ExecuteJavaScriptOnPageLoad();


                //// Get the directory and filename (without extension) of the input PDF
                //string inputDirectory = Path.GetDirectoryName(pdfFilePath);
                //string inputFileNameWithoutExt = Path.GetFileNameWithoutExtension(pdfFilePath);
                //string outputFilePath = Path.Combine(inputDirectory, $"{inputFileNameWithoutExt}_outputwords.txt");

                //await WriteOutputToTextFile(outputFilePath);

                SpellChecker();


            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }

        /// <summary>
        /// Screenshot the current screen and save it as an image file.
        /// </summary>
        private void ScreenShot()
        {
            try
            {
                if (string.IsNullOrEmpty(GlobalXmlFilePath))
                {
                    Growl.Error(new GrowlInfo
                    { 
                        Message = "File is Not Selected.",
                        WaitTime = 3,
                        ShowDateTime = false,
                        Type = InfoType.Error,
                        IsCustom = false
                    });
                        
                    return;
                }

                // Get the directory and filename (without extension) of the input PDF
                string inputDirectory = Path.GetDirectoryName(GlobalXmlFilePath);
                string inputFileNameWithoutExt = Path.GetFileNameWithoutExtension(GlobalXmlFilePath);
                string datetimeMark = DateTime.Now.ToString("MM_dd_yyyy_HH_mm_ss");
                //string outImageFile = Path.Combine(inputDirectory, $"{inputFileNameWithoutExt}_ScreenShotViewer.jpg");
                string outImageFile = Path.Combine(inputDirectory, $"{inputFileNameWithoutExt}_ScreenShotViewer_{datetimeMark}.jpg");


                // Get the primary screen size
                int screenWidth = (int)SystemParameters.PrimaryScreenWidth;
                int screenHeight = (int)SystemParameters.PrimaryScreenHeight;

                using (Bitmap bmp = new Bitmap(screenWidth, screenHeight))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.CopyFromScreen(0, 0, 0, 0, bmp.Size);
                    }

                    bmp.Save(outImageFile, ImageFormat.Png);
                }

                // Show growl with 3-second timeout
                Growl.Success(new GrowlInfo
                {
                    Message = "Screenshot saved!",
                    WaitTime = 3,            
                    ShowDateTime = false,
                    Type = InfoType.Success,
                    IsCustom = false
                });

                ////Process.Start(outImageFile);
                //// Try to open the image
                //try
                //{
                //    Process.Start(new ProcessStartInfo(outImageFile) { UseShellExecute = true });
                //}
                //catch (Exception startEx)
                //{
                //    Growl.Warning("Screenshot saved but could not open the file.");
                //}
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }


        /// <summary>
        /// Resets the viewer and clears old data before loading a new file.
        /// </summary>
        private void CloseOrResetAll()
        {
            RemoveSpellcheckerTags();
            RemoveXMLHeaderTag();

            // Clear the previous document content
            CodingTxtDocument = new TextDocument();
            GlobalXmlFilePath = null;

            // Remove old event handlers
            //CefBrowsers.JavascriptMessageReceived -= OnJavaScriptMessageReceived;

            if (CefBrowsers != null)
            {
                //Stop any active text search
                CefBrowsers.GetBrowser().StopFinding(true);

                CefBrowsers.JavascriptMessageReceived -= OnJavaScriptMessageReceived;

                //Load a blank page to fully reset
                CefBrowsers.Address = "about:blank";

                CefBrowsers.LoadingStateChanged += (sender, args) =>
                {
                    if (!args.IsLoading)
                    {
                        ExecuteJavaScriptOnPageLoad();
                    }
                };
                //CefBrowsers.Reload();
            }



            // Clear previously loaded PDF and XML data
            DocumentCollection.Clear();
            XmlTagsCollection.Clear();
            ErrorReportCollection.Clear();
            GenericErrorList.Clear();
            SidenoteCollection.Clear();
            lastDetectedPage = null;
            SelectedPDFSourceStream = null;


        }


        #region Xml Text Editor Scroll
        /// <summary>
        /// Load xml text editor attach with scroll event
        /// Change Single Tag Color 
        /// </summary>
        /// <param name="xamlInterfaceControlElement"></param>
        /// <returns></returns>
        private async Task AvalonTextEditor_Loaded(TextEditor xamlInterfaceControlElement)
        {
            try
            {
                CodingTextControl = xamlInterfaceControlElement;

                //CodingTextControl.SyntaxHighlighting.GetNamedColor("XmlTag").Foreground = new SimpleHighlightingBrush(Colors.Blue);


                if (CodingTextControl?.TextArea?.TextView != null)
                {
                    //Change Tag color
                    CodingTextControl.TextArea.TextView.LineTransformers.Add(new TagChangeColor());
                    CodingTextControl.TextArea.TextView.ScrollOffsetChanged += TextViewScrollOffsetChanged;
                }

            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }

        /// <summary>
        /// Get the Visible text in xml text editor
        /// Get the pdf page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void TextViewScrollOffsetChanged(object sender, EventArgs e)
        {
            try
            {
                //Check if XML Scroll is enabled
                if (!IsCheckboxXMLscroll) { return; }

                GetPDFContent GetPDFContentService = new GetPDFContent();

                var textView = sender as ICSharpCode.AvalonEdit.Rendering.TextView;
                if (textView == null || CodingTxtDocument == null)
                { return; }

                //Todo error exception of type 'ICSharpCode.AvalonEdit.Rendering.VisualLinesInvalidException' was thrown.
                //Get the visible lines from the viewer
                List<int> visibleLines = textView.VisualLines.Select(vl => vl.FirstDocumentLine.LineNumber).ToList();


                if (visibleLines.Count == 0)
                { return; }

                //Get the visible text
                int firstVisibleLine = visibleLines.First();
                int lastVisibleLine = visibleLines.Last();

                // Construct the visible text from the document lines between the first and last visible line
                string visibleText = string.Join(
                       Environment.NewLine,
                       CodingTxtDocument.Lines.Skip(firstVisibleLine - 1)
                        .Take(lastVisibleLine - firstVisibleLine + 1) //Center view // Taking lines within the visible range
                        .Select(line => CodingTxtDocument.GetText(line)) // Get the text of each line
                );

                // Extract the page number from the visible text
                string pageText = TagPageRegex(visibleText);


                //// Find pagetext in pdf 
                if (pageText != null && lastDetectedPage != pageText)
                {
                    lastDetectedPage = pageText;
                    int getPdfPage = GetPDFContentService.FindPDFPageByContent(DocumentCollection, pageText, UpperPage, BottomPage, BothTopBottom);

                    if (getPdfPage != 0)
                    {
                        CurrentPDFPage = getPdfPage;

                        //find page text in browser viewer
                        CefBrowsers.GetBrowser().Find(pageText, forward: true, matchCase: false, findNext: true);

                    }
                    else { return; }
                }

                #region comment code

                ////// Find pagetext in pdf 
                //if (pageText != null && lastDetectedPage != pageText)
                //{
                //    lastDetectedPage = pageText;
                //    await FindPDFPageByContent(GlobalPDFFilePath, pageText, UpperPage, BottomPage);
                //    //WarningMessage($"the text {pageText}");
                //}



                //if (LastPageNumber != CurrentPDFPage && pageText != null && pageText != lastDetectedPage)
                //{

                //    if (LastPageNumber != CurrentPDFPage)
                //    {


                //        LastPageNumber = await FindPDFPageByContent(GlobalPDFFilePath, pageText, UpperPage, BottomPage);
                //        lastDetectedPage = pageText;

                //    }
                //}


                // Update CurrentPDFPage if a page tag is found and is different
                // Now find the corresponding page in the PDF

                //if (int.TryParse(pageText, out int pageNumbers))
                //{
                //    int? pageNumber = pageNumbers;

                //    if (pageNumber.HasValue && pageNumber.Value != CurrentPDFPage)
                //    {
                //        if (lastDetectedPage != pageNumber.Value)
                //        {
                //            lastDetectedPage = pageNumber.Value;
                //            await FindPDFPageByContent(GlobalPDFFilePath, pageNumber.Value, UpperPage, BottomPage);
                //        }
                //    }
                //}
                //else
                //{
                //    await FindPDFPageByContent(GlobalPDFFilePath, pageNumber.Value, UpperPage, BottomPage);
                //}
                #endregion

            }
            catch (Exception ex)
            {
                ErrorMessage(ex);

            }

        }

        /// <summary>
        /// Get the Page Tag in visible text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string TagPageRegex(string text)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text))
                    return null;

                var pageonly = new Regex(@"<page>\s*([IVXLCDM\d]+)\s*</page>", RegexOptions.IgnoreCase);
                var pageIdentifier = new Regex(@"<page\s+identifier=""([^""]+)""\s*>\s*(\d+)\s*</page>", RegexOptions.IgnoreCase);
                var pageIdentifierPosition = new Regex(@"<page\s+identifier=""([^""]+)""\s+renderingPosition=""([^""]+)"">\s*(\d+)\s*</page>", RegexOptions.IgnoreCase);


                var pageIdentifierbrowser = new Regex(@"<page\s+xmlns=""http://schemas\.gpo\.gov/xml/uslm""\s+identifier=""([^""]+)""\s*>\s*(\d+)\s*</page>", RegexOptions.IgnoreCase);
                var pageIdentifierPositionbroser = new Regex(@"<page\s+xmlns=""http://schemas.gpo.gov/xml/uslm""\s+identifier=""([^""]+)""\s+renderingPosition=""([^""]+)"">\s*(\d+)\s*</page>", RegexOptions.IgnoreCase);

                if (pageIdentifierPosition.IsMatch(text))
                {
                    foreach (Match match in pageIdentifierPosition.Matches(text))
                    {
                        string identifier = match.Groups[1].Value;
                        string renderingPosition = match.Groups[2].Value;
                        string pagevalue = match.Groups[3].Value;

                        if (renderingPosition == "bottom")
                        {
                            BottomPage = true;
                            UpperPage = false;
                        }
                        else { BottomPage = false; }

                        return pagevalue;
                    }
                }
                else if (pageIdentifier.IsMatch(text))
                {
                    foreach (Match match in pageIdentifier.Matches(text))
                    {
                        string identifier = match.Groups[1].Value;
                        string pagevalue = match.Groups[2].Value;

                        BottomPage = false;
                        UpperPage = true;

                        return pagevalue;
                    }
                }
                else if (pageonly.IsMatch(text))
                {
                    foreach (Match match in pageonly.Matches(text))
                    {
                        string pagevalue = match.Groups[1].Value.Trim();

                        BothTopBottom = true;
                        UpperPage = false;
                        BottomPage = false;

                        return pagevalue;
                    }
                }
                else if (pageIdentifierbrowser.IsMatch(text))
                {
                    foreach (Match match in pageIdentifierbrowser.Matches(text))
                    {
                        string identifier = match.Groups[1].Value;
                        string pagevalue = match.Groups[2].Value;

                        BottomPage = false;
                        UpperPage = true;

                        return pagevalue;

                    }
                }
                if (pageIdentifierPositionbroser.IsMatch(text))
                {
                    foreach (Match match in pageIdentifierPositionbroser.Matches(text))
                    {
                        string identifier = match.Groups[1].Value;
                        string renderingPosition = match.Groups[2].Value;
                        string pagevalue = match.Groups[3].Value;

                        if (renderingPosition == "bottom")
                        {
                            BottomPage = true;
                            UpperPage = false;
                        }
                        else { BottomPage = false; }

                        return pagevalue;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
                return null;
            }
        }
        #endregion

        #region Pdf View Control Scroll
        /// <summary>
        /// PDF Page Scroll find the tags in xml text editor
        /// Highlight the page tag in xml text editor
        /// </summary>
        /// <param name="pdfViewer"></param>
        private async void PDFPageScroll(PdfViewerControl pdfViewer)
        {
            try
            {
                // Check if PDF Scroll is enabled
                if (!IsCheckboxPDFscroll) { return; }

                int newPDFPage = pdfViewer.CurrentPageIndex;

                if (pdfViewer == null) return;

                #region old
                //if (pdfViewer != null)
                //{
                //    foreach (DocumentModel page in DocumentCollection)
                //    {
                //        if (page.pdfPage == newPDFPage)
                //        {
                //            foreach (xmlTagModel tags in XmlTagsCollection)
                //            {
                //                if (page.TopContent.IndexOf(tags.PageOnly, StringComparison.OrdinalIgnoreCase) >= 0)
                //                {
                //                    int index = CodingTextControl.Text.IndexOf(tags.FormatTag, StringComparison.OrdinalIgnoreCase);

                //                    if (index >= 0)
                //                    {
                //                        CodingTextControl.ScrollToLine(CodingTextControl.Document.GetLineByOffset(index).LineNumber);
                //                        CodingTextControl.Select(index, tags.FormatTag.Length);
                //                        CodingTextControl.Focus();

                //                        // Search for the tag in the CefSharp browser
                //                        bool isInsidePage = await IsTextInsidePageTag(CefBrowsers, tags.PageOnly);
                //                        if (!isInsidePage) { return; }

                //                        return;
                //                    }
                //                }
                //            }

                //        }

                //    }
                //}
                #endregion

                // 1. find pdf page in Documentcollection
                // 2. for each matching page, find tags whose PageOnly text exists in the page's TopContent
                // 3. create an anonymous object containing the matched page and tag
                var matchingTag = DocumentCollection.Where(page => page.pdfPage == newPDFPage).SelectMany(page => XmlTagsCollection
                                 .Where(tag => page.TopContent.IndexOf(tag.PageOnly, StringComparison.OrdinalIgnoreCase) >= 0)
                                 .Select(tag => new { Page = page, Tag = tag }))
                                 .FirstOrDefault();


                //highlight and select match tag page ang page 
                if (matchingTag != null)
                {
                    int index = CodingTextControl.Text.IndexOf(matchingTag.Tag.FormatTag, StringComparison.OrdinalIgnoreCase);

                    if (index >= 0)
                    {
                        CodingTextControl.ScrollToLine(CodingTextControl.Document.GetLineByOffset(index).LineNumber);
                        CodingTextControl.Select(index, matchingTag.Tag.FormatTag.Length);
                        CodingTextControl.Focus();

                        // Search for the tag in the CefSharp browser
                        bool isInsidePage = await IsTextInsidePageTag(CefBrowsers, matchingTag.Tag.PageOnly);
                        if (!isInsidePage) return;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }

        }

        /// <summary>
        /// Scroll Browser Viewer only in text that have page tag
        /// </summary>
        /// <param name="browser"></param>
        /// <param name="searchText"></param>
        /// <returns></returns>
        private async Task<bool> IsTextInsidePageTag(ChromiumWebBrowser browser, string searchText)
        {
            if (browser.CanExecuteJavascriptInMainFrame)
            {
                string script = $@"
                        (() => {{
                            const pages = document.querySelectorAll('page');
                            for (const page of pages) {{
                                if (page.textContent.trim() === `{searchText}`) {{  // Exact match inside <page>
                                    page.scrollIntoView({{ behavior: 'smooth', block: 'center' }});  // Scroll to <page>
                    
                                    // Highlight the text inside the page tag
                                    const range = document.createRange();
                                    range.selectNodeContents(page);
                                    const selection = window.getSelection();
                                    selection.removeAllRanges();
                                    selection.addRange(range);

                                    return true; //Found and highlighted
                                }}
                            }}
                            return false; //No match found inside <page>
                        }})();";

                var response = await browser.EvaluateScriptAsync(script);
                return response.Success && response.Result is bool found && found;
            }
            return false;
        }

        #endregion

        #region Browser Viewer Scroll
        // Separate method to handle JavaScript messages
        private void OnJavaScriptMessageReceived(object sender, JavascriptMessageReceivedEventArgs e)
        {
            if (!IsCssViewer) { return; }
            var visiblePageTag = e.Message.ToString();
            VisibleChangeAsync(visiblePageTag);
        }

        /// <summary>
        /// Get the Visible Page Tag in Browser viewer
        /// Find page in PDF Viewer
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        private void VisibleChangeAsync(string page)
        {
            try
            {
                // Wait for the browser to fully load
                //if (CefBrowsers == null || !CefBrowsers.IsBrowserInitialized) return;

                if (string.IsNullOrEmpty(page)) return;

                GetPDFContent GetPDFContentService = new GetPDFContent();
                //string getTag = await GetVisiblePageNumberAsync();

                string browsertag = TagPageRegex(page);

                if (browsertag != null && lastDetectedPage != browsertag)
                {
                    lastDetectedPage = browsertag;

                    int getPdfPage = GetPDFContentService.FindPDFPageByContent(DocumentCollection, browsertag, UpperPage, BottomPage, BothTopBottom);

                    if (getPdfPage != 0)
                    {
                        CurrentPDFPage = getPdfPage;

                        //Xml Highlight Page Tag
                        IsCheckboxPDFscroll = true;
                    }
                    else { return; }
                }
                else { return; }


            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }

        }

        private async Task ExecuteJavaScriptOnPageLoad()
        {
            try
            {
                if (CefBrowsers.GetBrowser() != null && CefBrowsers.GetBrowser().MainFrame != null)
                {
                    var script = @"
                            window.addEventListener('scroll', () => {
                                const pages = document.querySelectorAll('page');
                                for (const page of pages) {
                                    const rect = page.getBoundingClientRect();
                                    if (rect.top >= 0 && rect.bottom <= window.innerHeight) {
                                        CefSharp.PostMessage(page.outerHTML);
                                        return;
                                    }
                                }
                            });
                        ";

                    await CefBrowsers.GetBrowser().MainFrame.EvaluateScriptAsync(script);

                }
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }
        #endregion

        #region Extract pdf and xml content
        /// <summary>
        /// Get Pdf Content top and bottom
        /// </summary>
        /// <param name="pdfFile"></param>
        /// <returns></returns>
        private async Task GetPDFTopBottomContent(string pdfFile)
        {
            try
            {
                GetPDFContent GetPDFContentService = new GetPDFContent();

                using (PdfDocument document = PdfDocument.Open(pdfFile))
                {
                    // Loop through all pages in the PDF document
                    for (int i = 1; i <= document.NumberOfPages; i++)
                    {
                        //Get the text content of the page
                        var pageText = document.GetPage(i);

                        //Extract words from the page
                        var words = pageText.GetWords();

                        // Use RecursiveXYCut to analyze layout
                        var recursiveXYCut = new RecursiveXYCut(new RecursiveXYCut.RecursiveXYCutOptions()
                        {
                            MinimumWidth = pageText.Width / 3.0,  // Split page width into thirds
                            DominantFontWidthFunc = letters => letters.Select(l => l.GlyphRectangle.Width).Average(),
                            DominantFontHeightFunc = letters => letters.Select(l => l.GlyphRectangle.Height).Average()
                        });

                        var blocks = recursiveXYCut.GetBlocks(words);

                        string toptext = GetPDFContentService.TopLeftRightText(blocks, pageText);
                        string bottomtext = GetPDFContentService.BottomLeftRightText(blocks, pageText);

                        // Save to collection
                        DocumentCollection.Add(new DocumentModel
                        {
                            TopContent = toptext,
                            BottomContent = bottomtext,
                            pdfPage = i
                        });


                        #region trash code


                        // If 'checkUpperOnly' is true, only process top blocks
                        //if (checkUpperOnly)
                        //{
                        //    //var topBlocks = blocks.OrderBy(b => b.BoundingBox.Top).Take(1).ToList();
                        //    //var topBlocks = blocks.Where(b => b.BoundingBox.Bottom > (pageText.Height * 2 / 3)).ToList();
                        //    //var topBlocks = blocks.OrderByDescending(b => b.BoundingBox.Top).Take(1).ToList();
                        //    //string topText = string.Join(" ", topBlocks.Select(b => b.Text));

                        //    string toptext = TopLeftRightText(blocks, pageText);

                        //    if (toptext.Contains(pageNumber))
                        //    {
                        //        //WarningMessage($"Found page number {pageNumber} in the top content of PDF page {i}");
                        //        CurrentPDFPage = i;
                        //        break;
                        //    }
                        //}
                        //else if (checkBottomOnly)
                        //{

                        //    // Filter blocks that are in the bottom third of the page
                        //    //var bottomBlocks = blocks.Where(b => b.BoundingBox.Top < (pageText.Height / 3)).ToList();
                        //    //var bottomBlocks = blocks.OrderByDescending(b => b.BoundingBox.Top).Take(3).ToList();
                        //    //var bottomBlocks = blocks.OrderBy(b => b.BoundingBox.Bottom).Take(1).ToList();
                        //    //string bottomText = string.Join(" ", bottomBlocks.Select(b => b.Text));

                        //    string bottomtext = BottomLeftRightText(blocks, pageText);

                        //    if (bottomtext.Contains(pageNumber))
                        //    {
                        //        //WarningMessage($"Found page number {pageNumber} in the bottom content of PDF page {i}");
                        //        CurrentPDFPage = i;
                        //        break;
                        //    }

                        //}
                        //else
                        //{
                        //    //var fulltopBlocks = blocks.Where(b => b.BoundingBox.Bottom > (pageText.Height * 2 / 3)).ToList();
                        //    //string fulltopText = string.Join(" ", fulltopBlocks.Select(b => b.Text));

                        //    //var fullbottomBlocks = blocks.OrderBy(b => b.BoundingBox).Take(1).ToList();
                        //    //string fullbottomText = string.Join(" ", fulltopBlocks.Select(b => b.Text));
                        //    string toptext = TopLeftRightText(blocks, pageText);
                        //    string bottomtext = BottomLeftRightText(blocks, pageText);


                        //    string allText = string.Join(" ", toptext, bottomtext);

                        //    // Roman numbers
                        //    //string fullPageText = pageText.Text;

                        //    if (allText.Contains(pageNumber))
                        //    {
                        //        //WarningMessage($"Found page number {pageNumber} in PDF page {i}");
                        //        CurrentPDFPage = i;
                        //        break;
                        //    }
                        //}
                        #endregion

                    }
                }

            }
            catch (Exception ex)
            {
                ErrorMessage(ex);

            }
        }

        /// <summary>
        /// Get all page tags in xml
        /// </summary>
        /// <param name="xmlContent"></param>
        /// <returns></returns>
        private async Task ExtractPageTags(string xmlContent)
        {
            try
            {
                // Regular expressions for different tag types
                //var pageOnlyRegex = new Regex(@"<page>\s*([IVXLCDM\d]+)\s*</page>", RegexOptions.IgnoreCase);
                var pageIdentifierRegex = new Regex(@"<page\s+identifier=""([^""]+)""\s*>\s*([IVXLCDM\d]+)\s*</page>", RegexOptions.IgnoreCase);
                var pageIdentifierPositionRegex = new Regex(@"<page\s+identifier=""([^""]+)""\s+renderingPosition=""([^""]+)"">\s*([IVXLCDM\d]+)\s*</page>", RegexOptions.IgnoreCase);


                // Match and process <page> tags
                //foreach (Match match in pageOnlyRegex.Matches(xmlContent))
                //{
                //    XmlTagsCollection.Add(new xmlTagModel
                //    {
                //        FormatTag = match.Value,
                //        PageOnly = match.Groups[1].Value,
                //        PageIdentifier = null,
                //        PageRenderingPosition = null
                //    });
                //}

                // Match and process <page identifier="..." > tags
                foreach (Match match in pageIdentifierRegex.Matches(xmlContent))
                {
                    XmlTagsCollection.Add(new xmlTagModel
                    {
                        FormatTag = match.Value,
                        PageOnly = match.Groups[2].Value,
                        PageIdentifier = match.Groups[1].Value,
                        PageRenderingPosition = null
                    });
                }

                // Match and process <page identifier="..." renderingPosition="..." > tags
                foreach (Match match in pageIdentifierPositionRegex.Matches(xmlContent))
                {
                    XmlTagsCollection.Add(new xmlTagModel
                    {
                        FormatTag = match.Value,
                        PageOnly = match.Groups[3].Value,
                        PageIdentifier = match.Groups[1].Value,
                        PageRenderingPosition = match.Groups[2].Value
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }

        }
        #endregion



        #region Mouse Enter
        private void OnXMLViewerMouseHover()
        {
            //WarningMessage("Mouse Hover is in Xml text editor");
            if (IsMouseEnterChecked)
            {
                IsCheckboxXMLscroll = true;
            }
            else { IsCheckboxXMLscroll = false; }
        }

        private void OnPDFViewerMouseHover()
        {
            //WarningMessage("Mouse Hover is in PDF viewer");
            if (IsMouseEnterChecked)
            {
                IsCheckboxPDFscroll = true;
            }
            else { IsCheckboxPDFscroll = false; }
        }

        private void OnCssViewerMouseHover()
        {
            //WarningMessage("Mouse Hover is in Css viewer");
            if (IsMouseEnterChecked)
            {
                IsCssViewer = true;
                IsCheckboxXMLscroll = false;
                IsCheckboxPDFscroll = false;
            }
            else { IsCssViewer = false; }

        }
        #endregion

        #region Search 
        /// <summary>
        /// Search word in Browser viewer
        /// Get page that visible in Browser viewer
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="findNext"></param>
        private async void SearchTextInCss()
        {
            try
            {
                if (string.IsNullOrEmpty(SearchCssTextBox)) { WarningMessage("Search text cannot be empty"); return; }

                if (CefBrowsers == null || !CefBrowsers.IsBrowserInitialized)
                {
                    WarningMessage("Browser is not ready.");
                    return;
                }

                CefBrowsers.GetBrowser().Find(SearchCssTextBox, forward: true, matchCase: false, findNext: true);


                GetPDFContent GetPDFContentService = new GetPDFContent();
                string getTag = await GetVisiblePageNumberAsync();

                string browsertag = TagPageRegex(getTag);

                if (browsertag != null && lastDetectedPage != browsertag)
                {
                    lastDetectedPage = browsertag;

                    int getPdfPage = GetPDFContentService.FindPDFPageByContent(DocumentCollection, browsertag, UpperPage, BottomPage, BothTopBottom);

                    if (getPdfPage != 0)
                    {
                        CurrentPDFPage = getPdfPage;

                    }
                    else { return; }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }

        private void SearchTextInXml()
        {
            try
            {
                // Start searching after the last found index
                int startIndex = (lastSearchIndex == -1) ? 0 : lastSearchIndex + SearchXmlTextBox.Trim().Length;

                int index = CodingTextControl.Text.IndexOf(SearchXmlTextBox.Trim(), startIndex, StringComparison.OrdinalIgnoreCase);


                if (index == -1 && lastSearchIndex != -1)
                {
                    startIndex = 0;
                    index = CodingTextControl.Text.IndexOf(SearchXmlTextBox.Trim(), startIndex, StringComparison.OrdinalIgnoreCase);
                }

                if (index >= 0)
                {
                    //Update last found position
                    lastSearchIndex = index;

                    var line = CodingTextControl.Document.GetLineByOffset(index);
                    CodingTextControl.ScrollToLine(line.LineNumber);
                    CodingTextControl.Select(index, SearchXmlTextBox.Trim().Length);
                    CodingTextControl.Focus();

                }
                else
                {
                    //Reset if nothing is found
                    WarningMessage("No matches found.");
                    lastSearchIndex = -1;
                }

            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }

        private async Task<string> GetVisiblePageNumberAsync()
        {
            try
            {
                if (CefBrowsers.CanExecuteJavascriptInMainFrame)
                {
                    string script = @"
                            (() => {
                                const pages = document.querySelectorAll('page');
                                for (const page of pages) {
                                    const rect = page.getBoundingClientRect();
                                    if (rect.top >= 0 && rect.bottom <= window.innerHeight) {
                                        return page.outerHTML;
                                    }
                                }
                                return null;
                            })();";



                    var response = await CefBrowsers.EvaluateScriptAsync(script);

                    if (response.Success && response.Result is string visiblePageTag)
                    {
                        //InformationMessage($"Visible Page Tag: {visiblePageTag}", "Conformation");
                        //ShowSearchTag = visiblePageTag;

                        return visiblePageTag;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
                return null;
            }
        }
        #endregion




        /// <summary>
        /// Update and save Xml text editor
        /// Update the browser viewer
        /// </summary>
        private void SaveUpdateXmlFile()
        {
            try
            {
                if (CodingTxtDocument == null) return;

                // Get the updated XML text from AvalonEdit
                string updatedXml = CodingTxtDocument.Text;

                ReloadandUpadateXmlTextViewer(updatedXml);

                // Save it to the XML file
                //File.WriteAllText(GlobalXmlFilePath, updatedXml, Encoding.UTF8);


                //// Reload the updated XML file in the browser
                ReloadBrowserViewer(false);

                InformationMessage("Save Xml successful", "Success");
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }


        #region Error Modal
        /// <summary>
        /// Error Report
        /// Highlight the mismatch text in browser viewer
        /// </summary>
        /// <returns></returns>
        private async Task HighlightErrorText()
        {
            try
            {
                ErrorReportHighlightText errorReportModel = new ErrorReportHighlightText();

                var highlightInfo = await errorReportModel.HighlightErrorTextService(CefBrowsers, GlobalXmlFilePath, lastDetectedPage);

                //gets the first page pdf
                if (highlightInfo.pageNumber == 0 && lastDetectedPage == null)
                {
                    int frstPDFPage = 1;

                    foreach (DocumentModel page in DocumentCollection)
                    {
                        if (page.pdfPage == frstPDFPage)
                        {
                            foreach (xmlTagModel tags in XmlTagsCollection)
                            {
                                if (int.TryParse(tags.PageOnly, out int firstPage))
                                {
                                    highlightInfo.pageNumber = firstPage - frstPDFPage;
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }

                //Remove Hightlight page validation //add to github.
                //if (highlightInfo.pageNumber != 0 && highlightInfo.highlightedText != null && highlightInfo.xmlTag != null && highlightInfo.xmlLineNumber != 0)
                if (highlightInfo.highlightedText != null && highlightInfo.xmlTag != null && highlightInfo.xmlLineNumber != 0)
                {
                    IsPopUpErrorInput = true;
                    IsErrorInputGrid = Visibility.Visible;

                    ErrorHighlightInfo = $@"Page: {highlightInfo.pageNumber}
XML Line: {highlightInfo.xmlLineNumber}
Highlight Text: ""{highlightInfo.highlightedText}""
XML Tag: <{highlightInfo.xmlTag}>";

                    highlightPageNumber = highlightInfo.pageNumber;
                    highlightBrowserText = highlightInfo.highlightedText;
                    highlightXmlTag = highlightInfo.xmlTag;
                    highlightXmlLineNumber = highlightInfo.xmlLineNumber;


                }

            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }

        private void SaveErrorRemark(string buttons)
        {
            try
            {
                bool isInputEmpty = false;
                string inputFileNameWithoutExt = Path.GetFileNameWithoutExtension(GlobalXmlFilePath);


                //if (highlightXmlTag == null || highlightBrowserText == null || highlightPageNumber == 0 || highlightXmlLineNumber == 0) { isInputEmpty = true; }
                if (highlightXmlTag == null || highlightBrowserText == null || highlightXmlLineNumber == 0) { isInputEmpty = true; }


                switch (buttons)
                {
                    case "save":
                        if (isInputEmpty) { InformationMessage("Error report is not save. One of the input is empty.", "Failed Saving"); return; }

                        ErrorReportCollection.Add(new ErrorReportModel
                        {
                            FileName = inputFileNameWithoutExt,
                            VisiblePage = highlightPageNumber,
                            HighlightText = highlightBrowserText,
                            HighlightXmlTag = highlightXmlTag,
                            LineNumberXML = highlightXmlLineNumber,
                            Generic = SelectedGenericError,
                            Remarks = RemarksErrorInput,
                        });

                        IsPopUpErrorInput = false;
                        IsErrorInputGrid = Visibility.Hidden;
                        InformationMessage("Error report is save", "Successful Save");
                        ResetErrorInputs();
                        break;
                    case "cancel":
                        if (isInputEmpty) { IsPopUpErrorInput = false; }

                        IsPopUpErrorInput = false;
                        IsErrorInputGrid = Visibility.Hidden;
                        InformationMessage("Error report is cancel", "Cancel Save");
                        ResetErrorInputs();
                        break;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }

        private void LoadGenericErrorList(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    foreach (var line in File.ReadAllLines(filePath))
                    {
                        if (!GenericErrorList.Contains(line))
                        {
                            GenericErrorList.Add(line);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }

        private void UploadGenericErrorFile()
        {
            try
            {
                string genericNewUpload = GetFilePath(@"Select docx files (*.txt)", "*.txt", "Open multiple txt documents");
                if (genericNewUpload == null) return;

                LoadGenericErrorList(genericNewUpload);
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }

        private void ResetErrorInputs()
        {
            SelectedGenericError = null;
            RemarksErrorInput = string.Empty;
        }

        /// <summary>
        /// Export Error report to excel
        /// </summary>
        /// <returns></returns>
        private async Task ExportErrorReport()
        {
            ExportErrorReportToExcel ExporttoExcel = new ExportErrorReportToExcel();

            await ExporttoExcel.ExportErrorReportService(GlobalXmlFilePath, ErrorReportCollection);

        }

        #endregion

        #region Zoom 

        /// <summary>
        /// Zoom Browser Control
        /// Ctrl+MouseWheel
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private async Task ZoomBrowserViewer(MouseWheelEventArgs args)
        {
            try
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    if (CefBrowsers != null)
                    {
                        if (args.Delta > 0)
                        {
                            CefBrowsers.ZoomLevel += 0.5;
                        }
                        else
                        {
                            CefBrowsers.ZoomLevel -= 0.5;
                        }

                        args.Handled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }

        /// <summary>
        /// Zoom Xml Text Control
        /// Ctrl+MouseWheel
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private async Task ZoomXmlTextViewer(MouseWheelEventArgs args)
        {
            try
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    if (CodingTxtDocument != null)
                    {
                        if (args.Delta > 0)
                        {
                            XmlTextFontSize += 0.5;
                        }
                        else
                        {
                            XmlTextFontSize -= 0.5;
                        }

                        args.Handled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }

        #endregion


        #region Add and Remove XML Header Tags
        private async Task InsertXMLHeader(string xmlpath)
        {
            string[] requiredHeaders = new string[]
            {
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>",
            "<?xml-stylesheet type=\"text/css\" href=\"Styles/uslm.css\"?>",
            "<?xml-stylesheet type=\"text/css\" href=\"Styles/spellchecker.css\"?>"
            //"</meta>",
            //"<volume>50</volume>",
            //"<dc:date>1937</dc:date>",
            //"<session>1</session>",
            //"<congress>75</congress>",
            //"<processedDate>2024-11-23</processedDate>",
            //"<processedBy>Digitization Vendor</processedBy>",
            //"<dc:rights>Pursuant to Title 17 Section 105 of the United States Code, this file is not subject to copyright protection and is in the public domain.</dc:rights>",
            //"<dc:language>EN</dc:language>",
            //"<dc:format>text/xml</dc:format>",
            //"<dc:publisher>United States Government Publishing Office</dc:publisher>",
            //"<meta>",
            //"<statutesAtLarge xmlns=\"http://schemas.gpo.gov/xml/uslm\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:dcterms=\"http://purl.org/dc/terms/\" xml:lang=\"en\" xsi:schemaLocation=\"http://schemas.gpo.gov/xml/uslm https://www.govinfo.gov/schemas/xml/uslm/uslm-2.0.17.xsd\">",
            //"<?xml-stylesheet type=\"text/css\" href=\"uslm.css\"?>",
            //"<?xml version=\"1.0\" encoding=\"UTF-8\"?>",
            };

            string fileContent = File.ReadAllText(xmlpath, Encoding.UTF8);


            //// Ensure XML version and stylesheet are present at the top
            //foreach (var header in requiredHeaders)
            //{
            //    if (!fileContent.Contains(header))
            //    {
            //        fileContent = header + Environment.NewLine + fileContent;
            //    }
            //}


            // Ensure XML version and stylesheet are present at the top
            foreach (var header in requiredHeaders)
            {
                if (!fileContent.Contains(header) && !fileContent.Contains("<statutesAtLarge"))
                {
                    string xmlBody = @"
<statutesAtLarge xmlns=""http://schemas.gpo.gov/xml/uslm"" xmlns:dc=""http://purl.org/dc/elements/1.1/"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:dcterms=""http://purl.org/dc/terms/"" xml:lang=""en"" xsi:schemaLocation=""http://schemas.gpo.gov/xml/uslm https://www.govinfo.gov/schemas/xml/uslm/uslm-2.0.17.xsd"">
<meta>
<dc:publisher>United States Government Publishing Office</dc:publisher>
<dc:format>text/xml</dc:format>
<dc:language>EN</dc:language>
<dc:rights>Pursuant to Title 17 Section 105 of the United States Code, this file is not subject to copyright protection and is in the public domain.</dc:rights>
<processedBy>Digitization Vendor</processedBy>
<processedDate>2024-11-23</processedDate>
<congress>75</congress>
<session>1</session>
<dc:date>1937</dc:date>
<volume>50</volume>
</meta>";

                    fileContent = string.Join(Environment.NewLine, requiredHeaders) + xmlBody + Environment.NewLine + fileContent;

                    //WarningMessage("The Header is Added");
                }
            }

            // Ensure </statutesAtLarge> exists at the end
            if (!fileContent.Trim().EndsWith("</statutesAtLarge>"))
            {
                fileContent = fileContent.TrimEnd() + Environment.NewLine + "</statutesAtLarge>" + Environment.NewLine;
            }
            //else { WarningMessage("There is already EndTag"); }


            //Write the updated content
            File.WriteAllText(xmlpath, fileContent, Encoding.UTF8);
        }

        /// <summary>
        /// Remove Xml Header tag
        /// </summary>
        /// <returns></returns>
        private async Task RemoveXMLHeaderTag()
        {
            string[] headersToRemove = new string[]
            {

            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n",
            "<?xml-stylesheet type=\"text/css\" href=\"Styles/uslm.css\"?>\r\n",
            "<?xml-stylesheet type=\"text/css\" href=\"Styles/spellchecker.css\"?>\r\n",
            "<statutesAtLarge xmlns=\"http://schemas.gpo.gov/xml/uslm\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:dcterms=\"http://purl.org/dc/terms/\" xml:lang=\"en\" xsi:schemaLocation=\"http://schemas.gpo.gov/xml/uslm https://www.govinfo.gov/schemas/xml/uslm/uslm-2.0.17.xsd\">\r\n",
            "<meta>\r\n",
            "<dc:publisher>United States Government Publishing Office</dc:publisher>\r\n",
            "<dc:format>text/xml</dc:format>\r\n",
            "<dc:language>EN</dc:language>\r\n",
            "<dc:rights>Pursuant to Title 17 Section 105 of the United States Code, this file is not subject to copyright protection and is in the public domain.</dc:rights>\r\n",
            "<processedBy>Digitization Vendor</processedBy>\r\n",
            "<processedDate>2024-11-23</processedDate>\r\n",
            "<congress>75</congress>\r\n",
            "<session>1</session>\r\n",
            "<dc:date>1937</dc:date>\r\n",
            "<volume>50</volume>\r\n",
            "</meta>\r\n",


            };


            string fileContent = File.ReadAllText(GlobalXmlFilePath, Encoding.UTF8);


            // Remove the headers from the content if they exist
            foreach (var header in headersToRemove)
            {
                if (fileContent.Contains(header))
                {
                    fileContent = fileContent.Replace(header, "");
                }
            }

            string endTag = "</statutesAtLarge>";

            if (!fileContent.Trim().EndsWith(endTag))
            {
                fileContent = fileContent.Substring(0, fileContent.Length - endTag.Length);
            }


            //Write the updated content
            File.WriteAllText(GlobalXmlFilePath, fileContent, Encoding.UTF8);
            InformationMessage("XML header tag has remove.", "Successful Remove");
        }

        #endregion


        #region Spelling Checker 

        /// <summary>
        /// Add Spell Checker 
        /// Highlight red color the wrong spelling
        /// </summary>
        private void SpellChecker()
        {
            try
            {
                string xmlDirectory = Path.GetDirectoryName(GlobalXmlFilePath);
                string stylesFolder = Path.Combine(xmlDirectory, "Styles");

                // Ensure "Styles" folder exists
                if (!Directory.Exists(stylesFolder))
                {
                    Directory.CreateDirectory(stylesFolder);
                }

                string uslmCssDestinationPath = Path.Combine(stylesFolder, "uslm.css");
                string spellcheckerCssDestinationPath = Path.Combine(stylesFolder, "spellchecker.css");

                


                // Copy CSS files to Styles folder if they don't exist
                if (!File.Exists(uslmCssDestinationPath))
                {
                    File.Copy(UsmlCssPath, uslmCssDestinationPath, true);
                }

                if (!File.Exists(spellcheckerCssDestinationPath))
                {
                    File.Copy(SpellingCheckerCssPath, spellcheckerCssDestinationPath, true);
                }



                //string cssSourcePath = SpellingCheckerCssPath;
                //string cssDestinationPath = Path.Combine(xmlDirectory, "spellchecker.css");
                //// Ensure spellchecker.css exists in the same folder as XML
                //if (!File.Exists(cssDestinationPath))
                //{
                //    File.Copy(cssSourcePath, cssDestinationPath, true);
                //}


                SpellCheckerService spellchecker = new SpellCheckerService();

                string updatedXmlContent = spellchecker.SpellCheckerservice(GlobalXmlFilePath);

                // Save the modified XML without altering its structure
                ReloadandUpadateXmlTextViewer(updatedXmlContent);

                // Reload the updated XML in the browser
                ReloadBrowserViewer(true);

            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }

        /// <summary>
        /// Remove all <spellchecker> tag 
        /// </summary>
        private void RemoveSpellcheckerTags()
        {
            try
            {
                string xmlContent = File.ReadAllText(GlobalXmlFilePath, Encoding.UTF8);

                //Remove <spellchecker> tags while keeping
                string cleanedXmlContent = Regex.Replace(xmlContent, @"<spellchecker>(.*?)</spellchecker>", "$1");

                //Reload the Xml text editor
                ReloadandUpadateXmlTextViewer(cleanedXmlContent);

                // Reload the updated XML in the browser
                ReloadBrowserViewer(true);
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }


        #endregion

        #region SideNote

        /// <summary>
        /// <sidenote></sidenote> to <sidenote1></sidenote1>
        /// </summary>
        private void AddIdOnSidenote()
        {
            try
            {
                //string xmlContent = File.ReadAllText(GlobalXmlFilePath, Encoding.UTF8);
                string xmlContent = CodingTextControl.Text;
                SidenoteService sidenoteService = new SidenoteService();
                string SidenoteWithID = sidenoteService.AddIdOnSidenote(xmlContent);

                // Save the updated XML content back to the file
                ReloadandUpadateXmlTextViewer(SidenoteWithID);
                ReloadBrowserViewer(true);
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }

        /// <summary>
        /// add <S#> 
        /// </summary>
        private void AddPlaceholderTagS()
        {
            try
            {
                if (CodingTextControl == null) return;

                // Get current text from AvalonEdit
                string xmlContent = CodingTextControl.Text;
                //string xmlContent = File.ReadAllText(GlobalXmlFilePath, Encoding.UTF8);
                SidenoteService sidenoteService = new SidenoteService();
                string updatedCodingText = sidenoteService.AddPlaceholderTagS(xmlContent);

                if(updatedCodingText == null) { return; }

                ReplaceCodingText(updatedCodingText);
                ReloadBrowserViewer(true);

            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }

        /// <summary>
        /// <sidenote1></sidenote1> place to <S#>
        /// </summary>
        private void LinkSidenote()
        {
            string xmlContent = CodingTextControl.Text;
            //string xmlContent = File.ReadAllText(GlobalXmlFilePath, Encoding.UTF8);
            SidenoteService sidenoteService = new SidenoteService();
            string updatedXmlContent = sidenoteService.LinkSidenote(xmlContent, SidenoteCollection);

            // Save the updated XML content
            ReloadandUpadateXmlTextViewer(updatedXmlContent);
            ReloadBrowserViewer(true);
        }

        /// <summary>
        /// Update the Selected text in the Coding text
        /// </summary>
        /// <param name="UpdatedSelectionFromCodingText"></param>
        private void ReplaceCodingText(string UpdatedSelectionText)
        {
            try
            {
                if (CodingTextControl.Document == null) return;

                // Validate caret index forward selection.
                // The caret must be at the start of the selection always.
                int currentIndex = CodingTextControl.SelectionStart < CodingTextControl.CaretOffset ? CodingTextControl.SelectionStart : CodingTextControl.CaretOffset;

                // Set into coding text
                CodingTextControl.Document.Remove(currentIndex, CodingTextControl.SelectionLength);
                CodingTextControl.Document.Insert(currentIndex, UpdatedSelectionText);

                // Insert word with tag in caret position
                CodingTextControl.CaretOffset = currentIndex + UpdatedSelectionText.Length;

                // Update Xml file
                string updatedXmlContent = CodingTextControl.Text;
                //ReloadandUpadateXmlTextViewer(updatedXmlContent);
                ReloadBrowserViewer(true);
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }

       

        /// <summary>
        /// Handle shortcut key gestures (F3)
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ShortcutTag()
        {
            try
            {
                if (CodingTextControl == null) return;

                //AddPlaceholderTagS();
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }

        #endregion

        #region Reload Viewer
        /// <summary>
        /// Reload BrowserViewer/Css Viewer
        /// </summary>
        /// <param name="isEnableBrowserScroll"></param>
        private void ReloadBrowserViewer(bool isEnableBrowserScroll)
        {
            try
            {
                // Reload the updated XML in the browser
                if (CefBrowsers != null && CefBrowsers.IsBrowserInitialized)
                {

                    if (isEnableBrowserScroll)
                    {
                        CefBrowsers.JavascriptMessageReceived += OnJavaScriptMessageReceived;
                    }
                    else
                    {
                        CefBrowsers.JavascriptMessageReceived -= OnJavaScriptMessageReceived;
                    }


                    CefBrowsers.LoadingStateChanged += (sender, args) =>
                    {
                        if (!args.IsLoading)
                        {
                            ExecuteJavaScriptOnPageLoad();
                        }
                    };
                    CefBrowsers.Reload();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }


        /// <summary>
        /// Reload Xml text Editor and xml file
        /// </summary>
        /// <param name="isEnableBrowserScroll"></param>
        private void ReloadandUpadateXmlTextViewer(string updatexml)
        {
            try
            {
                // Save the updated XML content
                File.WriteAllText(GlobalXmlFilePath, updatexml, Encoding.UTF8);

                // Update AvalonEdit text editor
                if (CodingTextControl != null)
                {
                    CodingTextControl.Text = updatexml;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }

        }

        #endregion

        #region Write xml and pdf content extraction
        private async Task WriteOutputToTextFile(string outputFilePath)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                foreach (DocumentModel document in DocumentCollection)
                {
                    sb.AppendLine($"{document.TopContent} - {document.BottomContent} - Page {document.pdfPage}");
                }

                File.WriteAllText(outputFilePath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }

        private async Task WriteOutputToTextFileXml(string outputFilePath)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                foreach (xmlTagModel document in XmlTagsCollection)
                {
                    sb.AppendLine($"{document.PageOnly} - {document.PageIdentifier} - {document.PageRenderingPosition}");
                }

                File.WriteAllText(outputFilePath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }

        #endregion

        #region trash code

        /// <summary>
        /// Export the incorrect spelling to .txt
        /// </summary>
        //private void SpellChecker()
        //{
        //    try
        //    {

        //        string xmlContent = File.ReadAllText(BrowserSource, Encoding.UTF8);
        //        string[] lines = xmlContent.Split('\n');

        //        List<WordXmlContentModel> wordlist = new List<WordXmlContentModel>();
        //        List<string> linesToWrite = new List<string>();


        //        using (Hunspell hunspell = new Hunspell(@"Asset\en_US.aff", @"Asset\en_US.dic"))
        //        {
        //            for (int i = 0; i < lines.Length; i++)
        //            {
        //                string line = lines[i];
        //                string wordsOnly = Regex.Replace(line, "<.*?>", " ").Trim();

        //                string[] words = wordsOnly.Split(' ');

        //                foreach (string selectedword in words)
        //                {
        //                    // Split word by '/' or other special characters
        //                    string[] parts = selectedword.Split(new char[] { '/', '-', ':', ';', ',', '.' }, StringSplitOptions.RemoveEmptyEntries);

        //                    foreach (var part in parts)
        //                    {
        //                        string cleanedPart = Regex.Replace(part, @"[^a-zA-Z]", "");

        //                        if (!hunspell.Spell(cleanedPart))
        //                        {
        //                            wordlist.Add(new WordXmlContentModel
        //                            {
        //                                Word = cleanedPart,
        //                                xmlLine = i + 1
        //                            });
        //                        }
        //                    }
        //                }
        //            }

        //            foreach (var item in wordlist)
        //            {
        //                linesToWrite.Add($"word: {item.Word} line: {item.xmlLine}");
        //            }

        //            string inputDirectory = Path.GetDirectoryName(BrowserSource);
        //            string inputFileNameWithoutExt = Path.GetFileNameWithoutExtension(BrowserSource);
        //            string outputFilePath = Path.Combine(inputDirectory, $"{inputFileNameWithoutExt}_IncorrectWords.txt");


        //            File.WriteAllLines(outputFilePath, linesToWrite, Encoding.UTF8);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorMessage(ex);
        //    }
        //}

        //public string BottomLeftRightText(IReadOnlyList<UglyToad.PdfPig.DocumentLayoutAnalysis.TextBlock> blocks, UglyToad.PdfPig.Content.Page pageText)
        //{
        //    // Get top-left and top-right blocks (only the first line)
        //    var bottomLeftBlock = blocks
        //        .Where(b => b.BoundingBox.Bottom > (pageText.Height * 2 / 3) && b.BoundingBox.Right <= (pageText.Width / 2))
        //        .OrderByDescending(b => b.BoundingBox.Top)
        //        .Take(1).ToList();

        //    var bottomRightBlock = blocks
        //        .Where(b => b.BoundingBox.Bottom > (pageText.Height * 2 / 3) && b.BoundingBox.Left > (pageText.Width / 2))
        //        .OrderByDescending(b => b.BoundingBox.Top)
        //        .Take(1).ToList();

        //    string bottomLeftRight = string.Join(" ", bottomLeftBlock, bottomRightBlock);

        //    return $"{bottomLeftRight}".Trim();

        // Get bottom-left blocks (first two lines from the bottom left quarter)
        //var bottomLeftBlocks = blocks
        //    .Where(b => b.BoundingBox.TopRight.Y < (pageText.Height / 3) && b.BoundingBox.TopRight.X <= (pageText.Width / 2))
        //    .OrderBy(b => b.BoundingBox.TopRight.Y)
        //    .Take(1)  // Take first two lines
        //    .Select(b => b.Text);

        //// Get bottom-right blocks (first two lines from the bottom right quarter)
        //var bottomRightBlocks = blocks
        //    .Where(b => b.BoundingBox.TopRight.Y < (pageText.Height / 3) && b.BoundingBox.TopRight.X > (pageText.Width / 2))
        //    .OrderBy(b => b.BoundingBox.TopRight.Y)
        //    .Take(1)  // Take first two lines
        //    .Select(b => b.Text);

        //string bottomLeftText = string.Join(" ", bottomLeftBlocks);
        //string bottomRightText = string.Join(" ", bottomRightBlocks);

        //return $"{bottomLeftText} {bottomRightText}".Trim();


        //}


        //public string TopLeftRightText(IReadOnlyList<UglyToad.PdfPig.DocumentLayoutAnalysis.TextBlock> blocks, UglyToad.PdfPig.Content.Page pageText)
        //{
        //    // Get bottom-left and bottom-right blocks (only the first line)
        //    var topLeftBlock = blocks
        //        .Where(b => b.BoundingBox.Top < (pageText.Height / 3) && b.BoundingBox.Right <= (pageText.Width / 2))
        //        .OrderBy(b => b.BoundingBox.Top)
        //        .Take(1).ToList();

        //    var topRightBlock = blocks
        //        .Where(b => b.BoundingBox.Top < (pageText.Height / 3) && b.BoundingBox.Left > (pageText.Width / 2))
        //        .OrderBy(b => b.BoundingBox.Top)
        //        .Take(1).ToList();
        //    //.FirstOrDefault();

        //    string topLeftRight = string.Join(" ", topLeftBlock, topRightBlock);

        //    return $"{topLeftRight}".Trim();

        // Get top-left blocks 
        //var topLeftBlocks = blocks
        //    .Where(b => b.BoundingBox.TopLeft.Y > (pageText.Height * 2 / 3) && b.BoundingBox.TopLeft.X <= (pageText.Width / 2))
        //    .OrderByDescending(b => b.BoundingBox.TopLeft.Y)
        //    .Take(1)  // Take first two lines
        //    .Select(b => b.Text);

        //// Get top-right blocks 
        //var topRightBlocks = blocks
        //    .Where(b => b.BoundingBox.TopRight.Y > (pageText.Height * 2 / 3) && b.BoundingBox.TopRight.X > (pageText.Width / 2))
        //    .OrderByDescending(b => b.BoundingBox.TopRight.Y)
        //    .Take(1)  // Take first two lines
        //    .Select(b => b.Text);


        //string topLeftText = string.Join(" ", topLeftBlocks);
        //string topRightText = string.Join(" ", topRightBlocks);
        //return $"{topLeftText} {topRightText}".Trim();


        //}


        //private async Task getPageFromPDF(string pdfFile, string pageToFind)
        //{
        //    //List<string> pagesInsidePDF = new List<string>();
        //    List<int> matchingPages = new List<int>();
        //    using (UglyToad.PdfPig.PdfDocument document = UglyToad.PdfPig.PdfDocument.Open(pdfFile))
        //    {
        //        //PdfDocumentBuilder builder = new PdfDocumentBuilder();

        //        // Loop through all pages in the input PDF
        //        for (int pageNumber = 1; pageNumber <= document.NumberOfPages; pageNumber++)
        //        {
        //            UglyToad.PdfPig.Content.Page page = document.GetPage(pageNumber);
        //            //builder.AddPage(page); // Add the current page to the new document
        //            string pageText = page.Text;

        //            if (pageText.Contains(pageToFind))
        //            {
        //                matchingPages.Add(pageNumber); // Store the page number that contains "203"
        //            }

        //            //pagesInsidePDF.Add(pageText);
        //        }
        //    }

        //}

        //string script = @"
        //    (() => {
        //        let pages = document.querySelectorAll('page');
        //        let visiblePages = [];

        //        pages.forEach(page => {
        //            let rect = page.getBoundingClientRect();
        //            if (rect.top >= 0 && rect.bottom <= window.innerHeight) {
        //                visiblePages.push(page.innerText.trim());
        //            }
        //        });

        //        return visiblePages.length > 0 ? visiblePages[0] : null;
        //    })();
        //";


        //string script = @"
        //                (() => {
        //                    window.addEventListener('scroll', function() {
        //                        let visiblePage = null;
        //                        let pages = document.querySelectorAll('page'); // All <page> elements in the DOM

        //                        // Loop through all pages and check if they are visible in the viewport
        //                        for (let page of pages) {
        //                            let rect = page.getBoundingClientRect();
        //                            if (rect.top >= 0 && rect.bottom <= window.innerHeight) {
        //                                visiblePage = page.outerHTML;
        //                                break;
        //                            }
        //                        }

        //                        // Send the visible page information to C#
        //                        if (visiblePage) {
        //                            CefSharp.PostMessage(visiblePage);
        //                        }
        //                    });
        //                })();
        //            ";

        //private async Task<int?> GetVisiblePageNumberWithoutJS()
        //{
        //    if (CefBrowsers.GetBrowser() is null) return null;

        //    var host = CefBrowsers.GetBrowser().GetHost();
        //    var frame = CefBrowsers.GetBrowser().MainFrame;

        //    // Get scroll position
        //    var scrollPosition = await GetScrollPositionAsync();
        //    if (scrollPosition is null) return null;

        //    double scrollY = scrollPosition.Value;

        //    // Estimate the visible page
        //    int? visiblePage = EstimateVisiblePage(scrollY);

        //    WarningMessage($"Currently visible page: {visiblePage}");
        //    return visiblePage;
        //}

        //// Helper method to get scroll position
        //private async Task<double?> GetScrollPositionAsync()
        //{
        //    var response = await CefBrowsers.EvaluateScriptAsync("window.scrollY;");
        //    if (response.Success && response.Result is double scrollY)
        //    {
        //        return scrollY;
        //    }
        //    return null;
        //}

        //// Method to estimate which page is visible based on scroll position
        //private int? EstimateVisiblePage(double scrollY)
        //{
        //    // Define estimated Y-positions of <page> elements
        //    var pagePositions = new Dictionary<int, double>
        //        {
        //            { 1, 0 },   // Page 1 starts at Y = 0
        //            { 2, 800 }, // Example: Page 2 starts at Y = 800
        //            { 3, 1600 },
        //            { 4, 2400 },
        //            // Add more based on your XML layout
        //        };

        //    foreach (var page in pagePositions.OrderByDescending(p => p.Value))
        //    {
        //        if (scrollY >= page.Value)
        //        {
        //            return page.Key;
        //        }
        //    }

        //    return null;
        //}
        #endregion
    }
}
