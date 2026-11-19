namespace ExceptionHandling;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Enter numbers separated by comma:");
        string? input = Console.ReadLine();

        // تغییر: ورودی خالی یا null را قبل از Split بررسی می‌کنیم تا برنامه خطا ندهد.
        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Input cannot be empty.");
            return;
        }

        string[] tokens = input.Split(',');
        List<int> numbers = new List<int>();

        foreach (var token in tokens)
        {
            // تغییر: TryParse به‌جای ایجاد Exception، نامعتبر بودن عدد را اعلام می‌کند.
            // بنابراین متن غیرعددی، مقدار خالی و عدد خارج از محدوده int مدیریت می‌شوند.
            if (!int.TryParse(token.Trim(), out int number))
            {
                Console.WriteLine($"Invalid input: '{token}'. Please enter valid integers.");
                return;
            }

            // عدد صفر مجاز نیست؛ چون مقلوب ریاضی آن تعریف نشده است.
            if (number == 0)
            {
                Console.WriteLine("Zero is not allowed (division by zero).");
                return;
            }

            numbers.Add(number);
        }

        // تغییر: نتیجه اعشاری است؛ نگهداری آن در int بخش کسری را از بین می‌برد.
        double result = CalculateSumOfReciprocals(numbers);
        Console.WriteLine($"Sum of reciprocals: {result}");
    }

    // تغییر: خروجی متد از int به double تغییر کرده تا مقلوب‌ها دقیق‌تر محاسبه شوند.
    public static double CalculateSumOfReciprocals(List<int> numbers)
    {
        double sum = 0;
        foreach (var n in numbers)
        {
            // تغییر: استفاده از 1.0 باعث تقسیم اعشاری به‌جای تقسیم صحیح می‌شود.
            sum += 1.0 / n;
        }

        return sum;
    }
}