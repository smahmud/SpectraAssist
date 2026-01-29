using System.Text;
using CortexView.Domain.Entities;
using CortexView.Domain.Interfaces;

namespace CortexView.Infrastructure.AI;

/// <summary>
/// Mock implementation of AI analysis service for testing and development.
/// </summary>
/// <remarks>
/// Returns simulated responses without calling external AI services.
/// Useful for testing UI behavior and avoiding API costs during development.
/// </remarks>
public sealed class MockAiService : IAiAnalysisService
{
    private const int SimulationDelayMs = 2000;

    /// <inheritdoc/>
    public async Task<AnalysisResponse> AnalyzeImageAsync(
        AnalysisRequest request,
        CancellationToken cancellationToken = default)
    {
        // Simulate network latency
        await Task.Delay(SimulationDelayMs, cancellationToken);

        // Generate mock response with Markdown formatting
        var sb = new StringBuilder();
        sb.AppendLine($"### Analysis of '{request.WindowTitle}'");
        sb.AppendLine();
        sb.AppendLine("I see you are looking at a window. Here is a simulated analysis based on the screenshot provided:");
        sb.AppendLine();
        sb.AppendLine($"* **Window Title:** {request.WindowTitle}");
        sb.AppendLine("* **Content Detected:** Standard user interface elements.");
        sb.AppendLine($"* **OCR Text Length:** {request.OcrText.Length} chars");
        sb.AppendLine($"* **Image Size:** {request.ImageData.Length / 1024} KB");
        sb.AppendLine();
        sb.AppendLine("> **Note:** This is a mock response from `MockAiService`. No data was sent to AWS.");

        return AnalysisResponse.Success(sb.ToString(), tokenUsage: 42);
    }
}
