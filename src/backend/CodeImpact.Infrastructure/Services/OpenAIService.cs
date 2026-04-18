using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CodeImpact.Application.Common.Interfaces;
using CodeImpact.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CodeImpact.Infrastructure.Services;

public sealed class OpenAIService : ILLMService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly OpenAISettings _settings;
    private readonly ILogger<OpenAIService> _logger;

    public OpenAIService(HttpClient httpClient, IOptions<OpenAISettings> settings, ILogger<OpenAIService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        if (_httpClient.BaseAddress is null)
        {
            _httpClient.BaseAddress = new Uri(_settings.BaseUrl.TrimEnd('/') + "/");
        }
    }

    public async Task<string> GenerateTextAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new InvalidOperationException("OpenAI ApiKey não configurada.");
        }

        _logger.LogInformation("Enviando prompt para LLM. SystemPrompt: {SystemPrompt}. UserPrompt: {UserPrompt}", systemPrompt, userPrompt);

        for (var attempt = 1; attempt <= Math.Max(1, _settings.MaxRetries); attempt++)
        {
            using var request = BuildRequest(systemPrompt, userPrompt);
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var content = ParseContent(body);
                _logger.LogInformation("Resposta recebida do LLM: {Response}", content);
                return content;
            }

            var statusCode = (int)response.StatusCode;
            var canRetry = response.StatusCode == HttpStatusCode.TooManyRequests || statusCode >= 500;
            _logger.LogWarning("Falha na chamada LLM. Attempt={Attempt} Status={StatusCode} Body={Body}", attempt, statusCode, body);

            if (!canRetry || attempt >= _settings.MaxRetries)
            {
                throw new InvalidOperationException($"Falha ao chamar LLM. Status={statusCode}. Body={body}");
            }

            var delayMs = 500 * attempt * attempt;
            await Task.Delay(delayMs, cancellationToken);
        }

        throw new InvalidOperationException("Falha inesperada ao chamar o LLM.");
    }

    private HttpRequestMessage BuildRequest(string systemPrompt, string userPrompt)
    {
        var payload = new
        {
            model = _settings.Model,
            temperature = _settings.Temperature,
            max_tokens = _settings.MaxTokens,
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            }
        };

        var json = JsonSerializer.Serialize(payload, JsonOptions);
        var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        return request;
    }

    private static string ParseContent(string responseBody)
    {
        using var document = JsonDocument.Parse(responseBody);
        var root = document.RootElement;

        if (!root.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array || choices.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("Resposta do LLM sem choices.");
        }

        var firstChoice = choices[0];
        if (!firstChoice.TryGetProperty("message", out var message)
            || !message.TryGetProperty("content", out var contentElement))
        {
            throw new InvalidOperationException("Resposta do LLM sem message.content.");
        }

        return contentElement.ValueKind == JsonValueKind.String
            ? contentElement.GetString() ?? string.Empty
            : contentElement.GetRawText();
    }
}