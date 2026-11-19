using System.Net.Http.Headers;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Enter file URL:");
        var url = Console.ReadLine();
        Console.WriteLine("Enter destination file name:");
        var dest = Console.ReadLine();

        var service = new DownloadService();
        try
        {
            await service.DownloadFileWithResumeAsync(url, dest);
            Console.WriteLine("Download completed (with resume support).");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}

public class DownloadService
{
    private const int BufferSize = 81920;

    public async Task DownloadFileWithResumeAsync(string url, string destination)
    {
        long existingLength = 0;
        if (File.Exists(destination))
        {
            existingLength = new FileInfo(destination).Length;
        }

        using var httpClient = new HttpClient();


        var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (existingLength > 0) request.Headers.Range = new RangeHeaderValue(existingLength, null);

        using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        using var inputStream = await response.Content.ReadAsStreamAsync();
        using var outputStream = new FileStream(destination, FileMode.Append, FileAccess.Write, FileShare.None);

        var buffer = new byte[BufferSize];
        int bytesRead;
        while ((bytesRead = await inputStream.ReadAsync(buffer.AsMemory(0, BufferSize))) > 0)
        {
            await outputStream.WriteAsync(buffer.AsMemory(0, bytesRead));
        }
    }
}