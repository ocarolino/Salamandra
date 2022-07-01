using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Domain.Settings
{
    public class ScheduledEventSettings
    {
        public string ScheduledEventFilename { get; set; }
        public bool AlwaysEnableEventsOnStartup { get; set; }
        public bool EnableEvents { get; set; }

        public ScheduledEventSettings()
        {
            this.ScheduledEventFilename = String.Empty;

            this.AlwaysEnableEventsOnStartup = false;
            this.EnableEvents = true;
        }
    }
}
