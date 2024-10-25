﻿using Newtonsoft.Json.Linq;

namespace NrExtras.DynamicObjects_Helper
{
    public static class DynamicObjects_Helper
    {
        /// <summary>
        /// Extention which addes the possibility to do IsNullOrEmpty to arrays
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns>true if not null or empty. false if so</returns>
        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length == 0;
        }

        /// <summary>
        /// Convert object to dictionary of key and value
        /// </summary>
        /// <param name="inObj"></param>
        /// <returns>Diconary of props and values</returns>
        public static Dictionary<string, string> objectToDictionary(object inObj)
        {
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

            Type myType = inObj.GetType();
            if (myType == typeof(JObject))
            {//handle JObject
                foreach (var x in (JObject)inObj)
                {
                    string propName = x.Key;
                    var propVal = x.Value;
                    //only add if we have value
                    if (propVal != null)
                        keyValuePairs.Add(propName, propVal.ToString());
                }
            }
            else
            {//normal object
                IList<System.Reflection.PropertyInfo> props = new List<System.Reflection.PropertyInfo>(myType.GetProperties());
                foreach (System.Reflection.PropertyInfo prop in props)
                {
                    string propName = prop.Name;
                    var propVal = prop.GetValue(inObj, null);
                    //only add if we have value
                    if (propVal != null)
                        keyValuePairs.Add(propName, propVal.ToString());
                }
            }
            //return result
            return keyValuePairs;
        }
    }
}