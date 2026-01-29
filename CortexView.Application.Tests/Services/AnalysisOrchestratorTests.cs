using CortexView.Application.Services;
using CortexView.Domain.Entities;
using CortexView.Domain.Interfaces;
using Moq;

namespace CortexView.Application.Tests.Services;

/// <summary>
/// Unit tests for the AnalysisOrchestrator using Moq for dependency mocking.
/// </summary>
public class AnalysisOrchestratorTests
{
    private readonly Mock<IScreenCaptureService> _mockCaptureService;
    private readonly Mock<IChangeDetectionService> _mockChangeDetectionService;
    private readonly Mock<IAiAnalysisService> _mockAiService;
    private readonly Mock<IStorageService> _mockStorageService;
    private readonly AnalysisOrchestrator _orchestrator;

    public AnalysisOrchestratorTests()
    {
        _mockCaptureService = new Mock<IScreenCaptureService>();
        _mockChangeDetectionService = new Mock<IChangeDetectionService>();
        _mockAiService = new Mock<IAiAnalysisService>();
        _mockStorageService = new Mock<IStorageService>();

        _orchestrator = new AnalysisOrchestrator(
            _mockCaptureService.Object,
            _mockChangeDetectionService.Object,
            _mockAiService.Object,
            _mockStorageService.Object);
    }

    [Fact]
    public async Task CaptureAndAnalyzeAsync_SignificantChange_CallsAiService()
    {
        // Arrange
        var persona = CreateTestPersona();
        byte[] imageData = new byte[] { 1, 2, 3, 4 };

        _mockCaptureService
            .Setup(x => x.CaptureWindowAsync(It.IsAny<IntPtr>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(imageData);

        _mockChangeDetectionService
            .Setup(x => x.ComputeChangedFraction(imageData))
            .Returns(0.15); // 15% changed

        _mockChangeDetectionService
            .Setup(x => x.IsSignificantChange(0.15, 0.10))
            .Returns(true);

        _mockAiService
            .Setup(x => x.AnalyzeImageAsync(It.IsAny<AnalysisRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(AnalysisResponse.Success("Test suggestion", 100));

        // Act
        var result = await _orchestrator.CaptureAndAnalyzeAsync(
            IntPtr.Zero,
            "Test Window",
            persona,
            0.10,
            forceAnalysis: false);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Test suggestion", result.SuggestionText);
        _mockAiService.Verify(x => x.AnalyzeImageAsync(It.IsAny<AnalysisRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockStorageService.Verify(x => x.SaveScreenshotAsync(imageData, persona.Name, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CaptureAndAnalyzeAsync_MinorChange_SkipsAnalysis()
    {
        // Arrange
        var persona = CreateTestPersona();
        byte[] imageData = new byte[] { 1, 2, 3, 4 };

        _mockCaptureService
            .Setup(x => x.CaptureWindowAsync(It.IsAny<IntPtr>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(imageData);

        _mockChangeDetectionService
            .Setup(x => x.ComputeChangedFraction(imageData))
            .Returns(0.05); // 5% changed

        _mockChangeDetectionService
            .Setup(x => x.IsSignificantChange(0.05, 0.10))
            .Returns(false);

        // Act
        var result = await _orchestrator.CaptureAndAnalyzeAsync(
            IntPtr.Zero,
            "Test Window",
            persona,
            0.10,
            forceAnalysis: false);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("No significant change detected", result.ErrorMessage);
        _mockAiService.Verify(x => x.AnalyzeImageAsync(It.IsAny<AnalysisRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockStorageService.Verify(x => x.SaveScreenshotAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CaptureAndAnalyzeAsync_ForceAnalysis_AlwaysAnalyzes()
    {
        // Arrange
        var persona = CreateTestPersona();
        byte[] imageData = new byte[] { 1, 2, 3, 4 };

        _mockCaptureService
            .Setup(x => x.CaptureWindowAsync(It.IsAny<IntPtr>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(imageData);

        _mockAiService
            .Setup(x => x.AnalyzeImageAsync(It.IsAny<AnalysisRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(AnalysisResponse.Success("Forced analysis", 100));

        // Act
        var result = await _orchestrator.CaptureAndAnalyzeAsync(
            IntPtr.Zero,
            "Test Window",
            persona,
            0.10,
            forceAnalysis: true); // Force analysis

        // Assert
        Assert.True(result.IsSuccess);
        _mockChangeDetectionService.Verify(x => x.ComputeChangedFraction(It.IsAny<byte[]>()), Times.Never);
        _mockAiService.Verify(x => x.AnalyzeImageAsync(It.IsAny<AnalysisRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CaptureAndAnalyzeAsync_EmptyImageData_ReturnsFailure()
    {
        // Arrange
        var persona = CreateTestPersona();
        byte[] emptyImageData = Array.Empty<byte>();

        _mockCaptureService
            .Setup(x => x.CaptureWindowAsync(It.IsAny<IntPtr>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyImageData);

        // Act
        var result = await _orchestrator.CaptureAndAnalyzeAsync(
            IntPtr.Zero,
            "Test Window",
            persona,
            0.10);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("empty data", result.ErrorMessage);
    }

    [Fact]
    public async Task CaptureAndAnalyzeAsync_NullPersona_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _orchestrator.CaptureAndAnalyzeAsync(
                IntPtr.Zero,
                "Test Window",
                null!,
                0.10));
    }

    [Fact]
    public async Task CaptureAndAnalyzeAsync_EmptyWindowTitle_ThrowsArgumentException()
    {
        // Arrange
        var persona = CreateTestPersona();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _orchestrator.CaptureAndAnalyzeAsync(
                IntPtr.Zero,
                "",
                persona,
                0.10));
    }

    [Fact]
    public async Task CaptureAndAnalyzeAsync_AiServiceFails_ReturnsFailureResponse()
    {
        // Arrange
        var persona = CreateTestPersona();
        byte[] imageData = new byte[] { 1, 2, 3, 4 };

        _mockCaptureService
            .Setup(x => x.CaptureWindowAsync(It.IsAny<IntPtr>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(imageData);

        _mockChangeDetectionService
            .Setup(x => x.ComputeChangedFraction(imageData))
            .Returns(0.15);

        _mockChangeDetectionService
            .Setup(x => x.IsSignificantChange(0.15, 0.10))
            .Returns(true);

        _mockAiService
            .Setup(x => x.AnalyzeImageAsync(It.IsAny<AnalysisRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(AnalysisResponse.Failure("AI service error"));

        // Act
        var result = await _orchestrator.CaptureAndAnalyzeAsync(
            IntPtr.Zero,
            "Test Window",
            persona,
            0.10);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("AI service error", result.ErrorMessage);
        _mockStorageService.Verify(x => x.SaveScreenshotAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CaptureAndAnalyzeAsync_CancellationRequested_ReturnsFailure()
    {
        // Arrange
        var persona = CreateTestPersona();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockCaptureService
            .Setup(x => x.CaptureWindowAsync(It.IsAny<IntPtr>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var result = await _orchestrator.CaptureAndAnalyzeAsync(
            IntPtr.Zero,
            "Test Window",
            persona,
            0.10,
            cancellationToken: cts.Token);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("cancelled", result.ErrorMessage);
    }

    private static Persona CreateTestPersona()
    {
        return new Persona
        {
            Name = "Test Persona",
            SystemPrompt = "You are a helpful assistant.",
            Temperature = 0.7f,
            TopP = 0.9f,
            MaxTokens = 1000
        };
    }
}
