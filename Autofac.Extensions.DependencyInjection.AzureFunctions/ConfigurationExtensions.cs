using Autofac.Builder;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Autofac.Extensions.DependencyInjection.AzureFunctions
{
    /// <summary>
    /// Extension methods for use with the <see cref="IFunctionsHostBuilder"/>
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

        /// <summary>
        /// Creates an <see cref="IConfiguration"/> instance and attaches to an `appsettings.json` file, also adding an `appsettings.{environment}.json` on top of it, if available, based on current ASPNETCORE_ENVIRONMENT environment variable.
        /// </summary>
        /// <param name="hostBuilder">An instance of <see cref="IFunctionsHostBuilder"/>.</param>
        /// <returns>The IFunctionsHostBuilder.</returns>
        public static IFunctionsHostBuilder UseAppSettings(this IFunctionsHostBuilder hostBuilder)
        {
            var fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string currentDirectory = fileInfo.Directory.Parent.FullName;

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.IsNullOrWhiteSpace(environment)) environment = "Development";

            return UseAppSettings(hostBuilder, (builder) =>
            {
                builder
                    .SetBasePath(currentDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables();
            });
        }

        /// <summary>
        /// Creates an <see cref="IConfiguration"/> instance and configures it as declared by <paramref name="configurationAction"/> action.
        /// </summary>
        /// <param name="hostBuilder">An instance of <see cref="IFunctionsHostBuilder"/>.</param>
        /// <param name="configurationAction">Action on a <see cref="IConfigurationBuilder"/> that adds configurations to a .NET Core application.</param>
        /// <returns>The IFunctionsHostBuilder.</returns>
        public static IFunctionsHostBuilder UseAppSettings(this IFunctionsHostBuilder hostBuilder, Action<IConfigurationBuilder> configurationAction = null)
        {
            var configurationBuilder = new ConfigurationBuilder() as IConfigurationBuilder;

            var descriptor = hostBuilder.Services.FirstOrDefault(d => d.ServiceType == typeof(IConfiguration));
            if (descriptor?.ImplementationInstance is IConfiguration configRoot)
            {
                configurationBuilder.AddConfiguration(configRoot);
            }

            configurationAction?.Invoke(configurationBuilder);
            var configuration = configurationBuilder.Build();

            hostBuilder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), configuration));

            return hostBuilder;
        }

        /// <summary>
        /// Share one instance of the component within the context of a single
        /// azure function trigger request.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">The registration to configure.</param>
        /// <param name="lifetimeScopeTags">Additional tags applied for matching lifetime scopes.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="registration" /> is <see langword="null" />.
        /// </exception>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle> InstancePerTriggerRequest<TLimit, TActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration, params object[] lifetimeScopeTags)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));

            var tags = new[] { Scopes.RootLifetimeScopeTag }.Concat(lifetimeScopeTags).ToArray();
            return registration.InstancePerMatchingLifetimeScope(tags);
        }

    }
}
