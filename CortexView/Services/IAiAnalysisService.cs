using System.Threading.Tasks;
using CortexView.Models;

namespace CortexView.Services
{
    public interface IAiAnalysisService
    {
        /// <summary>
        /// Analyzes the provided screenshot and returns an AI-generated suggestion.
        /// </summary>
        Task<AnalysisResponse> AnalyzeImageAsync(AnalysisRequest request);
    }
}