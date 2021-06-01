using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Autofac.Extensions.DependencyInjection.AzureFunctions
{
    internal class LoggerModule : Module
    {
        public const string functionNameParam = "functionName";
        public const string loggerFactoryParam = "loggerFactory";
        public const string telemetryClientParam = "telemetryConfiguration";
        public static readonly Type telemetryType = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !a.IsDynamic)
            .SelectMany(x => x.GetExportedTypes())
            .Where(x => x.FullName == "Microsoft.ApplicationInsights.TelemetryClient")
            .FirstOrDefault();

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
                    var obj = p.Named<object>(telemetryClientParam);

                    return obj;
                })
                .As(telemetryType)
                .SingleInstance();

        }
    }
}