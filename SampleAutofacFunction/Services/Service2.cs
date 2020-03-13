using Microsoft.Extensions.Logging;
using System;

namespace SampleAutofacFunction.Services
{
    class Service2 : IService2, IDisposable
    {
        private readonly Guid _id = Guid.NewGuid();
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        public Service2(ILogger logger, ILoggerFactory loggerFactory)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
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
