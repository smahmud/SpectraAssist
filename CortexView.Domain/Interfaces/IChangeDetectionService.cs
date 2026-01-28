namespace CortexView.Domain.Interfaces;

/// <summary>
/// Defines the contract for image change detection services.
/// </summary>
/// <remarks>
/// Implementations provide algorithms to detect visual changes between
/// consecutive screenshots, enabling intelligent monitoring and analysis.
/// </remarks>
public interface IChangeDetectionService
{
    /// <summary>
    /// Computes the fraction of pixels that changed between the current and previous image.
    /// </summary>
    /// <param name="imageData">The current image data in PNG format.</param>
    /// <returns>
    /// A value between 0.0 (no change) and 1.0 (complete change) representing
    /// the fraction of changed pixels.
    /// </returns>
    double ComputeChangedFraction(byte[] imageData);

    /// <summary>
    /// Determines whether the change fraction exceeds the sensitivity threshold.
    /// </summary>
    /// <param name="changedFraction">The fraction of changed pixels (0.0 to 1.0).</param>
    /// <param name="sensitivityThreshold">The threshold for significant change (0.0 to 1.0).</param>
    /// <returns>True if the change is significant; otherwise, false.</returns>
    bool IsSignificantChange(double changedFraction, double sensitivityThreshold);

    /// <summary>
    /// Attempts to extract text from the image using OCR.
    /// </summary>
    /// <param name="imageData">The image data in PNG format.</param>
    /// <returns>
    /// The extracted text if successful; otherwise, null or empty string.
    /// </returns>
    string? TryExtractOcrText(byte[] imageData);
}
