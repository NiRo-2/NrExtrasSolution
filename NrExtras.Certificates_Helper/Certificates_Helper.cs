using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace NrExtras.Certificates_Helper
{
    public static class Certificates_Helper
    {
        #region local pfx certificate validator
        /// <summary>
        /// Validate pfx certificate for valid time
        /// </summary>
        /// <param name="pfxFilePath">pfx file path</param>
        /// <param name="pfxPassword">can leave empty if no pass is used</param>
        /// <param name="daysBeforeExpire">default is 14 days</param>
        /// <returns>true if about to expire, false otherwise</returns>
        /// <exception cref="Exception">on file not exists or unknown error</exception>
        public static bool IsAboutToExpire(string pfxFilePath, string? pfxPassword = null, int daysBeforeExpire = 14)
        {
            //validate file exists
            if (!File.Exists(pfxFilePath)) throw new Exception("Pfx file not found.");

            try
            {
                X509Certificate2 certificate = new X509Certificate2(pfxFilePath, pfxPassword); //load certificate
                DateTime expirationDate = certificate.NotAfter; //Get the expiration date of the certificate

                //Calculate the difference between the current date and the expiration date
                double daysLeft = (expirationDate - DateTime.Now).TotalDays;
                //return if we are in valid range
                return daysBeforeExpire >= daysLeft;
            }
            catch
            {
                throw;
            }
        }
        #endregion
        #region remote domain certificate validator
        /// <summary>
        /// Validate and return the amount of days a domain certificate have
        /// </summary>
        /// <param name="domain">domain to verify</param>
        /// <param name="httpsPort">default=443</param>
        /// <returns>How many days does certificate have</returns>        
        public static int get_ElapsedDays_ForRemoteDomainCertificate(string domain, int httpsPort = 443)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    client.Connect(domain, httpsPort);

                    try
                    {
                        // If we reached here, the connection is good - checking the certificate
                        using (var sslStream = new SslStream(client.GetStream(), false, ValidateServerCertificate))
                        {
                            sslStream.AuthenticateAsClient(domain);

                            // Retrieve the SSL certificate from the remote server
                            X509Certificate2 certificate = new X509Certificate2(sslStream.RemoteCertificate);

                            // Calculate the number of elapsed days until the certificate expires
                            int elapsedDays = (int)(certificate.NotAfter - DateTime.Now).TotalDays;

                            // Return the elapsed days
                            return elapsedDays;
                        }
                    }
                    catch
                    {
                        // Certificate error
                        throw new Exception("SSL certificate validation failed.");
                    }
                }
            }
            catch
            {
                // Connection error
                throw new Exception("Connection to the remote domain failed.");
            }
        }
        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // Check if there are any policy errors
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                // No policy errors, the certificate is trusted
                return true;
            }
            else
            {
                // Policy errors found, the certificate is not trusted
                return false;
            }
        }
        #endregion
    }
}