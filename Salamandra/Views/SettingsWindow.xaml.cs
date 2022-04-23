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

        public SettingsWindow(ApplicationSettings applicationSettings, SoundEngine soundEngine)
        {
            InitializeComponent();

            this.settingsViewModel = new SettingsViewModel(applicationSettings, soundEngine);
            this.DataContext = settingsViewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.settingsViewModel.Loading();
        }
    }
}
