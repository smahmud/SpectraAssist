using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;          // for WPF types like Window, RoutedEventArgs
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Threading;
using System.IO;
using System.Windows.Controls;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using CortexView.Services; // For IAiAnalysisService, MockAiService
using CortexView.Models;   // For AnalysisRequest
using System.Windows.Documents; // For FlowDocument manipulation

namespace CortexView;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private AppConfig _appConfig;
    
    private readonly IAiAnalysisService _aiService;

    private const int GWL_EXSTYLE = -20;
    
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    
    private const int WS_EX_APPWINDOW = 0x00040000;
    
    private readonly DispatcherTimer _captureTimer = new DispatcherTimer();

    private DateTime _lastCaptureTimeUtc;
    
    private enum ExclusionMode
    {
        DynamicDetection,
        StaticMask,
        ConfigurableExclusion
    }
    
    private ExclusionMode _currentExclusionMode = ExclusionMode.DynamicDetection;
    
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

    private readonly ChangeDetection _changeDetection = new ChangeDetection();

    private double _changeSensitivityFraction = 0.10; // 10% default

    private enum AnalysisTriggerReason
    {
        AutoChangeDetected,
        ManualOverride
    }

    //Custom Commands for Shortcuts
    public static readonly System.Windows.Input.RoutedCommand RequestNewInfoCmd = new System.Windows.Input.RoutedCommand();
    public static readonly System.Windows.Input.RoutedCommand NextSuggestionCmd = new System.Windows.Input.RoutedCommand();
    public static readonly System.Windows.Input.RoutedCommand ToggleCollapseCmd = new System.Windows.Input.RoutedCommand();

    private enum AnalysisStatus
    {
        Idle,
        Analyzing,
        SignificantChange,
        MinorChange,
        Error
    }

    public MainWindow()
    {
        try 
            {
                // AppDomain.CurrentDomain.BaseDirectory for reliable path resolution
                var builder = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                IConfigurationRoot configuration = builder.Build();
                
                _appConfig = new AppConfig();
                // Bind the "AiServiceConfig" section to the nested class inside AppConfig
                configuration.Bind(_appConfig);
            }
            catch (Exception ex)
            {
                // Show a popup if config fails, so you know why it crashed
                System.Windows.MessageBox.Show($"Error loading config: {ex.Message}\n\nPath checked: {Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json")}", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                
                // Initialize a default/fallback config to prevent null reference crashes later
                _appConfig = new AppConfig(); 
            }

        InitializeComponent();

        // Initialize the AI Service (Step 5.4: We hardcode Mock for now)
        _aiService = new MockAiService();

        //Register Command Bindings for Shortcuts
        CommandBindings.Add(new System.Windows.Input.CommandBinding(RequestNewInfoCmd, Execute_RequestNewInfo));
        CommandBindings.Add(new System.Windows.Input.CommandBinding(NextSuggestionCmd, Execute_NextSuggestion));
        CommandBindings.Add(new System.Windows.Input.CommandBinding(ToggleCollapseCmd, Execute_ToggleCollapse));
        UpdateStatusBar(AnalysisStatus.Idle, "Tip: For best results, keep CortexView outside the tracked app window so it is not included in captures."); 

        _currentExclusionMode = ExclusionMode.DynamicDetection;

        _captureTimer.Tick += (s, e) =>
        {
            try
            {
                CaptureAndStoreScreenshot();
            }
            catch (Exception ex)
            {
                Logger.Log($"Error during periodic capture: {ex.Message}");
                UpdateStatusBar(AnalysisStatus.Error, "Error during periodic capture. See log for details.");
                _captureTimer.Stop();
                MonitorToggleButton.IsChecked = false;
            }
        };

        // Initialize interval from slider (seconds to TimeSpan)
        _captureTimer.Interval = TimeSpan.FromSeconds(CaptureIntervalSlider.Value);
        _changeSensitivityFraction = ChangeSensitivitySlider.Value / 100.0;

        this.Loaded += (s, e) => InitialWindowPosition();

        OpacitySlider.ValueChanged += OpacitySlider_ValueChanged;
       
        var windows = GetTopLevelWindows();

        // simple sort: priority windows first, then alphabetical
        windows.Sort((a, b) =>
        {
            bool aIsPriority = IsPriorityWindow(a.Title);
            bool bIsPriority = IsPriorityWindow(b.Title);

            if (aIsPriority && !bIsPriority) return -1;
            if (!aIsPriority && bIsPriority) return 1;

            return string.Compare(a.Title, b.Title, StringComparison.CurrentCultureIgnoreCase);
        });

        WindowSelector.ItemsSource = windows;
        if (windows.Count > 0)
        {
            WindowSelector.SelectedIndex = 0;
        }

    }

    private static bool IsPriorityWindow(string title)
    {
        foreach (var keyword in PriorityWindowKeywords)
        {
            if (title.IndexOf(keyword, StringComparison.CurrentCultureIgnoreCase) >= 0)
                return true;
        }
        return false;
    }

    private class TopLevelWindowInfo
    {
        public IntPtr Hwnd { get; set; }
        public string Title { get; set; } = string.Empty;

        public override string ToString() => Title;
    }
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

    private static bool IsProbablyAppWindow(IntPtr hWnd, string title)
    {
        // Basic title filter: ignore very short or generic titles
        if (string.IsNullOrWhiteSpace(title) || title.Length < 2)
            return false;

        // Get extended window styles
        IntPtr exStylePtr = GetWindowLongPtr(hWnd, GWL_EXSTYLE);
        long exStyle = exStylePtr.ToInt64();

        // Exclude obvious tool windows; prefer windows marked as app windows
        bool isToolWindow = (exStyle & WS_EX_TOOLWINDOW) == WS_EX_TOOLWINDOW;
        bool isAppWindow = (exStyle & WS_EX_APPWINDOW) == WS_EX_APPWINDOW;

        if (isToolWindow && !isAppWindow)
            return false;

        return true;
    }

    private static List<TopLevelWindowInfo> GetTopLevelWindows()
    {
        var windows = new List<TopLevelWindowInfo>();
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

            windows.Add(new TopLevelWindowInfo
            {
                Hwnd = hWnd,
                Title = title
            });

            return true;
        }, IntPtr.Zero);

        return windows;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    private void InitialWindowPosition()
    {
        var screen = System.Windows.Forms.Screen.FromHandle(
            new System.Windows.Interop.WindowInteropHelper(this).Handle);

        // Physical pixel dimensions
        double screenPixelWidth = screen.Bounds.Width - 200;
        double screenPixelHeight = screen.Bounds.Height - 200;

        // Convert pixels to WPF DIPs using current DPI scaling
        var source = PresentationSource.FromVisual(this);
        double dpiX = 1.0, dpiY = 1.0;
        if (source != null)
        {
            dpiX = source.CompositionTarget.TransformToDevice.M11;
            dpiY = source.CompositionTarget.TransformToDevice.M22;
        }


        double screenDipWidth = screenPixelWidth / dpiX;
        double screenDipHeight = screenPixelHeight / dpiY;
        double screenDipHalfWidth = screenDipWidth / 2;

        this.Width = screenDipHalfWidth;
        this.Height = screenDipHeight;
        this.Left = screenDipHalfWidth; // Position at right half
        this.Top = 0;                   // Top of screen (always 0 for typical screen)

    } 

    private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        // Find the root Border element (the parent of RootLayoutGrid)
        if (RootLayoutGrid.Parent is Border rootBorder)
        {
            // Simply set the Opacity property of the Border to the slider's new value (0.2 to 1.0).
            // This is much cleaner and controls the transparency of the dark background tint.
            rootBorder.Opacity = e.NewValue;
        }
    }

    private void CaptureNowButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            CaptureAndStoreScreenshot();
        }
        catch (Exception ex)
        {
            Logger.Log($"Error capturing window screenshot: {ex.Message}");
            UpdateStatusBar(AnalysisStatus.Error, "Error capturing window. See log for details.");
        }
    }

    private void MonitorToggleButton_Checked(object sender, RoutedEventArgs e)
    {
        MonitorToggleButton.Content = "Stop Monitoring";
        _captureTimer.Start();
        // Green dot indicates system is active/ready
        UpdateStatusBar(AnalysisStatus.Idle, "Monitoring started.");
    }

    private void MonitorToggleButton_Unchecked(object sender, RoutedEventArgs e)
    {
        MonitorToggleButton.Content = "Start Monitoring";
        _captureTimer.Stop();
        UpdateStatusBar(AnalysisStatus.Idle, "Monitoring stopped.");
    } 

    private void NextSuggestionButton_Click(object? sender, RoutedEventArgs? e)
    {
        UpdateStatusBar(AnalysisStatus.Idle, "Next suggestion requested (Bedrock integration in later milestones).");
    }

    private void CaptureIntervalSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (IntervalLabel != null && _captureTimer != null)
        {
            int seconds = (int)CaptureIntervalSlider.Value;
            IntervalLabel.Text = $"Capture Interval: {seconds}s";
            _captureTimer.Interval = TimeSpan.FromSeconds(seconds);
        }
    }

    private void WindowSelector_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (WindowSelector.SelectedItem is TopLevelWindowInfo selected)
        {
            // Green dot indicates successful selection and ready state
            UpdateStatusBar(AnalysisStatus.Idle, $"Tracking window: \"{selected.Title}\". Ensure CortexView does not overlap this window to avoid self-capture.");
        }
    }

    protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
    {
        // Call the base method first.
        base.OnMouseLeftButtonDown(e);

        // Initiates the drag operation.
        this.DragMove(); // Allows window drag
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.MessageBox.Show("Settings dialog will be implemented here.");
    }

    private void CaptureAndStoreScreenshot()
    {
        string binDir = AppDomain.CurrentDomain.BaseDirectory;
        string projectRoot = Directory.GetParent(binDir)!.Parent!.Parent!.Parent!.FullName; // from bin/Debug/netX

        string outputDir = Path.Combine(projectRoot, "tests", "output");
        
        // When no window is selected
        if (WindowSelector.SelectedItem is not TopLevelWindowInfo selectedWindow)
        {
            UpdateStatusBar(AnalysisStatus.Idle, "No window selected for capture. Please choose an app window from the list.");
            return;
        }
        // When GetWindowRect fails
        if (!GetWindowRect(selectedWindow.Hwnd, out RECT rect))
        {
            UpdateStatusBar(AnalysisStatus.Error, $"Could not capture \"{selectedWindow.Title}\". The window handle is invalid or no longer available.");
            Logger.Log($"GetWindowRect failed for window: {selectedWindow.Title}");
            return;
        }

        // When the window is minimized or has invalid size
        int width = rect.Right - rect.Left;
        int height = rect.Bottom - rect.Top;

        if (width <= 0 || height <= 0)
        {
            UpdateStatusBar(AnalysisStatus.Error, $"Could not capture \"{selectedWindow.Title}\". Is the window minimized or off-screen?");
            Logger.Log($"Invalid window size for capture: {selectedWindow.Title} ({width}x{height})");
            return;
        }

        using (var bmp = new Bitmap(width, height))
        using (var g = Graphics.FromImage(bmp))
        {
            g.CopyFromScreen(rect.Left, rect.Top, 0, 0, bmp.Size);

            // 1) Pixel-based fraction (0.0–1.0)
            double changedFraction = _changeDetection.ComputeChangedFraction(bmp);        

            // 2) Hash-based guard: if hash says “identical”, short-circuit
            bool hasMeaningfulChange = _changeDetection.HasMeaningfulChange(bmp);
            if (!hasMeaningfulChange)
            {
                UpdateStatusBar(AnalysisStatus.Idle, $"No meaningful change detected for \"{selectedWindow.Title}\".");
                return;
            }
           
            // 3) Apply sensitivity threshold to pixel diff
            var decision = _changeDetection.DecideChange(changedFraction, _changeSensitivityFraction);
            if (decision == ChangeDecision.MinorChangeBelowThreshold)
            {
                UpdateStatusBar(AnalysisStatus.MinorChange, $"Minor change (~{changedFraction:P0}) below {(_changeSensitivityFraction):P0} threshold – skipping analysis.");
                return;
            }

            // Significant change → save screenshot as before
            _lastCaptureTimeUtc = DateTime.UtcNow;

            string fileName = $"window_capture_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            string filePath = Path.Combine(outputDir, fileName);
            bmp.Save(filePath, ImageFormat.Png);

            // Optionally show the percentage in the status text for now
            UpdateStatusBar(AnalysisStatus.SignificantChange, $"Captured window: \"{selectedWindow.Title}\" to {filePath} (changed ~{changedFraction:P0}).");

            _lastCaptureTimeUtc = DateTime.UtcNow;

            // trigger local analysis stub (auto reason)
            _ = RunAnalysisIfNeededAsync(AnalysisTriggerReason.AutoChangeDetected, selectedWindow, (Bitmap)bmp.Clone(), changedFraction);

        }
    }

    private void ExclusionModeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (ExclusionModeComboBox?.SelectedItem is not ComboBoxItem item)
            return;

        string mode = item.Content?.ToString() ?? string.Empty;

        _currentExclusionMode = mode switch
        {
            "Dynamic Detection" => ExclusionMode.DynamicDetection,
            "Static Mask" => ExclusionMode.StaticMask,
            "Configurable Exclusion" => ExclusionMode.ConfigurableExclusion,
            _ => ExclusionMode.DynamicDetection
        };

        if (StatusTextBlock != null)
        {
            // Green dot indicates successful selection and ready state
            UpdateStatusBar(AnalysisStatus.Idle, $"Exclusion mode: {_currentExclusionMode} (only DynamicDetection is active in v1.0.0).");
        }
    }

    private void ChangeSensitivitySlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        _changeSensitivityFraction = ChangeSensitivitySlider.Value / 100.0;
    }

    private async Task RunAnalysisIfNeededAsync(
        AnalysisTriggerReason reason, 
        TopLevelWindowInfo selectedWindow, 
        Bitmap latestBitmap, 
        double changedFraction)
    {
        // 1. Check if we should run (Significant change OR Manual override)
        bool shouldRun = (reason == AnalysisTriggerReason.ManualOverride) ||
                         (reason == AnalysisTriggerReason.AutoChangeDetected && 
                          _changeDetection.DecideChange(changedFraction, _changeSensitivityFraction) == ChangeDecision.SignificantChange);

        if (!shouldRun) return;

        // 2. Prepare UI for Analysis
        UpdateStatusBar(AnalysisStatus.Analyzing, $"Analyzing view... ({reason})");
        
        // SHOW THE OVERLAY
        if (ThinkingOverlay != null) 
            ThinkingOverlay.Visibility = Visibility.Visible;

        try
        {
            // 3. Create Request
            var request = new AnalysisRequest(latestBitmap, selectedWindow.Title)
            {
                OcrText = _changeDetection.TryExtractOcrText(latestBitmap) ?? string.Empty,
                UserPrompt = "Analyze this interface."
            };

            // 4. Call Service (Async)
            // This will take 2 seconds (simulated) where the UI remains responsive!
            var response = await _aiService.AnalyzeImageAsync(request);

            // 5. Handle Result
            // HIDE THE OVERLAY
            if (ThinkingOverlay != null) 
                ThinkingOverlay.Visibility = Visibility.Collapsed;

            if (response.IsSuccess)
            {
                UpdateStatusBar(AnalysisStatus.Idle, $"Analysis Complete. Used {response.TokenUsage} tokens.");
                
                // Clear and update the RichTextBox with the response
                AiContextTextBox.Document.Blocks.Clear();
                AiContextTextBox.Document.Blocks.Add(new Paragraph(new Run(response.SuggestionText)));
            }
            else
            {
                UpdateStatusBar(AnalysisStatus.Error, $"Analysis Failed: {response.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            if (ThinkingOverlay != null) 
                ThinkingOverlay.Visibility = Visibility.Collapsed;
                
            UpdateStatusBar(AnalysisStatus.Error, "System Error during analysis.");
            Logger.Log($"Critical Analysis Error: {ex}");
        }
    }

    private Task RunLocalAnalysisStubAsync(
        AnalysisTriggerReason reason,
        double changedFraction,
        string windowTitle,
        string? ocrText)
    {
        string reasonText = reason == AnalysisTriggerReason.ManualOverride
            ? "ManualOverride"
            : "AutoChangeDetected";

        var sb = new StringBuilder();
        sb.AppendLine($"[Stub] Analysis triggered ({reasonText}) for \"{windowTitle}\"");
        sb.AppendLine($"Estimated change: {changedFraction:P0}");
        if (!string.IsNullOrWhiteSpace(ocrText))
        {
            sb.AppendLine("OCR snapshot (experimental):");
            sb.AppendLine(ocrText);
        }

        // AiContextTextBox.Text = sb.ToString();

        return Task.CompletedTask;
    }

    private void RequestNewInformationButton_OnClick(object? sender, RoutedEventArgs? e)
    {
        if (WindowSelector.SelectedItem is not TopLevelWindowInfo selectedWindow)
        {
            // Green dot indicates successful selection and ready state
            UpdateStatusBar(AnalysisStatus.Error, "No window selected for manual analysis. Please choose an app window.");
            return;
        }

        // Reuse the same capture path as auto, but always treat as manual override.
        if (!GetWindowRect(selectedWindow.Hwnd, out RECT rect))
        {
            UpdateStatusBar(AnalysisStatus.Error, $"Could not capture \"{selectedWindow.Title}\" for manual analysis. Is it closed?");
            return;
        }

        int width = rect.Right - rect.Left;
        int height = rect.Bottom - rect.Top;
        if (width <= 0 || height <= 0)
        {
            UpdateStatusBar(AnalysisStatus.Error, $"Could not capture \"{selectedWindow.Title}\" for manual analysis. Is it minimized or off-screen?");
            return;
        }

        using var bmp = new Bitmap(width, height);
        using var g = Graphics.FromImage(bmp);
        g.CopyFromScreen(rect.Left, rect.Top, 0, 0, bmp.Size);

        double changedFraction = _changeDetection.ComputeChangedFraction(bmp);

        // Always run analysis stub, regardless of sensitivity or hash decision.
        _ = RunAnalysisIfNeededAsync(AnalysisTriggerReason.ManualOverride, selectedWindow, (Bitmap)bmp.Clone(), changedFraction);

        // Change: Use UpdateStatusBar with SignificantChange (Action occurred)
        UpdateStatusBar(AnalysisStatus.SignificantChange, $"Manual analysis requested for \"{selectedWindow.Title}\" (estimated change ~{changedFraction:P0}).");
    }

    private void PinToggleButton_Click(object sender, RoutedEventArgs e)
    {
        // If the button is Checked (True), make the window Topmost.
        // If Unchecked (False) or null, make it normal.
        Topmost = PinToggleButton.IsChecked == true;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        // Explicitly specifies System.Windows.Application (the WPF version)
        System.Windows.Application.Current.Shutdown();
    }

    private void ChangeSensitivitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (SensitivityLabel != null)
        {
            int percent = (int)ChangeSensitivitySlider.Value;
            SensitivityLabel.Text = $"Change Sensitivity: {percent}%";
        }
    }

    // Command Handlers
    private void Execute_RequestNewInfo(object? sender, System.Windows.Input.ExecutedRoutedEventArgs? e)
    {
        // Reuse existing logic. Pass null args since logic doesn't depend on them.
        RequestNewInformationButton_OnClick(null, null);
    }

    private void Execute_NextSuggestion(object? sender, System.Windows.Input.ExecutedRoutedEventArgs? e)
    {
        // Reuse existing logic.
        NextSuggestionButton_Click(null, null);
    }

    private void Execute_ToggleCollapse(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
    {
        // Toggle the Checked state. The XAML style triggers will handle the UI visibility.
        if (CollapseToggleButton.IsChecked.HasValue)
        {
            CollapseToggleButton.IsChecked = !CollapseToggleButton.IsChecked.Value;
        }
    }

    // Central helper to update status text and indicator color
    private void UpdateStatusBar(AnalysisStatus status, string message)
    {
        // Update Text
        if (StatusTextBlock != null)
        {
            StatusTextBlock.Text = message;
        }

        // Update Dot Color
        if (StatusIndicatorEllipse != null)
        {
            System.Windows.Media.SolidColorBrush newBrush;
            switch (status)
            {
                case AnalysisStatus.Idle:
                    newBrush = System.Windows.Media.Brushes.LimeGreen; // Ready
                    break;
                case AnalysisStatus.Analyzing:
                    newBrush = System.Windows.Media.Brushes.Gold; // Busy/Processing
                    break;
                case AnalysisStatus.SignificantChange:
                    newBrush = System.Windows.Media.Brushes.OrangeRed; // Action occurred
                    break;
                case AnalysisStatus.MinorChange:
                    newBrush = System.Windows.Media.Brushes.DeepSkyBlue; // Ignored change
                    break;
                case AnalysisStatus.Error:
                    newBrush = System.Windows.Media.Brushes.Red; // Error
                    break;
                default:
                    newBrush = System.Windows.Media.Brushes.Gray;
                    break;
            }
            StatusIndicatorEllipse.Fill = newBrush;
        }
    }
}