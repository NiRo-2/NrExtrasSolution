namespace NrExtras.EnumConverter
{
    public static class EnumConverter
    {
        /// <summary>
        /// Convert string to enum. example: ToEnum<EnumType>(inString)
        /// </summary>
        /// <typeparam name="T">enum type</typeparam>
        /// <param name="value">in string</param>
        /// <returns>enum object</returns>
        public static T ToEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }
    }
}