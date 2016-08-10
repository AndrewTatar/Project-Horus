using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Horus_Email
{
    class Program
    {
        static bool mailSent = false;
        private static void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            // Get the unique identifier for this asynchronous operation.
            String token = (string)e.UserState;

            if (e.Cancelled)
            {
                Console.WriteLine("[{0}] Send canceled.", token);
            }
            if (e.Error != null)
            {
                Console.WriteLine("[{0}] {1}", token, e.Error.ToString());
            }
            else
            {
                Console.WriteLine("Message sent.");
            }
            mailSent = true;
        }

        //turn on below
        //https://www.google.com/settings/security/lesssecureapps

        public static void Main(string[] args)
        {
            // Command line argument must the the SMTP host.
            // dummy email account will be closed next acadmic unit.
            SmtpClient client = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                UseDefaultCredentials = false,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new System.Net.NetworkCredential("duane.mcmahon.au@gmail.com", "!Qosti932"),
                Timeout = 20000
            };
            // Specify the e-mail sender.    
            // in the display name.
            MailAddress from = new MailAddress("duane.mcmahon.au@gmail.com",
               "TeamHorus",
            System.Text.Encoding.UTF8);
            // Set destinations for the e-mail message.
            MailAddress to = new MailAddress(args[0]);
            // Specify the message content.
            MailMessage message = new MailMessage(from, to);
            message.Body = "Pleae find the Android client at the following url: https://drive.google.com/file/d/0B-hJtziIY0dlYk43WDdqY3NxZ1U/view?usp=sharing. ";
            message.BodyEncoding = System.Text.Encoding.UTF8;
            message.Subject = "A link from TeamHorus";
            message.SubjectEncoding = System.Text.Encoding.UTF8;
            // Set the method that is called back when the send operation ends.
            client.SendCompleted += new
            SendCompletedEventHandler(SendCompletedCallback);
            // The userState can be any object that allows your callback 
            // method to identify this send operation.
            // For this example, the userToken is a string constant.
            string userState = "test message1";
            client.SendAsync(message, userState);
            Console.WriteLine("Please check your email for the download url... press c to cancel sending this mail. Press any other key to exit.");
            string answer = Console.ReadLine();
            // If the user canceled the send, and mail hasn't been sent yet,
            // then cancel the pending operation.
            if (answer.StartsWith("c") && mailSent == false)
            {
                client.SendAsyncCancel();
            }
            // Clean up.
            message.Dispose();
            Console.WriteLine("Goodbye.");
        }
    }
}
