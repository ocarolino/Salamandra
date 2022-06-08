using Salamandra.Engine.Domain.Settings;
using Salamandra.Engine.Services;
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
    /// Lógica interna para SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private SettingsViewModel settingsViewModel;

        public SettingsWindow(SettingsViewModel settingsViewModel)
        {
            InitializeComponent();

            this.settingsViewModel = settingsViewModel;
            this.DataContext = this.settingsViewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.settingsViewModel.Loading();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !this.settingsViewModel.Validate();

            if (!e.Cancel)
                this.settingsViewModel.Closing();
        }
    }
}
