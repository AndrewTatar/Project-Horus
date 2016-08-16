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

namespace Horus
{
    /// <summary>
    /// Interaction logic for Blank.xaml
    /// </summary>
    public partial class Blank : Window
    {
        public Blank()
        {
            InitializeComponent();

            this.Loaded += Blank_Loaded;
            //this.MouseMove += Blank_MouseMove;
            this.MouseDown += Blank_MouseDown;
            this.KeyUp += Blank_KeyUp;
        }

        private void Blank_MouseMove(object sender, MouseEventArgs e)
        {
            if (App.facialCheckDisabled)
            {
                App.CloseApplication = true;
                App.RequestClose();
            }
            else
            {
                App.ShowCamera();
            }
        }

        private void Blank_KeyUp(object sender, KeyEventArgs e)
        {
            if (App.facialCheckDisabled)
            {
                App.CloseApplication = true;
                App.RequestClose();
            }
            else
            {
                //HACK: Override Application Close Flag
                if (e.Key == Key.F3)
                {
                    App.CloseApplication = true;
                }

                App.ShowCamera();
            }
        }

        private void Blank_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (App.facialCheckDisabled)
            {
                App.CloseApplication = true;
                App.RequestClose();
            }
            else
            {
                App.ShowCamera();
            }
        }

        private void Blank_Loaded(object sender, RoutedEventArgs e)
        {
            //Hide Mouse Cursor
            Mouse.OverrideCursor = Cursors.None;

            //Full Screen
            this.WindowState = WindowState.Maximized;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!App.CloseApplication)
                e.Cancel = true;
        }
    }
}
