using CortexView.Domain.Entities;

namespace CortexView.Domain.Tests.Entities;

/// <summary>
/// Unit tests for the Persona entity.
/// </summary>
public class PersonaTests
{
    [Fact]
    public void IsValid_ValidPersona_ReturnsTrue()
    {
        // Arrange
        var persona = new Persona
        {
            Name = "Test Persona",
            SystemPrompt = "You are a helpful assistant.",
            Temperature = 0.7f,
            TopP = 0.9f,
            MaxTokens = 1000
        };

        // Act
        bool result = persona.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_EmptyName_ReturnsFalse()
    {
        // Arrange
        var persona = new Persona
        {
            Name = "",
            SystemPrompt = "You are a helpful assistant.",
            Temperature = 0.7f,
            TopP = 0.9f,
            MaxTokens = 1000
        };

        // Act
        bool result = persona.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_EmptySystemPrompt_ReturnsFalse()
    {
        // Arrange
        var persona = new Persona
        {
            Name = "Test Persona",
            SystemPrompt = "",
            Temperature = 0.7f,
            TopP = 0.9f,
            MaxTokens = 1000
        };

        // Act
        bool result = persona.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_InvalidTemperature_ReturnsFalse()
    {
        // Arrange
        var persona = new Persona
        {
            Name = "Test Persona",
            SystemPrompt = "You are a helpful assistant.",
            Temperature = 1.5f, // Invalid: > 1.0
            TopP = 0.9f,
            MaxTokens = 1000
        };

        // Act
        bool result = persona.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_InvalidTopP_ReturnsFalse()
    {
        // Arrange
        var persona = new Persona
        {
            Name = "Test Persona",
            SystemPrompt = "You are a helpful assistant.",
            Temperature = 0.7f,
            TopP = -0.1f, // Invalid: < 0.0
            MaxTokens = 1000
        };

        // Act
        bool result = persona.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_InvalidMaxTokens_ReturnsFalse()
    {
        // Arrange
        var persona = new Persona
        {
            Name = "Test Persona",
            SystemPrompt = "You are a helpful assistant.",
            Temperature = 0.7f,
            TopP = 0.9f,
            MaxTokens = 0 // Invalid: must be > 0
        };

        // Act
        bool result = persona.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ToString_ReturnsName()
    {
        // Arrange
        var persona = new Persona
        {
            Name = "Test Persona",
            SystemPrompt = "You are a helpful assistant."
        };

        // Act
        string result = persona.ToString();

        // Assert
        Assert.Equal("Test Persona", result);
    }
}
