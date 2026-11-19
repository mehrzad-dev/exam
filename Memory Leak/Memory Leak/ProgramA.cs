namespace Memory_Leak;

class LeakyManager
{
    // تغییر: به‌جای نگهداری اشیای بزرگ در لیست static، فقط تعداد آن‌ها را ثبت می‌کنیم.
    public int TotalAdded { get; private set; }

    public void AddObjects(int count)
    {
        TotalAdded += count;
    }
}

class Program
{
    static void Main(string[] args)
    {
        var manager = new LeakyManager();

        for (int i = 0; i < 1000000; i++)
        {
            // تغییر: دیگر شیء ۱۰ مگابایتی نمی‌سازیم؛ چون داده آن هیچ‌جا استفاده نمی‌شود.
            manager.AddObjects(5);
            Console.WriteLine($"Iteration {i + 1}: Objects added.");
            Thread.Sleep(500);
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}