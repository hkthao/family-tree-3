namespace McpServer.Services
{
    /// <summary>
    /// Định nghĩa giao diện cho các nhà cung cấp AI Assistant.
    /// </summary>
    public interface IAiProvider
    {
        /// <summary>
        /// Lấy phản hồi từ AI Assistant dựa trên prompt.
        /// </summary>
        /// <param name="prompt">Prompt từ người dùng.</param>
        /// <param name="context">Ngữ cảnh bổ sung (ví dụ: dữ liệu backend đã được truy xuất).</param>
        /// <returns>Phản hồi từ AI Assistant.</returns>
        IAsyncEnumerable<string> GenerateResponseStreamAsync(string prompt, string? context = null);

        /// <summary>
        /// Kiểm tra trạng thái hoạt động của nhà cung cấp AI.
        /// </summary>
        /// <returns>Trạng thái hoạt động.</returns>
        Task<string> GetStatusAsync();
    }
}