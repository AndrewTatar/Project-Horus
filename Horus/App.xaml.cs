using Horus.Classes;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public static bool CloseApplication = false;
        public static bool AllowGoogleDrive = false;

        //Facial Recognition
        public static readonly IFaceServiceClient faceServiceClient = new FaceServiceClient("00fd2d23618542208915def496e504ea");
        public static string faceGroupID = "";
        public static bool facialCheckDisabled = false;

        //Communications
        public static AbstractSMSService smsClient;

        //Events
        public static event EventHandler ShowCameraFeed;

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            bool displayScreensaver = false;

            if (e.Args.Length == 0 || e.Args[0].ToLower().StartsWith("/s"))
            {
                //Set Image Storage Path & Create Directory if required
                imageSavePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Captures");
                if (!Directory.Exists(imageSavePath))
                    Directory.CreateDirectory(imageSavePath);

                //Load Settings From Configuration File
                if (File.Exists("Settings.xml"))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load("Settings.xml");

                    bool SMSEnabled = false;
                    string SMSCarrier = "";
                    string SMSNumber = "";
                    string SMSMessage = "";

                    XmlNode node = doc.SelectSingleNode("Settings");
                    foreach (var ele in node)
                    {
                        if (ele.GetType() == typeof(XmlElement))
                        {
                            //We only want to collect data from XmlElements
                            XmlElement xele = (XmlElement)ele;
                            switch (xele.Name.ToLower())
                            {
                                case "faceid":
                                    faceGroupID = xele.InnerText;
                                    break;

                                case "sms":
                                    if (xele.HasChildNodes)
                                    {
                                        //Get SMS Details from Child Node
                                        foreach(var eSMS in xele.ChildNodes)
                                        {
                                            if (eSMS.GetType() == typeof(XmlElement))
                                            {
                                                //Again we only want XmlElements
                                                XmlElement xeSMS = (XmlElement)eSMS;

                                                switch (xeSMS.Name.ToLower())
                                                {
                                                    case "enabled":
                                                        SMSEnabled = Boolean.Parse(xeSMS.InnerText);
                                                        break;

                                                    case "carrier":
                                                        SMSCarrier = xeSMS.InnerText;
                                                        break;

                                                    case "mobilenumber":
                                                        SMSNumber = xeSMS.InnerText;
                                                        break;

                                                    case "message":
                                                        SMSMessage = xeSMS.InnerText;
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                    break;
                            }
                        }
                    }

                    //Configure Software for Settings
                    if (faceGroupID != "")
                    {
                        //Check Face Group
                        var result = await faceServiceClient.GetPersonGroupAsync(faceGroupID);
                        if (result.PersonGroupId == "")
                            facialCheckDisabled = true;
                    }
                    else
                    {
                        //No Face Group
                        //Facial Recognition will be disabled - this is now a general screensaver
                        facialCheckDisabled = true;
                    }
                    
                    //SMS Settings
                    if (SMSEnabled)
                    {
                        switch (SMSCarrier.ToLower())
                        {
                            case "telstra":
                                smsClient = new TelstraSMSService(SMSNumber);
                                break;

                            case "optus":
                                smsClient = new OptusSMSService(SMSNumber);
                                break;

                            case "Vodafone":
                                smsClient = new VodafoneSMSService(SMSNumber);
                                break;
                        }

                        if (smsClient != null && SMSMessage != "")
                            smsClient.setMessage(SMSMessage);

                        App.WriteMessage("SMS Service Configured");
                    }                   

                    App.WriteMessage("Settings File Loaded");
                }
                else
                {
                    //No settings file found
                    //Disable Facial Recognition
                    facialCheckDisabled = true;
                }

                //Google Drive Authorisation
                string storageDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Credentials");
                if (Directory.Exists(storageDirectory))
                {
                    if (Directory.EnumerateFiles(storageDirectory).Count() > 0)
                        AllowGoogleDrive = true;
                }

                if (AllowGoogleDrive)
                    GoogleAppAuthorisation.BuildCredentails(storageDirectory);

                //Get IP Address of System
                fetchIP();

                //Display the Screensaver
                displayScreensaver = true;
            }
            else if (e.Args[0].ToLower().StartsWith("/p"))
            {
                facialCheckDisabled = true;

                //No Preview Available
                
                //Close this application
                App.CloseApplication = true;
                Environment.Exit(0);
            }
            else if (e.Args[0].ToLower().StartsWith("/c"))
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Horus");

                if (key != null)
                {
                    string appPath = (string)key.GetValue("AppPath");

                    if (appPath != null)
                    {
                        //Start configuration Tool
                        Process process = new Process();
                        process.StartInfo.FileName = "horus-config.exe";
                        process.StartInfo.UseShellExecute = true;
                        process.StartInfo.WorkingDirectory = appPath;
                        process.Start();
                    }
                }
                
                //Close this application
                App.CloseApplication = true;
                Environment.Exit(0);
            }

            if (displayScreensaver)
            {
                //Load Screensaver Windows
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
            }
        }

        public static void RequestClose()
        {
            if (CloseApplication)
                App.Current.Shutdown();
        }

        public static async void fetchIP()
        {
            await
               Task.Run(
                   () =>
                   //This code is intended to run on a new thread, control is returned to the caller on the UI thread.
                   {
                       try
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
                       }
                       catch (Exception)
                       {

                       }
                   });
        }

        public static void WriteMessage(string message)
        {
            Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.fff") + " - " + message);
        }

        ///<summary></summary>
        public static async Task<List<Face>> UploadAndDetectFaces(string imagePath)
        {
            try
            {
                using (Stream imgFile = File.OpenRead(imagePath))
                {
                    var faces = await faceServiceClient.DetectAsync(imgFile, true, true);

                    return faces.ToList();
                }
            }
            catch (Exception)
            {
                return new List<Face>();
            }
        }

        public static async Task<string> VerifyPerson(Guid[] personID)
        {
            try
            {
                string username = "";
                if (faceGroupID != "")
                {                   
                    var results = await faceServiceClient.IdentifyAsync(faceGroupID, personID, 1);
                    foreach (var identifyResult in results)
                    {
                        if (identifyResult.Candidates.Length == 0)
                        {
                            //Not Verified
                            App.WriteMessage("User Not Verified");
                        }
                        else
                        {
                            //Get Verified Person Details
                            var candidate = identifyResult.Candidates[0];
                            if (candidate.Confidence > 0.70)
                            {
                                var candidateId = identifyResult.Candidates[0].PersonId;
                                var person = await faceServiceClient.GetPersonAsync(faceGroupID, candidateId);
                                App.WriteMessage("User Verfied as " + person.Name + " (Confidence: " + candidate.Confidence + ")");
                                username = person.Name;
                            }
                            else
                            {
                                username = "N/C";
                                App.WriteMessage("User Verification Failed - Confidence: " + candidate.Confidence);
                            }                            
                        }
                    }
                }
                else
                {
                    //Identification Not Possible - Unknown Owner
                    username = "User";
                }
                
                return username;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static void ShowCamera()
        {
            try
            {
                EventHandler handler = ShowCameraFeed;
                if (handler != null)
                    handler(null, EventArgs.Empty);
            }
            catch (Exception)
            {

            }
        }
    }
}
