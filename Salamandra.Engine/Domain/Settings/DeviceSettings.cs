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
        public int PreListenOutputDevice { get; set; }

        public DeviceSettings()
        {
            this.MainOutputDevice = 0;
            this.PreListenOutputDevice = 0;
        }

        public void CheckDevices(List<SoundOutputDevice> devices)
        {
            if (devices.FirstOrDefault(x => x.DeviceIndex == this.MainOutputDevice) == null)
                this.MainOutputDevice = devices.First().DeviceIndex;

            if (devices.FirstOrDefault(x => x.DeviceIndex == this.PreListenOutputDevice) == null)
                this.PreListenOutputDevice = devices.First().DeviceIndex;
        }

#pragma warning disable 67
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore 67
    }
}
