using Microsoft.Azure.WebJobs;
using System;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("MsSqlWebJobExtensions.Tests")]

namespace MsSqlWebJobExtensions
{
    public static class MsSqlTriggerExtensions
    {
        public static void UseMsSql(this JobHostConfiguration config, string connectionString = "MsSql")
        {
            string configuration;
            if (false) //TODO: "connectionString" can be resolved as a app.config connection name
            {
                throw new NotImplementedException();
            }
            else
            {
                //connectionString is expected to be a proper mssql connection string
                configuration = connectionString;// @"data source=.\SQLEXPRESS;initial catalog=CosmoWeb;integrated security=True;";//;multipleactiveresultsets=True;App=CosmoWeb
            }

            var provider = new MsSqlBindingProvider(configuration);
            config.RegisterBindingExtension(provider);
        }
    }
}
