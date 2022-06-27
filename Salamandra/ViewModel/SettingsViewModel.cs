using Newtonsoft.Json;
using Salamandra.Commands;
using Salamandra.Engine.Domain;
using Salamandra.Engine.Domain.Settings;
using Salamandra.Engine.Extensions;
using Salamandra.Engine.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Salamandra.ViewModel
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private ApplicationSettings originalApplicationSettings;
        private SoundEngine soundEngine;

        public ApplicationSettings? Settings { get; set; }
        public ObservableCollection<SoundOutputDevice> OutputDevices { get; set; }

        public ICommand? OpenTimePathDialogCommand { get; set; }
        public ICommand? OpenLogsPathDialogCommand { get; set; }

        public SettingsViewModel(ApplicationSettings applicationSettings, SoundEngine soundEngine)
        {
            this.originalApplicationSettings = applicationSettings;
            this.soundEngine = soundEngine;

            this.OutputDevices = new ObservableCollection<SoundOutputDevice>();

            this.OpenTimePathDialogCommand = new RelayCommand(p => OpenTimePathDialog());
            this.OpenLogsPathDialogCommand = new RelayCommand(p => OpenLogsPathDialog());
        }

        public void Loading()
        {
            try
            {
                var serialized = JsonConvert.SerializeObject(this.originalApplicationSettings);
                this.Settings = JsonConvert.DeserializeObject<ApplicationSettings>(serialized);
            }
            catch (Exception)
            {
                this.Settings = new ApplicationSettings();
            }

            this.OutputDevices = new ObservableCollection<SoundOutputDevice>(this.soundEngine.EnumerateDevices());
        }

        public bool Validate()
        {
            if (this.Settings!.LoggingSettings.EnableLogging)
            {
                if (String.IsNullOrWhiteSpace(this.Settings.LoggingSettings.LoggingOutputPath) ||
                    !Directory.Exists(this.Settings.LoggingSettings.LoggingOutputPath))
                {
                    MessageBox.Show(Salamandra.Strings.ViewsTexts.SettingsWindow_Validation_ValidLogFolder,
                        Salamandra.Strings.ViewsTexts.SettingsWindow_WindowTitle,
                        MessageBoxButton.OK, MessageBoxImage.Warning);

                    return false;
                }
            }

            return true;
        }

        public void Closing()
        {
            this.Settings!.DeviceSettings.MainOutputDeviceName =
                this.OutputDevices.First(x => x.DeviceIndex == this.Settings.DeviceSettings.MainOutputDevice).Name!;

            this.Settings!.DeviceSettings.PreListenOutputDeviceName =
                this.OutputDevices.First(x => x.DeviceIndex == this.Settings.DeviceSettings.PreListenOutputDevice).Name!;
        }

        private void OpenTimePathDialog()
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog vistaFolderBrowserDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();

            if (vistaFolderBrowserDialog.ShowDialog() == true)
                this.Settings!.GeneralSettings.TimeAnnouncementFilesPath = vistaFolderBrowserDialog.SelectedPath;
        }

        private void OpenLogsPathDialog()
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog vistaFolderBrowserDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();

            if (vistaFolderBrowserDialog.ShowDialog() == true)
                this.Settings!.LoggingSettings.LoggingOutputPath = vistaFolderBrowserDialog.SelectedPath;
        }


#pragma warning disable 67
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore 67
    }
}
