﻿using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Autofac.Extensions.DependencyInjection.AzureFunctions
{
    /// <summary>
    /// Extension methods to initialize support <see cref="ILogger" /> for use with the <see cref="IFunctionsHostBuilder"/>.
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// Adds logging services to the specified HostBuilder
        /// </summary>
        /// <param name="hostBuilder">An instance of <see cref="IFunctionsHostBuilder"/>.</param>
        /// <param name="configure">The <see cref="ILoggingBuilder"/> configuration delegate.</param>
        /// <returns>The IFunctionsHostBuilder.</returns>
        [Obsolete("This may replace Azure Function host logger settings and disable telemetry. Prefer using host.json `logging` settings or `services.Configure<LoggerFilterOptions>()`.")]
        public static IFunctionsHostBuilder UseLogger(this IFunctionsHostBuilder hostBuilder, Action<ILoggingBuilder, IConfiguration> configure)
        {
            var configuration = hostBuilder.GetContext().Configuration;

            hostBuilder.Services.AddLogging((config) => configure?.Invoke(config, configuration));

            return hostBuilder;
        }
    }
}
