using TimeZoneConverter;

namespace NrExtras.DateAndTime
{
    /// <summary>
    /// Extension class for DateTime and DateTimeOffset - holds time zone information along side the date.
    /// </summary>
    public class DateTimeWithZone
    {
        public DateTimeOffset DateTimeOffset { get; set; }
        public TimeZoneInfo TimeZoneInfo { get; set; }
        public string TimeZoneId => TimeZoneInfo.Id;
        public TimeZoneInfo TimeZone => TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
        public DateTime UtcDateTime => DateTimeOffset.UtcDateTime;
        public DateTime LocalDateTime
        {
            get
            {
                // Convert the current DateTimeOffset to the time zone of the object
                DateTimeOffset convertedTime = TimeZoneInfo.ConvertTime(DateTimeOffset, TimeZone);

                // Now convert that to the local system time zone
                DateTimeOffset localTime = TimeZoneInfo.ConvertTime(convertedTime, TimeZoneInfo.Local);

                // Return the local time as DateTime (DateTime represents the moment in the local time zone)
                return localTime.DateTime;
            }
        }

        /// <summary>
        /// Constructor using DateTime and optional TimeZoneInfo. Falls back to local time zone if null.
        /// </summary>
        /// <param name="localDateTime">The local DateTime</param>
        /// <param name="timeZoneInfo">The time zone info; if null, defaults to local time zone</param>
        public DateTimeWithZone(DateTime localDateTime, TimeZoneInfo? timeZoneInfo = null)
        {
            TimeZoneInfo = timeZoneInfo ?? TimeZoneInfo.Local;
            var offset = TimeZoneInfo.GetUtcOffset(localDateTime);
            DateTimeOffset = new DateTimeOffset(localDateTime, offset);
        }

        /// <summary>
        /// Compares two DateTimeWithZone objects and returns the difference in time.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static TimeSpan Compare(DateTimeWithZone first, DateTimeWithZone second)
            => second.DateTimeOffset - first.DateTimeOffset;

        /// <summary>
        /// Get the time zone info from the time zone ID using TZConvert which is a wrapper around the Windows and IANA time zone databases.
        /// </summary>
        /// <param name="timeZoneId"></param>
        /// <returns></returns>
        public static TimeZoneInfo GetTimeZoneInfoFromId(string timeZoneId)
        {
            return TZConvert.GetTimeZoneInfo(timeZoneId);
        }

        /// <summary>
        /// Returns a string representation of the DateTimeWithZone object.
        /// </summary>
        /// <param name="includeZone">true by default</param>
        /// <returns></returns>
        public string ToString(bool includeZone = true)
        {
            return includeZone
                ? $"{DateTimeOffset} ({TimeZoneId})"
                : $"{LocalDateTime}";
        }

        /// <summary>
        /// Returns a string representation of the DateTimeWithZone object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(true);
        }

        /// <summary>
        /// Converts this DateTimeWithZone to a DateTime in another time zone.
        /// </summary>
        /// <param name="targetTimeZone">The target time zone.</param>
        /// <returns>The DateTime in the target time zone.</returns>
        public DateTime ToTimeZone(TimeZoneInfo targetTimeZone)
        {
            return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcDateTime, targetTimeZone);
        }
    }
}