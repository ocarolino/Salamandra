using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Domain
{
    public abstract class BaseTrack : INotifyPropertyChanged
    {
        public string? FriendlyName { get; set; }
        public TimeSpan? Duration { get; set; }
        public abstract bool HasTrackFinished { get; }

#pragma warning disable 67
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore 67
    }
}
