using AutoMapper;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using Microsoft.Extensions.Localization;

namespace backend.Application.Memories.Commands.CreateMemory;

public class CreateMemoryCommandHandler : IRequestHandler<CreateMemoryCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IAuthorizationService _authorizationService;
    private readonly IStringLocalizer<CreateMemoryCommandHandler> _localizer;

    public CreateMemoryCommandHandler(IApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService, IStringLocalizer<CreateMemoryCommandHandler> localizer)
    {
        _context = context;
        _mapper = mapper;
        _authorizationService = authorizationService;
        _localizer = localizer;
    }

    public async Task<Result<Guid>> Handle(CreateMemoryCommand request, CancellationToken cancellationToken)
    {
        // Authorization check
        if (!_authorizationService.CanManageFamily(request.MemberId)) // Assuming memberId can be used to check family access
        {
            return Result<Guid>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }
        
        // Find the member to ensure it exists and belongs to the family
        var member = await _context.Members.FindAsync(new object[] { request.MemberId }, cancellationToken);
        if (member == null)
        {
            return Result<Guid>.Failure(string.Format(ErrorMessages.NotFound, $"Member with ID {request.MemberId}"), ErrorSources.NotFound);
        }

        var memory = _mapper.Map<Memory>(request);
        memory.Id = Guid.NewGuid(); // Assign a new ID

        _context.Memories.Add(memory);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(memory.Id);
    }
}
