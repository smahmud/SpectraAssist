# 🧭 System Architecture

This document outlines the high-level architecture of **CortexView**, an enterprise-grade Windows overlay assistant built with Clean Architecture principles. The system implements strict layer isolation, MVVM design patterns, and comprehensive dependency injection for maximum maintainability and testability.

---

## 🏗️ Architectural Overview

CortexView follows Clean Architecture principles with four distinct layers, each with specific responsibilities and dependency rules.

### Dependency Rule

**Inner layers NEVER depend on outer layers**:
- Domain → No dependencies
- Application → Domain only
- Infrastructure → Domain only
- Presentation → Application + Domain (orchestration layer)

### Layer Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                        │
│  (CortexView - WPF, MVVM, Dependency Injection)             │
│  - MainWindow.xaml (23 lines code-behind)                   │
│  - MainViewModel (Commands, Properties, Data Binding)       │
└────────────────────┬────────────────────────────────────────┘
                     │ Depends on ↓
┌────────────────────┴────────────────────────────────────────┐
│                   Application Layer                          │
│  (CortexView.Application - Business Logic Orchestration)    │
│  - AnalysisOrchestrator (Capture → Analyze → Store)        │
│  - ChangeDetectionService (Pixel comparison, OCR)          │
│  - WindowMonitoringService (Timer, Events)                 │
└────────────────────┬────────────────────────────────────────┘
                     │ Depends on ↓
┌────────────────────┴────────────────────────────────────────┐
│                     Domain Layer                             │
│  (CortexView.Domain - Pure Business Logic, Zero Deps)      │
│  - Entities (Persona, AnalysisRequest, AnalysisResponse)   │
│  - Interfaces (IAiAnalysisService, IScreenCaptureService)  │
└─────────────────────────────────────────────────────────────┘
                     ↑ Implemented by
┌─────────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                        │
│  (CortexView.Infrastructure - External Dependencies)        │
│  - AwsBedrockService (AWS SDK)                             │
│  - Win32ScreenCaptureService (Win32 APIs)                  │
│  - LocalStorageService (File I/O)                          │
└─────────────────────────────────────────────────────────────┘
```

---

## 🧩 Layer 1: Domain Layer

**Project**: `CortexView.Domain`  
**Target Framework**: net9.0  
**Dependencies**: None (pure .NET)

### Purpose
Contains pure business logic with zero external dependencies. Defines the core domain model, business rules, and contracts that all other layers depend on.


### Domain Entities

#### `Persona.cs`
Represents an AI assistant persona with behavioral parameters.

```csharp
public sealed class Persona
{
    public string Name { get; init; } = string.Empty;
    public string SystemPrompt { get; init; } = string.Empty;
    public float Temperature { get; init; } = 0.7f;
    public float TopP { get; init; } = 0.9f;
    public int MaxTokens { get; init; } = 1024;

    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(Name) &&
        !string.IsNullOrWhiteSpace(SystemPrompt) &&
        Temperature >= 0 && Temperature <= 1 &&
        TopP >= 0 && TopP <= 1 &&
        MaxTokens > 0;
}
```

**Key Features**:
- Immutable (`init` properties)
- Sealed (prevents inheritance)
- Domain validation (`IsValid()`)
- XML documentation

#### `AnalysisRequest.cs`
Represents a request to analyze a screenshot with AI.

```csharp
public sealed class AnalysisRequest
{
    public required byte[] ImageData { get; init; }
    public required string ImageFormat { get; init; }
    public required string WindowTitle { get; init; }
    public string OcrText { get; init; } = string.Empty;
    public string UserPrompt { get; init; } = string.Empty;
    public float Temperature { get; init; } = 0.7f;
    public float TopP { get; init; } = 0.9f;
    public int MaxTokens { get; init; } = 1024;

    public bool IsValid() =>
        ImageData.Length > 0 &&
        !string.IsNullOrWhiteSpace(ImageFormat) &&
        !string.IsNullOrWhiteSpace(WindowTitle);
}
```

**Key Features**:
- Framework-agnostic (`byte[]` instead of `Bitmap`)
- Required properties (`required` keyword)
- Explicit image format specification
- Domain validation

#### `AnalysisResponse.cs`
Represents the result of an AI analysis operation.

```csharp
public sealed class AnalysisResponse
{
    public required bool IsSuccess { get; init; }
    public string SuggestionText { get; init; } = string.Empty;
    public string ErrorMessage { get; init; } = string.Empty;
    public int TokenUsage { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public static AnalysisResponse Success(string suggestionText, int tokenUsage) =>
        new()
        {
            IsSuccess = true,
            SuggestionText = suggestionText,
            TokenUsage = tokenUsage
        };

    public static AnalysisResponse Failure(string errorMessage) =>
        new()
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
}
```

**Key Features**:
- Factory methods for clean instantiation
- UTC timestamps for consistency
- Immutable design


### Domain Interfaces

#### `IAiAnalysisService.cs`
Contract for AI-powered image analysis services.

```csharp
public interface IAiAnalysisService
{
    Task<AnalysisResponse> AnalyzeImageAsync(
        AnalysisRequest request,
        CancellationToken cancellationToken = default);
}
```

**Key Features**:
- Async-first design
- Cancellation token support
- Uses domain entities only
- Framework-agnostic

#### `IScreenCaptureService.cs`
Contract for screen capture operations.

```csharp
public interface IScreenCaptureService
{
    Task<byte[]> CaptureWindowAsync(
        IntPtr windowHandle,
        CancellationToken cancellationToken = default);

    (int Left, int Top, int Right, int Bottom) GetWindowRect(IntPtr windowHandle);
}
```

**Key Features**:
- Returns `byte[]` (framework-agnostic)
- Tuple return type for coordinates
- Platform-agnostic interface

#### `IStorageService.cs`
Contract for file storage operations.

```csharp
public interface IStorageService
{
    Task<string?> SaveScreenshotAsync(
        byte[] imageData,
        string personaName,
        CancellationToken cancellationToken = default);

    Task CleanupOldFilesAsync(CancellationToken cancellationToken = default);

    Task PurgeAllAsync(CancellationToken cancellationToken = default);
}
```

#### `IChangeDetectionService.cs`
Contract for detecting changes between screenshots.

```csharp
public interface IChangeDetectionService
{
    double ComputeChangedFraction(byte[] currentImageData);

    bool IsSignificantChange(double changedFraction, double sensitivityThreshold);

    string? TryExtractOcrText(byte[] imageData);
}
```

---

## 🧩 Layer 2: Application Layer

**Project**: `CortexView.Application`  
**Target Framework**: net9.0  
**Dependencies**: CortexView.Domain, System.Drawing.Common

### Purpose
Orchestrates business logic workflows by coordinating multiple domain services. Contains no external dependencies except for image processing utilities.

### Components

#### `AnalysisOrchestrator.cs`
Orchestrates the complete capture → analyze → store workflow.

```csharp
public sealed class AnalysisOrchestrator
{
    private readonly IScreenCaptureService _captureService;
    private readonly IChangeDetectionService _changeDetectionService;
    private readonly IAiAnalysisService _aiService;
    private readonly IStorageService _storageService;

    public async Task<AnalysisResponse> CaptureAndAnalyzeAsync(
        IntPtr windowHandle,
        string windowTitle,
        Persona persona,
        double sensitivityThreshold,
        bool forceAnalysis = false,
        CancellationToken cancellationToken = default)
    {
        // 1. Capture screenshot
        byte[] imageData = await _captureService.CaptureWindowAsync(windowHandle, cancellationToken);

        // 2. Check for significant change (unless forced)
        if (!forceAnalysis)
        {
            double changedFraction = _changeDetectionService.ComputeChangedFraction(imageData);
            bool isSignificant = _changeDetectionService.IsSignificantChange(changedFraction, sensitivityThreshold);

            if (!isSignificant)
                return AnalysisResponse.Failure("No significant change detected.");
        }

        // 3. Extract OCR text (optional)
        string? ocrText = _changeDetectionService.TryExtractOcrText(imageData);

        // 4. Build analysis request
        var request = new AnalysisRequest
        {
            ImageData = imageData,
            ImageFormat = "PNG",
            WindowTitle = windowTitle,
            OcrText = ocrText ?? string.Empty,
            UserPrompt = persona.SystemPrompt,
            Temperature = persona.Temperature,
            TopP = persona.TopP,
            MaxTokens = persona.MaxTokens
        };

        // 5. Analyze with AI
        var response = await _aiService.AnalyzeImageAsync(request, cancellationToken);

        // 6. Store screenshot (if successful)
        if (response.IsSuccess)
        {
            await _storageService.SaveScreenshotAsync(imageData, persona.Name, cancellationToken);
        }

        return response;
    }
}
```

**Key Features**:
- Single method orchestrates entire workflow
- Dependency injection for all services
- Async-first design
- `forceAnalysis` parameter for manual captures
- Error handling with graceful degradation

#### `ChangeDetectionService.cs`
Detects changes between screenshots using pixel comparison and hashing.

```csharp
public sealed class ChangeDetectionService : IChangeDetectionService
{
    private byte[]? _previousImageData;
    private string? _previousHash;

    public double ComputeChangedFraction(byte[] currentImageData)
    {
        if (_previousImageData == null)
        {
            _previousImageData = currentImageData;
            _previousHash = ComputeHash(currentImageData);
            return 1.0; // First capture = 100% change
        }

        // Quick hash check first
        string currentHash = ComputeHash(currentImageData);
        if (currentHash == _previousHash)
            return 0.0; // Identical images

        // Pixel-by-pixel comparison
        double changedFraction = ComparePixels(_previousImageData, currentImageData);

        // Update cache
        _previousImageData = currentImageData;
        _previousHash = currentHash;

        return changedFraction;
    }

    private string ComputeHash(byte[] imageData)
    {
        using var sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(imageData);
        return Convert.ToBase64String(hashBytes);
    }

    private double ComparePixels(byte[] previousData, byte[] currentData)
    {
        // Convert to bitmaps and compare pixel-by-pixel
        // Returns fraction of changed pixels (0.0 to 1.0)
    }
}
```

**Key Features**:
- SHA256 hashing for quick comparison
- Pixel-by-pixel comparison for accuracy
- Caches previous image for comparison
- Returns normalized change fraction (0.0 to 1.0)

#### `WindowMonitoringService.cs`
Manages periodic capture timer and events.

```csharp
public sealed class WindowMonitoringService
{
    private readonly DispatcherTimer _timer;

    public event EventHandler? CaptureRequested;

    public void Start()
    {
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
    }

    public void UpdateInterval(TimeSpan interval)
    {
        _timer.Interval = interval;
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        CaptureRequested?.Invoke(this, EventArgs.Empty);
    }
}
```

**Key Features**:
- DispatcherTimer for UI thread synchronization
- Event-based notification pattern
- Configurable interval (1-60 seconds)
- Start/Stop control

---

## 🧩 Layer 3: Infrastructure Layer

**Project**: `CortexView.Infrastructure`  
**Target Framework**: net9.0  
**Dependencies**: CortexView.Domain, AWSSDK.BedrockRuntime, System.Drawing.Common

### Purpose
Implements domain interfaces with external dependencies. Contains all platform-specific code and third-party integrations.

### Components

#### `AwsBedrockService.cs`
AWS Bedrock integration for AI analysis.

```csharp
public sealed class AwsBedrockService : IAiAnalysisService
{
    private readonly AmazonBedrockRuntimeClient _client;
    private readonly AwsConfig _config;

    public AwsBedrockService(IOptions<AwsConfig> config)
    {
        _config = config.Value;
        _client = new AmazonBedrockRuntimeClient(
            Amazon.RegionEndpoint.GetBySystemName(_config.Region));
    }

    public async Task<AnalysisResponse> AnalyzeImageAsync(
        AnalysisRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var invokeRequest = new InvokeModelRequest
            {
                ModelId = _config.ModelId,
                Body = BuildRequestBody(request),
                ContentType = "application/json"
            };

            var response = await _client.InvokeModelAsync(invokeRequest, cancellationToken);
            var result = ParseResponse(response.Body);
            
            return AnalysisResponse.Success(result.Text, result.TokenUsage);
        }
        catch (Exception ex)
        {
            return AnalysisResponse.Failure($"AWS Bedrock error: {ex.Message}");
        }
    }
}
```

**Key Features**:
- IOptions pattern for configuration
- Async-first with cancellation support
- Error handling with graceful degradation
- Returns domain entities

#### `Win32ScreenCaptureService.cs`
Windows-specific screen capture using Win32 APIs.

```csharp
public sealed class Win32ScreenCaptureService : IScreenCaptureService
{
    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    public Task<byte[]> CaptureWindowAsync(
        IntPtr windowHandle,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var rect = GetWindowRect(windowHandle);
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            using var bitmap = new Bitmap(width, height);
            using var graphics = Graphics.FromImage(bitmap);
            
            graphics.CopyFromScreen(rect.Left, rect.Top, 0, 0, bitmap.Size);

            using var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Png);
            return memoryStream.ToArray();
        }, cancellationToken);
    }
}
```

**Key Features**:
- Win32 API integration (DllImport)
- Returns byte[] (framework-agnostic)
- Async with Task.Run
- PNG format output

#### `LocalStorageService.cs`
File storage implementation with retention policies.

```csharp
public sealed class LocalStorageService : IStorageService
{
    private readonly StorageConfig _config;

    public LocalStorageService(IOptions<StorageConfig> config)
    {
        _config = config.Value;
    }

    public async Task<string?> SaveScreenshotAsync(
        byte[] imageData,
        string personaName,
        CancellationToken cancellationToken = default)
    {
        if (!_config.Enabled)
            return null;

        string filename = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{personaName}.png";
        string fullPath = Path.Combine(_config.StoragePath, filename);

        await File.WriteAllBytesAsync(fullPath, imageData, cancellationToken);
        return fullPath;
    }

    public async Task CleanupOldFilesAsync(CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-_config.RetentionDays);
        var files = Directory.GetFiles(_config.StoragePath, "*.png");

        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            if (fileInfo.CreationTimeUtc < cutoffDate)
            {
                File.Delete(file);
            }
        }
    }
}
```

**Key Features**:
- IOptions pattern for configuration
- Configurable retention policies
- Async file I/O
- UTC timestamps for consistency

---

## 🧩 Layer 4: Presentation Layer

**Project**: `CortexView`  
**Target Framework**: net9.0-windows  
**Dependencies**: All layers (orchestration point)

### Purpose
WPF UI with MVVM pattern. Orchestrates all layers via Dependency Injection.

### Components

#### `MainViewModel.cs`
Main window view model with commands and properties.

```csharp
public sealed class MainViewModel : ViewModelBase
{
    private readonly AnalysisOrchestrator _orchestrator;
    private readonly WindowMonitoringService _monitoringService;

    // Observable Properties
    private string _statusText = "Ready";
    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    private bool _isMonitoring;
    public bool IsMonitoring
    {
        get => _isMonitoring;
        set => SetProperty(ref _isMonitoring, value);
    }

    // Commands
    public ICommand CaptureNowCommand { get; }
    public ICommand ToggleMonitoringCommand { get; }

    public MainViewModel(
        AnalysisOrchestrator orchestrator,
        WindowMonitoringService monitoringService)
    {
        _orchestrator = orchestrator;
        _monitoringService = monitoringService;

        CaptureNowCommand = new RelayCommand(_ => CaptureNow(), _ => !IsMonitoring);
        ToggleMonitoringCommand = new RelayCommand(_ => ToggleMonitoring());
    }

    private async void CaptureNow()
    {
        StatusText = "Capturing...";

        var response = await _orchestrator.CaptureAndAnalyzeAsync(
            windowHandle: SelectedWindow.Handle,
            windowTitle: SelectedWindow.Title,
            persona: SelectedPersona,
            sensitivityThreshold: ChangeSensitivity / 100.0,
            forceAnalysis: true);

        if (response.IsSuccess)
        {
            SuggestionText = response.SuggestionText;
            StatusText = $"Analysis complete. Used {response.TokenUsage} tokens.";
        }
        else
        {
            StatusText = $"Error: {response.ErrorMessage}";
        }
    }
}
```

**Key Features**:
- Inherits from ViewModelBase (INotifyPropertyChanged)
- Constructor injection for services
- RelayCommand for ICommand implementation
- Async command handlers
- Data binding properties

#### `ViewModelBase.cs`
Base class for ViewModels with INotifyPropertyChanged.

```csharp
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
```

**Key Features**:
- CallerMemberName for automatic property name detection
- SetProperty helper reduces boilerplate
- ~25 lines of code

#### `RelayCommand.cs`
ICommand implementation for MVVM pattern.

```csharp
public sealed class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    public void Execute(object? parameter) => _execute(parameter);
}
```

**Key Features**:
- Zero dependencies (pure WPF)
- ~25 lines of code
- CommandManager.RequerySuggested for automatic updates

#### `MainWindow.xaml.cs`
Minimal code-behind (23 lines).

```csharp
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        DragMove();
    }
}
```

**Key Features**:
- Constructor injection for ViewModel
- DataContext binding
- Minimal logic (only DragMove)
- 23 lines total (77% reduction from monolithic design)

#### `App.xaml.cs`
Dependency Injection configuration and startup.

```csharp
public partial class App : System.Windows.Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var services = new ServiceCollection();

        // Configuration (IOptions pattern)
        services.Configure<AwsConfig>(configuration.GetSection("AiServiceConfig"));
        services.Configure<StorageConfig>(configuration.GetSection("StorageConfig"));

        // Domain Interfaces → Infrastructure Implementations
        services.AddSingleton<IAiAnalysisService, AwsBedrockService>();
        services.AddSingleton<IScreenCaptureService, Win32ScreenCaptureService>();
        services.AddSingleton<IStorageService, LocalStorageService>();
        services.AddSingleton<IChangeDetectionService, ChangeDetectionService>();

        // Application Services
        services.AddSingleton<AnalysisOrchestrator>();
        services.AddSingleton<WindowMonitoringService>();

        // ViewModels & Views
        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindow>();

        _serviceProvider = services.BuildServiceProvider();

        ValidateServices(_serviceProvider);

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}
```

**Key Features**:
- Microsoft.Extensions.DependencyInjection
- IOptions pattern for configuration
- Service lifetime management (Singleton, Transient)
- Service validation on startup

---

## 🔄 System Workflows

### Capture and Analysis Flow

```
┌─────────────────────────────────────────────────────────────┐
│ 1. User Action (Manual) or Timer (Automatic)                │
└────────────────────┬────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ 2. MainViewModel.CaptureNow() or                            │
│    MonitoringService.CaptureRequested Event                 │
└────────────────────┬────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ 3. AnalysisOrchestrator.CaptureAndAnalyzeAsync()           │
│    ├─→ IScreenCaptureService.CaptureWindowAsync()          │
│    ├─→ IChangeDetectionService.ComputeChangedFraction()    │
│    ├─→ IChangeDetectionService.IsSignificantChange()       │
│    ├─→ IAiAnalysisService.AnalyzeImageAsync()              │
│    └─→ IStorageService.SaveScreenshotAsync()               │
└────────────────────┬────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ 4. AnalysisResponse returned to MainViewModel               │
└────────────────────┬────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ 5. UI updated via data binding                              │
│    (StatusText, SuggestionText properties)                  │
└─────────────────────────────────────────────────────────────┘
```

### Change Detection Algorithm

```
┌─────────────────────────────────────────────────────────────┐
│ 1. Capture current screenshot as byte[]                     │
└────────────────────┬────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ 2. Compute SHA256 hash of image data                        │
└────────────────────┬────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ 3. Compare hash with previous capture                       │
│    ├─→ If identical: Return 0% change (skip analysis)       │
│    └─→ If different: Proceed to pixel comparison            │
└────────────────────┬────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ 4. Pixel-by-pixel comparison                                │
│    ├─→ Convert byte[] to Bitmap                             │
│    ├─→ Compare each pixel (Color equality)                  │
│    └─→ Calculate changed fraction (0.0 to 1.0)              │
└────────────────────┬────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ 5. Compare against sensitivity threshold                    │
│    ├─→ If below threshold: Skip analysis                    │
│    └─→ If above threshold: Trigger AI analysis              │
└─────────────────────────────────────────────────────────────┘
```

### Dependency Injection Flow

```
┌─────────────────────────────────────────────────────────────┐
│ App.OnStartup()                                              │
│ ├─→ Load Configuration (appsettings.json)                   │
│ ├─→ Create ServiceCollection                                │
│ ├─→ Register Services                                       │
│ │   ├─→ Configuration (IOptions)                            │
│ │   ├─→ Domain Interfaces → Infrastructure Implementations  │
│ │   ├─→ Application Services                                │
│ │   └─→ ViewModels & Views                                  │
│ ├─→ Build ServiceProvider                                   │
│ ├─→ Validate Services                                       │
│ └─→ Resolve MainWindow                                      │
└────────────────────┬────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ MainWindow Constructor                                       │
│ ├─→ Inject MainViewModel                                    │
│ └─→ Set DataContext                                         │
└────────────────────┬────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ MainViewModel Constructor                                    │
│ ├─→ Inject AnalysisOrchestrator                            │
│ ├─→ Inject WindowMonitoringService                         │
│ └─→ Initialize Commands                                     │
└────────────────────┬────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ AnalysisOrchestrator Constructor                            │
│ ├─→ Inject IScreenCaptureService                           │
│ ├─→ Inject IChangeDetectionService                         │
│ ├─→ Inject IAiAnalysisService                              │
│ └─→ Inject IStorageService                                  │
└─────────────────────────────────────────────────────────────┘
```

---

## 🎯 SOLID Principles Implementation

### Single Responsibility Principle
- Each class has one reason to change
- `AnalysisOrchestrator`: Orchestrates workflow only
- `ChangeDetectionService`: Detects changes only
- `AwsBedrockService`: AWS integration only

### Open/Closed Principle
- Open for extension via interfaces
- Closed for modification (sealed classes)
- New AI providers can be added without modifying existing code

### Liskov Substitution Principle
- All implementations are substitutable via interfaces
- `AwsBedrockService` and `MockAiService` both implement `IAiAnalysisService`
- Tests use mocks without knowing implementation details

### Interface Segregation Principle
- Small, focused interfaces
- `IAiAnalysisService`: Single method
- `IScreenCaptureService`: Two methods
- No client forced to depend on unused methods

### Dependency Inversion Principle
- High-level modules depend on abstractions
- `AnalysisOrchestrator` depends on interfaces, not concrete implementations
- Infrastructure implementations depend on domain interfaces

---

## 📊 Performance Characteristics

### Build Performance
- **Full Build**: 5.2s (Release configuration)
- **Incremental Build**: <2s
- **Test Execution**: 5.1s (67 tests)

### Runtime Performance
- **DI Resolution**: <10ms (estimated)
- **Capture Time**: 50-100ms (depends on window size)
- **Change Detection**: <50ms (hash comparison)
- **Pixel Comparison**: 100-500ms (depends on image size)
- **AI Analysis**: 2-5s (depends on AWS Bedrock response time)
- **UI Refresh**: <16ms (60 FPS maintained)

### Resource Usage
- **Memory**: ~50-100MB (idle)
- **CPU**: <5% (monitoring mode)
- **Disk**: Configurable (screenshot storage)
- **Network**: Only during AI analysis

---

## 🔒 Security Architecture

### Data Flow Security
- **Screenshots**: Captured in-memory, optionally saved to disk
- **AI Requests**: Sent to AWS Bedrock only when analysis is triggered
- **Audit Logs**: Local storage only, never transmitted
- **Credentials**: Stored in appsettings.json (user-controlled)

### Security Best Practices
- **No Hardcoded Credentials**: All credentials from configuration
- **Async Cancellation**: All operations support cancellation tokens
- **Error Handling**: Graceful degradation on failures
- **Input Validation**: All domain entities validate input
- **Immutable Entities**: Thread-safe by design

---

## 🔮 Future Architecture Enhancements

### Planned for v0.9.0+
- **Plugin Architecture**: Dynamic loading of AI providers
- **Multi-Monitor Support**: Capture from multiple monitors
- **Cloud Storage**: Azure Blob, AWS S3 adapters
- **REST API Layer**: HTTP API for remote control
- **Real-time Observability**: Structured logging, tracing, metrics

### Architectural Considerations
- All enhancements will maintain Clean Architecture principles
- New features will be added as new implementations of existing interfaces
- No breaking changes to domain layer
- Backward compatibility maintained

---

For project structure details, see [project_structure.md](project_structure.md).  
For testing strategy, see [test_strategy.md](test_strategy.md).  
For installation instructions, see [installation-guide.md](installation-guide.md).
