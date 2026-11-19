namespace Availability;

public class TaskManager
{
    private readonly List<string> _tasks;
    private int _currentIndex;

    public TaskManager(List<string> tasks)
    {
        _tasks = tasks;
        _currentIndex = 0;
    }

    public async Task RunTasksAsync()
    {
        for (; _currentIndex < _tasks.Count; _currentIndex++)
        {
            await ExecuteTaskAsync(_tasks[_currentIndex]);
        }
    }

    private Task ExecuteTaskAsync(string taskName)
    {
        return Task.Delay(500);
    }
}