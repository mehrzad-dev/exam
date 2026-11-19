class Counter
{
    private int _value = 0;
    public int Value => _value;

    // قفل مشترک بین تمام فراخوانی‌های Increment روی همین نمونه
    private readonly object _lock = new object();

    public void Increment()
    {
        // این بخش از ابتدا در کد اصلی وجود داشت؛
        // دسترسی همزمان به شمارنده را ایمن می‌کند.
        lock (_lock)
        {
            var temp = _value;

            // شبیه‌سازی تأخیر؛ سایر Threadها پشت قفل منتظر می‌مانند.
            Thread.Sleep(1);

            _value = temp + 1;
        }
    }
}

class DataProcessor
{
    public async Task ProcessAsync(Counter counter, int iterations)
    {
        List<Task> tasks = new List<Task>();

        for (int i = 0; i < iterations; i++)
        {
            // هر عملیات روی ThreadPool صف‌بندی می‌شود.
            // تمام Taskها از یک Counter مشترک استفاده می‌کنند.
            tasks.Add(Task.Run(() => counter.Increment()));
        }

        // منتظر پایان تمام عملیات افزایش شمارنده می‌مانیم.
        await Task.WhenAll(tasks);
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        var counter = new Counter();
        var processor = new DataProcessor();

        // هزار بار افزایش شمارنده با استفاده از Taskهای متعدد
        await processor.ProcessAsync(counter, 1000);

        // بعد از پایان تمام Taskها، مقدار نهایی نمایش داده می‌شود.
        Console.WriteLine($"Final Counter Value: {counter.Value}");
    }
}