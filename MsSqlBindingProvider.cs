using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Triggers;

namespace MsSqlWebJobExtensions
{
    class MsSqlBindingProvider : ITriggerBindingProvider
    {
        private readonly string _configuration;
        public MsSqlBindingProvider(string configuration)
        {
            _configuration = configuration;
        }

        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var binding = CreateTriggerBindingFor(context.Parameter);
            return Task.FromResult(binding);
        }

        private ITriggerBinding CreateTriggerBindingFor(ParameterInfo parameter)
        {
            var tableAttribute = parameter.GetCustomAttribute<MsSqlTableTriggerAttribute>(inherit: false);
            if (tableAttribute != null)
                return new MsSqlTableTriggerBinding(_configuration, parameter, tableAttribute);

            var attribute = parameter.GetCustomAttribute<MsSqlTriggerAttribute>(inherit: false);
            if (attribute != null)
                return new MsSqlTriggerBinding(_configuration, parameter, attribute);

            return null;
        }
    }
}
