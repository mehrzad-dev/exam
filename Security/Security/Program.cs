using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/echo", async (HttpContext context) =>
{
    var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
    requestBody = Regex.Replace(input, @"[^\w\s\u0600-\u06FF]", string.Empty);
    if (requestBody.length > 500)
        return Results.BadRequest("Message too long.");

    var response = $"You entered: {requestBody}";
    await context.Response.WriteAsync(response);
});

app.Run();