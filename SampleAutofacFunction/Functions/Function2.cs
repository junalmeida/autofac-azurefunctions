using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SampleAutofacFunction.Services;
using SampleAutofacFunction.Settings;
using System;
using System.Threading.Tasks;

namespace SampleAutofacFunction.Functions
{
    public class Function2 : IDisposable
    {
        private readonly IService1 _service1;
        private readonly MySettings _settings;
        private readonly Guid _id = Guid.NewGuid();

        public Function2(IService1 service1, MySettings settings)
        {
            _service1 = service1;
            _settings = settings;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Creating {this}");
            Console.ResetColor();
        }

        public void Dispose()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Disposing {this}");
            Console.ResetColor();
        }

        [FunctionName(nameof(Function2))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string value = _service1.ToString();

            string responseMessage = @$"
This HTTP triggered function executed successfully. 
Injected Service Value: {value}
MySettings Value1: {_settings.Value1}
MySettings Value2: {_settings.Value2}
UrlRequested: {req.Path}
";

            await Task.Delay(1000);
            log.LogInformation(responseMessage);
            return new OkObjectResult(responseMessage);
        }

        public override string ToString()
        {
            return $"{_id} ({nameof(Function2)})";
        }

    }
}
