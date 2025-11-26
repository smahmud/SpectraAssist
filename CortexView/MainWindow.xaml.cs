using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;          // for WPF types like Window, RoutedEventArgs
using System.Windows.Forms;    // for Screen, etc.



namespace CortexView;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.Loaded += (s, e) => InitialWindowPosition();


        this.SizeChanged += (s, e) => UpdateInfoDisplay();
        this.LocationChanged += (s, e) => UpdateInfoDisplay();
        OpacitySlider.ValueChanged += OpacitySlider_ValueChanged;
        foreach (var screen in Screen.AllScreens)
        {
            ScreenSelector.Items.Add(screen.DeviceName);
        }
        if (ScreenSelector.Items.Count > 0)
            ScreenSelector.SelectedIndex = 0;
    }

    private void InitialWindowPosition()
    {
        var screen = System.Windows.Forms.Screen.FromHandle(
            new System.Windows.Interop.WindowInteropHelper(this).Handle);

        // Physical pixel dimensions
        double screenPixelWidth = screen.Bounds.Width;
        double screenPixelHeight = screen.Bounds.Height;

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


        // Optionally display info in a TextBlock for debugging
        InfoText.Text = $"Screen width: {screenDipWidth}\n" +
                        $"Screen height: {screenDipHeight}\n" +
                        $"Screen width half: {screenDipHalfWidth}\n" +
                        $"Window Width: {this.Width}\n" +
                        $"Window Height: {this.Height}\n" +
                        $"Window Left: {this.Left}\n" +
                        $"Window Top: {this.Top}";
    }


    private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        this.Opacity = e.NewValue;
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.MessageBox.Show("Settings dialog will be implemented here.");
    }

    private void CaptureScreenshot_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            CaptureAndStoreScreenshot();
        }
        catch (Exception ex)
        {
            Logger.Log($"Error capturing screenshot: {ex.Message}");
            System.Windows.MessageBox.Show("An error occurred while capturing the screenshot.");
        }
    }

    private void CaptureAndStoreScreenshot()
    {
        var selectedScreenIndex = ScreenSelector.SelectedIndex;
        if (selectedScreenIndex < 0) return;

        var screen = Screen.AllScreens[selectedScreenIndex];
        using (var bmp = new Bitmap(screen.Bounds.Width, screen.Bounds.Height))
        {
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(screen.Bounds.Left, screen.Bounds.Top, 0, 0, bmp.Size);
            }

            if (StoreOnDiskCheckbox.IsChecked == true)
            {
                string filePath = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                bmp.Save(filePath, ImageFormat.Png);
                System.Windows.MessageBox.Show($"Screenshot saved to {filePath}");
            }
            else
            {
                // Store bitmap in memory/transient list
                // Example: InMemoryScreenshots.Add((Bitmap)bmp.Clone());
                System.Windows.MessageBox.Show("Screenshot captured in memory (not saved to disk).");
            }
        }
    }

    protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        this.DragMove(); // Allows window drag
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

    private void UpdateInfoDisplay()
    {
        var screen = System.Windows.Forms.Screen.FromHandle(
            new System.Windows.Interop.WindowInteropHelper(this).Handle);

        double screenWidth = screen.Bounds.Width;
        double screenHeight = screen.Bounds.Height;
        double screenHalf = screenWidth / 2;

        int left = (int)screenHalf; //screen.Bounds.Left;
        int top = screen.Bounds.Top;
        int right = screen.Bounds.Right;
        int bottom = screen.Bounds.Bottom;

        string message = $"Screen width: {screenWidth}\n" +
                         $"Screen height: {screenHeight}\n" +
                         $"Screen width half: {screenHalf}\n" +
                         $"Screen Left: {left}\n" +
                         $"Screen Top: {top}\n" +
                         $"Screen Right: {right}\n" +
                         $"Screen Bottom: {bottom}\n" +
                         $"Window Width: {this.Width}\n" +
                         $"Window Height: {this.Height}\n" +
                         $"Window Left: {this.Left}\n" +
                         $"Window Top: {this.Top}";

        InfoText.Text = message;
    }

}