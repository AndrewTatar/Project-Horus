using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
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

namespace Horus_Config
{
    /// <summary>
    /// Interaction logic for Saving.xaml
    /// </summary>
    public partial class Saving : Window
    {
        public string faceGroupID;

        public Saving()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TrainPersonGroup();
        }

        private async void TrainPersonGroup()
        {
            await MainWindow.faceServiceClient.TrainPersonGroupAsync(faceGroupID);

            TrainingStatus trainingStatus = null;
            while(true)
            {
                trainingStatus = await MainWindow.faceServiceClient.GetPersonGroupTrainingStatusAsync(faceGroupID);

                if (trainingStatus.Status != Status.Running)
                    break;

                await Task.Delay(1000);
            }

            this.Close();
        }
    }
}
