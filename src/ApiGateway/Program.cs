using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

app.UseCors();

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/health")
    {
        context.Response.StatusCode = 200;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"status\":\"Healthy\",\"service\":\"ChefFlow Gateway\"}");
        return;
    }

    if (!context.Request.Headers.ContainsKey("X-Correlation-Id"))
        context.Request.Headers["X-Correlation-Id"] = Guid.NewGuid().ToString();

    var start = DateTime.UtcNow;
    var method = context.Request.Method;
    var path = context.Request.Path;
    var correlationId = context.Request.Headers["X-Correlation-Id"].ToString();

    await next();

    var duration = (DateTime.UtcNow - start).TotalMilliseconds;
    Console.WriteLine($"[Gateway] {method} {path} → {context.Response.StatusCode} ({duration:F0}ms) | CorrelationId: {correlationId}");
});

await app.UseOcelot();
await app.RunAsync();