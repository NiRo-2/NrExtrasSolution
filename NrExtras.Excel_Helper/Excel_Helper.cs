﻿using ExcelDataReader;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Data;
using static NrExtras.Logger.Logger;

namespace NrExtras.Excel_Helper
{
    public class Excel_Helper
    {
        //A==0, B==1 and so on...
        public enum ColumnLettersToNum
        {
            NullColumn = -1, A = 0, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z, AA, AB, AC, AD, AE, AF, AG, AH, AI, AJ, AK, AL, AM, AN, AO, AP, AQ, AR, AS, AT, AU, AV, AW, AX, AY, AZ, BA, BB, BC, BD, BE, BF, BG, BH, BI, BJ, BK, BL, BM, BN, BO, BP, BQ, BR, BS, BT, BU, BV, BW, BX, BY, BZ, CA, CB, CC, CD, CE, CF, CG, CH, CI, CJ, CK, CL, CM, CN, CO, CP, CQ, CR, CS, CT, CU, CV, CW, CX, CY, CZ, DA, DB, DC, DD, DE, DF, DG, DH, DI, DJ, DK, DL, DM, DN, DO, DP, DQ, DR, DS, DT, DU, DV, DW, DX, DY, DZ
        }

        /// <summary>
        /// read xls file to Dictionary holding sheet name and sheet data (2d list)
        /// using ExcelDataReader
        /// </summary>
        /// <param name="filePath">file path</param>
        /// <param name="sheetName">sheet name. can be null to read whole workbook</param>
        /// <returns>workbook data in struct of dictionary of sheet name and sheetData</returns>
        public static Dictionary<string, List<List<string>>> ReadExcelFile_Values(string filePath, string? sheetName = null)
        {
            try
            {
                //add support encoding 1252
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                //prepare to return results
                Dictionary<string, List<List<string>>> sheetList = new Dictionary<string, List<List<string>>>();

                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    // Auto-detect format, supports:
                    //  - Binary Excel files (2.0-2003 format; *.xls)
                    //  - OpenXml Excel files (2007 format; *.xlsx, *.xlsb)
                    using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        DataSet result = reader.AsDataSet();

                        //convering dataset to our set
                        foreach (DataTable dataTable in result.Tables)
                        {
                            string _sheetName = dataTable.TableName;
                            //if we need to read only single sheet
                            if (!string.IsNullOrEmpty(sheetName))
                                if (_sheetName != sheetName)
                                    continue; //not this sheet - skip this sheet

                            //parsing table to sheet (list of list)
                            List<List<string>> sheet = new List<List<string>>();
                            foreach (var dataRow in dataTable.AsEnumerable().ToArray())
                            {
                                List<string> row = new List<string>();
                                foreach (var cell in dataRow.ItemArray)
                                {
                                    try
                                    {
                                        if (cell == null)//if null add empty string
                                            row.Add("");
                                        else //not null, get cell value
                                            row.Add(cell.ToString());
                                    }
                                    catch (Exception ex)
                                    {
                                        row.Add("");
                                        Logger.Logger.WriteToLog("Error while reading cell, adding empty val instead. Err: " + ex.Message, LogLevel.Error);
                                    }
                                }

                                //adding row to sheet
                                sheet.Add(row);
                            }

                            //adding sheet
                            sheetList.Add(_sheetName, sheet);
                        }
                    }
                }

                //return sheets
                return sheetList;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Get list of dynamic objects and return table in format of 2d lists.
        /// Accept different types of objects with unequal fields count.
        /// </summary>
        /// <param name="list">dynamic objects list</param>
        /// <returns>table</returns>
        /// <exception cref="Exception"></exception>
        public static List<List<string>> listOfObjectsToTable(List<object> list)
        {
            Dictionary<string, List<string>> table_columns = new Dictionary<string, List<string>>();

            //create unique headers row
            HashSet<string> headers = new HashSet<string>();
            foreach (dynamic item in list)
                foreach (string key in DynamicObjects_Helper.DynamicObjects_Helper.objectToDictionary(item).Keys)
                    headers.Add(key);

            //convert hash to list and prepare table
            foreach (string header in headers)
                table_columns.Add(header, new List<string>());

            //going through all items to create rows
            foreach (dynamic item in list)
            {
                //using dictionary to add each val to it's column
                Dictionary<string, string> keyValuePairs = DynamicObjects_Helper.DynamicObjects_Helper.objectToDictionary(item);
                foreach (var pair in keyValuePairs)
                    table_columns[pair.Key].Add(pair.Value);

                //add empty string to places we have no key for it
                for (int columnIndex = 1; columnIndex < table_columns.Count; columnIndex++)
                    if (table_columns.ElementAt(columnIndex).Value.Count < table_columns.ElementAt(columnIndex - 1).Value.Count) //compare each columns with the columns before it. if count if not the same, add empty string to it
                        table_columns.ElementAt(columnIndex).Value.Add(""); //found missing value
            }

            //rotate table and creating final table
            List<List<string>> finalTable = new List<List<string>>();
            //add headers row
            finalTable.Add(headers.ToList());

            //validate we have any values to go through
            if (table_columns.Count > 0)
            {
                //adding line by line
                for (int rowIndex = 0; rowIndex < table_columns.ElementAt(0).Value.Count; rowIndex++)
                {
                    //building rows
                    List<string> row = new List<string>();
                    foreach (var col in table_columns)
                        row.Add(col.Value[rowIndex]); //adding each cell by it's right index

                    //adding row
                    finalTable.Add(row);
                }
            }

            //return results
            return finalTable;
        }

        /// <summary>
        /// create excel file using APOI - https://github.com/nissl-lab/npoi
        /// </summary>
        /// <param name="filePath">out file path. should be with xlsx extention</param>
        /// <param name="excelData">excel data as dictionary of sheets. each sheet is list of lists creating 2d table</param>
        /// <param name="overWriteExists">default=ture. is false and file exists, throw exception</param>
        /// <exception cref="Exception"></exception>
        public static void WriteExclFile(string filePath, Dictionary<string, List<List<string>>> excelData, bool overWriteExists = true)
        {
            //if file exists and settings not to overwrite - throw exception
            if (File.Exists(filePath) && overWriteExists == false)
                throw new Exception("File " + filePath + " already exists.");

            try
            {
                IWorkbook workbook = new XSSFWorkbook();
                //write sheets
                foreach (var sheet in excelData)
                {
                    XSSFSheet xssf_sheet = (XSSFSheet)workbook.CreateSheet(sheet.Key);

                    for (int rowNum = 0; rowNum < sheet.Value.Count; rowNum++)
                    {//each row
                        var row = xssf_sheet.CreateRow(rowNum);
                        for (int colNum = 0; colNum < sheet.Value[rowNum].Count; colNum++)
                        {//each cell
                            var cell = row.CreateCell(colNum);
                            cell.SetCellValue(sheet.Value[rowNum][colNum]);
                        }
                    }
                }

                //write file
                using (FileStream sw = File.Create(filePath))
                    workbook.Write(sw, false);
            }
            catch
            {
                throw;
            }
        }
    }
}