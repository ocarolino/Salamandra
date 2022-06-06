using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Domain.Settings
{
    public class GeneralSettings : INotifyPropertyChanged
    {
        public string TimeAnnouncementFilesPath { get; set; }

        public GeneralSettings()
        {
            this.TimeAnnouncementFilesPath = String.Empty;
        }

#pragma warning disable 67
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore 67
    }
}
