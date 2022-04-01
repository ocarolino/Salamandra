using Salamandra.Engine.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Events
{
    public class SoundStoppedEventArgs : EventArgs
    {
        public PlaybackStopType PlaybackStopType { get; set; }
    }
}
