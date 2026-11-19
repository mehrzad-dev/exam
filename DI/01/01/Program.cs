using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace _01;

public interface IOperation
{
    string Execute();
}

public class OperationA : IOperation
{
    public string Execute()
    {
        return "OperationA executed";
    }
}

public class OperationB : IOperation
{
    public string Execute()
    {
        return "OperationB executed";
    }
}

public class OperationResolver
{
    private readonly IServiceProvider _provider;

    public OperationResolver(IServiceProvider provider)
    {
        _provider = provider;
    }

    public IOperation Resolve(string input)
    {
        return input.ToLower() switch
        {
            "a" => _provider.GetRequiredService<OperationA>(),
            "b" => _provider.GetRequiredService<OperationB>(),
            _ => throw new ArgumentException("Invalid input")
        };
    }
}

public class Processor
{
    private readonly OperationResolver _resolver;

    public Processor(OperationResolver resolver)
    {
        _resolver = resolver;
    }

    public void Process(string input)
    {
        var op = _resolver.Resolve(input);
        Console.WriteLine(op.Execute());
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddTransient<OperationA>();
                services.AddTransient<OperationB>();
                services.AddTransient<OperationResolver>();
                services.AddTransient<Processor>();
            })
            .Build();

        var processor = host.Services.GetRequiredService<Processor>();

        Console.Write("Enter impl: a or b > ");
        string userInput = Console.ReadLine();
        processor.Process(userInput);
    }
}