using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Domain
{
    public class SoundFileTrack : INotifyPropertyChanged
    {
        public string Filename { get; private set; }
        public string FriendlyName { get; set; }
        public TimeSpan? Duration { get; set; }

        public SoundFileTrack(string filename, string friendlyName)
        {
            this.Filename = filename;
            this.FriendlyName = friendlyName;
        }

#pragma warning disable 67
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore 67
    }
}
