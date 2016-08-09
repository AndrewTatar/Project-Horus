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

            if (App.facialCheckDisabled)
            {
                this.MouseDown += Blank_MouseDown;
                this.KeyUp += Blank_KeyUp;
            }
        }

        private void Blank_KeyUp(object sender, KeyEventArgs e)
        {
            App.RequestClose();
        }

        private void Blank_MouseDown(object sender, MouseButtonEventArgs e)
        {
            App.RequestClose();
        }

        private void Blank_Loaded(object sender, RoutedEventArgs e)
        {
            //Hide Mouse Cursor
            Mouse.OverrideCursor = Cursors.None;

            //Full Screen
            this.WindowState = WindowState.Maximized;
        }
    }
}
