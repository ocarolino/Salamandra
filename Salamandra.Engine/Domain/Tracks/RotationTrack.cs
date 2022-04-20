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

        public RotationTrack() : base()
        {
        }

        // ToDo: Esses métodos fazem mais sentido diretamente na MultiFileTrack, porém vou pensar por hora...
        public override string? GetNextFile()
        {
            if (this.Filenames.Count == 0)
                return null;

            /*if (this.CurrentFileIndex == -1)
                this.CurrentFileIndex++;

            string file = this.Filenames[this.CurrentFileIndex];

            this.CurrentFileIndex++;

            if (this.CurrentFileIndex >= this.Filenames.Count)
                this.CurrentFileIndex = 0;*/

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
