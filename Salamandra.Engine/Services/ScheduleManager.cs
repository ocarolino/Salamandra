﻿using Salamandra.Engine.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Services
{
    public class ScheduleManager
    {
        public List<ScheduledEvent> Events { get; set; }

        public ScheduleManager()
        {
            this.Events = new List<ScheduledEvent>();
        }
    }
}