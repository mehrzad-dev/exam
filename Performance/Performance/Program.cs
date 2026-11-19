using System.Collections.Concurrent;
using System.Diagnostics;

namespace Performance;

class Program
{
    static void Main(string[] args)
    {
        var processor = new DataProcessor();
        var data = processor.GenerateData(1000000);

        _ = processor.ProcessData(data.Take(1000).ToList());

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
        var data = new List<int>(count); // pre-allocate capacity
        for (int i = 0; i < count; i++)
        {
            data.Add(i);
        }

        return data;
    }

    public List<int> ProcessData(List<int> data)
    {
        // استفاده از ConcurrentBag برای thread-safety در پردازش موازی
        var resultBag = new ConcurrentBag<int>();

        // پردازش موازی با استفاده از تمام هسته‌های CPU
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

        // تبدیل به لیست و مرتب‌سازی (اختیاری - اگر ترتیب مهم است)
        return resultBag.OrderBy(x => x).ToList();
    }

    private bool IsPrime(int number)
    {
        if (number < 2)
            return false;

        if (number == 2)
            return true;

        if (number % 2 == 0)
            return false;

        int boundary = (int)Math.Sqrt(number);
        for (int i = 3; i <= boundary; i += 2)
        {
            if (number % i == 0) return false;
        }

        return true;
    }
}