using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Timeout;
using Polly.CircuitBreaker;

// برای هر سرویس Policy مستقل ایجاد می‌کنیم تا خرابی یکی روی دیگری تأثیر نگذارد.
var primary = CreatePolicies("Primary");
var backup = CreatePolicies("Backup");

for (int round = 1; round <= 10; round++)
{
    Console.WriteLine($"\n--- Request {round} ---");

    // ابتدا سرویس اصلی را فراخوانی می‌کنیم.
    bool ready = await TryCallAsync("Primary", primary.Policy, round);

    // اگر سرویس اصلی در دسترس نبود، از سرویس جایگزین استفاده می‌کنیم.
    if (!ready)
        ready = await TryCallAsync("Backup", backup.Policy, round);

    // Health Check: برنامه زنده است و آمادگی آن بر اساس موفقیت درخواست مشخص می‌شود.
    Console.WriteLine("Liveness: Healthy");
    Console.WriteLine($"Readiness: {(ready ? "Healthy" : "Unhealthy")}");

    Console.WriteLine($"Primary Circuit: {primary.Breaker.CircuitState}");
    Console.WriteLine($"Backup Circuit: {backup.Breaker.CircuitState}");

    // فاصله بین درخواست‌ها برای مشاهده بازیابی خودکار سرویس
    await Task.Delay(1000);
}

static (IAsyncPolicy Policy, AsyncCircuitBreakerPolicy Breaker)
    CreatePolicies(string name)
{
    // Timeout: هر تلاش حداکثر یک ثانیه فرصت دارد.
    var timeout = Policy.TimeoutAsync(1, TimeoutStrategy.Optimistic);

    // Retry: در صورت خطای موقت یا Timeout، حداکثر دو بار تلاش مجدد انجام می‌شود.
    var retry = Policy
        .Handle<HttpRequestException>()
        .Or<TimeoutRejectedException>()
        .WaitAndRetryAsync(2,
            _ => TimeSpan.FromMilliseconds(250),
            (ex, delay, attempt, context) =>
                Console.WriteLine($"{name} Retry {attempt}: {ex.GetType().Name}"));

    // Circuit Breaker: بعد از سه خطای متوالی، درخواست‌ها پنج ثانیه متوقف می‌شوند.
    var breaker = Policy
        .Handle<HttpRequestException>()
        .Or<TimeoutRejectedException>()
        .CircuitBreakerAsync(
            3,
            TimeSpan.FromSeconds(5),
            onBreak: (ex, delay) => Console.WriteLine($"{name}: Circuit Open"),
            onReset: () => Console.WriteLine($"{name}: Circuit Closed"),
            onHalfOpen: () => Console.WriteLine($"{name}: Circuit Half-Open"));

    // ترکیب سیاست‌ها؛ Timeout روی هر تلاش و Circuit Breaker روی خطاهای هر تلاش اعمال می‌شود.
    return (Policy.WrapAsync(retry, breaker, timeout), breaker);
}

static async Task<bool> TryCallAsync(
    string name, IAsyncPolicy policy, int round)
{
    int attempt = 0;

    try
    {
        // اجرای سرویس تحت تمام سیاست‌های تعریف‌شده
        await policy.ExecuteAsync(
            ct => CallServiceAsync(name, round, ++attempt, ct),
            CancellationToken.None);

        Console.WriteLine($"{name}: Success");
        return true;
    }
    catch (Exception ex) when (
        ex is HttpRequestException or
        TimeoutRejectedException or
        BrokenCircuitException)
    {
        // مدیریت خطا بدون متوقف شدن کل برنامه
        Console.WriteLine($"{name}: {ex.GetType().Name}");
        return false;
    }
}

static async Task CallServiceAsync(
    string name, int round, int attempt, CancellationToken ct)
{
    // شبیه‌سازی خرابی سرویس اصلی در چهار درخواست اول
    if (name == "Primary" && round <= 4)
    {
        if (attempt == 1)
            throw new HttpRequestException("Service unavailable");

        // شبیه‌سازی کندی سرویس؛ CancellationToken امکان لغو عملیات را فراهم می‌کند.
        await Task.Delay(3000, ct);
    }

    // شبیه‌سازی پاسخ موفق سرویس
    await Task.Delay(100, ct);
}