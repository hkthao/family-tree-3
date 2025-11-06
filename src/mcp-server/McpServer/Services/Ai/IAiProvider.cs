using McpServer.Models; // Add this using statement
using McpServer.Services.Ai.Tools;

namespace McpServer.Services.Ai
{
    /// <summary>
    /// Định nghĩa giao diện cho các nhà cung cấp AI Assistant.
    /// </summary>
    public interface IAiProvider
    {
        /// <summary>
        /// Tạo phản hồi từ AI, có thể bao gồm các yêu cầu gọi tool.
        /// </summary>
        /// <param name="messages">Lịch sử trò chuyện và prompt hiện tại.</param>
        /// <returns>Một stream các phần phản hồi, có thể là text hoặc yêu cầu gọi tool.</returns>
        IAsyncEnumerable<AiResponsePart> GenerateToolUseResponseStreamAsync(List<AiMessage> messages);

        /// <summary>
        /// Tạo phản hồi từ AI chỉ dựa trên prompt, không liên quan đến tool.
        /// </summary>
        /// <param name="prompt">Prompt từ người dùng.</param>
        /// <returns>Một stream các phần phản hồi là text.</returns>
        IAsyncEnumerable<AiResponsePart> GenerateChatResponseStreamAsync(string prompt);

        /// <summary>
        /// Kiểm tra trạng thái hoạt động của nhà cung cấp AI.
        /// </summary>
        /// <returns>Trạng thái hoạt động.</returns>
        Task<string> GetStatusAsync();
    }
}