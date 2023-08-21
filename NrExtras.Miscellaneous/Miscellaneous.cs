namespace NrExtras.Miscellaneous
{
    public static class Miscellaneous
    {
        /// <summary>
        /// parse all to bool
        /// </summary>
        /// <param name="val"></param>
        /// <returns>bool</returns>
        public static bool parseToBool(string val)
        {
            try
            {
                return bool.Parse(val);
            }
            catch
            {//faild, try different way. if 1, return true. else, return false
                return (val == "1");
            }
        }
    }
}