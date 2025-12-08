using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using CortexView.Services;
using CortexView.Models;
using System.Drawing; // Requires System.Drawing.Common package if missing

namespace CortexView.Tests.Services
{
    public class MockAiServiceTests
    {
        [Fact]
        public async Task AnalyzeImageAsync_ShouldReturnSuccess_AfterDelay()
        {
            // Arrange
            var service = new MockAiService();
            // Create a dummy 1x1 bitmap for the test
            using var dummyImage = new Bitmap(1, 1);
            var request = new AnalysisRequest(dummyImage, "Test Window");

            // Act
            var response = await service.AnalyzeImageAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.IsSuccess.Should().BeTrue();
            response.SuggestionText.Should().Contain("Test Window");
            response.SuggestionText.Should().Contain("MockAiService");
        }
    }
}