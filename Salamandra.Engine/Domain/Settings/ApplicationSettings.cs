using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Domain.Settings
{
    public class ApplicationSettings : INotifyPropertyChanged
    {
        public GeneralSettings GeneralSettings { get; set; }
        public DeviceSettings DeviceSettings { get; set; }
        public PlayerSettings PlayerSettings { get; set; }
        public ScheduledEventSettings ScheduledEventSettings { get; set; }

        public ApplicationSettings()
        {
            this.GeneralSettings = new GeneralSettings();
            this.DeviceSettings = new DeviceSettings();
            this.PlayerSettings = new PlayerSettings();
            this.ScheduledEventSettings = new ScheduledEventSettings();
        }

#pragma warning disable 67
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore 67
    }
}
