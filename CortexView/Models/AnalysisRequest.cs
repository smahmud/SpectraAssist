using System.Drawing; // For Bitmap

namespace CortexView.Models
{
    public class AnalysisRequest
    {
        public Bitmap ScreenImage { get; set; }
        public string OcrText { get; set; }
        public string WindowTitle { get; set; }
        public string UserPrompt { get; set; }

        public string SystemPrompt { get; set; } = "You are a helpful assistant.";
        public float Temperature { get; set; } = 0.5f;
        public float TopP { get; set; } = 1.0f;
        public int MaxTokens { get; set; } = 1000;
        
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