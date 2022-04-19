using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Domain
{
    public class SoundFileTrack : BaseTrack
    {
        public string? Filename { get; set; }

        public override bool HasTrackFinished => true;
    }
}
