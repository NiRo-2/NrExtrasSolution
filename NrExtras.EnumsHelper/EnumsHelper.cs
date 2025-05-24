using System.ComponentModel;
using System.Reflection;

namespace NrExtras.EnumsHelper
{
    public static class EnumsHelper
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

        /// <summary>
        /// Get the description of an enum value. If no description is found, the enum name is returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string GetEnumDescription<T>(T enumValue) where T : Enum
        {
            var fi = enumValue.GetType().GetField(enumValue.ToString());
            var descriptionAttribute = fi.GetCustomAttribute<DescriptionAttribute>();
            return descriptionAttribute != null ? descriptionAttribute.Description : enumValue.ToString();
        }
    }
}