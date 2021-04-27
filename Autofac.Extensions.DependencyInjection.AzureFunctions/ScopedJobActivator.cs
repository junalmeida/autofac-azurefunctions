using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

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
            // FunctionsStartup, thus not allowing us to get the 
            // ILoggerFactory from Autofac container. 
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

            return CreateInstance<T>(scope);
        }

        private T CreateInstance<T>(ILifetimeScope scope)
        {
            return scope.Resolve<T>();
        }
    }
}