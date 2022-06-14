using Salamandra.Engine.Converter;
using System.ComponentModel;

namespace Salamandra.Engine.Domain.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum PlaylistMode
    {
        [Description("Padrão")]
        Default = 0,
        [Description("Repetir")]
        Repeat = 1,
        [Description("Aleatório")]
        Random = 2,
        [Description("Manual")]
        Manual = 3,
        [Description("Embaralhar")]
        Shuffle = 4,
    }
}
