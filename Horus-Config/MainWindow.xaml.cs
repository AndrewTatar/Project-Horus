using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;

namespace Horus_Config
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly IFaceServiceClient faceServiceClient = new FaceServiceClient("00fd2d23618542208915def496e504ea");

        //Setting Variables
        bool SMSEnabled = false;
        string SMSCarrier = "";
        string SMSNumber = "";
        string SMSMessage = "";
        string faceGroupID = "";
        List<Person> allowedPeople = new List<Person>();

        private bool noMessage = false;

        public static bool FaceChangesMade = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Hide();

            //Check if Initial Install
            if (App.INSTALLATION)
            {
                //Initial Application Install Configuration
                //Email User Link to Android APK
                if (App.EMAIL_ADDRESS != "")
                {
                    EmailAPK email = new EmailAPK { Owner = this };
                    email.ShowDialog();
                }
            }

            //Load Settings from File
            LoadSettingsFile();

            //Google Drive Authorisation
            CheckGoogleDriveAccess();

            this.Show();
        }

        private bool CheckGoogleDriveAccess()
        {
            try
            {
                //Google Drive Authorisation
                string storageDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Credentials");
                bool RequireAuthorisation = false;

                if (!Directory.Exists(storageDirectory))
                    RequireAuthorisation = true;
                else
                    if (Directory.EnumerateFiles(storageDirectory).Count() == 0)
                    RequireAuthorisation = true;

                if (RequireAuthorisation)
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(@"/Horus-Config;component/cross-icon.png", UriKind.RelativeOrAbsolute);
                    bmp.EndInit();

                    gIcon.Source = bmp;

                    gLabel.Content = "Not Allowed";
                    return false;
                }
                else
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(@"/Horus-Config;component/check-icon.png", UriKind.RelativeOrAbsolute);
                    bmp.EndInit();

                    gIcon.Source = bmp;

                    gLabel.Content = "Allowed";

                    return true;
                }
            }
            catch (Exception)
            {

            }
            return false;
        }

        private async void LoadSettingsFile()
        {
            //Show Loading Window
            Loading l = new Loading { Owner = this };
            l.Show();

            //Load Settings from File
            if (File.Exists("Settings.xml"))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load("Settings.xml");

                XmlNode node = doc.SelectSingleNode("Settings");
                foreach (var ele in node)
                {
                    if (ele.GetType() == typeof(XmlElement))
                    {
                        //We only want to collect data from XmlElements
                        XmlElement xele = (XmlElement) ele;
                        switch (xele.Name.ToLower())
                        {
                            case "faceid":
                                faceGroupID = xele.InnerText;
                                break;

                            case "sms":
                                if (xele.HasChildNodes)
                                {
                                    //Get SMS Details from Child Node
                                    foreach (var eSMS in xele.ChildNodes)
                                    {
                                        if (eSMS.GetType() == typeof(XmlElement))
                                        {
                                            //Again we only want XmlElements
                                            XmlElement xeSMS = (XmlElement) eSMS;

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
                    if (result != null)
                    {
                        //Valid Group
                        LoadAllowedUsers();
                    }
                    else
                    {
                        faceGroupID = "";
                    }
                }

                chMobileEnabled.IsChecked = SMSEnabled;
                eMobileNumber.Text = SMSNumber;
                eMessage.Text = SMSMessage;

                //Select correct service provider in combobox
                foreach (ComboBoxItem item in eServiceProvider.Items)
                {
                    if (item.Content.ToString() == SMSCarrier)
                        item.IsSelected = true;
                }

                if (faceGroupID == "")
                {
                    //Generate new FaceGroupID
                    GenerateFaceGroupID();

                    //Save to file with default values
                    noMessage = true;
                    bSave_Click(bSave, new RoutedEventArgs());
                }

                App.WriteMessage("Settings File Loaded");
            }
            else
            {
                //Generate new FaceGroupID
                GenerateFaceGroupID();

                //Save to file with default values
                noMessage = true;
                bSave_Click(bSave, new RoutedEventArgs());
            }

            l.loaded = true;
        }

        private async void GenerateFaceGroupID()
        {
            try
            {
                //Generate New Owner Face Group
                string groupID = Guid.NewGuid().ToString();
                await faceServiceClient.CreatePersonGroupAsync(groupID, "HorusSecurity");

                faceGroupID = groupID;
            }
            catch (Exception)
            {

            }
        }

        private async void LoadAllowedUsers()
        {
            allowedPeople = new List<Person>();

            if (faceGroupID != "")
            {
                var allowed = await faceServiceClient.GetPersonsAsync(faceGroupID);
                foreach(var p in allowed)
                {
                    allowedPeople.Add(p);
                }                   
            }

            listBox.Items.Clear();

            foreach (Person p in allowedPeople)
            {
                listBox.Items.Add(new ListBoxItem { Content = p.Name, Tag = p });
            }
        }

        private void bClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void bSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Save settings to file
                XmlWriterSettings settings = new XmlWriterSettings { Indent = true };

                using (XmlWriter w = XmlWriter.Create("Settings.xml", settings))
                {
                    w.WriteStartDocument();
                    w.WriteStartElement("Settings");

                    //Face Settings
                    w.WriteElementString("FaceID", faceGroupID);
  
                    //SMS Settings
                    w.WriteStartElement("SMS");
                    w.WriteElementString("Enabled", chMobileEnabled.IsChecked.ToString());
                    w.WriteElementString("Carrier", ((ComboBoxItem)eServiceProvider.SelectedItem).Content.ToString());
                    w.WriteElementString("MobileNumber", eMobileNumber.Text);
                    w.WriteElementString("Message", eMessage.Text);
                    w.WriteEndElement();

                    w.WriteEndElement();
                    w.WriteEndDocument();
                }

                if (FaceChangesMade)
                {
                    //Retrain Faces
                    Saving s = new Saving { faceGroupID = faceGroupID, Owner = this };
                    s.ShowDialog();
                }                

                if (!noMessage)
                    MessageBox.Show("Settings Saved!", "HORUS Configuration");
            }
            catch (Exception)
            {
               
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //TODO: Check for changes made and prompt user to save before exiting
            bool changesFound = false;

            if (changesFound)
            {
                //Save Changes
                bSave_Click(bSave, new RoutedEventArgs());
            }            
        }

        private void bManageAccess_Click(object sender, RoutedEventArgs e)
        {
            if (listBox.SelectedItem != null)
            {
                Person person = (listBox.SelectedItem as ListBoxItem).Tag as Person;

                UserWizard uw = new UserWizard { person = person, faceGroupID = faceGroupID, Owner = this };
                var response = uw.ShowDialog();
                if (response != null)
                {
                    if (response == true)
                    {
                        //Refresh User List
                        LoadAllowedUsers();

                        if (FaceChangesMade)
                            bSave_Click(bSave, new RoutedEventArgs());
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a user first!");
            }
        }

        private void bAddUser_Click(object sender, RoutedEventArgs e)
        {
            UserWizard uw = new UserWizard { faceGroupID = faceGroupID, Owner = this };
            var response = uw.ShowDialog();
            if (response != null)
            {
                if (response == true)
                {
                    //Refresh User List
                    LoadAllowedUsers();

                    if (FaceChangesMade)
                        bSave_Click(bSave, new RoutedEventArgs());
                }
            }
        }

        private void bGoogleDrive_Click(object sender, RoutedEventArgs e)
        {
            if (gLabel.Content.ToString() != "Allowed")
            {
                //Show Dialog for Log into your Google Account
                GoogleDriveAuth gd = new GoogleDriveAuth { Owner = this };
                gd.Show();

                //Start monitoring timer
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += Timer_Tick;
                timer.Start();

                //Start the Authorisation Request
                string storageDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Credentials");

                GoogleAppAuthorisation.BuildCredentails(storageDirectory);
                GoogleAppAuthorisation.AuthoriseUser();
            }            
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (CheckGoogleDriveAccess())
            {
                var timer = sender as DispatcherTimer;
                timer.Stop();
            }
        }

        private void bAndroidAPK_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(App.ANDROID_APK));
            e.Handled = true;
        }
    }
}
