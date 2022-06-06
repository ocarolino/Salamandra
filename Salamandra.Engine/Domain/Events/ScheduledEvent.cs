using Salamandra.Engine.Domain.Enums;
using Salamandra.Engine.Domain.Tracks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Domain.Events
{
    public class ScheduledEvent : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public bool IsEnabled { get; set; }
        public bool Immediate { get; set; }
        public DateTime StartingDateTime { get; set; }
        public bool UsePlayingHours { get; set; }
        public ObservableCollection<int> PlayingHours { get; set; }
        public bool UseExpirationDateTime { get; set; }
        public DateTime ExpirationDateTime { get; set; }
        public bool UseDaysOfWeek { get; set; }
        public ObservableCollection<DayOfWeek> DaysOfWeek { get; set; }
        public TrackScheduleType TrackScheduleType { get; set; }
        public string Filename { get; set; }
        public string FriendlyName { get; set; }
        public int QueueOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ScheduledEvent()
        {
            this.IsEnabled = true;
            this.StartingDateTime = DateTime.Now;
            this.ExpirationDateTime = DateTime.Now;
            this.PlayingHours = new ObservableCollection<int>();
            this.DaysOfWeek = new ObservableCollection<DayOfWeek>();
            this.Filename = String.Empty;
            this.FriendlyName = String.Empty;

            this.QueueOrder = 1;

            this.CreatedAt = DateTime.Now;
            this.UpdatedAt = DateTime.Now;
        }

        public BaseTrack GetTrack()
        {
            switch (this.TrackScheduleType)
            {
                case TrackScheduleType.FileTrack:
                    return new AudioFileTrack()
                    {
                        FriendlyName = this.FriendlyName,
                        Filename = this.Filename
                    };
                case TrackScheduleType.RandomFileTrack:
                    return new RandomFileTrack()
                    {
                        FriendlyName = this.FriendlyName,
                        Filename = this.Filename
                    };
                case TrackScheduleType.TimeAnnouncementTrack:
                    return new TimeAnnouncementTrack();
                case TrackScheduleType.StartPlaylistTrack:
                    return new PlayerCommandTrack(PlayerCommandType.Play);
                case TrackScheduleType.StopPlaylistTrack:
                    return new PlayerCommandTrack(PlayerCommandType.Stop);
                case TrackScheduleType.OpenPlaylistTrack:
                    return new PlaylistFileTrack()
                    {
                        FriendlyName = this.FriendlyName,
                        Filename = this.Filename
                    };
                default:
                    throw new NotImplementedException();
            }
        }

#pragma warning disable 67
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore 67
    }
}
