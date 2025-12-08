using System.Drawing; // For Bitmap

namespace CortexView.Models
{
    public class AnalysisRequest
    {
        public Bitmap ScreenImage { get; set; }
        public string OcrText { get; set; }
        public string WindowTitle { get; set; }
        public string UserPrompt { get; set; }
        
        // Constructor to ensure required fields
        public AnalysisRequest(Bitmap image, string windowTitle)
        {
            ScreenImage = image;
            WindowTitle = windowTitle;
            OcrText = string.Empty;
            UserPrompt = "Analyze this window.";
        }
    }
}