using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Usml_Xml_Pdf_Viewer.Model
{
    internal class ErrorReportModel
    {
        public string FileName { get; set; }
        public int VisiblePage { get; set; }
        public string HighlightText { get; set; }
        public string HighlightXmlTag { get; set; }
        public int LineNumberXML { get; set; }

        public string Generic { get; set; }
        public string Remarks { get; set; }

    }
}
