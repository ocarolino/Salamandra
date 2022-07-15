using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Utils
{
    public static class FileSystemUtils
    {
        public static string GetApplicationCurrentDirectory()
        {
            return AppContext.BaseDirectory;
        }
    }
}
