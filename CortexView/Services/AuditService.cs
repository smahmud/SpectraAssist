using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using CortexView.Models;

namespace CortexView.Services
{
    public class AuditService
    {
        private readonly AppConfig _config;

        public AuditService(AppConfig config)
        {
            _config = config;
        }

        private string GetLogPath()
        {
            string dir = string.IsNullOrWhiteSpace(_config.StorageConfig.StoragePath)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CortexView_Captures")
                : _config.StorageConfig.StoragePath;

            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            // Daily Rotation: audit_2025-12-08.json
            return Path.Combine(dir, $"audit_{DateTime.Now:yyyy-MM-dd}.json");
        }

        public async Task LogInteractionAsync(AuditEntry entry)
        {
            await Task.Run(() =>
            {
                try
                {
                    string filePath = GetLogPath();
                    string jsonLine = JsonSerializer.Serialize(entry);
                    
                    // Append line-by-line (NDJSON style) for performance and safety
                    File.AppendAllText(filePath, jsonLine + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Audit Error: {ex.Message}");
                }
            });
        }
    }
}