using McpServer.Config;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace McpServer.Services
{
    /// <summary>
    /// Dịch vụ chính để tương tác với AI Assistant, sử dụng một nhà cung cấp AI được cấu hình.
    /// </summary>
    public class AiService
    {
        private readonly FamilyTreeBackendService _familyTreeBackendService;
        private readonly AiProviderFactory _aiProviderFactory;
        private readonly ILogger<AiService> _logger;
        private readonly string _defaultAiProvider; // To store the default provider name

        public AiService(
            FamilyTreeBackendService familyTreeBackendService,
            AiProviderFactory aiProviderFactory,
            ILogger<AiService> logger,
            IConfiguration configuration) // Inject IConfiguration to get default provider
        {
            _familyTreeBackendService = familyTreeBackendService;
            _aiProviderFactory = aiProviderFactory;
            _logger = logger;
            _defaultAiProvider = configuration["DefaultAiProvider"] ?? "Gemini"; // Get from config, default to Gemini
        }

        /// <summary>
        /// Gửi prompt đến AI Assistant được cấu hình và nhận kết quả.
        /// </summary>
        /// <param name="prompt">Prompt từ người dùng.</param>
        /// <param name="jwtToken">JWT Token để truy vấn dữ liệu backend.</param>
        /// <param name="providerName">Tên của nhà cung cấp AI cụ thể để sử dụng (tùy chọn).</param>
        /// <returns>Kết quả từ AI Assistant.</returns>
        public async IAsyncEnumerable<string> GetAiResponseStreamAsync(string prompt, string? jwtToken, string? providerName = null)
        {
            var selectedProviderName = providerName ?? _defaultAiProvider;
            IAiProvider? aiProvider = null; // Initialize to null
            string? errorMessage = null;
            List<MemberDto>? members = null; // Declare members here
            try
            {
                aiProvider = _aiProviderFactory.GetProvider(selectedProviderName);

                // Example of RAG: Fetching data from Family Tree backend
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    members = await _familyTreeBackendService.GetMembersAsync(jwtToken);
                    if (members != null && members.Any())
                    {
                        // Incorporate member data into the prompt
                        var memberData = JsonSerializer.Serialize(members.Take(5), new JsonSerializerOptions { WriteIndented = true }); // Take first 5 members as example
                        prompt = $"Consider the following family members: {memberData}\n\n{prompt}";
                    }
                }

            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid AI provider specified: {ProviderName}", selectedProviderName);
                errorMessage = $"Error: Invalid AI provider '{selectedProviderName}'.";
            }

            if (aiProvider == null) // If provider could not be initialized due to an error
            {
                if (errorMessage != null)
                {
                    yield return errorMessage;
                }
                yield break;
            }

            // Now, outside the try-catch, iterate and yield
            await foreach (var chunk in aiProvider.GenerateResponseStreamAsync(prompt, members != null && members.Any() ? JsonSerializer.Serialize(members) : null))
            {
                yield return chunk;
            }
        }

        /// <summary>
        /// Kiểm tra trạng thái của AI Assistant được cấu hình.
        /// </summary>
        /// <param name="providerName">Tên của nhà cung cấp AI cụ thể để kiểm tra (tùy chọn).</param>
        /// <returns>Trạng thái của AI Assistant.</returns>
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
}