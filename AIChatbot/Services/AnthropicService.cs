using AIChatbot.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace AIChatbot.Services
{
    /// <summary>
    /// Handles all communication with the Google Gemini API (free tier).
    /// Get your free key at: https://aistudio.google.com/app/apikey
    /// </summary>
    public class GeminiService : IDisposable
    {
        // ── Constants ───────────────────────────────────────────────────────────
        private const string API_BASE = "https://generativelanguage.googleapis.com/v1beta/models";

        // ── Fields ──────────────────────────────────────────────────────────────
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly string _systemPrompt;
        private bool _disposed;

        // ── Events ──────────────────────────────────────────────────────────────
        public event EventHandler<string>? StatusChanged;

        // ── Constructor ─────────────────────────────────────────────────────────
        public GeminiService(IConfiguration configuration)
        {
            _apiKey       = configuration["Gemini:ApiKey"]      ?? throw new InvalidOperationException("Gemini API key not configured.");
            _model        = configuration["Gemini:Model"]       ?? "gemini-1.5-flash";
            _systemPrompt = configuration["Gemini:SystemPrompt"] ?? "You are a helpful AI assistant.";

            if (_apiKey == "YOUR_GEMINI_API_KEY_HERE")
                throw new InvalidOperationException("Please set your Gemini API key in appsettings.json.");

            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // ── Public API ──────────────────────────────────────────────────────────

        /// <summary>
        /// Sends the full conversation history to Gemini and returns the reply text.
        /// </summary>
        public async Task<string> SendMessageAsync(
            List<ChatMessage> conversationHistory,
            CancellationToken cancellationToken = default)
        {
            OnStatusChanged("Connecting to Gemini API…");

            // Gemini uses "user" and "model" roles (not "assistant")
            var contents = conversationHistory.Select(m => new GeminiContent
            {
                Role  = m.Role == "assistant" ? "model" : "user",
                Parts = new List<GeminiPart> { new GeminiPart { Text = m.Content } }
            }).ToList();

            var request = new GeminiRequest
            {
                SystemInstruction = new GeminiSystemInstruction
                {
                    Parts = new List<GeminiPart> { new GeminiPart { Text = _systemPrompt } }
                },
                Contents         = contents,
                GenerationConfig = new GeminiGenerationConfig
                {
                    MaxOutputTokens = 1024,
                    Temperature     = 0.9
                }
            };

            var json        = JsonConvert.SerializeObject(request, Formatting.None);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            // API key goes in the URL for Gemini
            var url = $"{API_BASE}/{_model}:generateContent?key={_apiKey}";

            OnStatusChanged("Waiting for Gemini response…");

            HttpResponseMessage httpResponse;
            try
            {
                httpResponse = await _httpClient.PostAsync(url, httpContent, cancellationToken);
            }
            catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException("Request was cancelled.");
            }
            catch (TaskCanceledException)
            {
                throw new TimeoutException("Request timed out. Check your internet connection.");
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException($"Network error: {ex.Message}", ex);
            }

            var responseBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

            GeminiResponse? apiResponse;
            try
            {
                apiResponse = JsonConvert.DeserializeObject<GeminiResponse>(responseBody);
            }
            catch (JsonException)
            {
                throw new InvalidOperationException($"Unexpected response format. Status: {httpResponse.StatusCode}");
            }

            // Surface Gemini errors clearly
            if (!httpResponse.IsSuccessStatusCode)
            {
                var msg = apiResponse?.Error?.Message ?? responseBody;
                throw httpResponse.StatusCode switch
                {
                    System.Net.HttpStatusCode.Unauthorized  => new UnauthorizedAccessException($"Invalid API key. {msg}"),
                    System.Net.HttpStatusCode.TooManyRequests => new InvalidOperationException($"Rate limit exceeded. Please wait. {msg}"),
                    System.Net.HttpStatusCode.BadRequest    => new ArgumentException($"Bad request: {msg}"),
                    _ => new HttpRequestException($"API error ({(int)httpResponse.StatusCode}): {msg}")
                };
            }

            if (apiResponse == null)
                throw new InvalidOperationException("Empty response from Gemini API.");

            var text = apiResponse.GetText();
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidOperationException("Gemini returned an empty message.");

            if (apiResponse.UsageMetadata != null)
                OnStatusChanged($"Done — {apiResponse.UsageMetadata.PromptTokenCount} in / {apiResponse.UsageMetadata.CandidatesTokenCount} out tokens");
            else
                OnStatusChanged("Ready");

            return text;
        }

        // ── Helpers ─────────────────────────────────────────────────────────────
        private void OnStatusChanged(string message) =>
            StatusChanged?.Invoke(this, message);

        public void Dispose()
        {
            if (!_disposed)
            {
                _httpClient.Dispose();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}
