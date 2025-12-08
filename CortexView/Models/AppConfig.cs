using CortexView.Services;

namespace CortexView.Models
{
    public class AppConfig
    {
        public AiServiceOptions AiServiceConfig { get; set; } = new AiServiceOptions();

        public StorageConfig StorageConfig { get; set; } = new StorageConfig();
    }

    public class AiServiceOptions
    {
        public string Provider { get; set; } = "Mock";
        public string AwsRegion { get; set; } = "us-east-1";
        public string ModelId { get; set; } = "anthropic.claude-3-sonnet-20240229-v1:0";
    }

    public class StorageConfig
    {
        public bool Enabled { get; set; } = false;
        public string StoragePath { get; set; } = string.Empty;
        public int RetentionDays { get; set; } = 7;
    }
}