using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Amazon;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using CortexView.Models;
using System.Drawing;
using System.Drawing.Imaging;

namespace CortexView.Services
{
    public class AwsBedrockService : IAiAnalysisService
    {
        private readonly AppConfig _config;
        private readonly AmazonBedrockRuntimeClient _client;

        private const string AnthropicApiVersion = "bedrock-2023-05-31";

        public AwsBedrockService(AppConfig config)
        {
            _config = config;
            
            // Uses default AWS Credential Chain (Profile or Env Vars)
            // Ensure you have run `aws configure` or set AWS_PROFILE
            var region = RegionEndpoint.GetBySystemName(_config.AiServiceConfig.AwsRegion);
            _client = new AmazonBedrockRuntimeClient(region);
        }

        public async Task<AnalysisResponse> AnalyzeImageAsync(AnalysisRequest request)
        {
            try
            {
                // 1. Convert Image to Base64
                string base64Image = ConvertImageToBase64(request.ScreenImage);

                // 2. Construct Claude 3 Message Payload (JSON)
                // We use JsonNode for flexible construction without strict DTOs
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
                                // A. The Image
                                new JsonObject
                                {
                                    ["type"] = "image",
                                    ["source"] = new JsonObject
                                    {
                                        ["type"] = "base64",
                                        ["media_type"] = "image/png",
                                        ["data"] = base64Image
                                    }
                                },
                                // B. The Text Context
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
                    ModelId = _config.AiServiceConfig.ModelId,
                    ContentType = "application/json",
                    Accept = "application/json",
                    Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonBody))
                };

                var response = await _client.InvokeModelAsync(invokeRequest);

                // 4. Parse Response
                using var reader = new StreamReader(response.Body);
                string responseBody = await reader.ReadToEndAsync();
                
                var responseNode = JsonNode.Parse(responseBody);
                string contentText = responseNode?["content"]?[0]?["text"]?.ToString() ?? "No content returned.";
                int inputTokens = responseNode?["usage"]?["input_tokens"]?.GetValue<int>() ?? 0;
                int outputTokens = responseNode?["usage"]?["output_tokens"]?.GetValue<int>() ?? 0;

                return AnalysisResponse.Success(contentText, inputTokens + outputTokens);
            }
            catch (Exception ex)
            {
                return AnalysisResponse.Failure($"AWS Bedrock Error: {ex.Message}");
            }
        }

        private string ConstructUserMessage(AnalysisRequest request)
        {
            // Combine Window Title, User Prompt, and OCR text into one context block
            return $@"
                Window Title: {request.WindowTitle}
                Context: {request.UserPrompt}
                {(string.IsNullOrWhiteSpace(request.OcrText) ? "" : $"Extracted Text: {request.OcrText}")}
                ";
        }

        private string ConvertImageToBase64(Bitmap bitmap)
        {
            using var ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            byte[] byteImage = ms.ToArray();
            return Convert.ToBase64String(byteImage);
        }
    }
}