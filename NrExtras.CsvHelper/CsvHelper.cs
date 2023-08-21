namespace NrExtras.CsvHelper
{
    public static class CsvHelper
    {
        /// <summary>
        /// Get list of dynamic objects and return csv string
        /// </summary>
        /// <param name="list">dynamic objects list</param>
        /// <param name="delimiter">"," by default</param>
        /// <returns>csv string</returns>
        /// <exception cref="Exception"></exception>
        public static string listOfObjectsToCSV(List<object> list, char delimiter = ',')
        {
            string csvString = "";
            int cellsCount = -1;
            List<string> headers = new List<string>();
            List<List<string>> rows = new List<List<string>>();

            //going through all items to create rows
            foreach (dynamic item in list)
            {
                Dictionary<string, string> keyValuePairs = DynamicObjects_Helper.DynamicObjects_Helper.objectToDictionary(item);

                //get cells count to make sure all items have same fields count
                if (cellsCount == -1)
                    cellsCount = keyValuePairs.Count;
                else
                { //validate we have same fields count
                    if (cellsCount != keyValuePairs.Count)
                        throw new Exception("All items must have same fields count");
                }

                //build header row if we don't have it yet
                if (headers.Count == 0)
                    foreach (string key in keyValuePairs.Keys)
                        headers.Add(key);

                //all good, build rows
                List<string> row = new List<string>();
                foreach (string key in keyValuePairs.Keys)
                {
                    string value = keyValuePairs[key].ToString();
                    row.Add(value);
                }

                rows.Add(row);
            }

            //building the final result
            List<List<string>> finalList = new List<List<string>>();
            finalList.Add(headers);
            finalList.AddRange(rows);

            //convert to csv
            foreach (List<string> row in finalList)
            {
                //write row
                foreach (string col in row)
                    csvString += "\"" + col + "\"" + delimiter;
                //write end of row
                csvString += "\n";
            }

            //return results
            return csvString;
        }

        /// <summary>
        /// Read csv to dictionary
        /// </summary>
        /// <param name="csv">csv string</param>
        /// <param name="skipHeaderRow">true by default</param>
        /// <param name="delimiter">',' by default</param>
        /// <returns>Dictionary of string,string</returns>
        public static Dictionary<string, string> ConvertCsvToDictionary(string csv, bool skipHeaderRow = true, char delimiter = ',')
        {
            var dictionary = new Dictionary<string, string>();

            using (var reader = new StringReader(csv))
            {
                // Skip the header row
                if (skipHeaderRow)
                    reader.ReadLine();

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    int commaIndex = line.IndexOf(delimiter);
                    if (commaIndex >= 0 && commaIndex < line.Length - 1)
                    {
                        string key = line.Substring(0, commaIndex).Trim().Trim('"');
                        string value = line.Substring(commaIndex + 1).Trim().Trim('"');
                        dictionary[key] = value;
                    }
                }
            }

            return dictionary;
        }
    }
}