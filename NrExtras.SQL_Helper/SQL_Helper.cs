using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Data;
using System.Reflection;

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

            // Basic pre-validation for numeric values
            if (decimal.TryParse(sql, out _) || string.IsNullOrEmpty(sql))
                return true; // Numeric values and empty strings are valid

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
                                Console.WriteLine($"SQL_Helper.DataReaderMapToList: Error mapping data to object: {ex.Message}");
                                continue; // Skip this property and continue with the next one
                            }
                            else
                            {
                                throw; // Rethrow the exception to allow it to propagate
                            }
                        }
                    }
                    list.Add(obj);
                }
            }
            catch (Exception ex)
            {
                throw; // Rethrow the exception to allow it to propagate
            }
            finally
            {
                reader.Close();
            }

            return list;
        }
    }
}