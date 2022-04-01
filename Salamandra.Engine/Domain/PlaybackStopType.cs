using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Domain
{
    public enum PlaybackStopType
    {
        StoppedByRequest = 0,
        ReachedEndOfFile = 1,
    }
}
