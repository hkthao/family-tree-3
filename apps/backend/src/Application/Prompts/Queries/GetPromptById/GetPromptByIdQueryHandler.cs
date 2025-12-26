using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Prompts.DTOs; // Reintroduce PromptDto

namespace backend.Application.Prompts.Queries.GetPromptById;

public class GetPromptByIdQueryHandler : IRequestHandler<GetPromptByIdQuery, Result<PromptDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper; // Reintroduce IMapper

    public GetPromptByIdQueryHandler(IApplicationDbContext context, IMapper mapper) // Reintroduce IMapper in constructor
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PromptDto>> Handle(GetPromptByIdQuery request, CancellationToken cancellationToken)
    {
        if (request.Id == null && string.IsNullOrWhiteSpace(request.Code))
        {
            return Result<PromptDto>.Failure("Either Id or Code must be provided.", "Validation");
        }

        var query = _context.Prompts.AsNoTracking();

        if (request.Id.HasValue)
        {
            query = query.Where(p => p.Id == request.Id.Value);
        }
        else if (!string.IsNullOrWhiteSpace(request.Code))
        {
            query = query.Where(p => p.Code == request.Code);
        }

        var entity = await query.FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            return Result<PromptDto>.NotFound("Prompt not found.", "NotFound");
        }

        var dto = _mapper.Map<PromptDto>(entity); // Reintroduce mapping
        return Result<PromptDto>.Success(dto);
    }
}
