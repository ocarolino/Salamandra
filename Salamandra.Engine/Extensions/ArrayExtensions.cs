using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Extensions
{
    public static class ArrayExtensions
    {
        public static string GetDialogFilterFromArray(this string[] input, string description)
        {
            string[] filter = new string[input.Length];

            for (int i = 0; i < filter.Length; i++)
                filter[i] = "*" + input[i];

            return String.Format("{0} ({1}) | {2}", description, String.Join(", ", filter), String.Join(";", filter));
        }
    }
}
