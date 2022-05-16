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
        PlayingTrack = 1,
        WaitingNextTrack = 3,
        JumpToNextTrack = 4,
        JumpToNextEvent = 5,
    }
}
