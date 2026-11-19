namespace Reliability
;

public class Program
{
    public static async Task Main(string[] args)
    {
        string inputDirectory = "input";
        string outputDirectory = "output";

        Directory.CreateDirectory(inputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var files = Directory.GetFiles(inputDirectory, "*.txt");
        var tasks = new Task[files.Length];

        int failedCount = 0;

        for (int i = 0; i < files.Length; i++)
        {
            string inputFile = files[i];
            string outputFile = Path.Combine(outputDirectory, Path.GetFileName(inputFile));

            // تغییر: ایجاد مسیر موقت و یکتا در همان پوشه خروجی
            string tempFile = Path.Combine(outputDirectory, $"{Guid.NewGuid():N}.tmp");

            tasks[i] = Task.Run(async () =>
            {
                try
                {
                    var lines = await File.ReadAllLinesAsync(inputFile);
                    var processedLines = ProcessLines(lines);

                    // تغییر: ابتدا اطلاعات کامل در فایل موقت نوشته می‌شود.
                    await File.WriteAllLinesAsync(tempFile, processedLines);

                    // تغییر: فقط پس از موفقیت، خروجی اصلی جایگزین می‌شود.
                    File.Move(tempFile, outputFile, overwrite: true);
                }
                catch (Exception ex)
                {
                    // تغییر: خطای هر فایل جداگانه مدیریت می‌شود.
                    Interlocked.Increment(ref failedCount);
                    Console.Error.WriteLine($"Failed: {Path.GetFileName(inputFile)} - {ex.Message}");
                }
                finally
                {
                    // تغییر: فایل موقت باقی‌مانده پاک می‌شود.
                    // خطای پاک‌سازی نباید خطای اصلی پردازش را پنهان کند.
                    try
                    {
                        File.Delete(tempFile);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(
                            $"Cleanup warning: {Path.GetFileName(tempFile)} - {ex.Message}");
                    }
                }
            });
        }

        await Task.WhenAll(tasks);

        // تغییر: موفقیت و شکست پردازش‌ها جداگانه گزارش می‌شوند.
        if (failedCount == 0)
        {
            Console.WriteLine("Processing complete.");
        }
        else
        {
            Console.WriteLine($"Processing finished with {failedCount} failed file(s).");

            Environment.ExitCode = 1;
        }
    }

    public static string[] ProcessLines(string[] lines)
    {
        var result = new string[lines.Length];

        for (int i = 0; i < lines.Length; i++)
        {
            result[i] = lines[i].ToUpperInvariant();
        }

        return result;
    }
}