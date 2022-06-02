﻿using Newtonsoft.Json;
using Salamandra.Engine.Domain;
using Salamandra.Engine.Domain.Settings;
using Salamandra.Engine.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.ViewModel
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private ApplicationSettings originalApplicationSettings;
        private SoundEngine soundEngine;

        public ApplicationSettings? Settings { get; set; }
        public ObservableCollection<SoundOutputDevice> OutputDevices { get; set; }

        public SettingsViewModel(ApplicationSettings applicationSettings, SoundEngine soundEngine)
        {
            this.originalApplicationSettings = applicationSettings;
            this.soundEngine = soundEngine;

            this.OutputDevices = new ObservableCollection<SoundOutputDevice>();
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

        public void Closing()
        {
            this.Settings!.DeviceSettings.MainOutputDeviceName =
                this.OutputDevices.First(x => x.DeviceIndex == this.Settings.DeviceSettings.MainOutputDevice).Name!;

            this.Settings!.DeviceSettings.PreListenOutputDeviceName =
                this.OutputDevices.First(x => x.DeviceIndex == this.Settings.DeviceSettings.PreListenOutputDevice).Name!;
        }

#pragma warning disable 67
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore 67
    }
}
