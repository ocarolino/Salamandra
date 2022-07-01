using Salamandra.Engine.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Extensions
{
    public static class ViewLanguageExtensions
    {
        public static string ViewLanguageToCultureName(this ViewLanguage input)
        {
            switch (input)
            {
                case ViewLanguage.Portuguese:
                    return "pt-BR";
                case ViewLanguage.English:
                    return "en-US";
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
