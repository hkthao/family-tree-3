using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.ImageRestorationJobs.Common;
using backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace backend.Application.ImageRestorationJobs.Commands.CreateImageRestorationJob;

public class CreateImageRestorationJobCommandHandler : IRequestHandler<CreateImageRestorationJobCommand, Result<ImageRestorationJobDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser; // Changed to ICurrentUser
    private readonly IDateTime _dateTime;
    private readonly ILogger<CreateImageRestorationJobCommandHandler> _logger;

    public CreateImageRestorationJobCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ICurrentUser currentUser, // Changed to ICurrentUser
        IDateTime dateTime,
        ILogger<CreateImageRestorationJobCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _currentUser = currentUser; // Changed to ICurrentUser
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task<Result<ImageRestorationJobDto>> Handle(CreateImageRestorationJobCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated) // Changed authentication check
        {
            _logger.LogWarning("Current user is not authenticated. Cannot create image restoration job.");
            return Result<ImageRestorationJobDto>.Failure("User is not authenticated.");
        }

        var userId = _currentUser.UserId.ToString(); // Converted Guid to string

        var entity = new ImageRestorationJob
        {
            OriginalImageUrl = request.OriginalImageUrl,
            FamilyId = request.FamilyId,
            Status = Domain.Enums.RestorationStatus.Processing, // Corrected to Processing
            Created = _dateTime.Now,
            CreatedBy = userId,
            LastModified = _dateTime.Now,
            LastModifiedBy = userId
        };

        _context.ImageRestorationJobs.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<ImageRestorationJobDto>(entity);
        return Result<ImageRestorationJobDto>.Success(dto);
    }
}
