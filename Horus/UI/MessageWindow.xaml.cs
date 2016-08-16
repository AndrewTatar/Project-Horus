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
using System.Windows.Threading;

namespace Horus
{
    /// <summary>
    /// Interaction logic for MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : Window
    {
        public int state = 0;
        public string username = "";
        DispatcherTimer timer;

        public MessageWindow()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (state == 0)
            {
                this.border.BorderBrush = new SolidColorBrush(Colors.Red);
                lMessage.Content = "ACCESS DENIED!";
            }
            else if (state == 1)
            {
                this.border.BorderBrush = new SolidColorBrush(Colors.LightGreen);
                lMessage.Content = "Hello " +  username + "!";
            }
            else if (state == 2)
            {
                this.border.BorderBrush = new SolidColorBrush(Colors.Orange);
                lMessage.Content = "Please Try Again!";
            }

            timer.Start();
        }
    }
}
