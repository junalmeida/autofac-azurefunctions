using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;

namespace Autofac.Extensions.DependencyInjection.AzureFunctions
{
    internal class LoggerModule : Module
    {
        public const string functionNameParam = "functionName";
        public const string loggerFactoryParam = "loggerFactory";
        public const string telemetryParam = "telemetry";

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register((ctx, p) =>
                {
                    var factory = p.Named<ILoggerFactory>(loggerFactoryParam);

                    return factory;
                })
                .AsSelf()
                .SingleInstance();

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
                    var client = p.Named<TelemetryClient>(telemetryParam);

                    return client;
                })
                .AsSelf()
                .InstancePerTriggerRequest();

            builder
                .Register((ctx, p) =>
                {
                    var config = p.Named<TelemetryConfiguration>(telemetryParam);

                    return config;
                })
                .AsSelf()
                .InstancePerTriggerRequest();
        }
    }
}