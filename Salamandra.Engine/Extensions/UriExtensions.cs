using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Extensions
{
    public static class UriExtensions
    {
        public static Uri MakeAbsoluteUri(this Uri u, Uri uri)
        {
            return new Uri(u, uri);
        }
    }
}
