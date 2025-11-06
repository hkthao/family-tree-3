using McpServer.Services.Ai.AITools;

namespace McpServer.Services.Ai;

/// <summary>
/// Dịch vụ chính để tương tác với AI, điều phối luồng tool-use.
/// </summary>
public class AiService
{
    private readonly AiProviderFactory _aiProviderFactory;
    private readonly ILogger<AiService> _logger;
    private readonly string _defaultAiProvider;
    private readonly ToolInteractionHandler _toolInteractionHandler; // Inject ToolInteractionHandler

    public AiService(
        AiProviderFactory aiProviderFactory,
        ILogger<AiService> logger,
        IConfiguration configuration,
        ToolInteractionHandler toolInteractionHandler) // Inject ToolInteractionHandler
    {
        _aiProviderFactory = aiProviderFactory;
        _logger = logger;
        _defaultAiProvider = configuration["DefaultAiProvider"] ?? "localllm";
        _toolInteractionHandler = toolInteractionHandler; // Assign ToolInteractionHandler
    }
    /// <summary>
    /// Lấy phản hồi từ AI, có thể qua nhiều bước gọi tool.
    /// </summary>
    public async IAsyncEnumerable<string> GetAiResponseStreamAsync(string prompt, string? jwtToken, string? providerName = null)
    {
        var selectedProviderName = providerName ?? _defaultAiProvider;
        string? errorMessage = null;
        try
        {
            _aiProviderFactory.GetProvider(selectedProviderName);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid AI provider specified: {ProviderName}", selectedProviderName);
            errorMessage = $"Error: Invalid AI provider '{selectedProviderName}'.";
        }

        if (errorMessage != null)
        {
            yield return errorMessage;
            yield break;
        }

        // Sử dụng ToolInteractionHandler để xử lý tương tác công cụ
        await foreach (var chunk in _toolInteractionHandler.HandleToolInteractionAsync(prompt, jwtToken))
        {
            yield return chunk;
        }
    }

    public async Task<string> GetStatusAsync(string? providerName = null)
    {
        var selectedProviderName = providerName ?? _defaultAiProvider;
        IAiProvider aiProvider;
        try
        {
            aiProvider = _aiProviderFactory.GetProvider(selectedProviderName);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid AI provider specified for status check: {ProviderName}", selectedProviderName);
            return $"Error: Invalid AI provider '{selectedProviderName}'.";
        }

        return await aiProvider.GetStatusAsync();
    }
}
