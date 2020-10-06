using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace SampleAutofacFunction.Functions
{
    public class Function3 : IDisposable
    {
        private readonly ILogger _logger;
        private readonly TelemetryClient _telemetryClient;
        private readonly Guid _id = Guid.NewGuid();
        public void Dispose()
        {
            _logger.LogWarning($"Disposing {this}");
        }
        public Function3(ILogger log, TelemetryClient telemetryClient)
        {
            _logger = log;
            _telemetryClient = telemetryClient;
            _logger.LogWarning($"Creating {this}");
        }

        [FunctionName(nameof(Function3))]
        public async Task Run([TimerTrigger("*/15 * * * * *")] TimerInfo myTimer)
        {
            var dep = new DependencyTelemetry()
            {
                Name = "Dependency Test",
                Type = "Test",
                Data = "C# Timer trigger function executed"
            };
            dep.Start();

            await Task.Delay(3000);

            dep.Stop();
            _telemetryClient.TrackDependency(dep);

            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }

        public override string ToString()
        {
            return $"{_id} ({nameof(Function3)})";
        }
    }
}
