using System.Net;
using System.Net.Http.Headers;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Enter file URL:");
        var url = Console.ReadLine();
        Console.WriteLine("Enter destination file name:");
        var dest = Console.ReadLine();

        // تغییر: جلوگیری از ارسال ورودی خالی به سرویس دانلود
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(dest))
        {
            Console.WriteLine("Error: URL and destination are required.");
            return;
        }

        var service = new DownloadService();

        try
        {
            await service.DownloadFileWithResumeAsync(url, dest);
            Console.WriteLine("Download completed (with resume support).");
        }
        catch (Exception ex)
        {
            // تغییر: اعلام شکست دانلود به سیستم‌عامل
            Console.WriteLine("Error: " + ex.Message);
            Environment.ExitCode = 1;
        }
    }
}

public class DownloadService
{
    private const int BufferSize = 81920;

    public async Task DownloadFileWithResumeAsync(string url, string destination)
    {
        // تغییر: فایل ناقص را جدا از فایل نهایی نگهداری می‌کنیم.
        string partialFile = destination + ".part";

        long existingLength = File.Exists(partialFile) ? new FileInfo(partialFile).Length : 0;

        using var httpClient = new HttpClient();

        // تغییر: امکان درخواست از ابتدا یا ادامه از بایت مشخص
        async Task<HttpResponseMessage> SendRequestAsync(long offset)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            if (offset > 0) request.Headers.Range = new RangeHeaderValue(offset, null);

            return await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        }

        var response = await SendRequestAsync(existingLength);

        // تغییر: مدیریت حالتی که Range درخواستی قابل دریافت نیست.
        if (existingLength > 0 && response.StatusCode == HttpStatusCode.RequestedRangeNotSatisfiable)
        {
            // اگر تعداد بایت‌های قبلی برابر طول فایل باشد، دانلود کامل است.
            if (response.Content.Headers.ContentRange?.Length == existingLength)
            {
                response.Dispose();
                File.Move(partialFile, destination, overwrite: true);
                return;
            }

            // در غیر این صورت، دانلود را دوباره از ابتدا درخواست می‌کنیم.
            response.Dispose();
            existingLength = 0;
            response = await SendRequestAsync(0);
        }

        using (response)
        {
            response.EnsureSuccessStatusCode();

            bool resume = existingLength > 0 && response.StatusCode == HttpStatusCode.PartialContent;

            // تغییر: بررسی اینکه سرور دقیقاً از بایت موردنظر ادامه داده باشد.
            if (resume &&
                (response.Content.Headers.ContentRange?.From != existingLength ||
                 response.Content.Headers.ContentRange?.Length is null))
            {
                throw new InvalidDataException("Invalid Content-Range for resume.");
            }

            // تغییر: فقط پاسخ کامل 200 یا پاسخ ادامه دانلود 206 معتبر است.
            if (!resume && response.StatusCode != HttpStatusCode.OK) throw new InvalidDataException("Unexpected HTTP response status.");

            long? expectedLength = resume ? response.Content.Headers.ContentRange?.Length : response.Content.Headers.ContentLength;

            await using var inputStream = await response.Content.ReadAsStreamAsync();

            await using (var outputStream = new FileStream(
                             partialFile,
                             resume ? FileMode.Append : FileMode.Create,
                             FileAccess.Write,
                             FileShare.None,
                             BufferSize,
                             useAsync: true))
            {
                // تغییر: انتقال مستقیم داده‌ها به دیسک، بدون بارگذاری کل فایل در حافظه
                await inputStream.CopyToAsync(outputStream, BufferSize);
                await outputStream.FlushAsync();
            }

            // تغییر: بررسی کامل بودن دانلود در صورت مشخص بودن اندازه فایل
            if (expectedLength.HasValue && new FileInfo(partialFile).Length != expectedLength.Value)
            {
                throw new IOException("Downloaded file size does not match the expected size.");
            }
        }

        // تغییر: فقط فایل کامل‌شده با نام نهایی منتشر می‌شود.
        File.Move(partialFile, destination, overwrite: true);
    }
}