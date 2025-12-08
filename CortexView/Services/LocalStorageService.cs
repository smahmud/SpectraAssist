using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using CortexView.Models;

namespace CortexView.Services
{
    public class LocalStorageService : IStorageService
    {
        private readonly AppConfig _config;

        public LocalStorageService(AppConfig config)
        {
            _config = config;
        }

        private string GetStoragePath()
        {
            if (string.IsNullOrWhiteSpace(_config.StorageConfig.StoragePath))
            {
                // Default: MyDocuments/CortexView_Captures
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CortexView_Captures");
            }
            return _config.StorageConfig.StoragePath;
        }

        public async Task<string?> SaveScreenshotAsync(Bitmap bitmap, string personaName)
        {
            if (!_config.StorageConfig.Enabled) return null;

            return await Task.Run(() =>
            {
                try
                {
                    string dir = GetStoragePath();
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                    // Sanitize filename
                    string safePersona = string.Join("_", personaName.Split(Path.GetInvalidFileNameChars()));
                    string filename = $"{timestamp}_{safePersona}.png";
                    string fullPath = Path.Combine(dir, filename);

                    // Clone to avoid GDI+ locking issues
                    using (var clone = new Bitmap(bitmap))
                    {
                        clone.Save(fullPath, ImageFormat.Png);
                    }
                    return fullPath;
                }
                catch { return null; }
            });
        }

        public async Task CleanupOldFilesAsync()
        {
            if (!_config.StorageConfig.Enabled) return;

            await Task.Run(() =>
            {
                try
                {
                    string dir = GetStoragePath();
                    if (!Directory.Exists(dir)) return;

                    var threshold = DateTime.Now.AddDays(-_config.StorageConfig.RetentionDays);
                    foreach (var file in Directory.GetFiles(dir))
                    {
                        if (File.GetCreationTime(file) < threshold) File.Delete(file);
                    }
                }
                catch { /* Log error in future */ }
            });
        }

        public async Task PurgeAllAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    string dir = GetStoragePath();
                    if (Directory.Exists(dir))
                    {
                        Directory.Delete(dir, true); // True = Recursive delete
                        Directory.CreateDirectory(dir);
                    }
                }
                catch { /* Log error in future */ }
            });
        }
    }
}