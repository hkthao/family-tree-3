using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Memories.DTOs;
using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Memories.Commands.GenerateStory;

public class GenerateStoryCommandHandler : IRequestHandler<GenerateStoryCommand, Result<GenerateStoryResponseDto>>
{
    private readonly IStoryGenerationService _storyGenerationService;
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly IMapper _mapper;

    public GenerateStoryCommandHandler(IStoryGenerationService storyGenerationService, IApplicationDbContext context, IAuthorizationService authorizationService, IMapper mapper)
    {
        _storyGenerationService = storyGenerationService;
        _context = context;
        _authorizationService = authorizationService;
        _mapper = mapper;
    }

    public async Task<Result<GenerateStoryResponseDto>> Handle(GenerateStoryCommand request, CancellationToken cancellationToken)
    {
        // Find the member to ensure it exists and belongs to the family
        var member = await _context.Members.FindAsync(new object[] { request.MemberId }, cancellationToken);
        if (member == null)
        {
            return Result<GenerateStoryResponseDto>.Failure(string.Format(ErrorMessages.NotFound, $"Member with ID {request.MemberId} not found."), ErrorSources.NotFound);
        }

        // Authorization check (can access family of the member)
        if (!_authorizationService.CanAccessFamily(member.FamilyId))
        {
            return Result<GenerateStoryResponseDto>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        // Check if PhotoAnalysisId is provided and exists
        if (request.PhotoAnalysisId.HasValue)
        {
            var photoAnalysis = await _context.PhotoAnalysisResults.FindAsync(new object[] { request.PhotoAnalysisId.Value }, cancellationToken);
            if (photoAnalysis == null)
            {
                return Result<GenerateStoryResponseDto>.Failure(string.Format(ErrorMessages.NotFound, $"PhotoAnalysisResult with ID {request.PhotoAnalysisId} not found."), ErrorSources.NotFound);
            }
            // Add check that PhotoAnalysisResult belongs to the same family/member if needed
        }

        var serviceResult = await _storyGenerationService.GenerateStoryAsync(
            new GenerateStoryRequestDto
            {
                MemberId = request.MemberId,
                PhotoAnalysisId = request.PhotoAnalysisId,
                RawText = request.RawText,
                Style = request.Style,
                MaxWords = request.MaxWords,
            },
            cancellationToken
        );

        if (serviceResult.IsSuccess)
        {
            return Result<GenerateStoryResponseDto>.Success(serviceResult.Value!);
        }

        return Result<GenerateStoryResponseDto>.Failure(serviceResult.Error!);
    }
}
