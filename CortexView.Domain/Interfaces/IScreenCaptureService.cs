namespace CortexView.Domain.Interfaces;

/// <summary>
/// Defines the contract for screen and window capture services.
/// </summary>
/// <remarks>
/// Implementations provide platform-specific screen capture functionality,
/// returning image data as byte arrays in PNG format.
/// </remarks>
public interface IScreenCaptureService
{
    /// <summary>
    /// Captures a screenshot of the specified window asynchronously.
    /// </summary>
    /// <param name="windowHandle">The handle (HWND) of the window to capture.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the image data in PNG format.
    /// </returns>
    Task<byte[]> CaptureWindowAsync(
        IntPtr windowHandle,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the bounding rectangle of the specified window.
    /// </summary>
    /// <param name="windowHandle">The handle (HWND) of the window.</param>
    /// <returns>
    /// A tuple containing (X, Y, Width, Height) of the window rectangle.
    /// </returns>
    (int X, int Y, int Width, int Height) GetWindowRect(IntPtr windowHandle);
}
