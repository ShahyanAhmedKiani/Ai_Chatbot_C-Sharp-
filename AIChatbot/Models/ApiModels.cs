using Newtonsoft.Json;

namespace AIChatbot.Models
{
    // ── Request Models ──────────────────────────────────────────────────────────

    public class GeminiRequest
    {
        [JsonProperty("system_instruction")]
        public GeminiSystemInstruction? SystemInstruction { get; set; }

        [JsonProperty("contents")]
        public List<GeminiContent> Contents { get; set; } = new();

        [JsonProperty("generationConfig")]
        public GeminiGenerationConfig? GenerationConfig { get; set; }
    }

    public class GeminiSystemInstruction
    {
        [JsonProperty("parts")]
        public List<GeminiPart> Parts { get; set; } = new();
    }

    public class GeminiContent
    {
        [JsonProperty("role")]
        public string Role { get; set; } = string.Empty;   // "user" or "model"

        [JsonProperty("parts")]
        public List<GeminiPart> Parts { get; set; } = new();
    }

    public class GeminiPart
    {
        [JsonProperty("text")]
        public string Text { get; set; } = string.Empty;
    }

    public class GeminiGenerationConfig
    {
        [JsonProperty("maxOutputTokens")]
        public int MaxOutputTokens { get; set; } = 1024;

        [JsonProperty("temperature")]
        public double Temperature { get; set; } = 0.9;
    }

    // ── Response Models ─────────────────────────────────────────────────────────

    public class GeminiResponse
    {
        [JsonProperty("candidates")]
        public List<GeminiCandidate>? Candidates { get; set; }

        [JsonProperty("usageMetadata")]
        public GeminiUsage? UsageMetadata { get; set; }

        [JsonProperty("error")]
        public GeminiError? Error { get; set; }

        public string GetText()
        {
            return Candidates?
                .FirstOrDefault()?.Content?.Parts?
                .FirstOrDefault()?.Text ?? string.Empty;
        }
    }

    public class GeminiCandidate
    {
        [JsonProperty("content")]
        public GeminiContent? Content { get; set; }

        [JsonProperty("finishReason")]
        public string? FinishReason { get; set; }
    }

    public class GeminiUsage
    {
        [JsonProperty("promptTokenCount")]
        public int PromptTokenCount { get; set; }

        [JsonProperty("candidatesTokenCount")]
        public int CandidatesTokenCount { get; set; }

        public int TotalTokens => PromptTokenCount + CandidatesTokenCount;
    }

    public class GeminiError
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string? Message { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }
    }
}
