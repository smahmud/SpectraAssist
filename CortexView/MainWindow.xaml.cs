using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;          // for WPF types like Window, RoutedEventArgs
using System.Windows.Forms;    // for Screen, etc.
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Threading;
using System.Security.Cryptography;
using System.IO;
using System.Windows.Controls;
using System.Threading.Tasks;


namespace CortexView;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{

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

    public MainWindow()
    {
        InitializeComponent();
        StatusTextBlock.Text =
            "Tip: For best results, keep CortexView outside the tracked app window so it is not included in captures.";

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
                StatusTextBlock.Text = "Error during periodic capture. See log for details.";
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
        this.Opacity = e.NewValue;
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
            StatusTextBlock.Text = "Error capturing window. See log for details.";
        }
    }

    private void MonitorToggleButton_Checked(object sender, RoutedEventArgs e)
    {
        MonitorToggleButton.Content = "Stop Monitoring";
        _captureTimer.Start();
        StatusTextBlock.Text = "Monitoring started.";
    }

    private void MonitorToggleButton_Unchecked(object sender, RoutedEventArgs e)
    {
        MonitorToggleButton.Content = "Start Monitoring";
        _captureTimer.Stop();
        StatusTextBlock.Text = "Monitoring stopped.";
    }

    private void NextSuggestionButton_Click(object sender, RoutedEventArgs e)
    {
        StatusTextBlock.Text = "Next suggestion requested (Bedrock integration in later milestones).";
    }

    private void CaptureIntervalSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (CaptureIntervalLabel is not null)
        {
            CaptureIntervalLabel.Text = $"{(int)e.NewValue}s";
        }

        if (_captureTimer != null)
        {
            _captureTimer.Interval = TimeSpan.FromSeconds(e.NewValue);
        }
    }

    private void WindowSelector_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (WindowSelector.SelectedItem is TopLevelWindowInfo selected)
        {
            StatusTextBlock.Text =
                $"Tracking window: \"{selected.Title}\". Ensure CortexView does not overlap this window to avoid self-capture.";
        }
    }

    private void Minimize_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private void Maximize_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = (this.WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
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
        
        //When no window is selected
        if (WindowSelector.SelectedItem is not TopLevelWindowInfo selectedWindow)
        {
            StatusTextBlock.Text = "No window selected for capture. Please choose an app window from the list.";
            return;
        }
        // When GetWindowRect fails
        if (!GetWindowRect(selectedWindow.Hwnd, out RECT rect))
        {
            StatusTextBlock.Text =
                $"Could not capture \"{selectedWindow.Title}\". The window handle is invalid or no longer available.";
            Logger.Log($"GetWindowRect failed for window: {selectedWindow.Title}");
            return;
        }

        // When the window is minimized or has invalid size
        int width = rect.Right - rect.Left;
        int height = rect.Bottom - rect.Top;

        if (width <= 0 || height <= 0)
        {
            StatusTextBlock.Text =
                $"Could not capture \"{selectedWindow.Title}\". Is the window minimized or off-screen?";
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
                StatusTextBlock.Text = $"No meaningful change detected for \"{selectedWindow.Title}\".";
                return;
            }
           
            // 3) Apply sensitivity threshold to pixel diff
            var decision = _changeDetection.DecideChange(changedFraction, _changeSensitivityFraction);
            if (decision == ChangeDecision.MinorChangeBelowThreshold)
            {
                StatusTextBlock.Text =
                    $"Minor change (~{changedFraction:P0}) below {(_changeSensitivityFraction):P0} threshold – skipping analysis.";
                return;
            }

            // Significant change → save screenshot as before
            _lastCaptureTimeUtc = DateTime.UtcNow;

            string fileName = $"window_capture_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            string filePath = Path.Combine(outputDir, fileName);
            bmp.Save(filePath, ImageFormat.Png);

            // Optionally show the percentage in the status text for now
            StatusTextBlock.Text = $"Captured window: \"{selectedWindow.Title}\" to {filePath} (changed ~{changedFraction:P0}).";

            _lastCaptureTimeUtc = DateTime.UtcNow;

            // NEW: trigger local analysis stub (auto reason)
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
            StatusTextBlock.Text =
                $"Exclusion mode: {_currentExclusionMode} (only DynamicDetection is active in v1.0.0).";
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
        // Experimental OCR (may return null for now)
        string? ocrText = _changeDetection.TryExtractOcrText(latestBitmap);

        // For Milestone 3, this method only calls the local stub.
        // Later milestones will add more branching here.
        await RunLocalAnalysisStubAsync(
            reason, 
            changedFraction, 
            selectedWindow.Title,
            ocrText);
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

        //AiContextTextBox.Text = sb.ToString();

        return Task.CompletedTask;
    }

    private void RequestNewInformationButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (WindowSelector.SelectedItem is not TopLevelWindowInfo selectedWindow)
        {
            StatusTextBlock.Text = "No window selected for manual analysis. Please choose an app window.";
            return;
        }

        // Reuse the same capture path as auto, but always treat as manual override.
        if (!GetWindowRect(selectedWindow.Hwnd, out RECT rect))
        {
            StatusTextBlock.Text = $"Could not capture \"{selectedWindow.Title}\" for manual analysis. Is it closed?";
            return;
        }

        int width = rect.Right - rect.Left;
        int height = rect.Bottom - rect.Top;
        if (width <= 0 || height <= 0)
        {
            StatusTextBlock.Text = $"Could not capture \"{selectedWindow.Title}\" for manual analysis. Is it minimized or off-screen?";
            return;
        }

        using var bmp = new Bitmap(width, height);
        using var g = Graphics.FromImage(bmp);
        g.CopyFromScreen(rect.Left, rect.Top, 0, 0, bmp.Size);

        double changedFraction = _changeDetection.ComputeChangedFraction(bmp);

        // Always run analysis stub, regardless of sensitivity or hash decision.
        _ = RunAnalysisIfNeededAsync(AnalysisTriggerReason.ManualOverride, selectedWindow, (Bitmap)bmp.Clone(), changedFraction);

        StatusTextBlock.Text = $"Manual analysis requested for \"{selectedWindow.Title}\" (estimated change ~{changedFraction:P0}).";
    }

    /// <summary>
    /// Allows the borderless window to be dragged by clicking and holding the mouse button.
    /// This fulfills a core M1/M4 requirement.
    /// </summary>
    private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
        {
            // Check if the user is attempting to resize, if not, drag the window.
            if (ResizeMode == ResizeMode.CanResize || ResizeMode == ResizeMode.CanResizeWithGrip)
            {
                // Do not drag if the mouse is near the border (WPF handles resizing automatically)
                // A perfect implementation is complex, but for a simple drag, we assume 
                // the user clicks away from the resize handle.
            }
            
            // This command is the standard way to initiate a drag operation in a borderless WPF window
            DragMove();
        }
    }

}