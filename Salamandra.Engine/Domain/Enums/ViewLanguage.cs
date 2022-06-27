using Salamandra.Engine.Converter;
using Salamandra.Engine.Strings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Domain.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ViewLanguage
    {
        [LocalizedDescription("ViewLanguage_Portuguese", typeof(EnumResources))]
        Portuguese = 0,
        [LocalizedDescription("ViewLanguage_English", typeof(EnumResources))]
        English = 1,
    }
}
