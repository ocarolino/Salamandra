using Newtonsoft.Json;
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
            var serialized = JsonConvert.SerializeObject(this.originalApplicationSettings);
            this.Settings = JsonConvert.DeserializeObject<ApplicationSettings>(serialized);

            this.OutputDevices = new ObservableCollection<SoundOutputDevice>(this.soundEngine.EnumerateDevices());
        }

#pragma warning disable 67
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore 67
    }
}
