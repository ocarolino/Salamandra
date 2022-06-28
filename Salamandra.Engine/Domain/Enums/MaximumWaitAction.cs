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
    public enum MaximumWaitAction
    {
        [LocalizedDescription("MaximumWaitAction_Discard", typeof(EnumResources))]
        Discard = 0,
        [LocalizedDescription("MaximumWaitAction_Play", typeof(EnumResources))]
        Play = 1,
    }
}
