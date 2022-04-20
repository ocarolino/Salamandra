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

        // ToDo: Esses métodos fazem mais sentido diretamente na MultiFileTrack, porém vou pensar por hora...
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

        public virtual void ResetSequence()
        {
            if (this.Filenames.Count == 0)
                this.CurrentFileIndex = -1;
            else
                this.CurrentFileIndex = 0;
        }
    }
}
