using System.Text;
using System.Text.Json.Nodes;
using Amazon;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using CortexView.Domain.Entities;
using CortexView.Domain.Interfaces;
using CortexView.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace CortexView.Infrastructure.AI;

/// <summary>
/// AWS Bedrock implementation of AI analysis service using Claude 3 models.
/// </summary>
public sealed class AwsBedrockService : IAiAnalysisService
{
    private readonly AwsConfig _config;
    private readonly AmazonBedrockRuntimeClient _client;
    private const string AnthropicApiVersion = "bedrock-2023-05-31";

    /// <summary>
    /// Initializes a new instance of the <see cref="AwsBedrockService"/> class.
    /// </summary>
    /// <param name="config">AWS configuration options.</param>
    public AwsBedrockService(IOptions<AwsConfig> config)
    {
        _config = config.Value;
        
        // Uses default AWS Credential Chain (Profile or Environment Variables)
        var region = RegionEndpoint.GetBySystemName(_config.AwsRegion);
        _client = new AmazonBedrockRuntimeClient(region);
    }

    /// <inheritdoc/>
    public async Task<AnalysisResponse> AnalyzeImageAsync(
        AnalysisRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Convert image bytes to Base64
            string base64Image = Convert.ToBase64String(request.ImageData);

            // 2. Construct Claude 3 Message Payload
            var payload = new JsonObject
            {
                ["anthropic_version"] = AnthropicApiVersion,
                ["max_tokens"] = request.MaxTokens,
                ["temperature"] = request.Temperature,
                ["top_p"] = request.TopP,
                ["system"] = request.SystemPrompt,
                ["messages"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["role"] = "user",
                        ["content"] = new JsonArray
                        {
                            // Image content
                            new JsonObject
                            {
                                ["type"] = "image",
                                ["source"] = new JsonObject
                                {
                                    ["type"] = "base64",
                                    ["media_type"] = $"image/{request.ImageFormat.ToLowerInvariant()}",
                                    ["data"] = base64Image
                                }
                            },
                            // Text context
                            new JsonObject
                            {
                                ["type"] = "text",
                                ["text"] = ConstructUserMessage(request)
                            }
                        }
                    }
                }
            };

            string jsonBody = payload.ToJsonString();

            // 3. Call AWS Bedrock
            var invokeRequest = new InvokeModelRequest
            {
                ModelId = _config.ModelId,
                ContentType = "application/json",
                Accept = "application/json",
                Body = new MemoryStream(Encoding.UTF8.GetBytes(jsonBody))
            };

            var response = await _client.InvokeModelAsync(invokeRequest, cancellationToken);

            // 4. Parse Response
            using var reader = new StreamReader(response.Body);
            string responseBody = await reader.ReadToEndAsync(cancellationToken);
            
            var responseNode = JsonNode.Parse(responseBody);
            string contentText = responseNode?["content"]?[0]?["text"]?.ToString() 
                ?? "No content returned.";
            int inputTokens = responseNode?["usage"]?["input_tokens"]?.GetValue<int>() ?? 0;
            int outputTokens = responseNode?["usage"]?["output_tokens"]?.GetValue<int>() ?? 0;

            return AnalysisResponse.Success(contentText, inputTokens + outputTokens);
        }
        catch (Exception ex)
        {
            return AnalysisResponse.Failure($"AWS Bedrock Error: {ex.Message}");
        }
    }

    private static string ConstructUserMessage(AnalysisRequest request)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Window Title: {request.WindowTitle}");
        sb.AppendLine($"Context: {request.UserPrompt}");
        
        if (!string.IsNullOrWhiteSpace(request.OcrText))
        {
            sb.AppendLine($"Extracted Text: {request.OcrText}");
        }

        return sb.ToString();
    }
}
