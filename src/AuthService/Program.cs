using System.Text;
using AuthService.Configuration;
using AuthService.Data;
using AuthService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var internalApiKey = builder.Configuration["Services:InternalApiKey"];
if (string.IsNullOrWhiteSpace(internalApiKey)
    || System.Text.Encoding.UTF8.GetByteCount(internalApiKey) < 32)
{
    throw new InvalidOperationException(
        "Services:InternalApiKey must be configured with at least 32 bytes.");
}

if (!builder.Environment.IsDevelopment())
{
    var grpcEndpointUrl = builder.Configuration["Kestrel:Endpoints:Grpc:Url"];
    var grpcProtocols = builder.Configuration["Kestrel:Endpoints:Grpc:Protocols"];
    if (!Uri.TryCreate(grpcEndpointUrl, UriKind.Absolute, out var grpcEndpoint)
        || grpcEndpoint.Scheme != Uri.UriSchemeHttps
        || grpcProtocols is not ("Http2" or "Http1AndHttp2"))
    {
        throw new InvalidOperationException(
            "Configure the gRPC Kestrel endpoint with HTTPS and HTTP/2 outside Development.");
    }
}

// DB
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// JWT конфігурація
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

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

// Сервіси
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthService.Services.AuthService>();

builder.Services.AddControllers();
builder.Services.AddGrpc();

// Swagger з підтримкою JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AuthService API", Version = "v1" });
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AuthService API v1");
    });
}

app.UseAuthentication();
app.UseAuthorization();

// Health endpoint
app.MapGet("/health", async (AuthDbContext db) =>
{
    try
    {
        await db.Database.CanConnectAsync();
        return Results.Ok(new { status = "Healthy", service = "AuthService", database = "Connected" });
    }
    catch (Exception ex)
    {
        return Results.Json(
            new { status = "Unhealthy", service = "AuthService", database = ex.Message },
            statusCode: 503);
    }
}).AllowAnonymous();

app.MapControllers();
app.MapGrpcService<UserDirectoryGrpcService>();


app.Run();