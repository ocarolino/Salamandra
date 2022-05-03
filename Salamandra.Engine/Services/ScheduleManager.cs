using Newtonsoft.Json;
using Salamandra.Engine.Domain;
using Salamandra.Engine.Domain.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Services
{
    public class ScheduleManager : INotifyPropertyChanged
    {
        public List<ScheduledEvent> Events { get; set; }
        public ObservableCollection<UpcomingEvent> EventsQueue { get; set; }
        public int LastHourChecked { get; set; }
        public bool HasLateImmediateEvent { get; set; }
        public bool HasLateWaitingEvent { get; set; }
        public bool HasLateEvent { get => this.HasLateImmediateEvent || this.HasLateWaitingEvent; }

        public ScheduleManager()
        {
            this.Events = new List<ScheduledEvent>();
            this.EventsQueue = new ObservableCollection<UpcomingEvent>();

            this.LastHourChecked = -1;
        }

        public void SwapEvents(ObservableCollection<ScheduledEvent> events)
        {
            // ToDo: Filename argument!
            this.Events = new List<ScheduledEvent>(events);
        }

        #region Saving, Loading and Reseting a List
        // ToDo: Criar um Save e Load genéricos depois!
        public void SaveToFile(string filename)
        {
            string json = JsonConvert.SerializeObject(this.Events);
            File.WriteAllText(filename, json);
        }

        public void LoadFromFile(string filename)
        {
            var list = JsonConvert.DeserializeObject<List<ScheduledEvent>>(File.ReadAllText(filename));

            if (list != null)
                this.Events = list;
            else
                this.Events = new List<ScheduledEvent>();
        }
        #endregion

        public void CleanEvents()
        {
            this.Events = new List<ScheduledEvent>();
        }

        public bool CheckEventSchedule(ScheduledEvent scheduledEvent, DateTime dateToTest)
        {
            if (scheduledEvent.StartingDateTime.Date > dateToTest.Date)
                return false;

            if (scheduledEvent.UseDaysOfWeek)
            {
                if (!scheduledEvent.DaysOfWeek.Contains(dateToTest.DayOfWeek))
                    return false;
            }
            else
            {
                if (scheduledEvent.StartingDateTime.Date != dateToTest.Date)
                    return false;
            }

            if (scheduledEvent.UsePlayingHours)
            {
                if (!scheduledEvent.PlayingHours.Contains(dateToTest.Hour))
                    return false;
            }
            else
            {
                if (scheduledEvent.StartingDateTime.Hour != dateToTest.Hour)
                    return false;
            }

            DateTime runningDate = new DateTime(dateToTest.Year, dateToTest.Month, dateToTest.Day,
                dateToTest.Hour, scheduledEvent.StartingDateTime.Minute, scheduledEvent.StartingDateTime.Second);

            if (runningDate < dateToTest || (scheduledEvent.UseExpirationDateTime && runningDate > scheduledEvent.ExpirationDateTime))
                return false;

            return true;
        }

        public void UpdateQueuedEventsList()
        {
            if (DateTime.Now.Hour != this.LastHourChecked)
            {
                CreateUpcomingEvents(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                    DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second));
                this.LastHourChecked = DateTime.Now.Hour;
            }

            if (this.EventsQueue.Count == 0)
            {
                this.HasLateImmediateEvent = false;
                this.HasLateWaitingEvent = false;

                return;
            }

            UpdateLateEvents();
        }

        private void UpdateLateEvents()
        {
            UpcomingEvent? immediate = GetLateImmediateEvent();
            UpcomingEvent? waiting = GetLateWaitingEvent();

            if (immediate != null)
                this.HasLateImmediateEvent = true;
            else
                this.HasLateImmediateEvent = false;

            if (waiting != null)
                this.HasLateWaitingEvent = true;
            else
                this.HasLateWaitingEvent = false;
        }

        private void CreateUpcomingEvents(DateTime startFromDate)
        {
            foreach (var item in this.Events)
            {
                if (CheckEventSchedule(item, startFromDate))
                    QueueEvent(item, startFromDate);
            }

            this.EventsQueue = new ObservableCollection<UpcomingEvent>(
                this.EventsQueue.OrderBy(x => x.StartDateTime).ThenByDescending(x => x.Immediate)
                );
        }

        private void QueueEvent(ScheduledEvent scheduledEvent, DateTime startFromDate)
        {
            Debug.WriteLine(String.Format("Queueing event: {0}", scheduledEvent.FriendlyName));

            UpcomingEvent upcomingEvent = new UpcomingEvent();
            upcomingEvent.EventId = scheduledEvent.Id;
            upcomingEvent.Immediate = scheduledEvent.Immediate;
            upcomingEvent.StartDateTime = new DateTime(startFromDate.Year, startFromDate.Month, startFromDate.Day,
                startFromDate.Hour, scheduledEvent.StartingDateTime.Minute, scheduledEvent.StartingDateTime.Second);
            upcomingEvent.Track = scheduledEvent.GetTrack();

            this.EventsQueue.Add(upcomingEvent);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void RefreshEventsQueue()
        {
            DateTime refreshDate = DateTime.Now;

            var futureQueue = this.EventsQueue.Where(x => x.StartDateTime >= refreshDate);

            for (int i = this.EventsQueue.Count - 1; i >= 0; i--)
            {
                var temp = this.EventsQueue[i];

                if (futureQueue.Contains(temp))
                {
                    this.EventsQueue.Remove(temp);
                    continue;
                }

                var model = this.Events.FirstOrDefault(x => x.Id == temp.EventId);

                if (model == null)
                {
                    this.EventsQueue.Remove(temp);
                    continue;
                }

                if (!CheckEventSchedule(model, temp.StartDateTime))
                {
                    this.EventsQueue.Remove(temp);
                    continue;
                }

                temp.Track = model.GetTrack();
            }

            CreateUpcomingEvents(refreshDate);
            UpdateLateEvents();
        }

        public UpcomingEvent? GetLateImmediateEvent()
        {
            return this.EventsQueue.FirstOrDefault(x => x.Immediate && x.StartDateTime < DateTime.Now);
        }

        public UpcomingEvent? GetLateWaitingEvent()
        {
            return this.EventsQueue.FirstOrDefault(x => !x.Immediate && x.StartDateTime < DateTime.Now);
        }

        public UpcomingEvent? DequeueLateEvent()
        {
            var upcomingEvent = this.GetLateImmediateEvent();

            if (upcomingEvent == null)
                upcomingEvent = this.GetLateWaitingEvent();

            if (upcomingEvent != null)
                this.EventsQueue.Remove(upcomingEvent);

            return upcomingEvent;
        }
    }
}
