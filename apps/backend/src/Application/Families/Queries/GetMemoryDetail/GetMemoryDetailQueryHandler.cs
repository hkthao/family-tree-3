using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Memories.DTOs; // Added

namespace backend.Application.Memories.Queries.GetMemoryDetail;

public class GetMemoryDetailQueryHandler : IRequestHandler<GetMemoryDetailQuery, Result<MemoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IAuthorizationService _authorizationService;

    public GetMemoryDetailQueryHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService)
    {
        _context = context;
        _mapper = mapper;
        _authorizationService = authorizationService;
    }

    public async Task<Result<MemoryDto>> Handle(GetMemoryDetailQuery request, CancellationToken cancellationToken)
    {
        var memory = await _context.Memories
            .Include(m => m.Member)
            .FirstOrDefaultAsync(m => m.Id == request.Id && !m.IsDeleted, cancellationToken);

        if (memory == null)
        {
            return Result<MemoryDto>.Failure(string.Format(ErrorMessages.NotFound, $"Memory with ID {request.Id}"), ErrorSources.NotFound);
        }

        // Authorization check
        if (!_authorizationService.CanAccessFamily(memory.Member.FamilyId))
        {
            return Result<MemoryDto>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var memoryDto = _mapper.Map<MemoryDto>(memory);
        return Result<MemoryDto>.Success(memoryDto);
    }
}
