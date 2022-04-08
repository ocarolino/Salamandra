using Salamandra.Engine.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Events
{
    public class SoundErrorEventArgs
    {
        public Exception? SoundErrorException { get; set; }
    }
}
