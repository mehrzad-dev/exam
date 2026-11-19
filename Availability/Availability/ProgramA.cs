using System.Text.Json;

namespace Availability;

public class Program
{
    public static async Task Main(string[] args)
    {
        var tasks = new List<string> { "Task1", "Task2", "Task3", "Task4", "Task5" };
        var manager = new TaskManager(tasks);

        // ابتدا آخرین وضعیت ذخیره‌شده را می‌خوانیم تا کارهای تکمیل‌شده تکرار نشوند.
        manager.LoadState();
        await manager.RunTasksAsync();
    }
}

public class TaskManager
{
    private readonly List<string> _tasks;
    private int _currentIndex;
    private const string StateFile = "state.json";

    public TaskManager(List<string> tasks)
    {
        _tasks = tasks;
        _currentIndex = 0;
    }

    public void LoadState()
    {
        if (File.Exists(StateFile))
        {
            _currentIndex = JsonSerializer.Deserialize<int>(File.ReadAllText(StateFile));

            // تغییر: از ادامه دادن با اندیس نامعتبر جلوگیری می‌کنیم.
            if (_currentIndex < 0 || _currentIndex > _tasks.Count) throw new InvalidDataException("Invalid saved task index.");
        }
    }

    public async Task RunTasksAsync()
    {
        for (; _currentIndex < _tasks.Count; _currentIndex++)
        {
            await ExecuteTaskAsync(_tasks[_currentIndex]);

            // پس از موفقیت هر Task، اندیس وظیفه بعدی ذخیره می‌شود.
            SaveState();
        }

        // تغییر: فایل وضعیت را حذف نمی‌کنیم؛ عدد نهایی نشان می‌دهد همه کارها تمام شده‌اند.
    }

    private async Task ExecuteTaskAsync(string taskName)
    {
        // شبیه‌سازی کار؛ در پروژه واقعی اینجا عملیات اصلی انجام می‌شود.
        await Task.Delay(500);
        Console.WriteLine($"Completed: {taskName}");
    }

    private void SaveState()
    {
        // تغییر: اندیس Task بعدی را ذخیره می‌کنیم، نه Task تکمیل‌شده را.
        string tempFile = StateFile + ".tmp";
        File.WriteAllText(tempFile, JsonSerializer.Serialize(_currentIndex + 1));

        // تغییر: ابتدا فایل موقت روی دیسک کامل می‌شود، سپس جایگزین وضعیت اصلی می‌شود.
        File.Move(tempFile, StateFile, overwrite: true);
    }
}