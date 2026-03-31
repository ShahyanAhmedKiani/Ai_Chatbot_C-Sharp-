# ============================================================
# AI Chatbot — Dockerfile
# Multi-stage build for .NET 8 Windows Forms application
#
# NOTE: Windows Forms requires Windows containers.
# Build:  docker build -t ai-chatbot .
# Run:    docker run --rm ai-chatbot
# ============================================================

# ── Stage 1: Build ──────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0-windowsservercore-ltsc2022 AS build
WORKDIR /src

# Copy solution & project files first (for layer caching)
COPY AIChatbot.sln ./
COPY AIChatbot/AIChatbot.csproj ./AIChatbot/

# Restore NuGet packages
RUN dotnet restore AIChatbot/AIChatbot.csproj

# Copy all remaining source
COPY . .

# Publish in Release mode — self-contained for the target OS
RUN dotnet publish AIChatbot/AIChatbot.csproj \
    --configuration Release \
    --runtime win-x64 \
    --self-contained true \
    --output /app/publish \
    -p:PublishSingleFile=true \
    -p:DebugType=None

# ── Stage 2: Runtime ─────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/runtime:8.0-windowsservercore-ltsc2022 AS runtime
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Copy configuration (API key set via environment or mounted file)
COPY AIChatbot/appsettings.json .

# Expose environment variable for API key override
ENV ANTHROPIC__APIKEY=""

# Entry point
ENTRYPOINT ["AIChatbot.exe"]
