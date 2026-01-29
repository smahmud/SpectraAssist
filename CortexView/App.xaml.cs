using System;
using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CortexView.Application.Services;
using CortexView.Domain.Interfaces;
using CortexView.Infrastructure.AI;
using CortexView.Infrastructure.Capture;
using CortexView.Infrastructure.Storage;
using CortexView.Infrastructure.Configuration;
using CortexView.Services;
using CortexView.ViewModels;

namespace CortexView;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Build Configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Configure Services
        var services = new ServiceCollection();

        // Configuration (IOptions pattern)
        services.Configure<AwsConfig>(configuration.GetSection("AiServiceConfig"));
        services.Configure<StorageConfig>(configuration.GetSection("StorageConfig"));

        // Domain Interfaces → Infrastructure Implementations
        services.AddSingleton<Domain.Interfaces.IAiAnalysisService, Infrastructure.AI.AwsBedrockService>();
        services.AddSingleton<IScreenCaptureService, Win32ScreenCaptureService>();
        services.AddSingleton<Domain.Interfaces.IStorageService, Infrastructure.Storage.LocalStorageService>();
        services.AddSingleton<IChangeDetectionService, ChangeDetectionService>();

        // Application Services
        services.AddSingleton<AnalysisOrchestrator>();
        services.AddSingleton<WindowMonitoringService>();
        services.AddSingleton<PromptService>();

        // ViewModels
        services.AddTransient<MainViewModel>();

        // Views
        services.AddTransient<MainWindow>();

        _serviceProvider = services.BuildServiceProvider();

        // Validate DI Container
        ValidateServices(_serviceProvider);

        // Show Main Window
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ValidateServices(ServiceProvider provider)
    {
        try
        {
            // Ensure all services can be resolved
            provider.GetRequiredService<Domain.Interfaces.IAiAnalysisService>();
            provider.GetRequiredService<IScreenCaptureService>();
            provider.GetRequiredService<Domain.Interfaces.IStorageService>();
            provider.GetRequiredService<IChangeDetectionService>();
            provider.GetRequiredService<AnalysisOrchestrator>();
            provider.GetRequiredService<WindowMonitoringService>();
            provider.GetRequiredService<PromptService>();
            provider.GetRequiredService<MainViewModel>();
            provider.GetRequiredService<MainWindow>();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Dependency Injection Error: {ex.Message}\n\nApplication will now exit.",
                "Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            System.Windows.Application.Current.Shutdown(1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}

