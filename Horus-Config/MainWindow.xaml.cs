using Horus.Classes;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
        string firstName = "";
        string lastName = "";
        bool SMSEnabled = false;
        string SMSCarrier = "";
        string SMSNumber = "";
        string SMSMessage = "";
        string faceGroupID = "";
        List<Person> allowedPeople = new List<Person>();

        public static bool FaceChangesMade = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettingsFile();
        }

        private async void LoadSettingsFile()
        {
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
                
                eFirstName.Text = firstName;
                eLastName.Text = lastName;
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

                    bSave_Click(bSave, new RoutedEventArgs());
                }

                App.WriteMessage("Settings File Loaded");
            }
            else
            {
                //Generate new FaceGroupID
                GenerateFaceGroupID();

                //Save to file with default values
                bSave_Click(bSave, new RoutedEventArgs());

                //TODO: Move this to the correct area. Remove reference to Horus main application
                //if there is no setting file then shareout the link to the mobile client APK.

                //HACK: Disabled
                //GoogleAppAuthorisation.addPermission();
            }
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

                    w.WriteElementString("FirstName", eFirstName.Text);
                    w.WriteElementString("LastName", eLastName.Text);

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
                    Saving s = new Saving { faceGroupID = faceGroupID };
                    s.ShowDialog();
                }                

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

                Users u = new Users { state = 1, person = person, faceGroupID = faceGroupID };
                u.ShowDialog();

                //Refresh User List
                LoadAllowedUsers();
            }
            else
            {
                MessageBox.Show("Please select a user first!");
            }
        }

        private void bAddUser_Click(object sender, RoutedEventArgs e)
        {
            Users u = new Users { state = 0, faceGroupID = faceGroupID };
            u.ShowDialog();

            //Refresh User List
            LoadAllowedUsers();
        }
    }
}
