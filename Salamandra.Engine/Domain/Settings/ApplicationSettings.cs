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
        public DeviceSettings DeviceSettings { get; set; }
        public PlayerSettings PlayerSettings { get; set; }

        public ApplicationSettings()
        {
            this.DeviceSettings = new DeviceSettings();
            this.PlayerSettings = new PlayerSettings();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
