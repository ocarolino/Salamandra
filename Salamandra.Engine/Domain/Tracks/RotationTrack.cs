using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Domain.Tracks
{
    public class RotationTrack : MultiFileTrack
    {
        public int CurrentFileIndex { get; set; }
        public override bool HasTrackFinished => true;

        public RotationTrack() : base()
        {
            this.CurrentFileIndex = -1;
        }

        public override string? GetFile()
        {
            if (this.Filenames.Count == 0)
                return null;

            this.CurrentFileIndex++;

            if (this.CurrentFileIndex >= this.Filenames.Count)
                this.CurrentFileIndex = 0;

            string file = this.Filenames[this.CurrentFileIndex];

            return file;
        }

        public virtual void ResetSequence()
        {
            this.CurrentFileIndex = -1;
        }
    }
}
