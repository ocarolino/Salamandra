using Salamandra.Controls;
using Salamandra.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

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

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();

            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                EventsListView.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;

            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            EventsListView.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }
    }
}
