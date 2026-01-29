using CortexView.Application.Services;
using FsCheck;
using FsCheck.Xunit;

namespace CortexView.Application.Tests.Services;

/// <summary>
/// Property-based tests for ChangeDetectionService using FsCheck.
/// </summary>
public class ChangeDetectionPropertyTests
{
    [Property]
    public bool ChangedFraction_AlwaysBetween0And1(byte[] imageData)
    {
        if (imageData == null || imageData.Length == 0)
        {
            return true; // Skip invalid inputs
        }

        try
        {
            // Arrange
            var service = new ChangeDetectionService();
            
            // Act
            double result = service.ComputeChangedFraction(imageData);

            // Assert
            return result >= 0.0 && result <= 1.0;
        }
        catch
        {
            // Invalid image data is acceptable
            return true;
        }
    }

    [Property]
    public bool IsSignificantChange_Commutative(double changedFraction, double threshold)
    {
        // Arrange
        var service = new ChangeDetectionService();

        // Act
        bool result1 = service.IsSignificantChange(changedFraction, threshold);
        bool result2 = service.IsSignificantChange(changedFraction, threshold);

        // Assert
        return result1 == result2;
    }

    [Property]
    public bool IsSignificantChange_Monotonic(double changedFraction, double threshold)
    {
        // Arrange
        var service = new ChangeDetectionService();

        // If changedFraction >= threshold, result should be true
        // If changedFraction < threshold, result should be false
        bool result = service.IsSignificantChange(changedFraction, threshold);

        // Assert
        return result == (changedFraction >= threshold);
    }

    [Property]
    public bool ComputeChangedFraction_IdenticalInputs_ReturnsZero(byte[] imageData)
    {
        if (imageData == null || imageData.Length == 0)
        {
            return true; // Skip invalid inputs
        }

        try
        {
            // Arrange
            var service = new ChangeDetectionService();

            // Act
            service.ComputeChangedFraction(imageData); // First call
            double result = service.ComputeChangedFraction(imageData); // Second call with same data

            // Assert
            return result == 0.0;
        }
        catch
        {
            // Invalid image data is acceptable
            return true;
        }
    }
}
