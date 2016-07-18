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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Horus
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
            this.MouseDown += MainWindow_MouseDown;
            this.KeyUp += MainWindow_KeyUp;
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            App.RequestClose();
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            App.RequestClose();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Hide Mouse Cursor
            Mouse.OverrideCursor = Cursors.None;

            //Full Screen
            WindowState = WindowState.Maximized;

            //Graphics/Animations for Screensaver
            //TODO: Graphics/Animation Loading

            Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#00ff00"));
        }
    }
}
