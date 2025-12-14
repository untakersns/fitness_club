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
});

builder.Services.AddScoped<TrainingSessionService>();

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
