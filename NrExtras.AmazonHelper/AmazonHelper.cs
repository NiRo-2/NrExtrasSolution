using Amazon.S3;
using Amazon.S3.Model;
using System.Web;
using static NrExtras.Logger.Logger;

namespace NrExtras.AmazonHelper
{
    public class AmazonHelper
    {
        private IAmazonS3 _s3Client;
        private string BUCKET_NAME;
        private string AWSAccessKey;
        private string AWSSecretKey;

        //constructor for this helper
        public AmazonHelper(string bUCKET_NAME, string aWSAccessKey, string aWSSecretKey, string AWSRegion)
        {
            BUCKET_NAME = bUCKET_NAME;
            AWSAccessKey = aWSAccessKey;
            AWSSecretKey = aWSSecretKey;
            _s3Client = GetS3Client(AWSRegion);
        }

        //get S3 client
        private AmazonS3Client GetS3Client(string AWSRegion)
        {
            var credentials = new Amazon.Runtime.BasicAWSCredentials(AWSAccessKey, AWSSecretKey);
            return new AmazonS3Client(credentials, Amazon.RegionEndpoint.GetBySystemName(AWSRegion));

            //AmazonS3Client s3Client = new AmazonS3Client(AWSAccessKey,AWSSecretKey);
            //return s3Client;
        }

        /// <summary>
        /// create download url for duration_hours
        /// </summary>
        /// <param name="duration_hours">if set to 0, return empty url</param>
        /// <param name="fileName"></param>
        /// <returns>download link</returns>
        private string GeneratePreSignedURL_Get(double duration_hours, string fileName)
        {
            //incase we have no duration defined
            if (duration_hours == 0)
                return "";

            //IAmazonS3 s3Client = new AmazonS3Client(bucketRegion);
            //IAmazonS3 s3Client = GetS3Client();
            var request = new GetPreSignedUrlRequest
            {
                BucketName = BUCKET_NAME,
                Key = fileName,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddHours(duration_hours)
            };

            string url = _s3Client.GetPreSignedURL(request);
            return url;
        }
        /// <summary>
        /// upload file to s3 - if set duration is not defind, download string return as null
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="uploadDir"></param>
        /// <param name="duration_hours">download link expire after "duration_hours". if 0, there will be no url to return</param>
        /// <param name="convertToUri">default=true - convert to legal uri filename. when false, bug may accure when mixing bidirection text of hebrew, enlgish and numbers</param>
        /// <param name="fixEscapeChars">default=true - convert all escape chars to "_"</param>
        /// <returns>url for downloading the file</returns>
        public string UploadFile(string filePath, string fileName, string uploadDir = "", int duration_hours = 0, bool convertToUri = true, bool fixEscapeChars = true)
        {
            try
            {
                //if we have dir - add "/" at the end of the dir
                if (!string.IsNullOrEmpty(uploadDir))
                    uploadDir = uploadDir + "/";

                //get s3 client
                //_s3Client = GetS3Client();

                //convert to uri
                if (convertToUri)
                    fileName = HttpUtility.UrlPathEncode(fileName);

                //fix escape chars
                if (fixEscapeChars)
                    fileName = StringsHelper.StringsHelper.FixEscapeCharacterSequence(fileName);

                //auto file content type
                string contentType = "application/pdf";

                //start task
                var task = Task.Run(async () => await UploadObjectFromFileAsync(_s3Client, BUCKET_NAME, uploadDir + fileName, filePath, contentType));
                task.Wait();

                //get url
                return GeneratePreSignedURL_Get(duration_hours, uploadDir + fileName);
            }
            catch (Exception e)
            {//error
                Logger.Logger.WriteToLog("Error uploading file to S3 - Err: " + e.Message, LogLevel.Error);
                throw;
            }
        }

        /// <summary>
        /// upload file to S3, return Task to use async/
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="contentType"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task UploadFileAsync(string filePath, string contentType, string? fileName = null)
        {
            try
            {
                //get file name if not provided
                if (fileName == null)
                    fileName = Path.GetFileName(filePath);

                //_s3Client = GetS3Client();
                await UploadObjectFromFileAsync(_s3Client, BUCKET_NAME, fileName, contentType, filePath);
            }
            catch (Exception e)
            {//error
                Logger.Logger.WriteToLog("Error uploading file to S3 - Err: " + e.Message, LogLevel.Error);
                throw;
            }
        }

        /// <summary>
        /// This method uploads a file to an Amazon S3 bucket. This
        /// example method also adds metadata for the uploaded file.
        /// </summary>
        /// <param name="client">An initialized Amazon S3 client object.</param>
        /// <param name="bucketName">The name of the S3 bucket to upload the
        /// file to.</param>
        /// <param name="objectName">The destination file name.</param>
        /// <param name="filePath">The full path, including file name, to the
        /// file to upload. This doesn't necessarily have to be the same as the
        /// name of the destination file.</param>
        private async Task UploadObjectFromFileAsync(
            IAmazonS3 client,
            string bucketName,
            string objectName,
            string filePath,
            string contentType)
        {
            try
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = objectName,
                    FilePath = filePath,
                    ContentType = contentType
                };

                PutObjectResponse response = await client.PutObjectAsync(putRequest);
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
                throw e;
            }
        }

        /// <summary>
        /// This method creates a file in an Amazon S3 bucket that contains the text
        /// passed to the method.
        /// </summary>
        /// <param name="client">An initialized S3 client object.</param>
        /// <param name="bucketName">The name of the bucket where the file will
        /// be created.</param>
        /// <param name="objectName">The name of the file that will be created.</param>
        /// <param name="content">A string containing the content to put in the
        /// file on the destination S3 bucket.</param>
        private async Task UploadObjectFromContentAsync(IAmazonS3 client,
            string bucketName,
            string objectName,
            string content)
        {
            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = objectName,
                ContentBody = content
            };

            PutObjectResponse response = await client.PutObjectAsync(putRequest);
        }
    }
}