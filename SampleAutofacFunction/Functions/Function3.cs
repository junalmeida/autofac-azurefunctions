using Microsoft.ApplicationInsights;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SampleAutofacFunction.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SampleAutofacFunction.Functions
{
    public class Function3 : IDisposable
    {
        private readonly IService1 _service1;
        private readonly ILogger _logger;
        private readonly TelemetryClient telemetry;
        private readonly Guid _id = Guid.NewGuid();

        public Function3(IService1 service1, ILogger logger, TelemetryClient telemetry)
        {
            _service1 = service1;
            _logger = logger;
            this.telemetry = telemetry;
            _logger.LogWarning($"Creating {this}");
        }

        public void Dispose()
        {
            _logger.LogWarning($"Disposing {this}");
        }

        [FunctionName(nameof(Function3))]
        public async Task Run([TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo timer)
        {
            await Task.Delay(2000);
            Activity.Current.AddTag("Test", "Hello World");
            _logger.LogInformation($"C# Timer trigger function processed.");
            telemetry.TrackEvent("C# Timer trigger function processed.");
        }

        public override string ToString()
        {
            return $"{_id} ({nameof(Function1)})";
        }
    }
}
