using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Domain.Tracks
{
    public class RandomFileTrack : MultiFileTrack
    {
        public override bool HasTrackFinished => true;

        public RandomFileTrack() : base()
        {
        }

        public override string? GetFile()
        {
            // ToDo: Evitar com que arquivos se repitam.

            if (this.Filenames.Count == 0)
                return null;

            Random random = new Random();

            return this.Filenames[random.Next(this.Filenames.Count)];
        }
    }
}
