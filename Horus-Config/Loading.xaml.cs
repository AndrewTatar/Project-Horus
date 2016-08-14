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
    public partial class Loading : Window
    {
        public bool loaded = false;

        public Loading()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WaitForLoading();
        }

        private async void WaitForLoading()
        {
            while(!loaded)
            {
                await Task.Delay(1000);
            }

            this.Close();
        }
    }
}
