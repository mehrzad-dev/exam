using System.Text.Json;

namespace reliable;

public class TaskItem
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsProcessed { get; set; }

    // تغییر: ثبت وضعیت میانی برای شناسایی کارهای نیمه‌تمام پس از توقف برنامه
    public bool IsProcessing { get; set; }
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

        // تغییر: کارهایی که موقع توقف در حال اجرا بوده‌اند دوباره قابل پردازش می‌شوند.
        if (_tasks.Any(t => t.IsProcessing))
        {
            foreach (var task in _tasks)
                task.IsProcessing = false;

            Save();
        }
    }

    // تغییر: شناسه جدید را یکتا انتخاب می‌کنیم تا دو کار اشتباهاً با هم تکمیل نشوند.
    public int NextId() => _tasks.Count == 0 ? 1 : checked(_tasks.Max(t => t.Id) + 1);

    public void AddTask(TaskItem task)
    {
        _tasks.Add(task);
        Save();
    }

    public List<TaskItem> GetUnprocessedTasks()
    {
        return _tasks.FindAll(t => !t.IsProcessed);
    }

    // تغییر: قبل از اجرای واقعی، وضعیت «در حال پردازش» روی دیسک ثبت می‌شود.
    public void MarkProcessing(int id)
    {
        var task = _tasks.Find(t => t.Id == id);
        if (task != null)
        {
            task.IsProcessing = true;
            Save();
        }
    }

    public void MarkProcessed(int id)
    {
        var task = _tasks.Find(t => t.Id == id);
        if (task != null)
        {
            // تغییر: تنها بعد از موفقیت، وضعیت نهایی ثبت و وضعیت میانی پاک می‌شود.
            task.IsProcessed = true;
            task.IsProcessing = false;
            Save();
        }
    }

    private void Save()
    {
        // تغییر: ابتدا JSON را در فایل موقت می‌نویسیم تا فایل اصلی نیمه‌نوشته نشود.
        string tempFile = _filePath + ".tmp";
        File.WriteAllText(tempFile, JsonSerializer.Serialize(_tasks));
        File.Move(tempFile, _filePath, overwrite: true);
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
            // تغییر: قبل از شروع کار، وضعیت آن را پایدار ثبت می‌کنیم.
            _taskManager.MarkProcessing(task.Id);

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
        // تغییر: هم‌زمان فقط یک نمونه برنامه فایل مشترک وظایف را تغییر می‌دهد.
        using var fileLock = new FileStream("tasks.lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

        var manager = new TaskManager();
        if (args.Length > 0 && args[0] == "add")
        {
            manager.AddTask(new TaskItem
            {
                Id = manager.NextId(),
                Description = string.Join(' ', args, 1, args.Length - 1),
                IsProcessed = false
            });
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
            Environment.ExitCode = 1; // تغییر: شکست اجرا به سیستم‌عامل نیز اعلام می‌شود.
        }
    }
}