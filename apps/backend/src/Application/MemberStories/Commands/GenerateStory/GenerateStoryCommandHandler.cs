using backend.Application.AI.DTOs; // For GenerateRequest
using backend.Application.AI.Prompts;
using backend.Application.Common.Constants; // For PromptConstants
using backend.Application.Common.Interfaces; // For IAiGenerateService
using backend.Application.Common.Models;
using backend.Application.MemberStories.DTOs; // For GenerateStoryResponseDto

namespace backend.Application.MemberStories.Commands.GenerateStory;

public class GenerateStoryCommandHandler : IRequestHandler<GenerateStoryCommand, Result<GenerateStoryResponseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly IAiGenerateService _aiGenerateService; // NEW

    public GenerateStoryCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IAiGenerateService aiGenerateService)
    {
        _context = context;
        _authorizationService = authorizationService;
        _aiGenerateService = aiGenerateService;
    }

    public async Task<Result<GenerateStoryResponseDto>> Handle(GenerateStoryCommand request, CancellationToken cancellationToken)
    {
        // Find the member to ensure it exists and belongs to the family
        var member = await _context.Members
            .Include(m => m.Family)
            .FirstOrDefaultAsync(m => m.Id == request.MemberId, cancellationToken);

        if (member == null)
        {
            return Result<GenerateStoryResponseDto>.Failure(string.Format(ErrorMessages.NotFound, $"Member with ID {request.MemberId} not found."), ErrorSources.NotFound);
        }

        // Authorization check (can access family of the member)
        if (!_authorizationService.CanAccessFamily(member.FamilyId))
        {
            return Result<GenerateStoryResponseDto>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        // Prepare request for AI Generate Service
        var generateRequest = new GenerateRequest
        {
            SessionId = Guid.NewGuid().ToString(), // Generate a new session ID for this generation
            ChatInput = PromptBuilder.BuildStoryGenerationPrompt(request, member, member.Family), // Use PromptBuilder
            SystemPrompt = await BuildSystemPrompt(request.MemberId), // Build SystemPrompt
            Metadata = new Dictionary<string, object>
            {
                { "memberId", request.MemberId.ToString() },
                { "memberName", request.MemberName ?? member.FullName },
                { "style", request.Style },
                { "maxWords", request.MaxWords },
                { "perspective", request.Perspective ?? "" },
                { "resizedImageUrl", request.ResizedImageUrl ?? "" },
                { "photoPersons", request.PhotoPersons != null ? System.Text.Json.JsonSerializer.Serialize(request.PhotoPersons) : "" }
            }
        };

        // Call AI Generate Service for story generation
        var generateResult = await _aiGenerateService.GenerateDataAsync<GenerateStoryResponseDto>(generateRequest, cancellationToken);

        if (!generateResult.IsSuccess)
        {
            // If the AI service call fails, return a failure result with the error message
            return Result<GenerateStoryResponseDto>.Failure(generateResult.Error ?? "Lỗi không xác định khi tạo câu chuyện bằng AI.", ErrorSources.ExternalServiceError);
        }

        var storyResponseDto = generateResult.Value;

        if (storyResponseDto == null)
        {
            return Result<GenerateStoryResponseDto>.Failure("Tạo câu chuyện từ AI trả về phản hồi rỗng hoặc không hợp lệ.");
        }

        return Result<GenerateStoryResponseDto>.Success(storyResponseDto);
    }

    private async Task<string> BuildSystemPrompt(Guid memberId)
    {
        var promptCode = PromptConstants.StoryGenerationPromptCode;

        var systemPrompt = await _context.Prompts
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(p => p.Code == promptCode);

        if (systemPrompt == null)
        {
            throw new InvalidOperationException($"System prompt with code '{promptCode}' not found in the database.");
        }

        // We can inject member-specific details into the prompt content if needed.
        // For example: systemPrompt.Content.Replace("{memberName}", memberName);
        return systemPrompt.Content;
    }
}

