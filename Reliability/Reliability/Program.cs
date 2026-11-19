namespace Reliability;

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
        for (int i = 0; i < files.Length; i++)
        {
            string inputFile = files[i];
            string outputFile = Path.Combine(outputDirectory, Path.GetFileName(inputFile));
            tasks[i] = Task.Run(async () =>
            {
                var lines = await File.ReadAllLinesAsync(inputFile);
                var processedLines = ProcessLines(lines);
                await File.WriteAllLinesAsync(outputFile, processedLines);
            });
        }

        await Task.WhenAll(tasks);
        Console.WriteLine("Processing complete.");
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