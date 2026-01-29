namespace CortexView.Domain.Entities;

/// <summary>
/// Represents an audit log entry for AI analysis operations with immutable properties.
/// </summary>
/// <remarks>
/// Tracks each AI interaction for accountability, including timestamp, persona used,
/// storage path, token usage, and request context.
/// </remarks>
public sealed class AuditEntry
{
    /// <summary>
    /// Gets the UTC timestamp when the analysis occurred.
    /// </summary>
    public required DateTime Timestamp { get; init; }

    /// <summary>
    /// Gets the name of the persona used for the analysis.
    /// </summary>
    public required string Persona { get; init; }

    /// <summary>
    /// Gets the file path where the screenshot was saved (null if storage disabled).
    /// </summary>
    public string? ImagePath { get; init; }

    /// <summary>
    /// Gets the number of tokens used in the AI response.
    /// </summary>
    public int TokenUsage { get; init; }

    /// <summary>
    /// Gets the request context (e.g., window title, application name).
    /// </summary>
    public string RequestContext { get; init; } = string.Empty;
}
