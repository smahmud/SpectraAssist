using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CortexView.Application.Services;
using CortexView.Domain.Entities;
using CortexView.Domain.Interfaces;
using CortexView.Services;

namespace CortexView.ViewModels;

/// <summary>
/// ViewModel for the main window.
/// </summary>
public sealed class MainViewModel : ViewModelBase
{
    private readonly AnalysisOrchestrator _orchestrator;
    private readonly WindowMonitoringService _monitoringService;
    private readonly PromptService _promptService;

    // Observable Properties
    private string _statusText = "Ready";
    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    private string _suggestionText = string.Empty;
    public string SuggestionText
    {
        get => _suggestionText;
        set => SetProperty(ref _suggestionText, value);
    }

    private bool _isMonitoring;
    public bool IsMonitoring
    {
        get => _isMonitoring;
        set
        {
            if (SetProperty(ref _isMonitoring, value))
            {
                if (value)
                {
                    _monitoringService.Start();
                    StatusText = "Monitoring started...";
                }
                else
                {
                    _monitoringService.Stop();
                    StatusText = "Monitoring stopped.";
                }
            }
        }
    }

    private double _captureInterval = 5.0;
    public double CaptureInterval
    {
        get => _captureInterval;
        set
        {
            if (SetProperty(ref _captureInterval, value))
            {
                _monitoringService.UpdateInterval(TimeSpan.FromSeconds(value));
            }
        }
    }

    private double _changeSensitivity = 10.0;
    public double ChangeSensitivity
    {
        get => _changeSensitivity;
        set => SetProperty(ref _changeSensitivity, value);
    }

    private double _opacity = 0.95;
    public double Opacity
    {
        get => _opacity;
        set => SetProperty(ref _opacity, value);
    }

    private bool _isTopmost;
    public bool IsTopmost
    {
        get => _isTopmost;
        set => SetProperty(ref _isTopmost, value);
    }

    private Persona? _selectedPersona;
    public Persona? SelectedPersona
    {
        get => _selectedPersona;
        set => SetProperty(ref _selectedPersona, value);
    }

    private WindowInfo? _selectedWindow;
    public WindowInfo? SelectedWindow
    {
        get => _selectedWindow;
        set => SetProperty(ref _selectedWindow, value);
    }

    // Storage Settings
    private bool _storageEnabled;
    public bool StorageEnabled
    {
        get => _storageEnabled;
        set => SetProperty(ref _storageEnabled, value);
    }

    private string _storagePath = "Default (MyDocuments)";
    public string StoragePath
    {
        get => _storagePath;
        set => SetProperty(ref _storagePath, value);
    }

    private int _retentionDays = 7;
    public int RetentionDays
    {
        get => _retentionDays;
        set => SetProperty(ref _retentionDays, value);
    }

    // Collections
    public ObservableCollection<Persona> Personas { get; } = new();
    public ObservableCollection<WindowInfo> Windows { get; } = new();

    // Commands
    public ICommand CaptureNowCommand { get; }
    public ICommand ToggleMonitoringCommand { get; }
    public ICommand NextSuggestionCommand { get; }
    public ICommand RefreshWindowListCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand BrowseStorageCommand { get; }
    public ICommand OpenStorageCommand { get; }
    public ICommand PurgeStorageCommand { get; }

    // Cache for "Next Suggestion" retries
    private byte[]? _lastAnalyzedImageData;
    private string? _lastWindowTitle;

    public MainViewModel(
        AnalysisOrchestrator orchestrator,
        WindowMonitoringService monitoringService)
    {
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        _monitoringService = monitoringService ?? throw new ArgumentNullException(nameof(monitoringService));
        _promptService = new PromptService();

        // Initialize Commands
        CaptureNowCommand = new RelayCommand(_ => CaptureNow(), _ => !IsMonitoring);
        ToggleMonitoringCommand = new RelayCommand(_ => ToggleMonitoring());
        NextSuggestionCommand = new RelayCommand(_ => NextSuggestion(), _ => _lastAnalyzedImageData != null);
        RefreshWindowListCommand = new RelayCommand(_ => RefreshWindowList());
        CloseCommand = new RelayCommand(_ => System.Windows.Application.Current.Shutdown());
        BrowseStorageCommand = new RelayCommand(_ => BrowseStorage());
        OpenStorageCommand = new RelayCommand(_ => OpenStorage());
        PurgeStorageCommand = new RelayCommand(_ => PurgeStorage());

        // Subscribe to monitoring service events
        _monitoringService.CaptureRequested += OnCaptureRequested;

        // Load initial data
        LoadPersonas();
        RefreshWindowList();

        StatusText = "Tip: For best results, keep CortexView outside the tracked app window so it is not included in captures.";
    }

    private void LoadPersonas()
    {
        Personas.Clear();
        var personas = _promptService.LoadPersonas();
        foreach (var persona in personas)
        {
            Personas.Add(persona);
        }

        // Select the first persona by default
        if (Personas.Count > 0)
        {
            SelectedPersona = Personas[0];
        }
    }

    private void RefreshWindowList()
    {
        Windows.Clear();
        var windows = GetTopLevelWindows();

        // Sort: Priority windows first, then alphabetically
        var sorted = windows
            .OrderByDescending(w => IsPriorityWindow(w.Title))
            .ThenBy(w => w.Title);

        foreach (var window in sorted)
        {
            Windows.Add(window);
        }

        // Select the first window by default
        if (Windows.Count > 0)
        {
            SelectedWindow = Windows[0];
        }
    }

    private async void CaptureNow()
    {
        if (SelectedWindow == null || SelectedPersona == null)
        {
            StatusText = "Please select a window and persona first.";
            return;
        }

        StatusText = "Capturing...";

        try
        {
            var response = await _orchestrator.CaptureAndAnalyzeAsync(
                windowHandle: SelectedWindow.Hwnd,
                windowTitle: SelectedWindow.Title,
                persona: SelectedPersona,
                sensitivityThreshold: ChangeSensitivity / 100.0,
                forceAnalysis: true);

            if (response.IsSuccess)
            {
                SuggestionText = response.SuggestionText;
                StatusText = $"Analysis complete. Used {response.TokenUsage} tokens.";
                
                // Cache for "Next Suggestion"
                _lastWindowTitle = SelectedWindow.Title;
                // Note: We'd need to modify AnalysisOrchestrator to return the image data
                // For now, we'll handle this in Phase 5 when wiring everything together
            }
            else
            {
                StatusText = $"Error: {response.ErrorMessage}";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
        }
    }

    private void ToggleMonitoring()
    {
        IsMonitoring = !IsMonitoring;
    }

    private async void NextSuggestion()
    {
        if (_lastAnalyzedImageData == null || SelectedPersona == null)
        {
            StatusText = "No previous capture to retry.";
            return;
        }

        StatusText = "Re-analyzing previous capture...";

        try
        {
            // Create a new analysis request with the cached image
            var request = new AnalysisRequest
            {
                ImageData = _lastAnalyzedImageData,
                ImageFormat = "PNG",
                WindowTitle = _lastWindowTitle ?? "Unknown",
                SystemPrompt = SelectedPersona.SystemPrompt,
                UserPrompt = "Analyze this screenshot and provide suggestions.",
                Temperature = SelectedPersona.Temperature,
                TopP = SelectedPersona.TopP,
                MaxTokens = SelectedPersona.MaxTokens
            };

            // Note: We need to add a method to AnalysisOrchestrator that accepts a pre-captured image
            // For now, this is a placeholder that will be completed in Phase 5
            StatusText = "Next Suggestion feature will be completed in Phase 5 (DI Setup)";
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
        }
    }

    private void OnCaptureRequested(object? sender, EventArgs e)
    {
        // This is called from the monitoring service timer
        // We need to marshal to the UI thread
        System.Windows.Application.Current.Dispatcher.Invoke(async () =>
        {
            if (SelectedWindow == null || SelectedPersona == null)
            {
                return;
            }

            try
            {
                var response = await _orchestrator.CaptureAndAnalyzeAsync(
                    windowHandle: SelectedWindow.Hwnd,
                    windowTitle: SelectedWindow.Title,
                    persona: SelectedPersona,
                    sensitivityThreshold: ChangeSensitivity / 100.0,
                    forceAnalysis: false); // Respect change detection

                if (response.IsSuccess)
                {
                    SuggestionText = response.SuggestionText;
                    StatusText = $"Auto-analysis complete. Used {response.TokenUsage} tokens.";
                }
                else if (response.ErrorMessage.Contains("No significant change"))
                {
                    // Don't update status for minor changes
                }
                else
                {
                    StatusText = $"Error: {response.ErrorMessage}";
                }
            }
            catch (Exception ex)
            {
                StatusText = $"Error during auto-capture: {ex.Message}";
                IsMonitoring = false; // Stop monitoring on error
            }
        });
    }

    private void BrowseStorage()
    {
        // This will be implemented with a folder browser dialog
        // For now, placeholder
        StatusText = "Browse storage folder (to be implemented)";
    }

    private void OpenStorage()
    {
        // This will open the storage folder in Explorer
        // For now, placeholder
        StatusText = "Open storage folder (to be implemented)";
    }

    private void PurgeStorage()
    {
        // This will purge all stored files
        // For now, placeholder
        StatusText = "Purge storage (to be implemented)";
    }

    #region Window Enumeration (Win32 APIs)

    // These will eventually be moved to a separate service, but for now keep them here
    // to maintain functionality during the refactoring

    public class WindowInfo
    {
        public IntPtr Hwnd { get; set; }
        public string Title { get; set; } = string.Empty;

        public override string ToString() => Title;
    }

    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_APPWINDOW = 0x00040000;

    private static readonly string[] PriorityWindowKeywords =
    {
        "Visual Studio Code",
        "VS Code",
        "Microsoft Edge",
        "Google Chrome",
        "Firefox",
        "Word",
        "Excel"
    };

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr GetShellWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

    private static bool IsPriorityWindow(string title)
    {
        foreach (var keyword in PriorityWindowKeywords)
        {
            if (title.IndexOf(keyword, StringComparison.CurrentCultureIgnoreCase) >= 0)
                return true;
        }
        return false;
    }

    private static bool IsProbablyAppWindow(IntPtr hWnd, string title)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length < 2)
            return false;

        IntPtr exStylePtr = GetWindowLongPtr(hWnd, GWL_EXSTYLE);
        long exStyle = exStylePtr.ToInt64();

        bool isToolWindow = (exStyle & WS_EX_TOOLWINDOW) == WS_EX_TOOLWINDOW;
        bool isAppWindow = (exStyle & WS_EX_APPWINDOW) == WS_EX_APPWINDOW;

        if (isToolWindow && !isAppWindow)
            return false;

        return true;
    }

    private static List<WindowInfo> GetTopLevelWindows()
    {
        var windows = new List<WindowInfo>();
        IntPtr shellWindow = GetShellWindow();

        EnumWindows((hWnd, lParam) =>
        {
            if (hWnd == shellWindow)
                return true;

            if (!IsWindowVisible(hWnd))
                return true;

            int length = GetWindowTextLength(hWnd);
            if (length == 0)
                return true;

            var builder = new StringBuilder(length + 1);
            GetWindowText(hWnd, builder, builder.Capacity);
            string title = builder.ToString();

            if (string.IsNullOrWhiteSpace(title))
                return true;

            if (!IsProbablyAppWindow(hWnd, title))
                return true;

            windows.Add(new WindowInfo
            {
                Hwnd = hWnd,
                Title = title
            });

            return true;
        }, IntPtr.Zero);

        return windows;
    }

    #endregion
}
