class Program
{
    static async Task Main(string[] args)
    {
        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "SampleFiles");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);

            for (int i = 1; i <= 5; i++)
            {
                File.WriteAllText(
                    Path.Combine(folderPath, $"file{i}.txt"),
                    $"Content of file {i}");
            }
        }

        var files = Directory.GetFiles(folderPath, "*.txt");

        // تغییر: ترتیب فایل‌ها را مشخص می‌کنیم تا خروجی در هر اجرا یکسان باشد.
        Array.Sort(files, StringComparer.Ordinal);

        // تغییر: هر خواندن غیرهمگام نتیجه خودش را برمی‌گرداند؛
        // دیگر به lock و داده مشترک نیاز نداریم.
        var tasks = new Task<string>[files.Length];

        for (int i = 0; i < files.Length; i++)
        {
            tasks[i] = File.ReadAllTextAsync(files[i]);
        }

        // تغییر: منتظر پایان همه خواندن‌ها می‌مانیم.
        // نتایج به ترتیب آرایه tasks بازگردانده می‌شوند.
        var contents = await Task.WhenAll(tasks);

        Console.WriteLine("Aggregated content:");

        // تغییر: محتوای فایل‌ها را تجمیع و به‌جای رشته خالی قبلی نمایش می‌دهیم.
        Console.WriteLine(string.Join(Environment.NewLine, contents));
    }
}