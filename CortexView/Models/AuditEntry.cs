using System;

namespace CortexView.Models
{
    public class AuditEntry
    {
        public DateTime Timestamp { get; set; }
        public string Persona { get; set; }
        public string ImagePath { get; set; } // Null if storage disabled
        public int TokenUsage { get; set; }
        public string RequestContext { get; set; } // e.g., Window Title
    }
}