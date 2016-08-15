using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Horus_Config
{
    /// <summary>
    /// Interaction logic for GoogleDriveAuth.xaml
    /// </summary>
    public partial class EmailAPK : Window
    {
        public EmailAPK()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                if (e.Result.ToString() == "COMPLETED")
                    App.WriteMessage("Email Sent");
                else
                    App.WriteMessage("Error Sending Email");
            }
            else
            {
                App.WriteMessage("Error Sending Email");
            }

            this.Close();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Emailing.EmailNewUser(App.EMAIL_ADDRESS);

                e.Result = "COMPLETED";
            }
            catch (Exception)
            {

            }
        }
    }
}
