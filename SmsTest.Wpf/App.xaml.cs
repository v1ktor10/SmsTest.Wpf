using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SmsTest.Wpf.Models;
using SmsTest.Wpf.Services.AppSettings;
using SmsTest.Wpf.Services.CommentStorage;
using SmsTest.Wpf.Services.Environment;
using SmsTest.Wpf.ViewModels;
using SmsTest.Wpf.Views;

namespace SmsTest.Wpf;

public partial class App : Application
{
    private IHost _host = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = BuildHost();
        await _host.StartAsync();

        var window = _host.Services.GetRequiredService<MainWindow>();
        window.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
        await Log.CloseAndFlushAsync();
        base.OnExit(e);
    }
    
    private static IHost BuildHost()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();
        
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        Log.Information("=== Application starting ===");

        return Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureAppConfiguration(cfg =>
            {
                cfg.SetBasePath(AppContext.BaseDirectory);
                cfg.AddJsonFile("appsettings.json", optional: false);
            })
            .ConfigureServices((ctx, services) =>
            {
                services.Configure<AppSettings>(
                    ctx.Configuration.GetSection(AppSettings.SectionName));

                services.AddSingleton<IAppSettingsService, AppSettingsService>();
                services.AddSingleton<IEnvironmentService, EnvironmentService>();
                services.AddSingleton<ICommentStorageService, CommentStorageService>();

                services.AddTransient<MainViewModel>();
                services.AddTransient<MainWindow>();
            })
            .Build();
    }
}