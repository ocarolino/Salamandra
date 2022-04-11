using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Domain
{
    public class SoundFileTrack
    {
        public string Filename { get; private set; }
        public string FriendlyName { get; private set; }
        public TimeSpan? Duration { get; private set; }

        public SoundFileTrack(string filename, string friendlyName)
        {
            this.Filename = filename;
            this.FriendlyName = friendlyName;
        }
    }
}
