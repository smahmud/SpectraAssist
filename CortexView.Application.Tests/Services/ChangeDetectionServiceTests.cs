using System.Drawing;
using System.Drawing.Imaging;
using CortexView.Application.Services;

namespace CortexView.Application.Tests.Services;

/// <summary>
/// Unit tests for the ChangeDetectionService.
/// </summary>
public class ChangeDetectionServiceTests
{
    [Fact]
    public void ComputeChangedFraction_FirstCapture_Returns100Percent()
    {
        // Arrange
        var service = new ChangeDetectionService();
        byte[] imageData = CreateTestImage(100, 100, Color.White);

        // Act
        double result = service.ComputeChangedFraction(imageData);

        // Assert
        Assert.Equal(1.0, result);
    }

    [Fact]
    public void ComputeChangedFraction_IdenticalImages_Returns0Percent()
    {
        // Arrange
        var service = new ChangeDetectionService();
        byte[] imageData = CreateTestImage(100, 100, Color.White);

        // Act
        service.ComputeChangedFraction(imageData); // First capture
        double result = service.ComputeChangedFraction(imageData); // Second capture (identical)

        // Assert
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void ComputeChangedFraction_DifferentImages_ReturnsPositiveValue()
    {
        // Arrange
        var service = new ChangeDetectionService();
        byte[] imageData1 = CreateTestImage(100, 100, Color.White);
        byte[] imageData2 = CreateTestImage(100, 100, Color.Black);

        // Act
        service.ComputeChangedFraction(imageData1); // First capture
        double result = service.ComputeChangedFraction(imageData2); // Second capture (different)

        // Assert
        Assert.True(result > 0.0);
        Assert.True(result <= 1.0);
    }

    [Fact]
    public void ComputeChangedFraction_NullImageData_ThrowsArgumentNullException()
    {
        // Arrange
        var service = new ChangeDetectionService();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => service.ComputeChangedFraction(null!));
    }

    [Fact]
    public void IsSignificantChange_AboveThreshold_ReturnsTrue()
    {
        // Arrange
        var service = new ChangeDetectionService();

        // Act
        bool result = service.IsSignificantChange(0.15, 0.10);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSignificantChange_BelowThreshold_ReturnsFalse()
    {
        // Arrange
        var service = new ChangeDetectionService();

        // Act
        bool result = service.IsSignificantChange(0.05, 0.10);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsSignificantChange_EqualToThreshold_ReturnsTrue()
    {
        // Arrange
        var service = new ChangeDetectionService();

        // Act
        bool result = service.IsSignificantChange(0.10, 0.10);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void TryExtractOcrText_ReturnsNull()
    {
        // Arrange
        var service = new ChangeDetectionService();
        byte[] imageData = CreateTestImage(100, 100, Color.White);

        // Act
        string? result = service.TryExtractOcrText(imageData);

        // Assert
        Assert.Null(result); // OCR not implemented yet
    }

    [Fact]
    public void ComputeChangedFraction_SlightChange_ReturnsSmallValue()
    {
        // Arrange
        var service = new ChangeDetectionService();
        byte[] imageData1 = CreateTestImage(100, 100, Color.White);
        byte[] imageData2 = CreateTestImageWithSmallChange(100, 100);

        // Act
        service.ComputeChangedFraction(imageData1);
        double result = service.ComputeChangedFraction(imageData2);

        // Assert
        Assert.True(result > 0.0);
        Assert.True(result < 0.5); // Should be less than 50% changed
    }

    private static byte[] CreateTestImage(int width, int height, Color color)
    {
        using var bitmap = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(color);

        using var ms = new MemoryStream();
        bitmap.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }

    private static byte[] CreateTestImageWithSmallChange(int width, int height)
    {
        using var bitmap = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.White);
        
        // Draw a small black rectangle (10% of image)
        graphics.FillRectangle(Brushes.Black, 0, 0, width / 10, height / 10);

        using var ms = new MemoryStream();
        bitmap.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }
}
