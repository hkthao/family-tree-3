using System.Text.Json; // Added for JsonDocument
using backend.Application.AI.DTOs;
using backend.Application.AI.Prompts; // NEW
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.AI.Commands.AnalyzePhoto;

public class AnalyzePhotoCommandHandler : IRequestHandler<AnalyzePhotoCommand, Result<PhotoAnalysisResultDto>>
{
    private readonly IApplicationDbContext _context; // To check if memberId exists
    private readonly IAuthorizationService _authorizationService;
    private readonly IN8nService _n8nService; // Use IN8nService

    public AnalyzePhotoCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IN8nService n8nService)
    {
        _context = context;
        _authorizationService = authorizationService;
        _n8nService = n8nService;
    }

    public async Task<Result<PhotoAnalysisResultDto>> Handle(AnalyzePhotoCommand request, CancellationToken cancellationToken)
    {
        // Check if memberId exists if provided in Input.MemberInfo
        if (request.Input.MemberInfo?.Id != null)
        {
            if (!Guid.TryParse(request.Input.MemberInfo.Id, out var memberIdGuid))
            {
                return Result<PhotoAnalysisResultDto>.Failure($"Invalid Member ID format: {request.Input.MemberInfo.Id}", ErrorSources.BadRequest);
            }

            var member = await _context.Members.FindAsync(new object[] { memberIdGuid }, cancellationToken);
            if (member == null)
            {
                return Result<PhotoAnalysisResultDto>.Failure(string.Format(ErrorMessages.NotFound, $"Member with ID {request.Input.MemberInfo.Id} not found."), ErrorSources.NotFound);
            }
            // Authorization check (can access family of the member)
            if (!_authorizationService.CanAccessFamily(member.FamilyId))
            {
                return Result<PhotoAnalysisResultDto>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
            }
        }

        // Use PromptBuilder to create message for AI and call CallChatWebhookAsync
        var sessionId = Guid.NewGuid().ToString(); // Generate a new session ID for this analysis
        var userMessage = PromptBuilder.BuildPhotoAnalysisPrompt(request.Input);

        var n8nChatResult = await _n8nService.CallChatWebhookAsync(sessionId, userMessage, cancellationToken);

        if (!n8nChatResult.IsSuccess)
        {
            return Result<PhotoAnalysisResultDto>.Failure(n8nChatResult.Error ?? "Lỗi khi gọi webhook n8n để phân tích ảnh.", ErrorSources.ExternalServiceError);
        }

        PhotoAnalysisResultDto? photoAnalysisResult;
        try
        {
            if (string.IsNullOrEmpty(n8nChatResult.Value))
            {
                return Result<PhotoAnalysisResultDto>.Failure("Phân tích ảnh từ n8n trả về phản hồi rỗng.", ErrorSources.ExternalServiceError);
            }
            // Ensure camelCase JSON from AI is deserialized correctly to PascalCase C# properties
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            // The result.Value is expected to be a JSON string from the AI
            photoAnalysisResult = System.Text.Json.JsonSerializer.Deserialize<PhotoAnalysisResultDto>(n8nChatResult.Value, options);
        }
        catch (JsonException ex)
        {
            return Result<PhotoAnalysisResultDto>.Failure($"Phân tích ảnh từ n8n trả về phản hồi không hợp lệ: {ex.Message}", ErrorSources.ExternalServiceError);
        }

        if (photoAnalysisResult == null)
        {
            return Result<PhotoAnalysisResultDto>.Failure("Phân tích ảnh từ n8n trả về phản hồi rỗng hoặc không hợp lệ.");
        }

        // Add CreatedAt to the result if it's not already set by n8n
        if (photoAnalysisResult.CreatedAt == default)
        {
            photoAnalysisResult.CreatedAt = DateTime.UtcNow;
        }

        return Result<PhotoAnalysisResultDto>.Success(photoAnalysisResult);
    }
}
