using Salamandra.Engine.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Domain.Tracks
{
    public class TimeAnnouncementTrack : SequentialTrack
    {
        public string? AudioFilesDirectory { get; set; }

        public TimeAnnouncementTrack() : base()
        {
            this.Filename = "command.stop";
            this.FriendlyName = "Locução de Horário";
        }

        public override void ResetSequence()
        {
            this.Filenames.Clear();

            this.AudioFilesDirectory = this.AudioFilesDirectory?.EnsureHasDirectorySeparatorChar();

            if (!string.IsNullOrEmpty(AudioFilesDirectory) && Directory.Exists(this.AudioFilesDirectory))
            {
                string hourFilename = this.AudioFilesDirectory + "HRS" + DateTime.Now.ToString("HH") + ".mp3";
                string minuteFilename = this.AudioFilesDirectory + "MIN" + DateTime.Now.ToString("mm") + ".mp3";

                if (File.Exists(hourFilename) && File.Exists(minuteFilename))
                    this.Filenames = new List<string>() { hourFilename, minuteFilename };
            }

            base.ResetSequence();
        }
    }
}
