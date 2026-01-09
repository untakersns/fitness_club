using fitness_club.Data;
using fitness_club.Profiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using fitness_club.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Identity & EF
builder.Services.AddDbContext<FCDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Host=localhost;Database=FitnessClubAPI;Username=postgres;Password=root";
    options.UseNpgsql(connectionString);
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
{
    // configure password/options as needed for development
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
    .AddEntityFrameworkStores<FCDbContext>()
    .AddDefaultTokenProviders();

// JWT configuration - centralized key management
string jwtKeyRaw = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured");
byte[] jwtKeyBytes;

// Try to parse as base64, otherwise treat as UTF8 string
try
{
    jwtKeyBytes = Convert.FromBase64String(jwtKeyRaw);
}
catch (FormatException)
{
    jwtKeyBytes = Encoding.UTF8.GetBytes(jwtKeyRaw);
}

// Validate key length (minimum 32 bytes = 256 bits for HS256)
if (jwtKeyBytes.Length < 32)
    throw new InvalidOperationException($"JWT key must be at least 256 bits (32 bytes). Current key has {jwtKeyBytes.Length * 8} bits.");

var signingKey = new SymmetricSecurityKey(jwtKeyBytes);
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "fitness_club";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "fitness_club_client";

// Register signing key as singleton for use in controllers
builder.Services.AddSingleton(signingKey);

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
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey,
        ValidateLifetime = true
    };
});

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

// CORS: разрешаем фронту (берётся из конфигурации или дефолт)
var frontendUrl = builder.Configuration["Frontend:BaseUrl"] ?? "http://localhost:5292";
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(frontendUrl)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
