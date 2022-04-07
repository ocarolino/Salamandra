using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Exceptions
{
    public class SoundEngineFileException : Exception
    {
        public SoundEngineFileException()
        {
        }

        public SoundEngineFileException(string message) : base(message)
        {
        }

        public SoundEngineFileException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
