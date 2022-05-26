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
using System.Windows.Shapes;

namespace Salamandra.Views
{
    /// <summary>
    /// Interaction logic for PreListenWindow.xaml
    /// </summary>
    public partial class PreListenWindow : Window
    {
        private PreListenViewModel preListenViewModel;

        public PreListenWindow(PreListenViewModel preListenViewModel)
        {
            InitializeComponent();

            this.preListenViewModel = preListenViewModel;
            this.preListenViewModel.CloseHandler += CloseHandler;
            this.DataContext = preListenViewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.preListenViewModel.Loading();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!this.preListenViewModel.HasSoundStopped)
            {
                this.preListenViewModel.StopPlayback();
                e.Cancel = true;
            }
        }

        private void CloseHandler()
        {
            this.Close();
        }
    }
}
