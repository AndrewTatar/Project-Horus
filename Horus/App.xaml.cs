using Accord.Vision.Detection;
using AForge.Video.DirectShow;
using Horus.Classes;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
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
        
        //Facial Recognition
        public static readonly IFaceServiceClient faceServiceClient = new FaceServiceClient("00fd2d23618542208915def496e504ea");
        public static string faceGroupID = "";

        //Communications
        public static AbstractSMSService smsClient;

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
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

                    string firstName = "";
                    string lastName = "";
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
                                case "firstname":
                                    firstName = xele.InnerText;
                                    break;

                                case "lastname":
                                    lastName = xele.InnerText;
                                    break;

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
                    if (faceGroupID == "")
                    {
                        //Create New Owner Face Group
                        string groupID = Guid.NewGuid().ToString();
                        await faceServiceClient.CreatePersonGroupAsync(groupID, firstName + lastName);
                    }
                    else
                    {
                        //Check Face Group
                        var result = await faceServiceClient.GetPersonGroupAsync(faceGroupID);
                    }

                    //SMS Settings
                    if (SMSEnabled)
                    {
                        switch (SMSCarrier.ToLower())
                        {
                            case "telstra":
                                smsClient = new TelstraSMSService(firstName + " " + lastName, SMSNumber);
                                break;

                            case "optus":
                                smsClient = new OptusSMSService(firstName + " " + lastName, SMSNumber);
                                break;

                            case "Vodafone":
                                smsClient = new VodafoneSMSService(firstName + " " + lastName, SMSNumber);
                                break;
                        }

                        if (smsClient != null && SMSMessage != "")
                            smsClient.setMessage(SMSMessage);

                        App.WriteMessage("SMS Service Configured");
                    }                   

                    App.WriteMessage("Settings File Loaded");
                }

                //Get IP Address of System
                fetchIP();

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

        public static async Task<Boolean> VerifyPerson(Guid[] personID)
        {
            try
            {
                bool identified = false;

                var results = await faceServiceClient.IdentifyAsync(faceGroupID, personID, 1);
                foreach (var identifyResult in results)
                {
                    if (identifyResult.Candidates.Length == 0)
                    {
                        //Not Verified
                        identified = false;
                        Console.WriteLine("Not Identified!");
                    }
                    else
                    {
                        identified = true;
                        //Get Verified Person Details
                        var candidateId = identifyResult.Candidates[0].PersonId;
                        var person = await faceServiceClient.GetPersonAsync(faceGroupID, candidateId);
                        Console.WriteLine("Identified as {0}", person.Name);
                    }
                }
                return identified;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
