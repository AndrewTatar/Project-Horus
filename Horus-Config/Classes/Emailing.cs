using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Horus_Config
{
    public class Emailing
    {
        //TEMPORARY EMAIL ACCOUNT
        private const string EMAIL_SERVER = "smtp.gmail.com";
        private const string EMAIL_USERNAME = "horusdesktopsec@gmail.com";
        private const string EMAIL_PASSWORD = "H0RU5S321!";
        private const int EMAIL_PORT = 587;

        public static bool EmailNewUser(string email)
        {
            try
            {
                //Configure SMTP
                SmtpClient client = new SmtpClient(EMAIL_SERVER, EMAIL_PORT);
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(EMAIL_USERNAME, EMAIL_PASSWORD);

                //Build Email Components
                MailAddress from = new MailAddress(EMAIL_USERNAME, "HORUS Desktop Security");
                MailAddress to = new MailAddress(email);

                string body = "";
                body += "Hello New User! <br/><br/>";
                body += "Download our Android App from the link below so you can have quick and easy access to review any unauthorised access attempts at your pc. <br/> <br/>";
                body += "<a href='" + App.ANDROID_APK + "'>Download Link</a> <br/> <br/>";
                body += "Best Regards <br/>";
                body += "HORUS Desktop Security Team";

                //Build Message
                MailMessage message = new MailMessage(from, to);
                message.Subject = "HORUS Security - Android App";
                message.Body = body;
                message.IsBodyHtml = true;

                //Send Email
                client.Send(message);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return false;
        }
    }
}
