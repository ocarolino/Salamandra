using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Exceptions
{
    public class PlaylistLoaderException : Exception
    {
        public PlaylistLoaderException()
        {
        }

        public PlaylistLoaderException(string message) : base(message)
        {
        }

        public PlaylistLoaderException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
