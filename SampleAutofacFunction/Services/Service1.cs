using System;

namespace SampleAutofacFunction.Services
{
    class Service1 : IService1, IDisposable
    {
        private readonly IService2 _service2;
        private readonly Guid _id = Guid.NewGuid();

        public Service1(IService2 service2)
        {
            _service2 = service2;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Creating {this}");
            Console.WriteLine($"Reading value of {nameof(service2)}: {service2.Value}");
            Console.ResetColor();

        }
        public string Value { get; set; } = "Example injected service 1";

        public void Dispose()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Disposing {this}");
            Console.ResetColor();
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
