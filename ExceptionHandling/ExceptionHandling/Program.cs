namespace ExceptionHandling;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Enter numbers separated by comma:");
        string input = Console.ReadLine();
        string[] tokens = input.Split(',');
        List<int> numbers = new List<int>();
        foreach (var token in tokens)
        {
            try
            {
                int number = int.Parse(token.Trim());
                if (number == 0)
                {
                    Console.WriteLine("Zero is not allowed (division by zero).");
                    return;
                }

                numbers.Add(number);
            }
            catch (FormatException)
            {
                Console.WriteLine($"Invalid input: '{token}'. Please enter only numbers.");
                return;
            }
        }

        int result = CalculateSumOfReciprocals(numbers);
        Console.WriteLine($"Sum of reciprocals: {result}");
    }

    public static int CalculateSumOfReciprocals(List<int> numbers)
    {
        int sum = 0;
        foreach (var n in numbers)
        {
            sum += 1 / n;
        }

        return sum;
    }
}