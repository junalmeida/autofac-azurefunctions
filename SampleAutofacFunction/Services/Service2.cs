using Microsoft.Extensions.Logging;
using System;

namespace SampleAutofacFunction.Services
{
    class Service2 : IService2, IDisposable
    {
        private readonly Guid _id = Guid.NewGuid();
        private readonly ILogger _logger;

        public Service2(ILogger logger)
        {
            _logger = logger;

            _logger.LogWarning($"Creating {this}");
        }
        public string Value { get; set; } = "Example injected service 2";

        public void Dispose()
        {
            _logger.LogWarning($"Disposing {this}");
        }

        public override string ToString()
        {
            return $"{_id} ({nameof(Service2)})";
        }

    }

    public interface IService2
    {
        string Value { get; set; }

    }
}
