namespace CortexView.Domain.Entities;

/// <summary>
/// Represents an AI assistant personality configuration with immutable properties.
/// </summary>
/// <remarks>
/// Personas define the behavior and response style of the AI assistant through
/// system prompts and model parameters (temperature, top-p, max tokens).
/// </remarks>
public sealed class Persona
{
    /// <summary>
    /// Gets the display name of the persona.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the system prompt that defines the persona's behavior and personality.
    /// </summary>
    public required string SystemPrompt { get; init; }

    /// <summary>
    /// Gets the temperature parameter for response randomness (0.0 to 1.0).
    /// Higher values produce more creative/random responses.
    /// </summary>
    public float Temperature { get; init; } = 0.5f;

    /// <summary>
    /// Gets the nucleus sampling parameter (0.0 to 1.0).
    /// Controls diversity via cumulative probability threshold.
    /// </summary>
    public float TopP { get; init; } = 1.0f;

    /// <summary>
    /// Gets the maximum number of tokens in the AI response.
    /// </summary>
    public int MaxTokens { get; init; } = 1000;

    /// <summary>
    /// Validates the persona configuration.
    /// </summary>
    /// <returns>True if all properties are valid; otherwise, false.</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Name)
            && !string.IsNullOrWhiteSpace(SystemPrompt)
            && Temperature is >= 0.0f and <= 1.0f
            && TopP is >= 0.0f and <= 1.0f
            && MaxTokens > 0;
    }

    /// <summary>
    /// Returns the persona name for display purposes.
    /// </summary>
    public override string ToString() => Name;
}
