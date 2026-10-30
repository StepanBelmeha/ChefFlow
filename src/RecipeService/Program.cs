using System.Text;
using ChefFlow.Contracts.Users;
using Grpc.Core;
using Grpc.Net.ClientFactory;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RecipeService.Data;

var builder = WebApplication.CreateBuilder(args);

var internalApiKey = builder.Configuration["Services:InternalApiKey"];
if (string.IsNullOrWhiteSpace(internalApiKey)
    || Encoding.UTF8.GetByteCount(internalApiKey) < 32)
{
    throw new InvalidOperationException(
        "Services:InternalApiKey must be configured with at least 32 bytes.");
}

var authGrpcAddress = new Uri(
    builder.Configuration["Services:AuthGrpc"]
    ?? throw new InvalidOperationException("Services:AuthGrpc must be configured."));

if (!builder.Environment.IsDevelopment()
    && authGrpcAddress.Scheme != Uri.UriSchemeHttps)
{
    throw new InvalidOperationException("Services:AuthGrpc must use HTTPS outside Development.");
}

if (builder.Environment.IsDevelopment() && authGrpcAddress.Scheme == Uri.UriSchemeHttp)
    AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

// DB
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<RecipeDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// JWT — той самий ключ що і в AuthService
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Key"]!)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<RecipeService.Services.RecipeService>();
builder.Services.AddControllers();


// Swagger з JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RecipeService API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введіть JWT токен"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services
    .AddGrpcClient<UserDirectory.UserDirectoryClient>(options =>
        options.Address = authGrpcAddress)
    .ConfigureChannel(options =>
        options.UnsafeUseInsecureChannelCallCredentials =
            builder.Environment.IsDevelopment()
            && authGrpcAddress.Scheme == Uri.UriSchemeHttp)
    .AddCallCredentials((_, metadata, _) =>
    {
        metadata.Add("x-internal-api-key", internalApiKey);
        return Task.CompletedTask;
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

// Health endpoint
app.MapGet("/health", async (RecipeDbContext db) =>
{
    try
    {
        await db.Database.CanConnectAsync();
        return Results.Ok(new { status = "Healthy", service = "RecipeService", database = "Connected" });
    }
    catch (Exception ex)
    {
        return Results.Json(
            new { status = "Unhealthy", service = "RecipeService", database = ex.Message },
            statusCode: 503);
    }
}).AllowAnonymous();

app.MapControllers();

app.Run();

public partial class Program { }