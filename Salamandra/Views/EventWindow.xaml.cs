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
    /// Lógica interna para EventWindow.xaml
    /// </summary>
    public partial class EventWindow : Window
    {
        private EventViewModel eventViewModel;

        public EventWindow(EventViewModel eventViewModel)
        {
            InitializeComponent();

            this.eventViewModel = eventViewModel;
            this.eventViewModel.CloseWindow += CloseWindow_Handler;
            this.DataContext = this.eventViewModel;
        }

        private void CloseWindow_Handler(bool result)
        {
            this.DialogResult = result;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.eventViewModel.Loading();
        }
    }
}
