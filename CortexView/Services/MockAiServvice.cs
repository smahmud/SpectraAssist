using System;
using System.Threading.Tasks;
using CortexView.Models;

namespace CortexView.Services
{
    public class MockAiService : IAiAnalysisService
    {
        // Simulate a 2-second delay to test the "Thinking..." UI
        private const int SimulationDelayMs = 2000;

        public async Task<AnalysisResponse> AnalyzeImageAsync(AnalysisRequest request)
        {
            // 1. Simulate Network Latency
            await Task.Delay(SimulationDelayMs);

            // 2. Return Fake "Claude" Response
            // We use Markdown formatting to test if the UI handles it cleanly.
            string fakeAiResponse = 
                $"### Analysis of '{request.WindowTitle}'\n\n" +
                "I see you are looking at a window. Here is a simulated analysis based on the screenshot provided:\n\n" +
                "* **Window Title:** " + request.WindowTitle + "\n" +
                "* **Content Detected:** Standard user interface elements.\n" +
                "* **OCR Text Length:** " + request.OcrText.Length + " chars\n\n" +
                "> **Note:** This is a mock response from `MockAiService`. No data was sent to AWS.";

            return AnalysisResponse.Success(fakeAiResponse, 42); // 42 dummy tokens
        }
    }
}