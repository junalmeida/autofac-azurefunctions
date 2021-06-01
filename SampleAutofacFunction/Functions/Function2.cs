using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SampleAutofacFunction.Services;
using SampleAutofacFunction.Settings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SampleAutofacFunction.Functions
{
    public class Function2 : IDisposable
    {
        private readonly IService1 _service1;
        private readonly MySettings _settings;
        private readonly ILogger _logger;
        private readonly TelemetryClient _client;
        private readonly Guid _id = Guid.NewGuid();

        public Function2(IService1 service1, MySettings settings, ILogger logger, TelemetryClient client)
        {
            _service1 = service1;
            _settings = settings;
            _logger = logger;
            _client = client;
            _logger.LogWarning($"Creating {this}");
        }

        public void Dispose()
        {
            _logger.LogWarning($"Disposing {this}");
        }

        [FunctionName(nameof(Function2))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string value = _service1.ToString();

            string responseMessage = @$"
This HTTP triggered function executed successfully. 
Injected Service Value: {value}
MySettings Value1: {_settings.Value1}
MySettings Value2: {_settings.Value2}
UrlRequested: {req.Path}
";

            _client.TrackEvent("HTTP Function Event", new Dictionary<string, string>() {
                { "Injected Service Value", value },
                { "MySettings Value1", _settings.Value1 },
                { "MySettings Value2", _settings.Value2 },
                { "UrlRequested", req.Path}
            });


            using (_client.StartOperation(new DependencyTelemetry("HTTP", "Fake HTTP dependency", "Delay", _id.ToString())))
            {
                await Task.Delay(1000);
                // make it fail randomly to test app insights exceptions.
                if (new Random().Next(0, 4) == 1)
                    throw new InvalidOperationException("Random exception to test application insights.");

            }

            _logger.LogInformation(responseMessage);
            return new OkObjectResult(responseMessage);
        }

        public override string ToString()
        {
            return $"{_id} ({nameof(Function2)})";
        }

    }
}
