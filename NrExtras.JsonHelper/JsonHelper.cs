using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NrExtras.JsonHelper
{
    public static class JsonHelper
    {
        /// <summary>
        /// Validating json string
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns>true if valid, false otherwise</returns>
        public static bool IsValid(string jsonString)
        {
            try
            {
                JObject.Parse(jsonString);
                return true;
            }
            catch (JsonReaderException)
            {
                return false;
            }
        }
    }
}