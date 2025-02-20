using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace NrExtras.Google
{
    public class GoogleDrive_Helper
    {
        static string[] Default_Scopes = { DriveService.Scope.DriveReadonly };
        DriveService service;

        //drive file definition
        public class DriveFile
        {
            public string Name;
            public string MimeType;
            public string Id;

            public DriveFile(string name, string mimeType, string id)
            {
                Name = name;
                MimeType = mimeType;
                Id = id;
            }

            //toStrings
            public override string ToString()
            {
                return "Name: " + Name + " Id: " + Id + " MimeType: " + MimeType;
            }
        }

        /// <summary>
        /// init google drive api
        /// </summary>
        /// <param name="ApplicationName">application name as set in Google AUTH settings</param>
        /// <param name="Scopes">can leave empty or null, if so, using read only - recommended</param>
        public GoogleDrive_Helper(string ApplicationName, string[]? Scopes = null)
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
                    string credPath = "token_Drive.json";
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true)).Result;
                    //Logger.Logger.WriteToLog("Credential file saved to: " + credPath);
                }

                // Create Google Sheets API service.
                service = new DriveService(new BaseClientService.Initializer()
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
        /// get list of files from google drive
        /// </summary>
        /// <param name="dirId">root dirId. if null, root of the drive selected</param>
        /// <param name="mimeType">mime type of searched files</param>
        /// <returns>list of found files</returns>
        public List<DriveFile> ListAllFilesInDir(string dirId = "", string mimeType = "")
        {
            try
            {
                var request = service.Files.List();
                request.PageSize = 1000;

                //incase we have dir - search in it
                if (!string.IsNullOrEmpty(dirId))
                    request.Q = "parents in '" + dirId + "'";

                //search by mimeType
                if (!string.IsNullOrEmpty(mimeType))
                    request.Q = request.Q + " and mimeType = '" + mimeType + "'";

                var results = request.Execute();
                List<DriveFile> files = new List<DriveFile>();

                //going through all files
                foreach (var driveFile in results.Files)
                    files.Add(new DriveFile(driveFile.Name, driveFile.MimeType, driveFile.Id));

                return files;
            }
            catch
            {
                throw;
            }
        }
    }
}