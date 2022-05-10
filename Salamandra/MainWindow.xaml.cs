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
using Salamandra.Extensions;
using System.Windows.Shapes;
using Salamandra.Controls;
using System.ComponentModel;
using Salamandra.Engine.Domain.Enums;
using Salamandra.Engine.Comparer;

namespace Salamandra
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel mainViewModel;

        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

        public MainWindow()
        {
            InitializeComponent();

            this.mainViewModel = new MainViewModel();
            this.mainViewModel.RemovePlaylistAdorner += RemovePlaylistAdorner;
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

        private void playlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.mainViewModel.UpdateSelectedTrackTags();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ScrollViewer? scrollViewer = playlist.GetChildOfType<ScrollViewer>();

            if (scrollViewer == null)
                return;
            //GetChildOfType<ScrollViewer>(playlist);

            double width = scrollViewer.ActualWidth - (SystemParameters.VerticalScrollBarWidth * 2);

            // ToDo: Magic numbers!
            PlaylistFriendlyNameColumn.Width = Math.Max(360, width - 160);
            PlaylistDurationColumn.Width = 160;
        }

        private void PlaylistColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader? column = (sender as GridViewColumnHeader);

            if (column == null || !this.mainViewModel.CanSortPlaylist())
                return;

            int sortBy = int.Parse(column.Tag.ToString());

            RemovePlaylistAdorner();

            ListSortDirection newDir = ListSortDirection.Ascending;

            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);

            this.mainViewModel.SortPlaylist((PlaylistSortCriteria)sortBy, newDir == ListSortDirection.Ascending ? SortDirection.Ascending : SortDirection.Descending);
        }

        private void RemovePlaylistAdorner()
        {
            if (listViewSortCol != null)
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
        }
    }
}
