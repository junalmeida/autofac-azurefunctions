using System;

namespace SampleAutofacFunction.Services
{
    class Service2 : IService2, IDisposable
    {
        private readonly Guid _id = Guid.NewGuid();

        public Service2()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Creating {this}");
            Console.ResetColor();
        }
        public string Value { get; set; } = "Example injected service 2";

        public void Dispose()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Disposing {this}");
            Console.ResetColor();
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
