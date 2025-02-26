using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit.Rendering;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using System.Windows;
using System.Text.RegularExpressions;

namespace Usml_Xml_Pdf_Viewer.Service
{
    internal class TagChangeColor : DocumentColorizingTransformer
    {
        private readonly Regex sTagRegex = new Regex(@"<(/?S\d+)[^>]*>", RegexOptions.Compiled);

        protected override void ColorizeLine(DocumentLine line)
        {
            string text = CurrentContext.Document.GetText(line);
            MatchCollection matches = sTagRegex.Matches(text);

            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    int startOffset = line.Offset + match.Index;
                    int endOffset = line.Offset + match.Index + match.Length;

                    ChangeLinePart(startOffset, endOffset, (visualElement) =>
                    {
                        //Apply Color
                        visualElement.TextRunProperties.SetForegroundBrush(Brushes.Red);
                        //visualElement.TextRunProperties.SetFontWeight(FontWeights.Bold);

                        //Apply bold font
                        var typeface = visualElement.TextRunProperties.Typeface;
                        visualElement.TextRunProperties.SetTypeface(new Typeface(
                            typeface.FontFamily,
                            typeface.Style,
                            FontWeights.Bold, 
                            typeface.Stretch
                        ));
                    });
                }
            }
        }
    }
}
