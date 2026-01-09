using fitness_club_front.Components;
using fitness_club_front.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var apiBase = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5285/";
builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri(apiBase);
})
.AddHttpMessageHandler(sp => sp.GetRequiredService<ApiAuthHandler>()) // Добавляем обработчик сообщений для HttpClient "Api"
.AddHttpMessageHandler<LoggingHandler>(); // Добавьте регистрацию логгера-хендлера и используйте его в пайплайне HttpClient

// регистрация AuthService
builder.Services.AddScoped<TrainingSessionService>();
builder.Services.AddScoped<AuthService>();

// register handler and IJSRuntime dependency
builder.Services.AddScoped<ApiAuthHandler>();
builder.Services.AddTransient<LoggingHandler>();

// register TokenStore for per-circuit token storage
builder.Services.AddScoped<TokenStore>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
