﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Domain.Tracks
{
    public class PlaylistFileTrack : SingleFileTrack
    {
        public override bool HasTrackFinished => true;
    }
}
