using System.Text;

namespace NrExtras.StringsHelper
{
    public static class StringsHelper
    {
        /// <summary>
        /// Remove everything which is not a char (space also). leaves only digits and letters
        /// </summary>
        /// <param name="dirtyString">problematic string</param>
        /// <returns>cleaned string</returns>
        public static string CleanStringFromAllButDigitsAndChars(string dirtyString)
        {
            return new String(dirtyString.Where(Char.IsLetterOrDigit).ToArray());
        }

        #region ltr/rtl handle - reverse string if rtl
        /// <summary>
        /// fix rtl reverse in string
        /// </summary>
        /// <param name="inStr">in string</param>
        /// <returns>same string with all rtl words reversed</returns>
        public static string FixRtlReverseInString(string inStr)
        {
            LanguageDetector languageDetector = new LanguageDetector();
            switch (languageDetector.Detect(inStr))
            {
                case "he": //reverse
                    return ReverseString(inStr);
                default: //don't reverse
                    return inStr;
            }
        }

        /// <summary>
        /// reverse string
        /// </summary>
        /// <param name="s"></param>
        /// <returns>revered string</returns>
        public static string ReverseString(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
        #endregion
        //TODO: maybe fix chars by replacing to escape chars, for now only repalce with _
        /// <summary>
        /// repair esacpe chars in string
        /// </summary>
        /// <param name="query">string to fix</param>
        /// <param name="allowSingleApostrophe">false by default. if true, allow "'"</param>
        /// <returns></returns>
        public static string FixEscapeCharacterSequence(string query, bool allowSingleApostrophe = false)
        {
            if (allowSingleApostrophe == false) query = query.Replace("'", "_");
            query = query.Replace("\"", "_");
            query = query.Replace("\\", "_");
            query = query.Replace("/", "_");
            query = query.Replace("?", "_");
            query = query.Replace("`", "_");
            query = query.Replace(";", "_");
            query = query.Replace("*", "_");
            query = query.Replace("=", "_");
            return query;
        }

        /// <summary>
        /// Get now date and time as string example: 20220821100140
        /// </summary>
        /// <returns>date and time string</returns>
        public static string GetDateAndTimeString_NoSpacesNoEscapeChars()
        {
            return long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss")).ToString();
        }

        /// <summary>
        /// Get string from base64 string
        /// </summary>
        /// <param name="encodedString">base64 string</param>
        /// <returns>decoded string</returns>
        public static string FromBase64(string encodedString)
        {
            byte[] data = Convert.FromBase64String(encodedString);
            return Encoding.UTF8.GetString(data);
        }
        /// <summary>
        /// Get string from base64 bytes
        /// </summary>
        /// <param name="encodedBytes">base64-encoded bytes</param>
        /// <returns>decoded string</returns>
        public static string FromBase64(byte[] encodedBytes)
        {
            return Encoding.UTF8.GetString(encodedBytes);
        }
        /// <summary>
        /// Get byte array from base64 string
        /// </summary>
        /// <param name="encodedString">base64 string</param>
        /// <returns>decoded byte array</returns>
        public static byte[] FromBase64ToBytes(string encodedString)
        {
            return Convert.FromBase64String(encodedString);
        }
        /// <summary>
        /// Convert string to base 64 string
        /// </summary>
        /// <param name="inputString">input string</param>
        /// <returns>base64 string</returns>
        public static string ToBase64(string inputString)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(inputString);
            return Convert.ToBase64String(plainTextBytes);
        }
        /// <summary>
        /// Convert byte array to base64 string
        /// </summary>
        /// <param name="inputBytes">input byte array</param>
        /// <returns>base64 string</returns>
        public static string ToBase64(byte[] inputBytes)
        {
            return Convert.ToBase64String(inputBytes);
        }

        //string compression using LZStringCSharp
        public static class StringCompressor_lzString
        {
            public static string CompressString(string text)
            {
                return LZStringCSharp.LZString.CompressToEncodedURIComponent(text);
            }

            public static string DecompressString(string compressedText)
            {
                return LZStringCSharp.LZString.DecompressFromEncodedURIComponent(compressedText);
            }
        }
    }
}