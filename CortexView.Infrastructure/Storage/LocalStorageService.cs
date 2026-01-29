using CortexView.Domain.Interfaces;
using CortexView.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace CortexView.Infrastructure.Storage;

/// <summary>
/// Local file system implementation of storage service.
/// </summary>
public sealed class LocalStorageService : IStorageService
{
    private readonly StorageConfig _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalStorageService"/> class.
    /// </summary>
    /// <param name="config">Storage configuration options.</param>
    public LocalStorageService(IOptions<StorageConfig> config)
    {
        _config = config.Value;
    }

    /// <inheritdoc/>
    public async Task<string?> SaveScreenshotAsync(
        byte[] imageData,
        string personaName,
        CancellationToken cancellationToken = default)
    {
        if (!_config.Enabled)
        {
            return null;
        }

        return await Task.Run(() =>
        {
            try
            {
                string directory = GetStoragePath();
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string safePersona = SanitizeFileName(personaName);
                string filename = $"{timestamp}_{safePersona}.png";
                string fullPath = Path.Combine(directory, filename);

                File.WriteAllBytes(fullPath, imageData);
                return fullPath;
            }
            catch
            {
                // Silently fail - storage is optional
                return null;
            }
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task CleanupOldFilesAsync(CancellationToken cancellationToken = default)
    {
        if (!_config.Enabled)
        {
            return;
        }

        await Task.Run(() =>
        {
            try
            {
                string directory = GetStoragePath();
                if (!Directory.Exists(directory))
                {
                    return;
                }

                var threshold = DateTime.Now.AddDays(-_config.RetentionDays);
                foreach (var file in Directory.GetFiles(directory))
                {
                    if (File.GetCreationTime(file) < threshold)
                    {
                        File.Delete(file);
                    }
                }
            }
            catch
            {
                // Silently fail - cleanup is best-effort
            }
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task PurgeAllAsync(CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            try
            {
                string directory = GetStoragePath();
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, recursive: true);
                    Directory.CreateDirectory(directory);
                }
            }
            catch
            {
                // Silently fail - purge is user-initiated
            }
        }, cancellationToken);
    }

    private string GetStoragePath()
    {
        if (string.IsNullOrWhiteSpace(_config.StoragePath))
        {
            // Default: MyDocuments/CortexView_Captures
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "CortexView_Captures");
        }

        return _config.StoragePath;
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars));
    }
}
