namespace NrExtras.DateAndTime
{
    public static class DateAndTime
    {
        /// <summary>
        /// Convert Unix time value to a DateTime object.
        /// </summary>
        /// <param name="unixtime">The Unix time stamp you want to convert to DateTime - seconds.</param>
        /// <param name="toLocalTime">false by default. of false, return universal time zone</param>
        /// <returns>Returns a DateTime object that represents value of the Unix time (utc time zone).</returns>
        public static DateTime UnixTimeToDateTime(long unixtime, bool toLocalTime = false)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixtime);

            if (toLocalTime)
                return dtDateTime.ToLocalTime();
            else
                return dtDateTime.ToUniversalTime();
        }

        /// <summary>
        /// Converts a given DateTime into a Unix timestamp
        /// </summary>
        /// <param name="value">Any DateTime</param>
        /// <returns>The given DateTime in Unix timestamp format</returns>
        public static int DateTimeToUnixTimeStamp(DateTime value)
        {
            return (int)Math.Truncate((value.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
        }

        /// <summary>
        /// Get nice formatted date string as yyyyMMdd
        /// </summary>
        /// <param name="dateTime">null as default. if left null, using Now</param>
        /// <returns>Date string formatted in a nice way</returns>
        public static string GetNiceFormattedDate(DateTime? dateTime = null)
        {
            DateTime currentDate = DateTime.Now;
            if (dateTime != null)
                currentDate = (DateTime)dateTime;

            return currentDate.ToString("yyyyMMdd");
        }
    }
}