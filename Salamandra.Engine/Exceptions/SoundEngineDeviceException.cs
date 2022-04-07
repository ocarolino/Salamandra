using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Exceptions
{
    public class SoundEngineDeviceException : Exception
    {
        public SoundEngineDeviceException()
        {
        }

        public SoundEngineDeviceException(string message) : base(message)
        {
        }

        public SoundEngineDeviceException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
