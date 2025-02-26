using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Usml_Xml_Pdf_Viewer.Model;

namespace Usml_Xml_Pdf_Viewer.Service
{
    internal class SidenoteService : Abstract.ViewModelBase
    {
        /// <summary>
        /// <sidenote></sidenote> to <sidenote1></sidenote1>
        /// </summary>
        public string AddIdOnSidenote(string xmlcontent)
        {
            try
            {

                int sidenoteCounter = 1;

                // Store changed sidenotes
                //List<string> changedSidenotes = new List<string>();

                // Use regex to find all <sidenote>...</sidenote> elements
                Regex sidenoteNoId = new Regex(@"<sidenote>(.*?)<\/sidenote>", RegexOptions.Singleline);

                // Replace each occurrence with an incrementing sidenote tag
                string SidenoteWithID = sidenoteNoId.Replace(xmlcontent, match =>
                {
                    string oldSidenote = match.Value;
                    string replacement = $"<sidenote{sidenoteCounter}>{match.Groups[1].Value}</sidenote{sidenoteCounter}>";

                    //// Track the changed sidenotes
                    //changedSidenotes.Add($"Old: {oldSidenote} → New: {replacement}");
                    sidenoteCounter++;
                    return replacement;
                });

                //WarningMessage($"All sidenote {changedSidenotes.Count}");

                // Save the updated XML content back to the file

                return SidenoteWithID;
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
                return xmlcontent;
            }
        }


        /// <summary>
        /// add <S#> 
        /// </summary>
        public string AddPlaceholderTagS(string xmlcontent)
        {
            try
            {

                string updatedCodingText = "";

                // Count existing sidenotes in the XML
                Regex sidenoteRegex = new Regex(@"<sidenote(\d+)>", RegexOptions.Singleline);
                int sidenoteCount = sidenoteRegex.Matches(xmlcontent).Count;

                // Find the lowest missing <S#> tag
                Regex placeholderRegex = new Regex(@"<S(\d+)>", RegexOptions.Singleline);

                HashSet<int> existingPlaceholders = new HashSet<int>();

                foreach (Match match in placeholderRegex.Matches(xmlcontent))
                {
                    existingPlaceholders.Add(int.Parse(match.Groups[1].Value));
                }

                // Find the first missing placeholder number
                int nextSidenoteIndex = 1;
                while (existingPlaceholders.Contains(nextSidenoteIndex))
                {
                    nextSidenoteIndex++;
                }

                // Check if the next sidenote exists
                if (nextSidenoteIndex > sidenoteCount)
                {
                    WarningMessage($"No available <sidenote{nextSidenoteIndex}> found in the document!");
                    return null;
                }

                // Insert "<S#>" at the caret position
                updatedCodingText += $"<S{nextSidenoteIndex}>";

                // Increment sidenote number for next click
                nextSidenoteIndex++;

                return updatedCodingText;
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
                return null;
            }
        }

        /// <summary>
        /// <sidenote1></sidenote1> place to <S#>
        /// </summary>
        public string LinkSidenote(string xmlcontent, ObservableCollection<SidenoteModel> sidenoteCollection)
        {

            try
            {
                Regex sidenoteWithId = new Regex(@"<sidenote(\d+)>([\s\S]*?)<\/sidenote\1>", RegexOptions.Singleline);
                Regex placeholderRegex = new Regex(@"<S(\d+)>");

                // Check if there are sidenoteWithId
                bool hassidenoteWithId = sidenoteWithId.IsMatch(xmlcontent);
                if (!hassidenoteWithId) { WarningMessage("No <sidenote#> found"); return xmlcontent; }


                // Replace placeholders with corresponding sidenotes
                // Check if there are placeholders
                bool hasPlaceholders = placeholderRegex.IsMatch(xmlcontent);
                if (!hasPlaceholders) { WarningMessage("No <S#> found"); return xmlcontent; }
                else
                {
                    // Extract, store, and remove all sidenotes in a single pass
                    xmlcontent = sidenoteWithId.Replace(xmlcontent, match =>
                    {
                        sidenoteCollection.Add(new SidenoteModel
                        {
                            SidenoteNum = match.Groups[1].Value, // "1", "2", etc.
                            SidenoteContext = match.Groups[2].Value // Content inside sidenote
                        });

                        // Remove the original sidenote by replacing it with an empty string
                        return "";
                    });

                    // Replace placeholders with the correct sidenote tags
                    string updatedXmlContent = placeholderRegex.Replace(xmlcontent, match =>
                    {
                        string sidenoteIndex = match.Groups[1].Value;

                        var sidenote = sidenoteCollection.FirstOrDefault(s => s.SidenoteNum == sidenoteIndex);

                        //return sidenote != null ? $"<sidenote{sidenoteIndex}>{sidenote.SidenoteContext}</sidenote{sidenoteIndex}>" : match.Value;
                        return sidenote != null ? $"<sidenote>{sidenote.SidenoteContext}</sidenote>" : match.Value;
                    });

                    return updatedXmlContent;
                }

            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
                return xmlcontent;
            }
        }

    }
}
