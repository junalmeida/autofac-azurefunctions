using Autofac;
using Autofac.Extensions.DependencyInjection.AzureFunctions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using SampleAutofacFunction.Settings;

[assembly: FunctionsStartup(typeof(SampleAutofacFunction.Startup))]

namespace SampleAutofacFunction
{
    class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder
                // You can call UseLogger to change how logging is done on your app by code. 
                .UseLogger(ConfigureLogger)
                // This is the required call in order to use autofac in your azure functions app
                .UseAutofacServiceProviderFactory(ConfigureContainer);
        }

        private void ConfigureLogger(ILoggingBuilder builder, IConfiguration configuration)
        {
            builder.AddConfiguration(configuration.GetSection("Logging"));
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            // this is optional and will bind IConfiguration with appsettings.json in
            // the container, like it is usually done in regular dotnet console and
            // web applications.
            builder.UseAppSettings(true);
        }

        private void ConfigureContainer(ContainerBuilder builder)
        {
            builder
                .Register(activator =>
                {
                    // Example on how to bind settings from appsettings.json
                    // to a class instance
                    var section = activator.Resolve<IConfiguration>().GetSection(nameof(MySettings));

                    var instance = section.Get<MySettings>();

                    // If you expect IConfiguration to change (with reloadOnChange=true), use
                    // token to rebind.
                    ChangeToken.OnChange(
                       () => section.GetReloadToken(),
                       (state) => section.Bind(state),
                       instance);

                    return instance;
                })
                .AsSelf()
                .SingleInstance();

            // Register all functions that resides in a given namespace
            // The function class itself will be created using autofac
            builder
                .RegisterAssemblyTypes(typeof(Startup).Assembly)
                .InNamespace("SampleAutofacFunction.Functions")
                .AsSelf() // Azure Functions core code resolves a function class by itself.
                .InstancePerTriggerRequest(); // This will scope nested dependencies to each function execution

            builder
                .RegisterAssemblyTypes(typeof(Startup).Assembly)
                .InNamespace("SampleAutofacFunction.Services")
                .AsImplementedInterfaces()
                .InstancePerTriggerRequest();

        }
    }
}
