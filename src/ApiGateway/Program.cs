using System.Text;
using ApiGateway.Aggregators;
using ApiGateway.Clients;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddCors(o =>
    o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!)),
        ClockSkew = TimeSpan.FromSeconds(30)
    });
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    o.OnRejected = async (ctx, ct) =>
    {
        var retryAfter = 60;
        if (ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out var ra))
            retryAfter = (int)ra.TotalSeconds;

        ctx.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();
        ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        ctx.HttpContext.Response.ContentType = "application/problem+json";
        await ctx.HttpContext.Response.WriteAsJsonAsync(new
        {
            type = "https://httpstatuses.com/429",
            title = "Забагато запитів",
            status = 429,
            detail = $"Перевищено ліміт запитів. Спробуйте через {retryAfter} с."
        }, ct);
    };

    // Глобальний ліміт: авторизований за userId, анонімний за IP
        o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
    {
        var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // Вхід: 5 спроб на хвилину з однієї IP (захист від підбору паролів)
        if (ctx.Request.Path.StartsWithSegments("/api/v1/auth/login"))
            return RateLimitPartition.GetFixedWindowLimiter($"login:{ip}",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                });

        var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAuth = userId != null;
        var key = isAuth ? $"user:{userId}" : $"ip:{ip}";

        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = isAuth ? 100 : 30,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });    o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
    {
        var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // Вхід: 5 спроб на хвилину з однієї IP (захист від підбору паролів)
        if (ctx.Request.Path.StartsWithSegments("/api/v1/auth/login"))
            return RateLimitPartition.GetFixedWindowLimiter($"login:{ip}",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                });

        var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAuth = userId != null;
        var key = isAuth ? $"user:{userId}" : $"ip:{ip}";

        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = isAuth ? 100 : 30,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });
});

builder.Services.AddHttpClient<RecipeClient>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["Services:Recipe"]!);
    c.Timeout = TimeSpan.FromSeconds(2);
});
builder.Services.AddHttpClient<TaskClient>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["Services:Task"]!);
    c.Timeout = TimeSpan.FromSeconds(2);
});
builder.Services.AddScoped<RecipeScreenAggregator>();

var app = builder.Build();

app.UseCors();

// Логування + correlation id + health
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
    var correlationId = context.Request.Headers["X-Correlation-Id"].ToString();

    await next();

    var duration = (DateTime.UtcNow - start).TotalMilliseconds;
    Console.WriteLine($"[Gateway] {context.Request.Method} {context.Request.Path} → {context.Response.StatusCode} ({duration:F0}ms) | CorrelationId: {correlationId}");
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.Use(async (ctx, next) =>
{
    await next();

    var isChange = ctx.Request.Method is "PUT" or "PATCH" or "DELETE"
                   && ctx.Response.StatusCode is >= 200 and < 300;
    if (!isChange) return;

    var path = ctx.Request.Path.Value ?? "";
    var m = System.Text.RegularExpressions.Regex.Match(path, @"^/api/v1/recipe/(\d+)");
    if (m.Success)
    {
        var cache = ctx.RequestServices.GetRequiredService<IMemoryCache>();
        cache.Remove(RecipeClient.Key(int.Parse(m.Groups[1].Value)));
    }
});
app.Use(async (ctx, next) =>
{
    ctx.Request.Headers.Remove("X-User-Id");

    var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (userId != null)
        ctx.Request.Headers["X-User-Id"] = userId;

    await next();
});
app.MapControllers();


app.UseWhen(
    ctx => !ctx.Request.Path.StartsWithSegments("/api/v1/screens"),
    branch => branch.UseOcelot().GetAwaiter().GetResult());

await app.RunAsync();
public partial class Program { }