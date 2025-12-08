namespace CortexView
{
    public class AppConfig
    {
        public AiServiceOptions AiServiceConfig { get; set; } = new AiServiceOptions();
    }

    public class AiServiceOptions
    {
        public string Provider { get; set; } = "Mock";
        public string AwsRegion { get; set; } = "us-east-1";
        public string ModelId { get; set; } = "anthropic.claude-3-sonnet-20240229-v1:0";
    }
}