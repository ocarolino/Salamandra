using Salamandra.Engine.Converter;
using Salamandra.Engine.Strings;
using System.ComponentModel;

namespace Salamandra.Engine.Domain.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum PlaylistMode
    {
        [LocalizedDescription("PlaylistMode_Default", typeof(EnumResources))]
        Default = 0,
        [LocalizedDescription("PlaylistMode_Repeat", typeof(EnumResources))]
        Repeat = 1,
        [LocalizedDescription("PlaylistMode_Random", typeof(EnumResources))]
        Random = 2,
        [LocalizedDescription("PlaylistMode_Manual", typeof(EnumResources))]
        Manual = 3,
        [LocalizedDescription("PlaylistMode_Shuffle", typeof(EnumResources))]
        Shuffle = 4,
    }
}
