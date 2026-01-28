namespace CortexView.Infrastructure.Configuration;

/// <summary>
/// Configuration for local storage and audit logging.
/// </summary>
public sealed class StorageConfig
{
    /// <summary>
    /// Gets or sets a value indicating whether local storage is enabled.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the directory path for storing screenshots and audit logs.
    /// </summary>
    public string StoragePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of days to retain files before automatic cleanup.
    /// </summary>
    public int RetentionDays { get; set; } = 30;
}
