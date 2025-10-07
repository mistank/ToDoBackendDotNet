using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ToDoBackend.Application.Services;
using ToDoBackend.Infrastructure.Data;
using ToDoBackend.Infrastructure.Repositories;

var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
var parentEnvPath = Path.Combine(Directory.GetCurrentDirectory(), "..", ".env");

if (File.Exists(envPath))
{
    Env.Load(envPath);
    Console.WriteLine($".env loaded from: {envPath}");
}
else if (File.Exists(parentEnvPath))
{
    Env.Load(parentEnvPath);
    Console.WriteLine($".env loaded from: {parentEnvPath}");
}
else
{
    Console.WriteLine("Warning: .env file not found. Using app settings or environment variables.");
}

var builder = WebApplication.CreateBuilder(args);

foreach (System.Collections.DictionaryEntry env in Environment.GetEnvironmentVariables())
{
    var key = env.Key?.ToString();
    var value = env.Value?.ToString();
    if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
    {
        builder.Configuration[key] = value;
    }
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["ConnectionStrings__DefaultConnection"]
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? throw new InvalidOperationException("Database connection string not configured");

Console.WriteLine($"Connection string loaded: {!string.IsNullOrEmpty(connectionString)}");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

var jwtSecret = builder.Configuration["JwtSettings:Secret"]
    ?? builder.Configuration["JwtSettings__Secret"]
    ?? Environment.GetEnvironmentVariable("JwtSettings__Secret")
    ?? throw new InvalidOperationException("JWT Secret not configured");

Console.WriteLine($"JWT Secret loaded: {!string.IsNullOrEmpty(jwtSecret)} (Length: {jwtSecret.Length})");

var jwtIssuer = builder.Configuration["JwtSettings:Issuer"]
    ?? builder.Configuration["JwtSettings__Issuer"]
    ?? Environment.GetEnvironmentVariable("JwtSettings__Issuer")
    ?? "ToDoBackend";

var jwtAudience = builder.Configuration["JwtSettings:Audience"]
    ?? builder.Configuration["JwtSettings__Audience"]
    ?? Environment.GetEnvironmentVariable("JwtSettings__Audience")
    ?? "ToDoBackend";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:4173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ToDoBackend API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
    app.UseSwaggerUI();
}

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
