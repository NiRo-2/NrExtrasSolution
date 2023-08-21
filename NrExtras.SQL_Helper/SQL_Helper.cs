using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Data;
using System.Reflection;
using static NrExtras.Logger.Logger;

namespace NrExtras.SQL_Helper
{
    public static class SQL_Helper
    {
        /// <summary>
        /// Validate string is sql query
        /// </summary>
        /// <param name="sql">sql query</param>
        /// <returns>true if valid, false otherwise</returns>
        public static bool IsSQLQueryValid(string sql)
        {
            List<string> errors = new List<string>();
            return IsSQLQueryValid(sql, out errors);
        }
        /// <summary>
        /// Validate string is sql query
        /// </summary>
        /// <param name="sql">sql query</param>
        /// <param name="errors">list of errors</param>
        /// <returns>true if valid sql query, false otherwise</returns>
        public static bool IsSQLQueryValid(string sql, out List<string> errors)
        {
            errors = new List<string>();
            TSql140Parser parser = new TSql140Parser(false);
            TSqlFragment fragment;
            IList<ParseError> parseErrors;

            using (TextReader reader = new StringReader(sql))
            {
                fragment = parser.Parse(reader, out parseErrors);
                if (parseErrors != null && parseErrors.Count > 0)
                {
                    errors = parseErrors.Select(e => e.Message).ToList();
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// convert reader to list
        /// </summary>
        /// <typeparam name="T">class type</typeparam>
        /// <param name="dr">reader</param>
        /// <param name="ignoreArgumentOutOfRangeException">if set to true, ignore this error. this error occure when there is a mismatch between the model and db return. can be on purpose.</param>
        /// <returns>list of found objects</returns>
        public static List<T> DataReaderMapToList<T>(this IDataReader reader, bool ignoreArgumentOutOfRangeException = false) where T : new()
        {
            List<T> list = new List<T>();

            try
            {
                while (reader.Read())
                {
                    T obj = new T();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        try
                        {
                            PropertyInfo property = obj.GetType().GetProperty(reader.GetName(i));
                            if (property != null && reader[i] != DBNull.Value)
                            {
                                property.SetValue(obj, reader[i], null);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log the exception and continue or rethrow based on ignoreArgumentOutOfRangeException
                            if (ex is ArgumentOutOfRangeException && ignoreArgumentOutOfRangeException)
                            {
                                Logger.Logger.WriteToLog($"Error mapping data to object: {ex.Message}", LogLevel.Warning);
                                continue; // Skip this property and continue with the next one
                            }
                            else
                            {
                                Logger.Logger.WriteToLog($"Error mapping data to object: {ex.Message}", LogLevel.Error);
                                throw; // Rethrow the exception to allow it to propagate
                            }
                        }
                    }
                    list.Add(obj);
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.WriteToLog($"Error reading data from data reader: {ex.Message}", LogLevel.Error);
                throw; // Rethrow the exception to allow it to propagate
            }
            finally
            {
                reader.Close();
            }

            return list;
        }

        //Old almost working function, here for archive reasons. can be remove in the future if new function is working well
        ///// <summary>
        ///// convert reader to list
        ///// </summary>
        ///// <typeparam name="T">class type</typeparam>
        ///// <param name="dr">reader</param>
        ///// <param name="ignoreArgumentOutOfRangeException">if set to true, ignore this error. this error occure when there is a mismatch between the model and db return. can be on purpose.</param>
        ///// <returns>list of found objects</returns>
        //public static List<T> DataReaderMapToList<T>(System.Data.IDataReader dr, bool ignoreArgumentOutOfRangeException = false)
        //{
        //    List<T> list = new List<T>();
        //    T obj = default(T);
        //    while (dr.Read())
        //    {
        //        obj = Activator.CreateInstance<T>();
        //        foreach (System.Reflection.PropertyInfo prop in obj.GetType().GetProperties())
        //        {
        //            try
        //            {
        //                //get field data
        //                var fieldData = dr[prop.Name];

        //                //check null
        //                if (!Equals(fieldData, DBNull.Value))
        //                {
        //                    Type type = prop.PropertyType;
        //                    //if we have long, convertion to int is needed
        //                    if (type == typeof(long))
        //                    {
        //                        //long to int
        //                        int iVal = (int)((long)fieldData);

        //                        //int or bool
        //                        if (prop.PropertyType == typeof(int)) //int
        //                            prop.SetValue(obj, iVal, null);
        //                        else if (prop.PropertyType == typeof(bool)) //bool
        //                            prop.SetValue(obj, Convert.ToBoolean(iVal), null);
        //                    }
        //                    else if (type == typeof(string)) //string
        //                        prop.SetValue(obj, fieldData, null);
        //                    else if (type == typeof(double)) //double
        //                        prop.SetValue(obj, fieldData, null);
        //                    else if (type == typeof(float)) //float
        //                        prop.SetValue(obj, fieldData, null);
        //                    else if (type == typeof(int)) //int
        //                        prop.SetValue(obj, fieldData, null);
        //                    else if (type == typeof(DateTime)) //DateTime
        //                        prop.SetValue(obj, fieldData, null);
        //                    else
        //                    {//type not found
        //                        Type t = dr[prop.Name].GetType();
        //                        NrExtras.Miscellaneous.WriteToLog("Converting data from reader - type not found in list. fix it. type name: " + t, LogLevel.Error);
        //                    }
        //                }
        //            }
        //            catch (ArgumentOutOfRangeException ex)
        //            {//add option to ignore this exception. sometimes it's ok.
        //                if (!ignoreArgumentOutOfRangeException)
        //                {
        //                    Miscellaneous.WriteToLog("ArgumentOutOfRange occured while parsing data from reader. this error can be due to mismatch between model and db return. Err: " + ex.Message, LogLevel.Error);
        //                    throw;
        //                }
        //            }
        //            catch (IndexOutOfRangeException ex)
        //            {
        //                {//add option to ignore this exception. sometimes it's ok.
        //                    if (!ignoreArgumentOutOfRangeException)
        //                    {
        //                        Miscellaneous.WriteToLog("ArgumentOutOfRange occured while parsing data from reader. this error can be due to mismatch between model and db return. Err: " + ex.Message, LogLevel.Error);
        //                        throw;
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                NrExtras.Miscellaneous.WriteToLog("Error parsing returned data from WebApi Err: " + ex.Message, LogLevel.Error);
        //                throw;
        //            }
        //        }
        //        list.Add(obj);
        //    }
        //    return list;
        //}
    }
}