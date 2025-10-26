using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.NotificationTemplates.Queries;
using backend.Domain.Enums;

namespace backend.Application.NotificationTemplates.Commands.GenerateNotificationTemplateContent;

/// <summary>
/// Xử lý lệnh tạo nội dung và chủ đề cho mẫu thông báo bằng AI.
/// </summary>
public class GenerateNotificationTemplateContentCommandHandler(IChatProviderFactory chatProviderFactory) : IRequestHandler<GenerateNotificationTemplateContentCommand, Result<GeneratedNotificationTemplateContentDto>>
{
    private readonly IChatProviderFactory _chatProviderFactory = chatProviderFactory;

    public async Task<Result<GeneratedNotificationTemplateContentDto>> Handle(GenerateNotificationTemplateContentCommand request, CancellationToken cancellationToken)
    {
        var chatProvider = _chatProviderFactory.GetProvider(ChatAIProvider.Local);

        // Xây dựng lời nhắc cho AI
        var systemPrompt = BuildAiSystemPrompt();
        var userPrompt = request.Prompt;

        var chatMessages = new List<ChatMessage>
        {
            new() { Role = "system", Content = systemPrompt },
            new() { Role = "user", Content = userPrompt }
        };

        var generatedContent = await chatProvider.GenerateResponseAsync(chatMessages);
        generatedContent = generatedContent.Trim();

        if (string.IsNullOrEmpty(generatedContent))
        {
            return Result<GeneratedNotificationTemplateContentDto>.Failure(ErrorMessages.NoAIResponse);
        }

        // Phân tích nội dung được tạo để trích xuất chủ đề và nội dung
        var (subject, body) = ParseGeneratedContent(generatedContent);

        return Result<GeneratedNotificationTemplateContentDto>.Success(new GeneratedNotificationTemplateContentDto
        {
            Subject = subject,
            Body = body
        });
    }

    /// <summary>
    /// Xây dựng lời nhắc hệ thống cho AI dựa trên các tham số yêu cầu.
    /// </summary>
    /// <returns>Lời nhắc hệ thống được định dạng cho AI.</returns>
    private string BuildAiSystemPrompt()
    {
        var promptBuilder = new System.Text.StringBuilder();
        promptBuilder.AppendLine("You are an AI assistant that generates JSON data for notification template content based on natural language descriptions. The output must always be a single JSON object containing 'subject' and 'body' fields. The body should be in the specified format.");
        promptBuilder.AppendLine("Please provide the output in a JSON format with 'subject' and 'body' fields.");
        promptBuilder.AppendLine("Example: { \"subject\": \"Your Subject\", \"body\": \"Your Body Content\" }");
        promptBuilder.AppendLine("Always respond with ONLY the JSON object. Do not include any conversational text.");

        return promptBuilder.ToString();
    }

    /// <summary>
    /// Phân tích nội dung được tạo bởi AI để trích xuất chủ đề và nội dung.
    /// </summary>
    /// <param name="generatedContent">Nội dung JSON được tạo bởi AI.</param>
    /// <returns>Một tuple chứa chủ đề và nội dung.</returns>
    private (string Subject, string Body) ParseGeneratedContent(string generatedContent)
    {
        try
        {
            // Cố gắng phân tích JSON
            var jsonDoc = System.Text.Json.JsonDocument.Parse(generatedContent);
            var subject = jsonDoc.RootElement.TryGetProperty("subject", out var subjectElement) ? subjectElement.GetString() : string.Empty;
            var body = jsonDoc.RootElement.TryGetProperty("body", out var bodyElement) ? bodyElement.GetString() : string.Empty;

            return (subject ?? string.Empty, body ?? string.Empty);
        }
        catch (System.Text.Json.JsonException)
        {
            // Nếu không phải JSON hợp lệ, cố gắng phân tích theo định dạng khác hoặc trả về toàn bộ nội dung làm body
            // Trong trường hợp này, chúng ta sẽ giả định toàn bộ nội dung là body và chủ đề là trống
            return (string.Empty, generatedContent);
        }
    }
}
