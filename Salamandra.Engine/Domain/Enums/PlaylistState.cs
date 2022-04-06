using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Domain.Enums
{
    public enum PlaylistState
    {
        Stopped = 0,
        PlayingPlaylistTrack = 1,
        PlayingEventTrack = 2,
        WaitingNextTrack = 3,
    }
}
