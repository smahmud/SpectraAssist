using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using CortexView.Domain.Interfaces;

namespace CortexView.Application.Services;

/// <summary>
/// Application service for detecting visual changes between consecutive screenshots.
/// </summary>
/// <remarks>
/// Uses SHA-256 hashing for quick comparison and pixel-by-pixel analysis
/// for detailed change detection. Caches the previous image for comparison.
/// </remarks>
public sealed class ChangeDetectionService : IChangeDetectionService
{
    private byte[]? _lastImageHash;
    private byte[]? _lastDownsampled;
    private const int DownsampledWidth = 64;
    private const int DownsampledHeight = 36;
    private const int NoiseThreshold = 10; // Brightness difference threshold (0-255)

    /// <inheritdoc/>
    public double ComputeChangedFraction(byte[] imageData)
    {
        ArgumentNullException.ThrowIfNull(imageData);

        // Quick hash check first
        byte[] currentHash = ComputeImageHash(imageData);
        
        if (_lastImageHash != null && AreHashesEqual(_lastImageHash, currentHash))
        {
            // Images are identical - no change
            return 0.0;
        }

        _lastImageHash = currentHash;

        // Detailed pixel comparison
        byte[] currentDownsampled = DownsampleToGrayscale(imageData);

        if (_lastDownsampled == null)
        {
            _lastDownsampled = currentDownsampled;
            return 1.0; // First capture = 100% changed
        }

        int changed = 0;
        int total = currentDownsampled.Length;

        for (int i = 0; i < total; i++)
        {
            int diff = Math.Abs(currentDownsampled[i] - _lastDownsampled[i]);
            if (diff > NoiseThreshold)
            {
                changed++;
            }
        }

        _lastDownsampled = currentDownsampled;

        return total == 0 ? 0.0 : (double)changed / total;
    }

    /// <inheritdoc/>
    public bool IsSignificantChange(double changedFraction, double sensitivityThreshold)
    {
        return changedFraction >= sensitivityThreshold;
    }

    /// <inheritdoc/>
    public string? TryExtractOcrText(byte[] imageData)
    {
        // Placeholder for future OCR integration
        // Could integrate Tesseract, Azure Computer Vision, or AWS Textract
        return null;
    }

    private static byte[] ComputeImageHash(byte[] imageData)
    {
        return SHA256.HashData(imageData);
    }

    private static bool AreHashesEqual(byte[] a, byte[] b)
    {
        if (a.Length != b.Length)
        {
            return false;
        }

        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i])
            {
                return false;
            }
        }

        return true;
    }

    private static byte[] DownsampleToGrayscale(byte[] imageData)
    {
        using var ms = new MemoryStream(imageData);
        using var source = new Bitmap(ms);
        using var resized = new Bitmap(DownsampledWidth, DownsampledHeight);
        
        using (var graphics = Graphics.FromImage(resized))
        {
            graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
            graphics.DrawImage(source, 0, 0, DownsampledWidth, DownsampledHeight);
        }

        var pixels = new byte[DownsampledWidth * DownsampledHeight];
        int index = 0;

        for (int y = 0; y < DownsampledHeight; y++)
        {
            for (int x = 0; x < DownsampledWidth; x++)
            {
                var color = resized.GetPixel(x, y);
                // Convert to grayscale using standard luminosity formula
                byte gray = (byte)((color.R * 0.3) + (color.G * 0.59) + (color.B * 0.11));
                pixels[index++] = gray;
            }
        }

        return pixels;
    }
}
