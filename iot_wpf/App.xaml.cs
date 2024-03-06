using System.Data;
using System.Windows;
using System.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using IOTLib;
using System;
using System.Diagnostics;

namespace iot_wpf;
public partial class App : Application
{
    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = Host.CreateDefaultBuilder(e.Args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddLogging(builder =>
                {
                    builder.AddDebug();
                    builder.AddSimpleConsole(options =>
                    {
                        options.SingleLine = true;
                        options.IncludeScopes = true;
                        options.TimestampFormat = "HH:mm:ss.fff ";
                    });
                });
                services.Configure<GPIOConfig>(configuration =>
                {
                    configuration.PinHandlers.Add(GPIOPin.D6, PinHandler);
                });
                services.AddSingleton<MainWindow>();
                services.AddHostedService<GPIOService>();
            })
        .Build();

        var logger = _host.Services.GetRequiredService<ILogger<App>>();
        logger.LogInformation("App Started.");

        _host.Start(); // Start the host in a non-blocking way

        // Resolve MainWindow
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void PinHandler(object? o, GPIOEventArgs e)
    {
        Debug.WriteLine($"I Got it! {e}");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        using (_host)
        {
            _host?.StopAsync(TimeSpan.FromSeconds(5)).Wait();
        }

        base.OnExit(e);
    }
}