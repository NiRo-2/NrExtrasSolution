using Google.Apis.Auth.OAuth2;
using Google.Apis.Requests;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NrExtras.Google
{
    public class GoogleSheet_Helper
    {
        #region RealTimeDelay - helper funcs for waiting while reading sheets
        public static Task RealTimeDelay(TimeSpan delay) => RealTimeDelay(delay, TimeSpan.FromMilliseconds(100));
        public static async Task RealTimeDelay(TimeSpan delay, TimeSpan precision)
        {
            DateTime start = DateTime.UtcNow;
            DateTime end = start + delay;

            while (DateTime.UtcNow < end)
                await Task.Delay(precision);
        }
        #endregion
        public const string MimeType_GoogleSheet = "application/vnd.google-apps.spreadsheet";
        const int onFail_MaximumRetries = 60;
        const int exponentialBackoffAlgorithm_MilSec_Min = 1000;
        const int exponentialBackoffAlgorithm_MilSec_Max = 2000;
        int[] RetryErrorCode = { 408, 429, 500, 502, 503, 504 };

        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/sheets.googleapis.com-dotnet-quickstart.json
        static string[] Default_Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        SheetsService service;

        /// <summary>
        /// init google sheet api
        /// </summary>
        /// <param name="ApplicationName">application name as set in Google AUTH settings</param>
        /// <param name="Scopes">can leave empty or null, if so, using read only - recommended</param>
        public GoogleSheet_Helper(string ApplicationName, string[]? Scopes = null)
        {
            try
            {
                //setting default scopes if non is given
                if (Scopes == null)
                    Scopes = Default_Scopes;

                UserCredential credential;

                using (var stream =
                    new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
                {
                    // The file token.json stores the user's access and refresh tokens, and is created
                    // automatically when the authorization flow completes for the first time.
                    string credPath = "token.json";
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true)).Result;
                    //Logger.Logger.WriteToLog("Credential file saved to: " + credPath);
                }

                // Create Google Sheets API service.
                service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// parse exception to google error
        /// </summary>
        /// <param name="exception">the desired exception to parse</param>
        /// <returns></returns>
        public static RequestError GetGoogleError_From_Exception(Exception exception)
        {
            try
            {
                RequestError requestError = new RequestError();
                string s = JsonConvert.SerializeObject(exception);
                dynamic d = JObject.Parse(s);
                //parsing
                requestError.Message = d.Message;
                requestError.Code = d.HttpStatusCode;

                return requestError;
            }
            catch
            {//error parsing
                throw;
            }
        }

        /// <summary>
        /// Read google sheet to list of list of string - single sheet
        /// </summary>
        /// <param name="spreadsheetId">sheet id</param>
        /// <param name="sheetName">sheet name</param>
        /// <param name="readEmtyRows">false by default</param>
        /// <param name="onFail_RetryCount">default 0. incase of -1, no retries. no need to change this, it will try by itself until reach ths limit</param>
        /// <returns>list of list of string of entire sheet</returns>
        public List<List<string>> ReadGoogleSheet_SingleSheet_ToListOfList(string spreadsheetId, string sheetName, bool readEmtyRows = false, int onFail_RetryCount = 0)
        {
            //check if we use retry counter
            if (onFail_RetryCount == -1)
                onFail_RetryCount = onFail_MaximumRetries;

            try
            {
                // Define request parameters.
                string range = sheetName;
                SpreadsheetsResource.ValuesResource.GetRequest request;
                ValueRange response;

                try
                {
                    //get values
                    request = service.Spreadsheets.Values.Get(spreadsheetId, range);
                    response = request.Execute();
                }
                catch (Exception ex)
                {//handling exception
                    try
                    {
                        //parsing to google api error
                        RequestError requestError = GetGoogleError_From_Exception(ex);

                        //make sure the exception is 408, 429, and 5xx
                        if (RetryErrorCode.Contains(requestError.Code))
                        {
                            //if we have more retries - try it. else, throw the exception
                            if (onFail_RetryCount < onFail_MaximumRetries)
                            {
                                //add random milliseconds to retry
                                Random r = new Random();
                                int milliseoncdsWait = (onFail_RetryCount * 1000) + r.Next(exponentialBackoffAlgorithm_MilSec_Min, exponentialBackoffAlgorithm_MilSec_Max);
                                Console.WriteLine("GoogleApi failed - try " + onFail_RetryCount + " out of " + onFail_MaximumRetries + ". waiting " + milliseoncdsWait + " milliseconds before next try");

                                //wait
                                RealTimeDelay(TimeSpan.FromMilliseconds(milliseoncdsWait)).Wait();

                                //continue after pause
                                return ReadGoogleSheet_SingleSheet_ToListOfList(spreadsheetId, sheetName, readEmtyRows, onFail_RetryCount + 1);
                            }
                            else
                            {//end of tries
                                throw new Exception("Retry limit reached(" + onFail_MaximumRetries + " tries + waiting time between) - Error: " + ex);
                            }
                        }
                        else //not our error
                            throw;
                    }
                    catch
                    {//error parsing google api error
                        throw;
                    }
                }

                IList<IList<object>> values = response.Values;

                //getting table size
                int rowCount = values.Count;
                int colCount = values[0].Count;

                List<List<string>> sheetArr = new List<List<string>>();

                //parsing to array
                if (values != null && values.Count > 0)
                    for (int row = 0; row < rowCount; row++)
                    {
                        List<string> r = new List<string>();
                        foreach (string value in values[row])
                            r.Add(value);

                        //if we need to remove empty lines
                        if (!readEmtyRows)
                        {//check columns count - if greater then 0. add
                            if (r.Count != 0)
                                sheetArr.Add(r);
                        }
                        else //no need to check columns count
                            sheetArr.Add(r);
                    }

                //return results
                return sheetArr;
            }
            catch
            {
                throw;
            }
        }
        /// <summary>
        /// Read google sheet to list of list of string - read all sheets
        /// </summary>
        /// <param name="spreadsheetId">sheet id</param>
        /// <param name="readEmptyRows">false by default</param>
        /// <returns>Dictionary of sheet name and sheet data (wors,cols)</returns>
        public Dictionary<string, List<List<string>>> ReadGoogleSheet_ToListOfList(string spreadsheetId, bool readEmptyRows = false)
        {
            try
            {
                Dictionary<string, List<List<string>>> spreadSheet = new Dictionary<string, List<List<string>>>();

                // True if grid data should be returned.
                // This parameter is ignored if a field mask was set in the request.
                bool includeGridData = false;  // TODO: Update placeholder value.

                SpreadsheetsResource.GetRequest request = service.Spreadsheets.Get(spreadsheetId);
                request.Ranges = new List<string>(); ;
                request.IncludeGridData = includeGridData;

                // To execute asynchronously in an async method, replace `request.Execute()` as shown:
                Spreadsheet response = request.Execute();
                // Data.Spreadsheet response = await request.ExecuteAsync();
                IList<Sheet> sheets_ranges = response.Sheets;

                //get all sheets
                foreach (Sheet gsheet in sheets_ranges)
                {
                    string sheetName = gsheet.Properties.Title;
                    int? sheetId = gsheet.Properties.SheetId;
                    List<List<string>> sheetArrey = ReadGoogleSheet_SingleSheet_ToListOfList(spreadsheetId, sheetName, readEmptyRows);
                    spreadSheet.Add(sheetName, sheetArrey);
                }

                //return results
                return spreadSheet;
            }
            catch
            {
                throw;
            }
        }
    }
}