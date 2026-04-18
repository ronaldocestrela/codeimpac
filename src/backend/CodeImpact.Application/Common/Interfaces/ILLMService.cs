namespace CodeImpact.Application.Common.Interfaces;

public interface ILLMService
{
    Task<string> GenerateTextAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default);
}