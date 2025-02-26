using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NHunspell;

namespace Usml_Xml_Pdf_Viewer.Service
{
    internal class SpellCheckerService : Abstract.ViewModelBase
    {
        public string SpellCheckerservice(string xmlpath)
        {
            try
            {
                string updatedXmlContent = null;

                // Read XML content
                string xmlContent = File.ReadAllText(xmlpath, Encoding.UTF8);

                // Match <publicLaws> content using regex
                //Match match = Regex.Match(xmlContent, @"(<publicLaws>)(.*?)(</publicLaws>)", RegexOptions.Singleline);
                Match match = Regex.Match(xmlContent, @"(<statutesAtLarge[\s\S]*?>)([\s\S]*)(</statutesAtLarge>)", RegexOptions.Singleline);
                if (!match.Success) return null; // No <publicLaws> found

                //string publicLawsContent = match.Groups[2].Value; // Get content inside <publicLaws>
                string statutesContent = match.Groups[2].Value;

                using (Hunspell hunspell = new Hunspell(@"Asset\en_US.aff", @"Asset\en_US.dic"))
                {
                    // Process text inside <publicLaws>
                    string updatedPublicLawsContent = Regex.Replace(statutesContent, @"(?<=>)([^<>]+)(?=<)", textMatch =>
                    {
                        return string.Join(" ", textMatch.Value.Split(' ').Select(word =>
                        {
                            string[] parts = word.Split(new char[] { '/', '-', ':', ';', ',', '.' }, StringSplitOptions.RemoveEmptyEntries);

                            foreach (var part in parts)
                            {
                                string cleanedPart = Regex.Replace(part, @"[^a-zA-Z]", "");

                                if (!string.IsNullOrEmpty(cleanedPart) && !hunspell.Spell(cleanedPart))
                                {
                                    return word.Replace(cleanedPart, $"<spellchecker>{cleanedPart}</spellchecker>");
                                }
                            }

                            

                            return word;
                        }));
                    });

                    // Replace original <publicLaws> content with spell-checked content
                   return updatedXmlContent = xmlContent.Replace(statutesContent, updatedPublicLawsContent);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
                return null;
            }
        }
    }
}
