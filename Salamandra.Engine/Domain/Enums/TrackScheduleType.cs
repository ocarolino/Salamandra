﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Domain.Enums
{
    public enum TrackScheduleType
    {
        AudioFileTrack = 0,
        RandomFileTrack = 1,
        TimeAnnouncementTrack = 2,
        StartPlaylistTrack = 3,
        StopPlaylistTrack = 4,
        OpenPlaylistTrack = 5,
    }
}
