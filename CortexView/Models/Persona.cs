namespace CortexView.Models
{
    public class Persona
    {
        public string Name { get; set; } = "Unknown";
        public string SystemPrompt { get; set; } = "";
        
        // Configuration Parameters with Defaults
        public float Temperature { get; set; } = 0.5f;
        public float TopP { get; set; } = 1.0f;
        public int MaxTokens { get; set; } = 1000;

        public override string ToString() => Name; // For ComboBox display
    }
}