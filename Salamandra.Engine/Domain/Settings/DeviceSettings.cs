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
        public string MainOutputDeviceName { get; set; }
        public int PreListenOutputDevice { get; set; }
        public string PreListenOutputDeviceName { get; set; }

        public DeviceSettings()
        {
            this.MainOutputDevice = 0;
            this.MainOutputDeviceName = String.Empty;

            this.PreListenOutputDevice = 0;
            this.PreListenOutputDeviceName = String.Empty;
        }

        public void CheckDevices(List<SoundOutputDevice> devices)
        {
            this.MainOutputDevice = CheckDeviceIndex(this.MainOutputDevice, this.MainOutputDeviceName,
                devices.FirstOrDefault(x => x.DeviceIndex == this.MainOutputDevice));

            this.PreListenOutputDevice = CheckDeviceIndex(this.PreListenOutputDevice, this.PreListenOutputDeviceName,
                devices.FirstOrDefault(x => x.DeviceIndex == this.PreListenOutputDevice));
        }

        public int CheckDeviceIndex(int index, string name, SoundOutputDevice? device)
        {
            if (device == null || (!String.IsNullOrWhiteSpace(name) && device.Name != name))
                return 0;

            return index;
        }

#pragma warning disable 67
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore 67
    }
}
