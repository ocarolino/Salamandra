using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Domain.Settings
{
    public class LoggingSettings : INotifyPropertyChanged
    {
        public bool EnableLogging { get; set; }
        public string LoggingOutputPath { get; set; }

        public LoggingSettings()
        {
            this.EnableLogging = false;
            this.LoggingOutputPath = String.Empty;
        }

#pragma warning disable 67
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore 67

    }
}
