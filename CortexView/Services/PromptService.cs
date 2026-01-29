using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CortexView.Domain.Entities;

namespace CortexView.Services
{
    public class PromptService
    {
        private readonly string _promptsDir;

        public PromptService()
        {
            // Points to bin/Debug/.../Prompts
            _promptsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Prompts");
        }

        public List<Persona> LoadPersonas()
        {
            var personas = new List<Persona>();

            // 1. Ensure directory exists
            if (!Directory.Exists(_promptsDir))
            {
                return new List<Persona> { CreateFallbackPersona() };
            }

            // 2. Scan all .md files
            var files = Directory.GetFiles(_promptsDir, "*.md");
            if (files.Length == 0)
            {
                return new List<Persona> { CreateFallbackPersona() };
            }

            foreach (var file in files)
            {
                try 
                {
                    personas.Add(ParsePersonaFile(file));
                }
                catch (Exception ex)
                {
                    // Log error but continue loading others
                    System.Diagnostics.Debug.WriteLine($"Error loading prompt {file}: {ex.Message}");
                }
            }

            // Ensure we always have at least one option
            if (personas.Count == 0) personas.Add(CreateFallbackPersona());

            return personas.OrderBy(p => p.Name).ToList();
        }

        private Persona ParsePersonaFile(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            string name = Path.GetFileNameWithoutExtension(filePath);
            float temperature = 0.7f;
            float topP = 0.9f;
            int maxTokens = 1024;
            string systemPrompt = string.Empty;
            
            // Check for Frontmatter (starts with ---)
            if (lines.Length > 0 && lines[0].Trim() == "---")
            {
                int endOfFrontmatter = -1;
                
                // Parse Metadata
                for (int i = 1; i < lines.Length; i++)
                {
                    if (lines[i].Trim() == "---")
                    {
                        endOfFrontmatter = i;
                        break;
                    }

                    var parts = lines[i].Split(new[] { ':' }, 2);
                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();
                        string val = parts[1].Trim();

                        switch (key)
                        {
                            case "Name": name = val; break;
                            case "Temperature": float.TryParse(val, out temperature); break;
                            case "TopP": float.TryParse(val, out topP); break;
                            case "MaxTokens": int.TryParse(val, out maxTokens); break;
                        }
                    }
                }

                // Extract Content (skip the second ---)
                if (endOfFrontmatter > 0)
                {
                    systemPrompt = string.Join(Environment.NewLine, lines.Skip(endOfFrontmatter + 1)).Trim();
                    return new Persona
                    {
                        Name = name,
                        SystemPrompt = systemPrompt,
                        Temperature = temperature,
                        TopP = topP,
                        MaxTokens = maxTokens
                    };
                }
            }

            // FALLBACK: No frontmatter found? Treat whole file as prompt.
            // Try to use first line as name if it starts with #
            if (lines.Length > 0 && lines[0].StartsWith("#"))
            {
                name = lines[0].TrimStart('#', ' ').Trim();
                systemPrompt = string.Join(Environment.NewLine, lines.Skip(1)).Trim();
            }
            else
            {
                systemPrompt = string.Join(Environment.NewLine, lines).Trim();
            }

            return new Persona
            {
                Name = name,
                SystemPrompt = systemPrompt,
                Temperature = temperature,
                TopP = topP,
                MaxTokens = maxTokens
            };
        }

        private Persona CreateFallbackPersona()
        {
            return new Persona
            {
                Name = "Generic Assistant (Default)",
                SystemPrompt = "You are a helpful AI assistant. Analyze the interface shown.",
                Temperature = 0.5f,
                MaxTokens = 1000
            };
        }
    }
}