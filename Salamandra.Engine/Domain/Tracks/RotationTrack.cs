using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Domain.Tracks
{
    public class RotationTrack : MultiFileTrack
    {
        public override bool HasTrackFinished => true;

        public override string? GetCurrentFile()
        {
            if (this.Filenames.Count == 0)
                return null;

            if (this.CurrentFileIndex == -1)
                this.CurrentFileIndex++;

            string file = this.Filenames[this.CurrentFileIndex];

            this.CurrentFileIndex++;

            if (this.CurrentFileIndex >= this.Filenames.Count)
                this.CurrentFileIndex = 0;

            return file;
        }
    }
}
