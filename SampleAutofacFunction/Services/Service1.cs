using Microsoft.Extensions.Logging;
using System;

namespace SampleAutofacFunction.Services
{
    class Service1 : IService1, IDisposable
    {
        private readonly IService2 _service2;
        private readonly ILogger _logger;
        private readonly Guid _id = Guid.NewGuid();

        public Service1(IService2 service2, ILogger logger)
        {
            _service2 = service2;
            _logger = logger;
            _logger.LogWarning($"Creating {this}");
            _logger.LogWarning($"Reading value of {nameof(service2)}: {service2.Value}");

        }
        public string Value { get; set; } = "Example injected service 1";

        public void Dispose()
        {
            _logger.LogWarning($"Disposing {this}");
        }

        public override string ToString()
        {
            return $"{_id} ({nameof(Service1)})";
        }
    }

    public interface IService1
    {
        string Value { get; set; }
    }
}
