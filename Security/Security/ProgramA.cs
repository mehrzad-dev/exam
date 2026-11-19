using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/echo", async (HttpContext context) =>
{
    // تغییر: فقط ۵۰۱ کاراکتر می‌خوانیم تا ورودی بزرگ باعث مصرف زیاد حافظه نشود.
    using var reader = new StreamReader(context.Request.Body);
    var buffer = new char[501];
    int length = await reader.ReadBlockAsync(buffer.AsMemory(), context.RequestAborted);

    // تغییر: ورودی بیشتر از ۵۰۰ کاراکتر را رد می‌کنیم.
    if (length > 500)
        return Results.BadRequest("Message too long.");

    // تغییر: متن ورودی را بدون حذف کاراکترهای معمول کاربر نگه می‌داریم.
    string requestBody = new string(buffer, 0, length);

    // تغییر: از تشخیص اشتباه نوع محتوا توسط مرورگر جلوگیری می‌کنیم.
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";

    // تغییر: خروجی را متن ساده برمی‌گردانیم تا به‌عنوان HTML اجرا نشود.
    return Results.Text($"You entered: {requestBody}", "text/plain; charset=utf-8");
});

app.Run();