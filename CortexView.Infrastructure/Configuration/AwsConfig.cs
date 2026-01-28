namespace CortexView.Infrastructure.Configuration;

/// <summary>
/// Configuration for AWS Bedrock AI service.
/// </summary>
public sealed class AwsConfig
{
    /// <summary>
    /// Gets or sets the AWS region (e.g., "us-east-1").
    /// </summary>
    public string AwsRegion { get; set; } = "us-east-1";

    /// <summary>
    /// Gets or sets the Bedrock model ID (e.g., "anthropic.claude-3-sonnet-20240229-v1:0").
    /// </summary>
    public string ModelId { get; set; } = "anthropic.claude-3-sonnet-20240229-v1:0";
}
