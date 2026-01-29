using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using CortexView.Domain.Interfaces;

namespace CortexView.Infrastructure.Capture;

/// <summary>
/// Windows-specific implementation of screen capture using Win32 APIs.
/// </summary>
public sealed class Win32ScreenCaptureService : IScreenCaptureService
{
    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    /// <inheritdoc/>
    public async Task<byte[]> CaptureWindowAsync(
        IntPtr windowHandle,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            // Get window rectangle
            if (!GetWindowRect(windowHandle, out RECT rect))
            {
                throw new InvalidOperationException("Failed to get window rectangle. Invalid window handle.");
            }

            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            if (width <= 0 || height <= 0)
            {
                throw new InvalidOperationException($"Invalid window dimensions: {width}x{height}");
            }

            // Capture screenshot
            using var bitmap = new Bitmap(width, height);
            using var graphics = Graphics.FromImage(bitmap);
            
            graphics.CopyFromScreen(rect.Left, rect.Top, 0, 0, bitmap.Size);

            // Convert to PNG byte array
            using var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Png);
            return memoryStream.ToArray();
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public (int X, int Y, int Width, int Height) GetWindowRect(IntPtr windowHandle)
    {
        if (!GetWindowRect(windowHandle, out RECT rect))
        {
            throw new InvalidOperationException("Failed to get window rectangle. Invalid window handle.");
        }

        return (
            X: rect.Left,
            Y: rect.Top,
            Width: rect.Right - rect.Left,
            Height: rect.Bottom - rect.Top
        );
    }
}
