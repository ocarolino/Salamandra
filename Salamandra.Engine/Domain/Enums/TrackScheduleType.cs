using Salamandra.Engine.Converter;
using Salamandra.Engine.Strings;
using System.ComponentModel;

namespace Salamandra.Engine.Domain.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum TrackScheduleType
    {
        [LocalizedDescription("TrackScheduleType_AudioFileTrack", typeof(EnumResources))]
        AudioFileTrack = 0,
        [LocalizedDescription("TrackScheduleType_RandomFileTrack", typeof(EnumResources))]
        RandomFileTrack = 1,
        [LocalizedDescription("TrackScheduleType_TimeAnnouncementTrack", typeof(EnumResources))]
        TimeAnnouncementTrack = 2,
        [LocalizedDescription("TrackScheduleType_StartPlaylistTrack", typeof(EnumResources))]
        StartPlaylistTrack = 3,
        [LocalizedDescription("TrackScheduleType_StopPlaylistTrack", typeof(EnumResources))]
        StopPlaylistTrack = 4,
        [LocalizedDescription("TrackScheduleType_OpenPlaylistTrack", typeof(EnumResources))]
        OpenPlaylistTrack = 5,
        [LocalizedDescription("TrackScheduleType_SystemProcessTrack", typeof(EnumResources))]
        SystemProcessTrack = 6,
        [LocalizedDescription("TrackScheduleType_OpenScheduleTrack", typeof(EnumResources))]
        OpenScheduleTrack = 7,
    }
}
