using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SampleAutofacFunction.Services;
using System;
using System.Threading.Tasks;

namespace SampleAutofacFunction.Functions
{
    public class Function1 : IDisposable
    {
        private readonly IService1 _service1;
        private readonly ILogger _logger;
        private readonly Guid _id = Guid.NewGuid();

        public Function1(IService1 service1, ILogger logger)
        {
            _service1 = service1;
            _logger = logger;
            _logger.LogWarning($"Creating {this}");
        }

        public void Dispose()
        {
            _logger.LogWarning($"Disposing {this}");
        }

        [FunctionName(nameof(Function1))]
        public async Task Run([QueueTrigger("myqueue-items", Connection = "AzureWebJobsStorage")]string myQueueItem)
        {
            await Task.Delay(2000);
            _logger.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }

        public override string ToString()
        {
            return $"{_id} ({nameof(Function1)})";
        }
    }
}
