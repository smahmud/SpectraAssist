using System.Text.Json;
using CortexView.Domain.Entities;
using CortexView.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace CortexView.Infrastructure.Storage;

/// <summary>
/// Audit logging service for tracking AI interactions.
/// </summary>
/// <remarks>
/// Uses daily log rotation (audit_YYYY-MM-DD.json) with NDJSON format
/// for performance and safety.
/// </remarks>
public sealed class AuditService
{
    private readonly StorageConfig _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditService"/> class.
    /// </summary>
    /// <param name="config">Storage configuration options.</param>
    public AuditService(IOptions<StorageConfig> config)
    {
        _config = config.Value;
    }

    /// <summary>
    /// Logs an AI interaction to the audit log asynchronously.
    /// </summary>
    /// <param name="entry">The audit entry to log.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task LogInteractionAsync(
        AuditEntry entry,
        CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            try
            {
                string filePath = GetLogPath();
                string jsonLine = JsonSerializer.Serialize(entry);
                
                // Append line-by-line (NDJSON style) for performance and safety
                File.AppendAllText(filePath, jsonLine + Environment.NewLine);
            }
            catch
            {
                // Silently fail - audit logging is best-effort
            }
        }, cancellationToken);
    }

    private string GetLogPath()
    {
        string directory = string.IsNullOrWhiteSpace(_config.StoragePath)
            ? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "CortexView_Captures")
            : _config.StoragePath;

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Daily rotation: audit_2026-01-28.json
        return Path.Combine(directory, $"audit_{DateTime.Now:yyyy-MM-dd}.json");
    }
}
