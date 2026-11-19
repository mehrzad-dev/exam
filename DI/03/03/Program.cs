using Microsoft.Extensions.DependencyInjection;


var services = new ServiceCollection();

// ثبت با کلید مشخص
services.AddKeyedSingleton<IMessageWriter, ConsoleMessageWriter>("console");
services.AddKeyedSingleton<IMessageWriter, FileMessageWriter>("file");
services.AddSingleton<MessageProcessor>();

using var provider = services.BuildServiceProvider();

var processor = provider.GetRequiredService<MessageProcessor>();

processor.ProcessToConsole("Test to Console");
processor.ProcessToFile("Test to File");
processor.ProcessToBoth("Test to Both");

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
    private readonly IMessageWriter _consoleWriter;
    private readonly IMessageWriter _fileWriter;

    public MessageProcessor([FromKeyedServices("console")] IMessageWriter consoleWriter, [FromKeyedServices("file")] IMessageWriter fileWriter)
    {
        _consoleWriter = consoleWriter;
        _fileWriter = fileWriter;
    }

    public void ProcessToConsole(string message)
    {
        _consoleWriter.Write(message);
    }

    public void ProcessToFile(string message)
    {
        _fileWriter.Write(message);
    }

    public void ProcessToBoth(string message)
    {
        _consoleWriter.Write(message);
        _fileWriter.Write(message);
    }
}