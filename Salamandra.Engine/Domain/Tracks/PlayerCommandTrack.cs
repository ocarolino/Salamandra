using Salamandra.Engine.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Domain.Tracks
{
    public class PlayerCommandTrack : SingleFileTrack
    {
        public PlayerCommandType Command { get; set; }
        public override bool HasTrackFinished => true;

        public PlayerCommandTrack(PlayerCommandType command) : base()
        {
            this.Command = command;

            UpdateNames();
        }

        private void UpdateNames()
        {
            switch (this.Command)
            {
                case PlayerCommandType.Play:
                    this.FriendlyName = Salamandra.Engine.Strings.TracksTexts.Track_StartPlayback;
                    this.Filename = "commandtrack.start";
                    break;
                case PlayerCommandType.Stop:
                    this.FriendlyName = Salamandra.Engine.Strings.TracksTexts.Track_StopPlayback;
                    this.Filename = "commandtrack.stop";
                    break;
                default:
                    break;
            }
        }
    }
}
