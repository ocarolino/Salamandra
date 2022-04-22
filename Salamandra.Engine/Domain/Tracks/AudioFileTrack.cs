using Salamandra.Engine.Domain.Tracks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Domain.Tracks
{
    public class AudioFileTrack : SingleFileTrack
    {
        public override bool HasTrackFinished => true;
    }
}
