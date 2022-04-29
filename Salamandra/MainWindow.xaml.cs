using Salamandra.ViewModel;
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

namespace Salamandra
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel mainViewModel;

        public MainWindow()
        {
            InitializeComponent();

            this.mainViewModel = new MainViewModel();
            this.DataContext = this.mainViewModel;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !this.mainViewModel.Closing();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.mainViewModel.Loading();
        }

        private void UpcomingEventsListView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HitTestResult r = VisualTreeHelper.HitTest(this, e.GetPosition(this));

            if (r.VisualHit.GetType() != typeof(ListViewItem))
                (sender as ListView)!.UnselectAll();
        }
    }
}
