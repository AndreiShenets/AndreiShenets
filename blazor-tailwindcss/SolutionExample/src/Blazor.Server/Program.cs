using Blazor.Server;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

#if DEBUG
// Tailwind hot reload integration
builder.Services.AddHostedService<TailwindHotReloadService>();
#endif

WebApplication app = builder.Build();

app.UseDeveloperExceptionPage();
app.UseStaticFiles();

app.MapRazorComponents<App>()
    .DisableAntiforgery()
    .AddInteractiveServerRenderMode();

await app.RunAsync();
