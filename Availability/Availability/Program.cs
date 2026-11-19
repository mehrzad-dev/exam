namespace Availability;

public class Program
{
    public static async Task Main(string[] args)
    {
        var tasks = new List<string> { "Task1", "Task2", "Task3", "Task4", "Task5" };
        var manager = new DotnetTaskRecoveryConsoleApp.TaskManager(tasks);
        await manager.RunTasksAsync();
    }
}