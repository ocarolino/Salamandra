using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Domain.Settings
{
    public class DeviceSettings : INotifyPropertyChanged
    {
        public int MainOutputDevice { get; set; }

        public DeviceSettings()
        {
            this.MainOutputDevice = 0;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

    }
}
