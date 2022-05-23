using System.ComponentModel;
using System.Drawing;

namespace Salamandra.Engine.Domain
{
    public class SongTagsInfo : INotifyPropertyChanged
    {
        public byte[] CoverArt { get; set; }
        public string? Artist { get; set; }
        public string? Title { get; set; }
        public string? Album { get; set; }
        public int? Year { get; set; }
        public string? Genre { get; set; }
        public string? Comment { get; set; }

#pragma warning disable 67
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore 67
    }
}
