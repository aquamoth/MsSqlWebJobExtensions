using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;

namespace MsSqlWebJobExtensions
{
    class MsSqlTableTriggerBinding : ITriggerBinding
    {
        readonly string _configuration;
        readonly ParameterInfo _parameter;
        readonly MsSqlTableTriggerAttribute _attribute;
        readonly BindingDataProvider _bindingDataProvider;

        public MsSqlTableTriggerBinding(string configuration, ParameterInfo parameter, MsSqlTableTriggerAttribute attribute)
        {
            _configuration = configuration;
            _parameter = parameter;
            _attribute = attribute;
            _bindingDataProvider = BindingDataProvider.FromType(parameter.ParameterType);
        }

        public Type TriggerValueType => typeof(string);

        public IReadOnlyDictionary<string, Type> BindingDataContract => _bindingDataProvider?.Contract;

        public async Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            IValueProvider provider = new JsonValueProvider(value, _parameter.ParameterType);

            var providerVal = await provider.GetValueAsync();
            var bindingData = _bindingDataProvider?.GetBindingData(providerVal);

            var result = new TriggerData(provider, bindingData);

            return result;
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            IListener listener = new MsSqlTableListener(_configuration, context.Executor, _attribute);

            return Task.FromResult(listener);
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new ParameterDescriptor
            {
                Name = _parameter.Name,
                DisplayHints = new ParameterDisplayHints
                {
                    DefaultValue = "MyDefaultvalue",
                    Description = "My Description",
                    Prompt = "My Prompt"
                }
            };
        }
    }
}
