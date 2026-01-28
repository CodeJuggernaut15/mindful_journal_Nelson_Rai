using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using MindfulJournal.Services;

namespace MindfulJournal;

/// <summary>
/// The entry point for the .NET MAUI application.
/// Configures services, logging, and the middleware pipeline.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Creates and configures the <see cref="MauiApp"/> instance.
    /// </summary>
    /// <returns>The configured <see cref="MauiApp"/>.</returns>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Register Blazor WebView services
        builder.Services.AddMauiBlazorWebView();

        // Register MudBlazor services
        builder.Services.AddMudServices();

        // Register application services
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddScoped<JournalService>();
        builder.Services.AddScoped<ExportService>();
        builder.Services.AddScoped<PdfExportService>();
        builder.Services.AddSingleton<ThemeService>();
        builder.Services.AddSingleton<SecurityService>();
        builder.Services.AddScoped<TagService>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // Initialize services asynchronously on startup
        Task.Run(async () =>
        {
            var dbService = app.Services.GetRequiredService<DatabaseService>();
            await dbService.InitializeAsync();
            
            var themeService = app.Services.GetRequiredService<ThemeService>();
            await themeService.InitializeAsync();
            
            var securityService = app.Services.GetRequiredService<SecurityService>();
            await securityService.InitializeAsync();

            var tagService = app.Services.GetRequiredService<TagService>();
            await tagService.InitializeAsync();
        }).Wait();

        return app;
    }
}
