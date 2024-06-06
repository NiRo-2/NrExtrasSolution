using ExcelDataReader;
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
            NullColumn = -1,
            A = 0,
            B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
            AA, AB, AC, AD, AE, AF, AG, AH, AI, AJ, AK, AL, AM, AN, AO, AP, AQ, AR, AS, AT, AU, AV, AW, AX, AY, AZ,
            BA, BB, BC, BD, BE, BF, BG, BH, BI, BJ, BK, BL, BM, BN, BO, BP, BQ, BR, BS, BT, BU, BV, BW, BX, BY, BZ,
            CA, CB, CC, CD, CE, CF, CG, CH, CI, CJ, CK, CL, CM, CN, CO, CP, CQ, CR, CS, CT, CU, CV, CW, CX, CY, CZ,
            DA, DB, DC, DD, DE, DF, DG, DH, DI, DJ, DK, DL, DM, DN, DO, DP, DQ, DR, DS, DT, DU, DV, DW, DX, DY, DZ,
            EA, EB, EC, ED, EE, EF, EG, EH, EI, EJ, EK, EL, EM, EN, EO, EP, EQ, ER, ES, ET, EU, EV, EW, EX, EY, EZ,
            FA, FB, FC, FD, FE, FF, FG, FH, FI, FJ, FK, FL, FM, FN, FO, FP, FQ, FR, FS, FT, FU, FV, FW, FX, FY, FZ,
            GA, GB, GC, GD, GE, GF, GG, GH, GI, GJ, GK, GL, GM, GN, GO, GP, GQ, GR, GS, GT, GU, GV, GW, GX, GY, GZ,
            HA, HB, HC, HD, HE, HF, HG, HH, HI, HJ, HK, HL, HM, HN, HO, HP, HQ, HR, HS, HT, HU, HV, HW, HX, HY, HZ,
            IA, IB, IC, ID, IE, IF, IG, IH, II, IJ, IK, IL, IM, IN, IO, IP, IQ, IR, IS, IT, IU, IV, IW, IX, IY, IZ,
            JA, JB, JC, JD, JE, JF, JG, JH, JI, JJ, JK, JL, JM, JN, JO, JP, JQ, JR, JS, JT, JU, JV, JW, JX, JY, JZ,
            KA, KB, KC, KD, KE, KF, KG, KH, KI, KJ, KK, KL, KM, KN, KO, KP, KQ, KR, KS, KT, KU, KV, KW, KX, KY, KZ,
            LA, LB, LC, LD, LE, LF, LG, LH, LI, LJ, LK, LL, LM, LN, LO, LP, LQ, LR, LS, LT, LU, LV, LW, LX, LY, LZ,
            MA, MB, MC, MD, ME, MF, MG, MH, MI, MJ, MK, ML, MM, MN, MO, MP, MQ, MR, MS, MT, MU, MV, MW, MX, MY, MZ,
            NA, NB, NC, ND, NE, NF, NG, NH, NI, NJ, NK, NL, NM, NN, NO, NP, NQ, NR, NS, NT, NU, NV, NW, NX, NY, NZ,
            OA, OB, OC, OD, OE, OF, OG, OH, OI, OJ, OK, OL, OM, ON, OO, OP, OQ, OR, OS, OT, OU, OV, OW, OX, OY, OZ,
            PA, PB, PC, PD, PE, PF, PG, PH, PI, PJ, PK, PL, PM, PN, PO, PP, PQ, PR, PS, PT, PU, PV, PW, PX, PY, PZ,
            QA, QB, QC, QD, QE, QF, QG, QH, QI, QJ, QK, QL, QM, QN, QO, QP, QQ, QR, QS, QT, QU, QV, QW, QX, QY, QZ,
            RA, RB, RC, RD, RE, RF, RG, RH, RI, RJ, RK, RL, RM, RN, RO, RP, RQ, RR, RS, RT, RU, RV, RW, RX, RY, RZ,
            SA, SB, SC, SD, SE, SF, SG, SH, SI, SJ, SK, SL, SM, SN, SO, SP, SQ, SR, SS, ST, SU, SV, SW, SX, SY, SZ,
            TA, TB, TC, TD, TE, TF, TG, TH, TI, TJ, TK, TL, TM, TN, TO, TP, TQ, TR, TS, TT, TU, TV, TW, TX, TY, TZ,
            UA, UB, UC, UD, UE, UF, UG, UH, UI, UJ, UK, UL, UM, UN, UO, UP, UQ, UR, US, UT, UU, UV, UW, UX, UY, UZ,
            VA, VB, VC, VD, VE, VF, VG, VH, VI, VJ, VK, VL, VM, VN, VO, VP, VQ, VR, VS, VT, VU, VV, VW, VX, VY, VZ,
            WA, WB, WC, WD, WE, WF, WG, WH, WI, WJ, WK, WL, WM, WN, WO, WP, WQ, WR, WS, WT, WU, WV, WW, WX, WY, WZ,
            XA, XB, XC, XD, XE, XF, XG, XH, XI, XJ, XK, XL, XM, XN, XO, XP, XQ, XR, XS, XT, XU, XV, XW, XX, XY, XZ,
            YA, YB, YC, YD, YE, YF, YG, YH, YI, YJ, YK, YL, YM, YN, YO, YP, YQ, YR, YS, YT, YU, YV, YW, YX, YY, YZ,
            ZA, ZB, ZC, ZD, ZE, ZF, ZG, ZH, ZI, ZJ, ZK, ZL, ZM, ZN, ZO, ZP, ZQ, ZR, ZS, ZT, ZU, ZV, ZW, ZX, ZY, ZZ,
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
                                        WriteToLog("Error while reading cell, adding empty val instead. Err: " + ex.Message, LogLevel.Error);
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
            // If the file exists and the settings are not to overwrite, throw an exception
            if (File.Exists(filePath) && overWriteExists == false)
                throw new Exception("File " + filePath + " already exists.");

            try
            {
                // Write the MemoryStream to the specified file
                using (MemoryStream excelStream = GenerateExcelStream(excelData))
                    File.WriteAllBytes(filePath, excelStream.ToArray());
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Get excel file (as MemoryStream) from excelData
        /// </summary>
        /// <param name="excelData">excel data as dictionary of sheets. each sheet is list of lists creating 2d table</param>
        /// <returns>MemoryStream object holding excel file</returns>
        public static MemoryStream GenerateExcelStream(Dictionary<string, List<List<string>>> excelData)
        {
            using (var stream = new MemoryStream())
            {
                IWorkbook workbook = new XSSFWorkbook();

                foreach (var sheet in excelData)
                {
                    ISheet xssf_sheet = workbook.CreateSheet(sheet.Key);

                    for (int rowNum = 0; rowNum < sheet.Value.Count; rowNum++)
                    {
                        var row = xssf_sheet.CreateRow(rowNum);
                        for (int colNum = 0; colNum < sheet.Value[rowNum].Count; colNum++)
                        {
                            var cell = row.CreateCell(colNum);
                            cell.SetCellValue(sheet.Value[rowNum][colNum]);
                        }
                    }
                }

                workbook.Write(stream);

                // Return the MemoryStream
                return stream;
            }
        }
    }
}