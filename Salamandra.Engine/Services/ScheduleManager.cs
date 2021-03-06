using Newtonsoft.Json;
using Salamandra.Engine.Domain;
using Salamandra.Engine.Domain.Enums;
using Salamandra.Engine.Domain.Events;
using Salamandra.Engine.Services.Playlists;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Salamandra.Engine.Services
{
    public class ScheduleManager : INotifyPropertyChanged
    {
        public List<ScheduledEvent> Events { get; set; }
        public WpfObservableRangeCollection<UpcomingEvent> EventsQueue { get; set; }
        public int LastHourChecked { get; set; }
        public bool HasLateImmediateEvent { get; set; }
        public bool HasLateWaitingEvent { get; set; }
        public bool HasLateEvent { get => this.HasLateImmediateEvent || this.HasLateWaitingEvent; }
        public static string[] SupportedScheduleFormats = { ".sche" };

        public ScheduleManager()
        {
            this.Events = new List<ScheduledEvent>();
            this.EventsQueue = new WpfObservableRangeCollection<UpcomingEvent>();

            this.LastHourChecked = -1;
        }

        public void SwapEvents(ObservableCollection<ScheduledEvent> events, bool resetQueue = false)
        {
            this.Events = new List<ScheduledEvent>(events);

            ResetAndRefreshQueue(resetQueue);
        }

        public void ResetAndRefreshQueue(bool resetQueue)
        {
            if (resetQueue)
                this.EventsQueue.Clear();

            RefreshEventsQueue();
        }

        #region Saving and Loading
        public void SaveToFile(string filename)
        {
            JsonEventsLoader jsonEventsLoader = new JsonEventsLoader();

            try
            {
                jsonEventsLoader.Save(filename, this.Events);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void LoadFromFile(string filename)
        {
            JsonEventsLoader jsonEventsLoader = new JsonEventsLoader();

            try
            {
                var list = jsonEventsLoader.Load(filename);
                this.Events = new List<ScheduledEvent>(list);
            }
            catch (Exception)
            {
                this.Events = new List<ScheduledEvent>();

                throw;
            }
        }
        #endregion

        public void CleanEvents()
        {
            this.Events = new List<ScheduledEvent>();
        }

        public bool CheckEventSchedule(ScheduledEvent scheduledEvent, DateTime dateToTest, bool refresh = false)
        {
            if (!scheduledEvent.IsEnabled)
                return false;

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

            if (runningDate < dateToTest || (scheduledEvent.UseExpirationDateTime && runningDate > scheduledEvent.ExpirationDateTime)
                || (refresh && (runningDate > dateToTest)))
                return false;

            return true;
        }

        public void UpdateQueuedEventsList(bool isEventPlaying, EventPriority eventPriority)
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

            if (isEventPlaying)
                DiscardLateEventsByPriority(eventPriority);

            DiscardMaximumWaitEvents();
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

            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasLateEvent)));
        }

        private void DiscardLateEventsByPriority(EventPriority eventPriority)
        {
            var lateEvents = this.EventsQueue.Where(x => x.StartDateTime < DateTime.Now && x.EventPriority < eventPriority).ToList();

            this.EventsQueue.RemoveRange(lateEvents);
        }

        private void DiscardMaximumWaitEvents()
        {
            var maximumWaitEvents = this.EventsQueue.Where(x =>
                x.MaximumWaitTime != null && x.MaximumWaitAction == Domain.Enums.MaximumWaitAction.Discard).ToList();

            for (int i = 0; i < maximumWaitEvents.Count(); i++)
            {
                var waitEvent = maximumWaitEvents[i];

                DateTime maximumDateTime = waitEvent.StartDateTime.Add(waitEvent.MaximumWaitTime!.Value);

                if (maximumDateTime < DateTime.Now)
                    this.EventsQueue.Remove(waitEvent);
            }
        }

        private void CreateUpcomingEvents(DateTime startFromDate)
        {
            var eventsToQueue = new List<UpcomingEvent>();

            foreach (var item in this.Events)
            {
                if (CheckEventSchedule(item, startFromDate))
                {
                    Debug.WriteLine(String.Format("Queueing event: {0}", item.FriendlyName));
                    eventsToQueue.Add(CreateUpcomingEvent(item, startFromDate));
                }
            }

            eventsToQueue.AddRange(this.EventsQueue);

            this.EventsQueue = new WpfObservableRangeCollection<UpcomingEvent>(
                eventsToQueue.OrderBy(x => x.StartDateTime)
                .ThenByDescending(x => x.Immediate)
                .ThenBy(x => x.QueueOrder)
                .ThenBy(x => x.EventId)
                );
        }

        private UpcomingEvent CreateUpcomingEvent(ScheduledEvent scheduledEvent, DateTime startFromDate)
        {
            UpcomingEvent upcomingEvent = new UpcomingEvent();
            upcomingEvent.EventId = scheduledEvent.Id;
            upcomingEvent.Immediate = scheduledEvent.Immediate;
            upcomingEvent.StartDateTime = new DateTime(startFromDate.Year, startFromDate.Month, startFromDate.Day,
                startFromDate.Hour, scheduledEvent.StartingDateTime.Minute, scheduledEvent.StartingDateTime.Second);
            upcomingEvent.QueueOrder = scheduledEvent.QueueOrder;
            upcomingEvent.MaximumWaitTime = scheduledEvent.UseMaximumWait ? scheduledEvent.MaximumWaitTime : null;
            upcomingEvent.MaximumWaitAction = scheduledEvent.MaximumWaitAction;
            upcomingEvent.EventPriority = scheduledEvent.EventPriority;
            upcomingEvent.Track = scheduledEvent.GetTrack();

            return upcomingEvent;
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

                var model = this.Events.FirstOrDefault(x => (x.Id == temp.EventId) && x.IsEnabled);

                if (model == null)
                {
                    this.EventsQueue.Remove(temp);
                    continue;
                }

                if (!CheckEventSchedule(model, temp.StartDateTime, true))
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
            var defaultEvent = this.EventsQueue.FirstOrDefault(x => x.Immediate && x.StartDateTime < DateTime.Now);

            if (defaultEvent == null)
            {
                var lateWaitingEvent = this.EventsQueue.FirstOrDefault
                    (x => x.MaximumWaitTime != null &&
                          x.MaximumWaitAction == Domain.Enums.MaximumWaitAction.Play &&
                          x.StartDateTime < DateTime.Now &&
                          x.StartDateTime.Add(x.MaximumWaitTime.Value) < DateTime.Now
                    );

                defaultEvent = lateWaitingEvent;
            }

            return defaultEvent;
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

            UpdateLateEvents();

            return upcomingEvent;
        }

        public void DiscardLateEvents()
        {
            var lateEvents = this.EventsQueue.Where(x => x.StartDateTime < DateTime.Now).ToList();

            this.EventsQueue.RemoveRange(lateEvents);
        }
    }
}
