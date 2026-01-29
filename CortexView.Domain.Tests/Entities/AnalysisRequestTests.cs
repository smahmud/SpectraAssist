using CortexView.Domain.Entities;

namespace CortexView.Domain.Tests.Entities;

/// <summary>
/// Unit tests for the AnalysisRequest entity.
/// </summary>
public class AnalysisRequestTests
{
    [Fact]
    public void IsValid_ValidRequest_ReturnsTrue()
    {
        // Arrange
        var request = new AnalysisRequest
        {
            ImageData = new byte[] { 1, 2, 3, 4 },
            ImageFormat = "PNG",
            WindowTitle = "Test Window",
            SystemPrompt = "You are a helpful assistant.",
            UserPrompt = "Analyze this image.",
            Temperature = 0.7f,
            TopP = 0.9f,
            MaxTokens = 1000
        };

        // Act
        bool result = request.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_EmptyImageData_ReturnsFalse()
    {
        // Arrange
        var request = new AnalysisRequest
        {
            ImageData = Array.Empty<byte>(),
            ImageFormat = "PNG",
            WindowTitle = "Test Window",
            SystemPrompt = "You are a helpful assistant."
        };

        // Act
        bool result = request.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_EmptyWindowTitle_ReturnsFalse()
    {
        // Arrange
        var request = new AnalysisRequest
        {
            ImageData = new byte[] { 1, 2, 3, 4 },
            ImageFormat = "PNG",
            WindowTitle = "",
            SystemPrompt = "You are a helpful assistant."
        };

        // Act
        bool result = request.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_EmptySystemPrompt_ReturnsFalse()
    {
        // Arrange
        var request = new AnalysisRequest
        {
            ImageData = new byte[] { 1, 2, 3, 4 },
            ImageFormat = "PNG",
            WindowTitle = "Test Window",
            SystemPrompt = ""
        };

        // Act
        bool result = request.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_InvalidTemperature_ReturnsFalse()
    {
        // Arrange
        var request = new AnalysisRequest
        {
            ImageData = new byte[] { 1, 2, 3, 4 },
            ImageFormat = "PNG",
            WindowTitle = "Test Window",
            SystemPrompt = "You are a helpful assistant.",
            Temperature = 1.5f // Invalid: > 1.0
        };

        // Act
        bool result = request.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_InvalidTopP_ReturnsFalse()
    {
        // Arrange
        var request = new AnalysisRequest
        {
            ImageData = new byte[] { 1, 2, 3, 4 },
            ImageFormat = "PNG",
            WindowTitle = "Test Window",
            SystemPrompt = "You are a helpful assistant.",
            TopP = -0.1f // Invalid: < 0.0
        };

        // Act
        bool result = request.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_InvalidMaxTokens_ReturnsFalse()
    {
        // Arrange
        var request = new AnalysisRequest
        {
            ImageData = new byte[] { 1, 2, 3, 4 },
            ImageFormat = "PNG",
            WindowTitle = "Test Window",
            SystemPrompt = "You are a helpful assistant.",
            MaxTokens = 0 // Invalid: must be > 0
        };

        // Act
        bool result = request.IsValid();

        // Assert
        Assert.False(result);
    }
}
