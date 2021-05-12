﻿using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

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
        /// <param name="builder">An instance of <see cref="IFunctionsConfigurationBuilder"/>.</param>
        /// <returns>The IFunctionsHostBuilder.</returns>
        public static IFunctionsConfigurationBuilder UseAppSettings(this IFunctionsConfigurationBuilder builder)
        {
            var context = builder.GetContext();
            builder.ConfigurationBuilder
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, "appsettings.json"), optional: true, reloadOnChange: false)
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"appsettings.{builder.GetCurrentEnvironmentName()}.json"), optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();

            return builder;
        }

        /// <summary>
        /// Calculate environment from unreliable function environment setting. 
        /// github.com/Azure/azure-functions-host/issues/6239 problem determining environment.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static string GetCurrentEnvironmentName(this IFunctionsHostBuilder builder)
        {
            return GetCurrentEnvironmentName(builder.GetContext().EnvironmentName);
        }

        /// <summary>
        /// Calculate environment from unreliable function environment setting.
        /// github.com/Azure/azure-functions-host/issues/6239 problem determining environment.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static string GetCurrentEnvironmentName(this IFunctionsConfigurationBuilder builder)
        {
            return GetCurrentEnvironmentName(builder.GetContext().EnvironmentName);
        }

        private static string GetCurrentEnvironmentName(string contextEnvironmentName)
        {

            string environmentName = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ??
                                     Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                                     Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ??
                                     contextEnvironmentName ?? "Development";
            return environmentName;
        }
    }
}
