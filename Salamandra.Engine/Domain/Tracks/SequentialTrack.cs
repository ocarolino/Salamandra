using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Domain.Tracks
{
    public class SequentialTrack : RotationTrack
    {
        public override bool HasTrackFinished => (this.CurrentFileIndex == -1) || (this.CurrentFileIndex >= this.Filenames.Count - 1);

        public SequentialTrack() : base()
        {
        }
    }
}
