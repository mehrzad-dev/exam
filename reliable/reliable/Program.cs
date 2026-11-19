using System.Text.Json;

namespace reliable;

public class TaskItem
{
    public int Id { get; set; }
    public string Description { get; set; }
    public bool IsProcessed { get; set; }
}

public class TaskManager
{
    private readonly string _filePath = "tasks.json";
    private List<TaskItem> _tasks;

    public TaskManager()
    {
        if (File.Exists(_filePath))
        {
            var json = File.ReadAllText(_filePath);
            _tasks = JsonSerializer.Deserialize<List<TaskItem>>(json) ?? new List<TaskItem>();
        }
        else
        {
            _tasks = new List<TaskItem>();
        }
    }

    public void AddTask(TaskItem task)
    {
        _tasks.Add(task);
        Save();
    }

    public List<TaskItem> GetUnprocessedTasks()
    {
        return _tasks.FindAll(t => !t.IsProcessed);
    }

    public void MarkProcessed(int id)
    {
        var task = _tasks.Find(t => t.Id == id);
        if (task != null)
        {
            task.IsProcessed = true;
            Save();
        }
    }

    private void Save()
    {
        var json = JsonSerializer.Serialize(_tasks);
        File.WriteAllText(_filePath, json);
    }
}

public class TaskProcessor
{
    private readonly TaskManager _taskManager;

    public TaskProcessor(TaskManager taskManager)
    {
        _taskManager = taskManager;
    }

    public void ProcessTasks()
    {
        var tasks = _taskManager.GetUnprocessedTasks();
        foreach (var task in tasks)
        {
            Console.WriteLine($"Processing Task {task.Id}: {task.Description}");
            if (task.Description.Contains("fail", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Simulated failure");
            }

            _taskManager.MarkProcessed(task.Id);
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        var manager = new TaskManager();
        if (args.Length > 0 && args[0] == "add")
        {
            manager.AddTask(new TaskItem { Id = Guid.NewGuid().GetHashCode(), Description = string.Join(' ', args, 1, args.Length - 1), IsProcessed = false });
            Console.WriteLine("Task added.");
            return;
        }

        var processor = new TaskProcessor(manager);
        try
        {
            processor.ProcessTasks();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}