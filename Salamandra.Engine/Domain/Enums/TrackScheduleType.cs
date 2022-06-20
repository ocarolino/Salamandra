using Salamandra.Engine.Converter;
using System.ComponentModel;

namespace Salamandra.Engine.Domain.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum TrackScheduleType
    {
        [Description("Faixa de Áudio")]
        AudioFileTrack = 0,
        [Description("Arquivo Aleatório de Pasta")]
        RandomFileTrack = 1,
        [Description("Locução de Hora")]
        TimeAnnouncementTrack = 2,
        [Description("Iniciar Playlist")]
        StartPlaylistTrack = 3,
        [Description("Parar Playlist")]
        StopPlaylistTrack = 4,
        [Description("Arquivo de Playlist")]
        OpenPlaylistTrack = 5,
        [Description("Executar Processo/Arquivo do Sistema")]
        SystemProcessTrack = 6,
        [Description("Arquivo de Eventos")]
        OpenScheduleTrack = 7,
    }
}
