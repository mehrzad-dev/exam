using System.Collections.Concurrent;
using System.Diagnostics;

// اصلاح: اضافه کردن فضای نام موردنیاز ConcurrentBag

namespace Performance;

class Program
{
    static void Main(string[] args)
    {
        var processor = new DataProcessor();
        var data = processor.GenerateData(1000000);

        // اجرای اولیه برای کاهش اثر راه‌اندازی روی اندازه‌گیری زمان
        _ = processor.ProcessData(Enumerable.Take<int>(data, 1000).ToList());

        // اندازه‌گیری زمان پردازش یک میلیون عدد
        var stopwatch = Stopwatch.StartNew();
        var result = processor.ProcessData(data);
        stopwatch.Stop();

        Console.WriteLine($"Processed {result.Count} prime numbers in {stopwatch.ElapsedMilliseconds} ms");
        Console.WriteLine($"Using {Environment.ProcessorCount} logical processors");
    }
}

public class DataProcessor
{
    public List<int> GenerateData(int count)
    {
        // تعیین ظرفیت اولیه برای جلوگیری از افزایش مکرر ظرفیت لیست
        var data = new List<int>(count);

        for (int i = 0; i < count; i++)
        {
            data.Add(i);
        }

        return data;
    }

    public List<int> ProcessData(List<int> data)
    {
        // ذخیره امن نتایج هنگام پردازش موازی
        var resultBag = new ConcurrentBag<int>();

        // پردازش هم‌زمان اعداد با محدودیت تعداد پردازنده‌های منطقی
        Parallel.ForEach(data, new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            },
            item =>
            {
                if (IsPrime(item))
                {
                    resultBag.Add(item);
                }
            });

        // مرتب‌سازی خروجی، چون ترتیب اجرای پردازش‌های موازی مشخص نیست
        return resultBag.OrderBy(x => x).ToList();
    }

    private bool IsPrime(int number)
    {
        // اعداد کمتر از ۲ اول نیستند.
        if (number < 2)
            return false;

        // عدد ۲ تنها عدد اول زوج است.
        if (number == 2)
            return true;

        // حذف سریع تمام اعداد زوج دیگر
        if (number % 2 == 0)
            return false;

        // برای تشخیص اول بودن، بررسی مقسوم‌علیه‌ها تا جذر عدد کافی است.
        int boundary = (int)Math.Sqrt(number);

        // فقط مقسوم‌علیه‌های فرد بررسی می‌شوند.
        for (int i = 3; i <= boundary; i += 2)
        {
            if (number % i == 0)
                return false;
        }

        return true;
    }
}