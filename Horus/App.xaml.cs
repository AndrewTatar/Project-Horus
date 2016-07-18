using Accord.Vision.Detection;
using AForge.Video.DirectShow;
using Horus.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Xml;

namespace Horus
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        //Global Variables
        public static string imageSavePath = "";
        public static string currentIP = "";

        public static AbstractSMSService smsClient;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length == 0 || e.Args[0].ToLower().StartsWith("/s"))
            {
                foreach (Screen s in Screen.AllScreens)
                {
                    if (s != Screen.PrimaryScreen)
                    {
                        Blank window = new Blank();
                        window.Left = s.WorkingArea.Left;
                        window.Top = s.WorkingArea.Top;
                        window.Width = s.WorkingArea.Width;
                        window.Height = s.WorkingArea.Height;
                        window.Show();
                    }
                    else
                    {
                        MainWindow window = new MainWindow();
                        window.Left = s.WorkingArea.Left;
                        window.Top = s.WorkingArea.Top;
                        window.Width = s.WorkingArea.Width;
                        window.Height = s.WorkingArea.Height;
                        window.Show();
                    }
                }

                //Set Image Storage Path & Create Directory if required
                imageSavePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Captures");
                if (!Directory.Exists(imageSavePath))
                    Directory.CreateDirectory(imageSavePath);

                //Load Settings From Configuration File
                if (File.Exists("Settings.xml"))
                {

                }

                //Get IP Address of System
                fetchIP();

                //Connect to Webcam
            }
            else if (e.Args[0].ToLower().StartsWith("/p"))
            {

            }
            else
            {

            }
        }

        public static void RequestClose()
        {
            App.Current.Shutdown();
        }

        public static async void fetchIP()
        {
            await
               Task.Run(
                   () =>
                   //This code is intended to run on a new thread, control is returned to the caller on the UI thread.
                   {
                       String direction = "";
                       WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
                       using (WebResponse response = request.GetResponse())
                       using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                       {
                           direction = stream.ReadToEnd();
                       }

                       //Search for the ip in the html
                       int first = direction.IndexOf("Address: ") + 9;
                       int last = direction.LastIndexOf("</body>");
                       direction = direction.Substring(first, last - first);
                       currentIP = direction;
                   });
        }

        public static void setSMSService(string carrierName)
        {
            switch (carrierName)
            {
                case "Telstra":
                    App.smsClient = new TelstraSMSService();
                    break;
                case "Optus":
                    App.smsClient = new OptusSMSService();
                    break;
                case "Vodafone":
                    App.smsClient = new VodafoneSMSService();
                    break;
            }
        }

        public static void setMobileNumber(string phoneNum)
        {
            smsClient.setMobileNumber(phoneNum);
        }

        public static void setUserName(string userName)
        {
            smsClient.setUserName(userName);
        }
    }
}
