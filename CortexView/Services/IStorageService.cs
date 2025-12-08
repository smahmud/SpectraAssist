using System.Drawing;
using System.Threading.Tasks;

namespace CortexView.Services
{
    public interface IStorageService
    {
        Task<string?> SaveScreenshotAsync(Bitmap bitmap, string personaName);
        Task CleanupOldFilesAsync();
        Task PurgeAllAsync();
    }
}