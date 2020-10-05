using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Autofac.Extensions.DependencyInjection.AzureFunctions
{
    /// <summary>
    /// Extension methods to initialize <see cref="Autofac" /> for use with the <see cref="IFunctionsHostBuilder"/>
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Attatch the <see cref="AutofacServiceProvider"/> to the <see cref="IFunctionsHostBuilder"/> of the current function host.
        /// </summary>
        /// <param name="hostBuilder">An instance of <see cref="IFunctionsHostBuilder"/>.</param>
        /// <param name="configurationAction">Action on a <see cref="ContainerBuilder"/> that adds component registrations to the conatiner.</param>
        /// <returns>The IFunctionsHostBuilder.</returns>
        public static IFunctionsHostBuilder UseAutofacServiceProviderFactory(this IFunctionsHostBuilder hostBuilder, Action<ContainerBuilder> configurationAction = null)
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Populate(hostBuilder.Services);
            containerBuilder.RegisterModule<LoggerModule>();

            // Call the user code to configure the container
            configurationAction?.Invoke(containerBuilder);

            var container = containerBuilder.Build();

            var scoped = new ScopedJobActivator(new AutofacServiceProvider(container));

            // Replacing Azure Functions ServiceProvider
            hostBuilder.Services.Replace(ServiceDescriptor.Singleton(typeof(IJobActivator), scoped));
            hostBuilder.Services.Replace(ServiceDescriptor.Singleton(typeof(IJobActivatorEx), scoped));

            // This will create a scoped execution when a function is triggered.
            hostBuilder.Services.AddScoped((provider) =>
            {
                var lifetimeScope = container.BeginLifetimeScope(Scopes.RootLifetimeScopeTag);

                return lifetimeScope;
            });

            return hostBuilder;
        }
    }
}
