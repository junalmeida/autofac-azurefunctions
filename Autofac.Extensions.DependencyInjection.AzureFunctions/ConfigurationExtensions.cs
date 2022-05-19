using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;

namespace Autofac.Extensions.DependencyInjection.AzureFunctions
{
    /// <summary>
    /// Extension methods to initialize <see cref="Autofac" /> for use with the <see cref="IFunctionsHostBuilder"/>
    /// </summary>
    public static class ConfigurationExtensions
    {
        internal const string functionInstanceParam = "functionInstance";
        internal const string iEnvironmentParam = "iEnvironment";

        internal static Type IEnvironmentType;
        /// <summary>
        /// Attatch the <see cref="AutofacServiceProvider"/> to the <see cref="IFunctionsHostBuilder"/> of the current function host.
        /// </summary>
        /// <param name="hostBuilder">An instance of <see cref="IFunctionsHostBuilder"/>.</param>
        /// <param name="configurationAction">Action on a <see cref="ContainerBuilder"/> that adds component registrations to the conatiner.</param>
        /// <returns>The IFunctionsHostBuilder.</returns>
        public static IFunctionsHostBuilder UseAutofacServiceProviderFactory(this IFunctionsHostBuilder hostBuilder, Action<ContainerBuilder> configurationAction = null)
        {
            // Adding as a Singleton service will make sure post-registered
            // services like TelemetryClient will be in the scope.
            hostBuilder.Services.AddSingleton((ctx) =>
            {
                var containerBuilder = new ContainerBuilder();

                containerBuilder.Populate(hostBuilder.Services);
                containerBuilder.RegisterModule<LoggerModule>();

                // Call the user code to configure the container
                configurationAction?.Invoke(containerBuilder);

                IEnvironmentType = hostBuilder.Services.Where(s => s.ServiceType.Namespace == "Microsoft.Azure.WebJobs.Script").FirstOrDefault()?.ServiceType.Assembly.GetExportedTypes().Where(x => x.Name == "IEnvironment").FirstOrDefault();
                if (IEnvironmentType != null)
                {
                    containerBuilder.Register((r, p) =>
                    {
                        var instance = p.Named<object>(iEnvironmentParam);
                        return instance;
                    }).As(IEnvironmentType)
                    .ExternallyOwned()
                    .SingleInstance();
                }

                containerBuilder.Register((r, p) =>
                {
                    var instance = p.Named<IFunctionInstanceEx>(functionInstanceParam);
                    return instance;
                })
                .AsSelf()
                .ExternallyOwned()
                .InstancePerTriggerRequest();

                var container = containerBuilder.Build();
                return new AutofacContainer(container);
            });

            // Replacing Azure Functions ServiceProvider
            hostBuilder.Services.Replace(ServiceDescriptor.Singleton(typeof(IJobActivator), typeof(ScopedJobActivator)));
            hostBuilder.Services.Replace(ServiceDescriptor.Singleton(typeof(IJobActivatorEx), typeof(ScopedJobActivator)));

            // This will create a scoped execution when a function is triggered.
            hostBuilder.Services.AddScoped<ScopedContainer>();

            return hostBuilder;
        }
    }

    internal class AutofacContainer : IDisposable
    {
        public IContainer Container { get; }

        public AutofacContainer(IContainer container)
        {
            Container = container;
        }

        public void Dispose()
        {
            Container.Dispose();
        }
    }

    internal class ScopedContainer : IDisposable
    {
        public ILifetimeScope Scope { get; }
        public ScopedContainer(AutofacContainer container)
        {
            Scope = container.Container.BeginLifetimeScope(Scopes.LifetimeScopeTag);
        }

        public void Dispose()
        {
            Scope.Dispose();
        }
    }
}
