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
                File.WriteAllText(Path.Combine(folderPath, $"file{i}.txt"), $"Content of file {i}");
            }
        }

        var files = Directory.GetFiles(folderPath, "*.txt");
        var content = string.Empty;
        var tasks = new Task[files.Length];
        for (int i = 0; i < files.Length; i++)
        {
            int idx = i;
            tasks[i] = Task.Run(async () =>
            {
                var fileContent = await File.ReadAllTextAsync(files[idx]);
                lock (lockObj)
                {
                    sb.AppendLine(fileContent);
                }
            });
        }

        await Task.WhenAll(tasks);
        Console.WriteLine("Aggregated content:");
        Console.WriteLine(content);
    }
}