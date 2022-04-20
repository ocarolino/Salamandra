using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Extensions
{
    public static class StringExtensions
    {
        public static string EnsureHasDirectorySeparatorChar(this string s)
        {
            return s.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        }

    }
}
