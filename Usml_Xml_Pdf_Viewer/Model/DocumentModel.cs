using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlzEx.Standard;

namespace Usml_Xml_Pdf_Viewer.Model
{
    internal class DocumentModel : Abstract.ViewModelBase
    {
        public string TopContent { get; set; }
        public string BottomContent { get; set; }
        public int pdfPage {  get; set; }

    }
}
