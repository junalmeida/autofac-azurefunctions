using Microsoft.Extensions.Logging;

namespace Autofac.Extensions.DependencyInjection.AzureFunctions
{
    internal class LoggerModule : Module
    {
        public const string functionNameParam = "functionName";
        public const string loggerFactoryParam = "loggerFactory";

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register((ctx, p) =>
                {
                    var factory = ctx.Resolve<ILoggerFactory>();
                    var functionName = p.Named<string>(functionNameParam);

                    return factory.CreateLogger(Microsoft.Azure.WebJobs.Logging.LogCategories.CreateFunctionUserCategory(functionName));
                })
                .AsSelf()
                .InstancePerTriggerRequest();

            builder
                .Register((ctx, p) =>
                {
                    var factory = p.Named<ILoggerFactory>(loggerFactoryParam) ?? ctx.Resolve<ILoggerFactory>();

                    return factory;
                })
                .AsSelf()
                .SingleInstance();

        }
    }
}