using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text.RegularExpressions;

namespace NrExtras.DocX_Helper
{
    public static class DocX_Helper
    {
        /// <summary>
        /// Find and replace in doc file
        /// Limitations: special signs are ignored if combine with string to find
        /// </summary>
        /// <param name="inFilePath">in doc file path</param>
        /// <param name="outFilePath">out file path</param>
        /// <param name="findAndReplaceValues">dicionary of key to find and value to replace with</param>
        /// <param name="overwriteOutFile">true by default</param>
        public static void FindAndReplace_InDocFile(string inFilePath, string outFilePath, Dictionary<string, string> findAndReplaceValues, bool overwriteOutFile = true)
        {
            try
            {
                // Validating input arguments
                if (!File.Exists(inFilePath)) throw new Exception("inFilePath not found");
                if (overwriteOutFile == false)
                    if (File.Exists(outFilePath)) throw new Exception($"outFilePath already exists and overwriteOutFile is set to {overwriteOutFile}");

                //clone in file to out file path inorder to edit it there
                File.Copy(inFilePath, outFilePath, overwriteOutFile);

                // Open the newly cloned file in outPath
                using WordprocessingDocument doc = WordprocessingDocument.Open(outFilePath, true);

                // Recursively find and replace in the document body
                TraverseAndReplace(doc.MainDocumentPart.Document.Body, findAndReplaceValues);

                // Recursively find and replace in headers
                foreach (var headerPart in doc.MainDocumentPart.HeaderParts)
                    TraverseAndReplace(headerPart.Header, findAndReplaceValues);

                // Recursively find and replace in footers
                foreach (var footerPart in doc.MainDocumentPart.FooterParts)
                    TraverseAndReplace(footerPart.Footer, findAndReplaceValues);

                // Save the changes and close the document after saving
                doc.Save();
                doc.Close();
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately (e.g., log, display error message, etc.)
                File.Delete(outFilePath); //remove out file on error
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        //get element and find and replace all found keys in it
        private static void TraverseAndReplace(OpenXmlElement element, Dictionary<string, string> findAndReplaceValues)
        {
            if (element is Run run)
            {
                // Handle Run elements separately to replace text within each Run
                foreach (var textElement in run.Elements<DocumentFormat.OpenXml.Drawing.Text>())
                {
                    foreach (var entry in findAndReplaceValues)
                    {
                        string oldValue = entry.Key;
                        string newValue = entry.Value;

                        // Use regex to find and replace the text value within the Run
                        textElement.Text = Regex.Replace(textElement.Text, Regex.Escape(oldValue), newValue, RegexOptions.IgnoreCase);
                    }
                }
            }
            else
            {
                // For other elements (e.g., paragraphs, tables, etc.), recursively traverse through their child elements
                foreach (var childElement in element.Elements())
                {
                    TraverseAndReplace(childElement, findAndReplaceValues);
                }
            }
        }
    }
}