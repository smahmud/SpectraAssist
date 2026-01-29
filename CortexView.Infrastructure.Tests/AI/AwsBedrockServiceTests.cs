using CortexView.Domain.Entities;
using CortexView.Infrastructure.AI;
using CortexView.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace CortexView.Infrastructure.Tests.AI;

/// <summary>
/// Tests for AwsBedrockService.
/// Most tests are skipped by default as they require AWS credentials and incur costs.
/// </summary>
public class AwsBedrockServiceTests
{
    [Fact(Skip = "Requires AWS credentials and incurs costs")]
    public async Task AnalyzeImageAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var config = Options.Create(new AwsConfig
        {
            AwsRegion = "us-east-1",
            ModelId = "anthropic.claude-3-sonnet-20240229-v1:0"
        });

        var service = new AwsBedrockService(config);

        var persona = new Persona
        {
            Name = "Test Persona",
            SystemPrompt = "You are a helpful assistant.",
            Temperature = 0.7f,
            TopP = 0.9f,
            MaxTokens = 1000
        };

        // Create a simple 1x1 pixel PNG image
        byte[] imageData = CreateSimplePngImage();

        var request = new AnalysisRequest
        {
            ImageData = imageData,
            ImageFormat = "png",
            WindowTitle = "Test Window",
            UserPrompt = "Describe this image",
            SystemPrompt = persona.SystemPrompt,
            Temperature = persona.Temperature,
            TopP = persona.TopP,
            MaxTokens = persona.MaxTokens
        };

        // Act
        var result = await service.AnalyzeImageAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.SuggestionText);
        Assert.True(result.TokenUsage > 0);
    }

    [Fact]
    public void Constructor_ValidConfig_CreatesInstance()
    {
        // Arrange
        var config = Options.Create(new AwsConfig
        {
            AwsRegion = "us-east-1",
            ModelId = "anthropic.claude-3-sonnet-20240229-v1:0"
        });

        // Act
        var service = new AwsBedrockService(config);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public async Task AnalyzeImageAsync_NullRequest_ReturnsFailure()
    {
        // Arrange
        var config = Options.Create(new AwsConfig
        {
            AwsRegion = "us-east-1",
            ModelId = "anthropic.claude-3-sonnet-20240229-v1:0"
        });

        var service = new AwsBedrockService(config);

        // Act
        var result = await service.AnalyzeImageAsync(null!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Error", result.ErrorMessage);
    }

    [Fact(Skip = "Requires AWS credentials")]
    public async Task AnalyzeImageAsync_InvalidCredentials_ReturnsFailure()
    {
        // Arrange
        var config = Options.Create(new AwsConfig
        {
            AwsRegion = "us-east-1",
            ModelId = "anthropic.claude-3-sonnet-20240229-v1:0"
        });

        var service = new AwsBedrockService(config);

        var persona = new Persona
        {
            Name = "Test Persona",
            SystemPrompt = "You are a helpful assistant.",
            Temperature = 0.7f,
            TopP = 0.9f,
            MaxTokens = 1000
        };

        byte[] imageData = CreateSimplePngImage();

        var request = new AnalysisRequest
        {
            ImageData = imageData,
            ImageFormat = "png",
            WindowTitle = "Test Window",
            UserPrompt = "Describe this image",
            SystemPrompt = persona.SystemPrompt,
            Temperature = persona.Temperature,
            TopP = persona.TopP,
            MaxTokens = persona.MaxTokens
        };

        // Act
        var result = await service.AnalyzeImageAsync(request);

        // Assert
        // With invalid credentials, we expect a failure response
        Assert.False(result.IsSuccess);
        Assert.Contains("Error", result.ErrorMessage);
    }

    [Fact(Skip = "Requires AWS credentials")]
    public async Task AnalyzeImageAsync_CancellationRequested_ReturnsFailure()
    {
        // Arrange
        var config = Options.Create(new AwsConfig
        {
            AwsRegion = "us-east-1",
            ModelId = "anthropic.claude-3-sonnet-20240229-v1:0"
        });

        var service = new AwsBedrockService(config);

        var persona = new Persona
        {
            Name = "Test Persona",
            SystemPrompt = "You are a helpful assistant.",
            Temperature = 0.7f,
            TopP = 0.9f,
            MaxTokens = 1000
        };

        byte[] imageData = CreateSimplePngImage();

        var request = new AnalysisRequest
        {
            ImageData = imageData,
            ImageFormat = "png",
            WindowTitle = "Test Window",
            UserPrompt = "Describe this image",
            SystemPrompt = persona.SystemPrompt,
            Temperature = persona.Temperature,
            TopP = persona.TopP,
            MaxTokens = persona.MaxTokens
        };

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await service.AnalyzeImageAsync(request, cts.Token);

        // Assert
        Assert.False(result.IsSuccess);
    }

    /// <summary>
    /// Creates a minimal valid PNG image (1x1 pixel, white).
    /// </summary>
    private static byte[] CreateSimplePngImage()
    {
        // Minimal PNG: 1x1 white pixel
        return new byte[]
        {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, // PNG signature
            0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, // IHDR chunk
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, // 1x1 dimensions
            0x08, 0x02, 0x00, 0x00, 0x00, 0x90, 0x77, 0x53,
            0xDE, 0x00, 0x00, 0x00, 0x0C, 0x49, 0x44, 0x41, // IDAT chunk
            0x54, 0x08, 0xD7, 0x63, 0xF8, 0xFF, 0xFF, 0x3F,
            0x00, 0x05, 0xFE, 0x02, 0xFE, 0xDC, 0xCC, 0x59,
            0xE7, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, // IEND chunk
            0x44, 0xAE, 0x42, 0x60, 0x82
        };
    }
}
