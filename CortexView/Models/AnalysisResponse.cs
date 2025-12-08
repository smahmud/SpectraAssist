namespace CortexView.Models
{
    public class AnalysisResponse
    {
        public string SuggestionText { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public int TokenUsage { get; set; }

        // Success Factory
        public static AnalysisResponse Success(string text, int tokens = 0)
        {
            return new AnalysisResponse 
            { 
                SuggestionText = text, 
                IsSuccess = true, 
                TokenUsage = tokens 
            };
        }

        // Failure Factory
        public static AnalysisResponse Failure(string error)
        {
            return new AnalysisResponse 
            { 
                SuggestionText = "Analysis failed.", 
                IsSuccess = false, 
                ErrorMessage = error 
            };
        }
    }
}