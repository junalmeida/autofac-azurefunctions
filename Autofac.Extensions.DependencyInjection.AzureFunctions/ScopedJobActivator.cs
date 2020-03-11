using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Extensions.DependencyInjection;
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
            var scope = _serviceProvider.GetService<ILifetimeScope>();

            return CreateInstance<T>(scope);
        }

        public T CreateInstance<T>(IFunctionInstanceEx functionInstance)
        {
            var scope = functionInstance.InstanceServices.GetService<ILifetimeScope>();

            return CreateInstance<T>(scope);
        }

        private T CreateInstance<T>(ILifetimeScope scope)
        {

            return scope.Resolve<T>();
        }
    }
}