using Microsoft.Azure.WebJobs;

namespace MsSqlWebJobExtensions
{
    public static class MsSqlTriggerExtensions
    {
        public static void UseMsSql(this JobHostConfiguration config)
        {
            var configuration = @"data source=.\SQLEXPRESS;initial catalog=CosmoWeb;integrated security=True;";//;multipleactiveresultsets=True;App=CosmoWeb
            var provider = new MsSqlBindingProvider(configuration);
            config.RegisterBindingExtension(provider);
        }
    }
}
