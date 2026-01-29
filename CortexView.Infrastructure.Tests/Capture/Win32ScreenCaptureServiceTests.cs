using CortexView.Infrastructure.Capture;

namespace CortexView.Infrastructure.Tests.Capture;

/// <summary>
/// Integration tests for Win32ScreenCaptureService.
/// These tests interact with actual Windows APIs and require a valid window handle.
/// </summary>
public class Win32ScreenCaptureServiceTests
{
    private readonly Win32ScreenCaptureService _service;

    public Win32ScreenCaptureServiceTests()
    {
        _service = new Win32ScreenCaptureService();
    }

    [Fact]
    public async Task CaptureWindowAsync_InvalidHandle_ThrowsInvalidOperationException()
    {
        // Arrange
        IntPtr invalidHandle = IntPtr.Zero;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CaptureWindowAsync(invalidHandle));
    }

    [Fact]
    public async Task CaptureWindowAsync_ValidHandle_ReturnsImageData()
    {
        // Arrange
        IntPtr currentWindowHandle = GetCurrentProcessMainWindowHandle();
        
        if (currentWindowHandle == IntPtr.Zero)
        {
            // Skip test if no valid window handle available
            return;
        }

        // Act
        byte[] result = await _service.CaptureWindowAsync(currentWindowHandle);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        // PNG files start with specific magic bytes
        Assert.Equal(0x89, result[0]); // PNG signature
        Assert.Equal(0x50, result[1]); // 'P'
        Assert.Equal(0x4E, result[2]); // 'N'
        Assert.Equal(0x47, result[3]); // 'G'
    }

    [Fact]
    public async Task CaptureWindowAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        IntPtr currentWindowHandle = GetCurrentProcessMainWindowHandle();
        
        if (currentWindowHandle == IntPtr.Zero)
        {
            // Skip test if no valid window handle available
            return;
        }

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _service.CaptureWindowAsync(currentWindowHandle, cts.Token));
    }

    [Fact]
    public void GetWindowRect_InvalidHandle_ThrowsInvalidOperationException()
    {
        // Arrange
        IntPtr invalidHandle = IntPtr.Zero;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            _service.GetWindowRect(invalidHandle));
    }

    [Fact]
    public void GetWindowRect_ValidHandle_ReturnsCoordinates()
    {
        // Arrange
        IntPtr currentWindowHandle = GetCurrentProcessMainWindowHandle();
        
        if (currentWindowHandle == IntPtr.Zero)
        {
            // Skip test if no valid window handle available
            return;
        }

        // Act
        var (x, y, width, height) = _service.GetWindowRect(currentWindowHandle);

        // Assert
        Assert.True(width > 0, "Width should be positive");
        Assert.True(height > 0, "Height should be positive");
        // X and Y can be negative on multi-monitor setups, so we don't assert their sign
    }

    [Fact]
    public void GetWindowRect_ValidHandle_ReturnsConsistentDimensions()
    {
        // Arrange
        IntPtr currentWindowHandle = GetCurrentProcessMainWindowHandle();
        
        if (currentWindowHandle == IntPtr.Zero)
        {
            // Skip test if no valid window handle available
            return;
        }

        // Act
        var rect1 = _service.GetWindowRect(currentWindowHandle);
        var rect2 = _service.GetWindowRect(currentWindowHandle);

        // Assert - dimensions should be consistent across calls
        Assert.Equal(rect1.Width, rect2.Width);
        Assert.Equal(rect1.Height, rect2.Height);
        Assert.Equal(rect1.X, rect2.X);
        Assert.Equal(rect1.Y, rect2.Y);
    }

    /// <summary>
    /// Helper method to get the main window handle of the current process.
    /// Returns IntPtr.Zero if no window is available (e.g., in CI/CD environments).
    /// </summary>
    private static IntPtr GetCurrentProcessMainWindowHandle()
    {
        try
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            return process.MainWindowHandle;
        }
        catch
        {
            return IntPtr.Zero;
        }
    }
}
