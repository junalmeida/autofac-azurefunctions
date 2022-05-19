using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace Autofac.Extensions.DependencyInjection.AzureFunctions
{
    internal class ScopedJobActivator : IJobActivator, IJobActivatorEx
    {
        private readonly IServiceProvider _serviceProvider;

        public ScopedJobActivator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public T CreateInstance<T>()
        {
            var scope = _serviceProvider.GetRequiredService<ScopedContainer>().Scope;

            return CreateInstance<T>(scope);
        }

        public T CreateInstance<T>(IFunctionInstanceEx functionInstance)
        {
            var scope = functionInstance.InstanceServices.GetService<ScopedContainer>()?.Scope ?? _serviceProvider.GetRequiredService<ScopedContainer>()?.Scope;
            // Some dependencies of ILoggerFactory are registered after 
            // FunctionsStartup, on a separate ServicesCollection, thus
            // not allowing us to get the ILoggerFactory from Autofac container. 
            // So we are retrieving it from InstanceServices.
            var loggerFactory = functionInstance.InstanceServices.GetService<ILoggerFactory>() ?? scope.Resolve<ILoggerFactory>();
            scope.Resolve<ILoggerFactory>(
                new NamedParameter(LoggerModule.loggerFactoryParam, loggerFactory)
            );

            // This will create the same ILogger of a regular ILogger not using DI.
            // This ILogger is scoped under the function trigger and will be disposed
            // right after the code execution.
            var functionName = functionInstance.FunctionDescriptor.ShortName;
            scope.Resolve<ILogger>(
                new NamedParameter(LoggerModule.functionNameParam, functionName)
            );

            // Add the functionInstanceEx itself to the scope
            scope.Resolve<IFunctionInstanceEx>(
                new NamedParameter(ConfigurationExtensions.functionInstanceParam, functionInstance)
            );
            if (ConfigurationExtensions.IEnvironmentType != null) // Required for TelemetryClient
            {
                var iEnvironment = functionInstance.InstanceServices.GetService(ConfigurationExtensions.IEnvironmentType);
                scope.Resolve(ConfigurationExtensions.IEnvironmentType, new NamedParameter(ConfigurationExtensions.iEnvironmentParam, iEnvironment));
            }
            if (Activity.Current == null)
            {
                var activity = new Activity(functionName);
                Activity.Current = activity.Start();

                scope.CurrentScopeEnding += (sender, e) =>
                {
                    activity.Stop();
                };
            }

            return CreateInstance<T>(scope);
        }

        private T CreateInstance<T>(ILifetimeScope scope)
        {
            return scope.Resolve<T>();
        }
    }
}