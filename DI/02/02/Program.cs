using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddSingleton<IMessageWriter, ConsoleMessageWriter>();
services.AddSingleton<MessageProcessor>();

using var provider = services.BuildServiceProvider();

var processor = provider.GetService<MessageProcessor>();

if (processor != null)
    processor.Process("Dependency Injection Test");
else
    Console.WriteLine("Processor not found.");


public interface IMessageWriter
{
    void Write(string message);
}

public class ConsoleMessageWriter : IMessageWriter
{
    public void Write(string message)
    {
        Console.WriteLine($"Console: {message}");
    }
}

public class FileMessageWriter : IMessageWriter
{
    public void Write(string message)
    {
        File.AppendAllText("output.txt", $"File: {message}\n");
    }
}

public class MessageProcessor
{
    private readonly IMessageWriter _writer;

    public MessageProcessor(IMessageWriter writer)
    {
        _writer = writer;
    }

    public void Process(string message)
    {
        _writer.Write(message);
    }
}
