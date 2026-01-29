namespace CortexView.Domain.Interfaces;

/// <summary>
/// Defines the contract for screenshot storage and file management services.
/// </summary>
/// <remarks>
/// Implementations provide file storage, cleanup, and purge operations
/// for screenshot images and related data.
/// </remarks>
public interface IStorageService
{
    /// <summary>
    /// Saves a screenshot image to storage asynchronously.
    /// </summary>
    /// <param name="imageData">The image data in PNG format.</param>
    /// <param name="personaName">The name of the persona used for the analysis.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the file path where the image was saved,
    /// or null if storage is disabled or failed.
    /// </returns>
    Task<string?> SaveScreenshotAsync(
        byte[] imageData,
        string personaName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes old files based on the configured retention policy asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CleanupOldFilesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all stored files and audit logs asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task PurgeAllAsync(CancellationToken cancellationToken = default);
}
