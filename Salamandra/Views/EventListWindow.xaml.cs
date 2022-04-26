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
    /// Lógica interna para EventListWindow.xaml
    /// </summary>
    public partial class EventListWindow : Window
    {
        private EventListViewModel eventListViewModel;

        public EventListWindow(EventListViewModel eventListViewModel)
        {
            InitializeComponent();

            this.eventListViewModel = eventListViewModel;
            this.DataContext = this.eventListViewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.eventListViewModel.Loading();
        }
    }
}
