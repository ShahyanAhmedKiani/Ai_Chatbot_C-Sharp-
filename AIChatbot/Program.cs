using AIChatbot;
using Microsoft.Extensions.Configuration;

// ── Configure DPI & visual styles ───────────────────────────────────────────
Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(false);
Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

// ── Load configuration from appsettings.json ────────────────────────────────
IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

// ── Global exception handlers ────────────────────────────────────────────────
Application.ThreadException += (_, e) =>
    MessageBox.Show($"Unexpected error:\n\n{e.Exception.Message}",
        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

AppDomain.CurrentDomain.UnhandledException += (_, e) =>
{
    var msg = e.ExceptionObject is Exception ex ? ex.Message : e.ExceptionObject.ToString();
    MessageBox.Show($"Fatal error:\n\n{msg}", "Fatal Error",
        MessageBoxButtons.OK, MessageBoxIcon.Stop);
};

// ── Validate Gemini API key before opening the window ────────────────────────
var apiKey = configuration["Gemini:ApiKey"];
if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "YOUR_GEMINI_API_KEY_HERE")
{
    MessageBox.Show(
        "Please set your free Gemini API key in appsettings.json.\n\n" +
        "Get one free at:\n  https://aistudio.google.com/app/apikey\n\n" +
        "Then edit:\n" +
        "  {\n" +
        "    \"Gemini\": {\n" +
        "      \"ApiKey\": \"YOUR_KEY_HERE\"\n" +
        "    }\n" +
        "  }",
        "API Key Required",
        MessageBoxButtons.OK,
        MessageBoxIcon.Warning);
}

// ── Launch main window ───────────────────────────────────────────────────────
Application.Run(new MainForm(configuration));
