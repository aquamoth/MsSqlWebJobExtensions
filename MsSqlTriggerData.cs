using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Triggers;

namespace MsSqlWebJobExtensions
{
    public class MsSqlTriggerData : ITriggerData
    {
        public IValueProvider ValueProvider => throw new NotImplementedException();

        public IReadOnlyDictionary<string, object> BindingData => throw new NotImplementedException();
    }
}
