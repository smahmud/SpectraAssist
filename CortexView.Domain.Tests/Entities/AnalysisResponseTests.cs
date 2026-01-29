using CortexView.Domain.Entities;

namespace CortexView.Domain.Tests.Entities;

/// <summary>
/// Unit tests for the AnalysisResponse entity.
/// </summary>
public class AnalysisResponseTests
{
    [Fact]
    public void Success_CreatesSuccessResponse()
    {
        // Arrange
        string suggestionText = "This is a test suggestion.";
        int tokenUsage = 150;

        // Act
        var response = AnalysisResponse.Success(suggestionText, tokenUsage);

        // Assert
        Assert.True(response.IsSuccess);
        Assert.Equal(suggestionText, response.SuggestionText);
        Assert.Equal(tokenUsage, response.TokenUsage);
        Assert.Null(response.ErrorMessage);
        Assert.True(response.Timestamp <= DateTime.UtcNow);
    }

    [Fact]
    public void Success_WithoutTokenUsage_DefaultsToZero()
    {
        // Arrange
        string suggestionText = "This is a test suggestion.";

        // Act
        var response = AnalysisResponse.Success(suggestionText);

        // Assert
        Assert.True(response.IsSuccess);
        Assert.Equal(suggestionText, response.SuggestionText);
        Assert.Equal(0, response.TokenUsage);
    }

    [Fact]
    public void Failure_CreatesFailureResponse()
    {
        // Arrange
        string errorMessage = "Test error message";

        // Act
        var response = AnalysisResponse.Failure(errorMessage);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal("Analysis failed.", response.SuggestionText);
        Assert.Equal(errorMessage, response.ErrorMessage);
        Assert.Equal(0, response.TokenUsage);
        Assert.True(response.Timestamp <= DateTime.UtcNow);
    }

    [Fact]
    public void Timestamp_IsSetToUtcNow()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var response = AnalysisResponse.Success("Test");
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.True(response.Timestamp >= beforeCreation);
        Assert.True(response.Timestamp <= afterCreation);
    }
}
