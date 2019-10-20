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

            ParameterInfo parameter = context.Parameter;

            MsSqlTriggerAttribute attribute = parameter.GetCustomAttribute<MsSqlTriggerAttribute>(inherit: false);
            if (attribute == null)
            {
                return Task.FromResult<ITriggerBinding>(null);
            }

            ITriggerBinding binding = new MsSqlTriggerBinding(_configuration, parameter, attribute);

            return Task.FromResult(binding);
        }
    }
}
