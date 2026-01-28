using CortexView.Domain.Entities;

namespace CortexView.Domain.Interfaces;

/// <summary>
/// Defines the contract for AI image analysis services.
/// </summary>
/// <remarks>
/// Implementations provide AI-powered analysis of screenshot images,
/// returning suggestions or insights based on the visual content and context.
/// </remarks>
public interface IAiAnalysisService
{
    /// <summary>
    /// Analyzes an image asynchronously using AI.
    /// </summary>
    /// <param name="request">The analysis request containing image data and parameters.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the analysis response with suggestions or error information.
    /// </returns>
    Task<AnalysisResponse> AnalyzeImageAsync(
        AnalysisRequest request,
        CancellationToken cancellationToken = default);
}
