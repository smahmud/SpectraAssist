namespace CortexView.Domain.Entities;

/// <summary>
/// Represents a request for AI image analysis with immutable properties.
/// </summary>
/// <remarks>
/// Contains the image data, context information, and AI model parameters
/// needed to perform analysis. Uses byte[] instead of Bitmap for platform independence.
/// </remarks>
public sealed class AnalysisRequest
{
    /// <summary>
    /// Gets the image data in PNG format.
    /// </summary>
    public required byte[] ImageData { get; init; }

    /// <summary>
    /// Gets the image format (e.g., "PNG", "JPEG").
    /// </summary>
    public string ImageFormat { get; init; } = "PNG";

    /// <summary>
    /// Gets the OCR-extracted text from the image (if available).
    /// </summary>
    public string OcrText { get; init; } = string.Empty;

    /// <summary>
    /// Gets the title of the captured window.
    /// </summary>
    public required string WindowTitle { get; init; }

    /// <summary>
    /// Gets the user's custom prompt for analysis.
    /// </summary>
    public string UserPrompt { get; init; } = "Analyze this window.";

    /// <summary>
    /// Gets the system prompt that defines AI behavior.
    /// </summary>
    public required string SystemPrompt { get; init; }

    /// <summary>
    /// Gets the temperature parameter for response randomness (0.0 to 1.0).
    /// </summary>
    public float Temperature { get; init; } = 0.5f;

    /// <summary>
    /// Gets the nucleus sampling parameter (0.0 to 1.0).
    /// </summary>
    public float TopP { get; init; } = 1.0f;

    /// <summary>
    /// Gets the maximum number of tokens in the AI response.
    /// </summary>
    public int MaxTokens { get; init; } = 1000;

    /// <summary>
    /// Validates the analysis request.
    /// </summary>
    /// <returns>True if all required properties are valid; otherwise, false.</returns>
    public bool IsValid()
    {
        return ImageData is { Length: > 0 }
            && !string.IsNullOrWhiteSpace(WindowTitle)
            && !string.IsNullOrWhiteSpace(SystemPrompt)
            && Temperature is >= 0.0f and <= 1.0f
            && TopP is >= 0.0f and <= 1.0f
            && MaxTokens > 0;
    }
}
