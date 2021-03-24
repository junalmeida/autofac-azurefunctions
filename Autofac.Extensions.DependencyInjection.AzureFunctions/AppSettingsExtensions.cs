using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.IO;
using System.Reflection;

namespace Autofac.Extensions.DependencyInjection.AzureFunctions
{
    /// <summary>
    /// Extension methods to initialize support for AppSettings file for use with the <see cref="IFunctionsHostBuilder"/>.
    /// </summary>
    public static class AppSettingsExtensions
    {
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
            if (string.IsNullOrWhiteSpace(environment))
                environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            if (string.IsNullOrWhiteSpace(environment))
                environment = "Development"; // Fallback to Development when none is set.

            return UseAppSettings(hostBuilder, (builder) =>
            {
                builder
                    .SetBasePath(currentDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                    .AddJsonFile("host.json", optional: true, reloadOnChange: true)
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

            using (var temporaryServiceProvider = hostBuilder.Services.BuildServiceProvider())
            {
                var configRoot = temporaryServiceProvider.GetService<IConfiguration>();
                if (configRoot != null)
                {
                    configurationBuilder.AddConfiguration(configRoot);
                }
            }

            configurationAction?.Invoke(configurationBuilder);
            var configuration = configurationBuilder.Build();

            hostBuilder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), configuration));

            return hostBuilder;
        }
    }
}
