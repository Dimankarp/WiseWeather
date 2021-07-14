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

namespace WiseWeather
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private double oldWindowHeight;
        private double oldWindowWidth;
        public  MainWindow()
        {
            InitializeComponent();
            oldWindowHeight = this.Height;
            oldWindowWidth = this.Width;
            DataContext = new ApplicationViewModel();

        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchWindowSize();
        }
        private void TopBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) SwitchWindowSize();

        }

        private void SwitchWindowSize()
        {
            if (this.Height == MaxHeight && this.Width == MaxWidth)
            {
                this.Height = oldWindowHeight;
                this.Width = oldWindowWidth;
            }
            else
            {

                oldWindowHeight = this.Height;
                oldWindowWidth = this.Width;
                this.Height = MaxHeight;
                this.Width = MaxWidth;

            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

    }
}
