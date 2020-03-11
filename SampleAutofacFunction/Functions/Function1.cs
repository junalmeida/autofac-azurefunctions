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
        private readonly Guid _id = Guid.NewGuid();

        public Function1(IService1 service1)
        {
            _service1 = service1;

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

        [FunctionName(nameof(Function1))]
        public async Task Run([QueueTrigger("myqueue-items", Connection = "AzureWebJobsStorage")]string myQueueItem, ILogger log)
        {
            await Task.Delay(2000);
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }

        public override string ToString()
        {
            return $"{_id} ({nameof(Function1)})";
        }
    }
}
