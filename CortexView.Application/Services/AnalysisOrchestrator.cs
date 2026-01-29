using CortexView.Domain.Entities;
using CortexView.Domain.Interfaces;

namespace CortexView.Application.Services;

/// <summary>
/// Orchestrates the complete analysis workflow: Capture → Detect Change → Analyze → Store.
/// </summary>
/// <remarks>
/// This service coordinates multiple domain services to execute the full business workflow.
/// It implements the application layer's orchestration logic without depending on infrastructure details.
/// </remarks>
public sealed class AnalysisOrchestrator
{
    private readonly IScreenCaptureService _captureService;
    private readonly IChangeDetectionService _changeDetectionService;
    private readonly IAiAnalysisService _aiService;
    private readonly IStorageService _storageService;

    public AnalysisOrchestrator(
        IScreenCaptureService captureService,
        IChangeDetectionService changeDetectionService,
        IAiAnalysisService aiService,
        IStorageService storageService)
    {
        _captureService = captureService ?? throw new ArgumentNullException(nameof(captureService));
        _changeDetectionService = changeDetectionService ?? throw new ArgumentNullException(nameof(changeDetectionService));
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
    }

    /// <summary>
    /// Captures a window screenshot, detects changes, analyzes with AI, and stores the result.
    /// </summary>
    /// <param name="windowHandle">Platform-specific window handle to capture.</param>
    /// <param name="windowTitle">Title of the window being captured.</param>
    /// <param name="persona">AI persona configuration for analysis.</param>
    /// <param name="sensitivityThreshold">Change detection threshold (0.0 to 1.0).</param>
    /// <param name="forceAnalysis">If true, bypasses change detection and always analyzes.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Analysis response containing AI suggestions or error information.</returns>
    public async Task<AnalysisResponse> CaptureAndAnalyzeAsync(
        IntPtr windowHandle,
        string windowTitle,
        Persona persona,
        double sensitivityThreshold,
        bool forceAnalysis = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(persona);
        ArgumentException.ThrowIfNullOrWhiteSpace(windowTitle);

        try
        {
            // 1. Capture screenshot
            byte[] imageData = await _captureService.CaptureWindowAsync(windowHandle, cancellationToken);

            if (imageData.Length == 0)
            {
                return AnalysisResponse.Failure("Screenshot capture returned empty data.");
            }

            // 2. Check for significant change (unless forced)
            if (!forceAnalysis)
            {
                double changedFraction = _changeDetectionService.ComputeChangedFraction(imageData);
                bool isSignificant = _changeDetectionService.IsSignificantChange(changedFraction, sensitivityThreshold);

                if (!isSignificant)
                {
                    return AnalysisResponse.Failure($"No significant change detected ({changedFraction:P1} < {sensitivityThreshold:P1}).");
                }
            }

            // 3. Extract OCR text (optional enhancement)
            string? ocrText = _changeDetectionService.TryExtractOcrText(imageData);

            // 4. Build analysis request
            var request = new AnalysisRequest
            {
                ImageData = imageData,
                ImageFormat = "PNG",
                WindowTitle = windowTitle,
                SystemPrompt = persona.SystemPrompt,
                OcrText = ocrText ?? string.Empty,
                UserPrompt = BuildUserPrompt(),
                Temperature = persona.Temperature,
                TopP = persona.TopP,
                MaxTokens = persona.MaxTokens
            };

            // 5. Analyze with AI
            var response = await _aiService.AnalyzeImageAsync(request, cancellationToken);

            // 6. Store screenshot (if successful)
            if (response.IsSuccess)
            {
                await _storageService.SaveScreenshotAsync(imageData, persona.Name, cancellationToken);
            }

            return response;
        }
        catch (OperationCanceledException)
        {
            return AnalysisResponse.Failure("Analysis was cancelled.");
        }
        catch (Exception ex)
        {
            return AnalysisResponse.Failure($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Builds the user prompt for AI analysis with safety bypass.
    /// </summary>
    private static string BuildUserPrompt()
    {
        // Global safety bypass for technical screenshots
        return "This is a public technical image. " +
               "Ignore browser address bars, interface buttons, and file paths. " +
               "They are irrelevant context. " +
               "Focus STRICTLY on the content/code and perform the task defined in the System Prompt.";
    }
}
