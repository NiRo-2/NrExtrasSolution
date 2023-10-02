using Ganss.Xss;
using System.IO.Compression;
using System.Text;
using System.Web;
using WebMarkupMin.Core;

namespace NrExtras.Html_Helper
{
    public static class Html_Helper
    {
        public const string htmlNewLine = "<br>";

        /// <summary>
        /// Sanitize html from html/javascript injections
        /// </summary>
        /// <param name="html">input html</param>
        /// <returns>sanitized html</returns>
        public static string SanitizeHtml(string html)
        {
            //incase there is nothing to do
            if (string.IsNullOrEmpty(html)) return html;
            //return sanitized html
            return new HtmlSanitizer().Sanitize(html);
        }

        /// <summary>
        /// create hyper link
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="str">text represent url</param>
        /// <returns>hyper link</returns>
        public static string GetHyperLink(string url, string str)
        {
            //no nulls
            if (url == null)
                url = "";
            if (str == null)
                str = "";

            //return result
            return "<a href=\"" + url + "\">" + str + "</a>";
        }

        /// <summary>
        /// get html and return html minified version
        /// </summary>
        /// <param name="htmlInput">html code input</param>
        /// <param name="writeStatisticsToConsole">false by default. if true, write space saving statistics to console</param>
        /// <returns>minified html as string</returns>
        /// <exception cref="Exception">incase of errors - throw all exceptions together</exception>
        public static string HtmlMinfier(string htmlInput, bool writeStatisticsToConsole = false)
        {
            var htmlMinifier = new HtmlMinifier();

            MarkupMinificationResult result = htmlMinifier.Minify(htmlInput, generateStatistics: true);
            if (result.Errors.Count == 0)
            {
                if (writeStatisticsToConsole)
                {
                    MinificationStatistics statistics = result.Statistics;
                    if (statistics != null)
                    {
                        Console.WriteLine("Original size: {0:N0} Bytes",
                            statistics.OriginalSize);
                        Console.WriteLine("Minified size: {0:N0} Bytes",
                            statistics.MinifiedSize);
                        Console.WriteLine("Saved: {0:N2}%",
                            statistics.SavedInPercent);
                    }
                }

                //return minified htnk
                return result.MinifiedContent;
            }
            else
            {//throw error - create nice string and throw it back
                IList<MinificationErrorInfo> errors = result.Errors;
                string errorsString = $"Found {0:N0} error(s): {errors.Count}";
                foreach (var error in errors)
                    errorsString += Environment.NewLine + $"Line {error.LineNumber}, Column {error.ColumnNumber}: {error.Message}";

                throw new Exception(errorsString);
            }
        }
    }
}