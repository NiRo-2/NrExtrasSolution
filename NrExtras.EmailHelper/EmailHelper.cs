using System.Net.Mail;

namespace NrExtras.EmailHelper
{
    public static class EmailHelper
    {
        /// <summary>
        /// Validate if email is valid
        /// </summary>
        /// <param name="email"></param>
        /// <returns>true if valid, false if invalid</returns>
        public static bool IsValidEmail(string email)
        {
            if(string.IsNullOrEmpty(email)) return false;

            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// send email
        /// </summary>
        /// <param name="fromEmail"></param>
        /// <param name="fromPassword"></param>
        /// <param name="mailServer"></param>
        /// <param name="mailServerPort"></param>
        /// <param name="toEmailAddress"></param>
        /// <param name="CC_emailAddress">should be null if not in use</param>
        /// <param name="filePathToSend">should be null if not in use</param>
        /// <param name="subject">email title</param>
        /// <param name="body">email body</param>
        /// <param name="isRtl">default=false, if true, body set to rtl</param>
        public static void sendEmail(string fromEmail, string fromPassword, string mailServer, int mailServerPort, List<string> toEmailAddress, List<string>? CC_emailAddress, List<string>? filePathToSend, string subject, string body, bool isRtl = false)
        {
            try
            {
                //creating message
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient(mailServer);
                mail.From = new MailAddress(fromEmail);
                //only if we have email to send
                if (toEmailAddress != null && toEmailAddress.Count > 0)
                    foreach (string email in toEmailAddress)
                        mail.To.Add(email);

                //only if we have CC
                if (CC_emailAddress != null && CC_emailAddress.Count > 0)
                    foreach (string email in CC_emailAddress)
                        mail.CC.Add(email);

                //if we have no one to send the email to, stop here
                if (mail.To.Count > 0 || mail.CC.Count > 0)
                {//all good, we have someone to send the email to
                    mail.IsBodyHtml = true; //set body html for rtl/ltr addition
                    mail.Subject = subject;
                    if (isRtl) //set rtl
                        body = "<div style='direction:rtl'>" + body + "</div>";

                    mail.Body = body;
                    //if we have files to send
                    if (filePathToSend != null)
                        foreach (string file in filePathToSend)
                            mail.Attachments.Add(new Attachment(file));

                    //send
                    SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
                    SmtpServer.UseDefaultCredentials = false;
                    SmtpServer.Port = mailServerPort;
                    SmtpServer.Credentials = new System.Net.NetworkCredential(fromEmail, fromPassword);
                    SmtpServer.EnableSsl = true;
                    SmtpServer.Send(mail);

                    //Clean up attachments
                    foreach (Attachment attach in mail.Attachments)
                        attach.Dispose();
                }
                else
                {//no recieptets
                    throw new Exception("Trying to send email without any receiepts");
                }
            }
            catch (Exception exception)
            {//error
             //incase we reach google size limit
                if (exception.Message.StartsWith("Exceeded storage allocation"))
                    throw new Exception("PDF file size limit reached");

                throw;
            }
        }
    }
}