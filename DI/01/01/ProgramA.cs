using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace _01A;

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

    public IOperation Resolve(string? input)
    {
        return input?.Trim().ToLowerInvariant() switch
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

    public void Process(string? input)
    {
        var op = _resolver.Resolve(input);
        Console.WriteLine((string?)op.Execute());
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                // ثبت جداگانه هر پیاده‌سازی با نوع واقعی خودش
                // تا Resolver بتواند دقیقاً همان کلاس را دریافت کند.
                services.AddTransient<OperationA>();
                services.AddTransient<OperationB>();

                // ثبت کلاس‌های موردنیاز برای انتخاب و اجرای عملیات
                services.AddTransient<OperationResolver>();
                services.AddTransient<Processor>();
            })
            .Build();

        // دریافت Processor و وابستگی‌های آن از DI
        var processor = host.Services.GetRequiredService<Processor>();

        Console.Write("Enter impl: a or b > ");

        // مقدار ورودی ممکن است null باشد.
        string? userInput = Console.ReadLine();

        processor.Process(userInput);
    }
}
