using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Memories.DTOs;
using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Memories.Commands.AnalyzePhoto;

public class AnalyzePhotoCommandHandler : IRequestHandler<AnalyzePhotoCommand, Result<PhotoAnalysisResultDto>>
{
    private readonly IPhotoAnalysisService _photoAnalysisService;
    private readonly IMapper _mapper;
    private readonly IApplicationDbContext _context; // To check if memberId exists
    private readonly IAuthorizationService _authorizationService;

    public AnalyzePhotoCommandHandler(IPhotoAnalysisService photoAnalysisService, IMapper mapper, IApplicationDbContext context, IAuthorizationService authorizationService)
    {
        _photoAnalysisService = photoAnalysisService;
        _mapper = mapper;
        _context = context;
        _authorizationService = authorizationService;
    }

    public async Task<Result<PhotoAnalysisResultDto>> Handle(AnalyzePhotoCommand request, CancellationToken cancellationToken)
    {
        // Check if memberId exists if provided
        if (request.MemberId.HasValue)
        {
            var member = await _context.Members.FindAsync(new object[] { request.MemberId.Value }, cancellationToken);
            if (member == null)
            {
                return Result<PhotoAnalysisResultDto>.Failure(string.Format(ErrorMessages.NotFound, $"Member with ID {request.MemberId} not found."), ErrorSources.NotFound);
            }
            // Authorization check (can access family of the member)
            if (!_authorizationService.CanAccessFamily(member.FamilyId))
            {
                return Result<PhotoAnalysisResultDto>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
            }
        }

        var serviceRequest = new AnalyzePhotoRequestDto
        {
            File = request.File,
            MemberId = request.MemberId,
        };

        var serviceResult = await _photoAnalysisService.AnalyzePhotoAsync(serviceRequest, cancellationToken);

        if (serviceResult.IsSuccess)
        {
            // Optionally, save the PhotoAnalysisResult to DB here if needed
            // For now, we just return the DTO. The spec says to save only if user chooses to use it.
            return Result<PhotoAnalysisResultDto>.Success(serviceResult.Value!);
        }

        return Result<PhotoAnalysisResultDto>.Failure(serviceResult.Error!);
    }
}
