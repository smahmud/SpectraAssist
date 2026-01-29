namespace CortexView.Domain.Entities;

/// <summary>
/// Represents the result of an AI image analysis with immutable properties.
/// </summary>
/// <remarks>
/// Contains the AI-generated suggestion text, success status, error information,
/// token usage, and timestamp. Use factory methods for creation.
/// </remarks>
public sealed class AnalysisResponse
{
    /// <summary>
    /// Gets the AI-generated suggestion or analysis text.
    /// </summary>
    public required string SuggestionText { get; init; }

    /// <summary>
    /// Gets a value indicating whether the analysis was successful.
    /// </summary>
    public required bool IsSuccess { get; init; }

    /// <summary>
    /// Gets the error message if the analysis failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets the number of tokens used in the AI response.
    /// </summary>
    public int TokenUsage { get; init; }

    /// <summary>
    /// Gets the timestamp when the analysis was completed.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a successful analysis response.
    /// </summary>
    /// <param name="suggestionText">The AI-generated suggestion text.</param>
    /// <param name="tokenUsage">The number of tokens used (default: 0).</param>
    /// <returns>A successful <see cref="AnalysisResponse"/>.</returns>
    public static AnalysisResponse Success(string suggestionText, int tokenUsage = 0)
    {
        return new AnalysisResponse
        {
            SuggestionText = suggestionText,
            IsSuccess = true,
            TokenUsage = tokenUsage,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a failed analysis response.
    /// </summary>
    /// <param name="errorMessage">The error message describing the failure.</param>
    /// <returns>A failed <see cref="AnalysisResponse"/>.</returns>
    public static AnalysisResponse Failure(string errorMessage)
    {
        return new AnalysisResponse
        {
            SuggestionText = "Analysis failed.",
            IsSuccess = false,
            ErrorMessage = errorMessage,
            TokenUsage = 0,
            Timestamp = DateTime.UtcNow
        };
    }
}
