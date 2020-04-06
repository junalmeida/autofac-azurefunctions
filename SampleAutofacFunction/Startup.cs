using Autofac;
using Autofac.Extensions.DependencyInjection.AzureFunctions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SampleAutofacFunction.Settings;

[assembly: FunctionsStartup(typeof(SampleAutofacFunction.Startup))]

namespace SampleAutofacFunction
{
    class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder
                .UseAppSettings()
                .UseLogger(ConfigureLogger)
                .UseAutofacServiceProviderFactory(ConfigureContainer);
        }

        private void ConfigureLogger(ILoggingBuilder builder, IConfiguration config)
        {
            builder.AddConfiguration(config.GetSection("Logging"));
        }

        private void ConfigureContainer(ContainerBuilder builder)
        {
            builder
                .Register(activator =>
                {
                    var mySettings = new MySettings();

                    var config = activator.Resolve<IConfiguration>();
                    config.GetSection(nameof(MySettings)).Bind(mySettings);

                    return mySettings;
                })
                .AsSelf()
                .SingleInstance();

            builder
                .RegisterAssemblyTypes(typeof(Startup).Assembly)
                .InNamespace("SampleAutofacFunction.Functions")
                .AsSelf()
                .InstancePerTriggerRequest();

            builder
                .RegisterAssemblyTypes(typeof(Startup).Assembly)
                .InNamespace("SampleAutofacFunction.Services")
                .AsImplementedInterfaces()
                .InstancePerTriggerRequest();


        }
    }
}
