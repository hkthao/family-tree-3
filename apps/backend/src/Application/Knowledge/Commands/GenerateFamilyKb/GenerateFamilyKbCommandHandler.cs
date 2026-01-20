using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace backend.Application.Knowledge.Commands.GenerateFamilyKb;

public class GenerateFamilyKbCommandHandler : IRequestHandler<GenerateFamilyKbCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IKnowledgeService _knowledgeService;
    private readonly ILogger<GenerateFamilyKbCommandHandler> _logger;
    private readonly IAuthorizationService _authorizationService;

    public GenerateFamilyKbCommandHandler(IApplicationDbContext context, IKnowledgeService knowledgeService, ILogger<GenerateFamilyKbCommandHandler> logger, IAuthorizationService authorizationService)
    {
        _context = context;
        _knowledgeService = knowledgeService;
        _logger = logger;
        _authorizationService = authorizationService;
    }

    public async Task<Result> Handle(GenerateFamilyKbCommand request, CancellationToken cancellationToken)
    {
        if (!_authorizationService.IsAdmin())
        {
            _logger.LogWarning("Authorization failed for non-admin user to generate knowledge base for FamilyId: {FamilyId}.", request.FamilyId);
            return Result.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        _logger.LogInformation("Generating knowledge base for FamilyId: {FamilyId}", request.FamilyId);

        var family = await _context.Families
            .FirstOrDefaultAsync(f => f.Id == request.FamilyId, cancellationToken);

        if (family == null)
        {
            _logger.LogWarning("Family with ID {FamilyId} not found. Cannot generate knowledge base.", request.FamilyId);
            return Result.Failure($"Family with ID {request.FamilyId} not found.");
        }

        // 1. Index Family data
        await _knowledgeService.IndexFamilyData(family.Id);
        _logger.LogInformation("Family data indexed for FamilyId: {FamilyId}", family.Id);

        // 2. Index Members data
        var members = await _context.Members
            .Where(m => m.FamilyId == request.FamilyId)
            .ToListAsync(cancellationToken);

        foreach (var member in members)
        {
            await _knowledgeService.IndexMemberData(member.Id);
            _logger.LogInformation("Member data indexed for MemberId: {MemberId}", member.Id);
        }

        // 3. Index Events data
        var events = await _context.Events
            .Where(e => e.FamilyId == request.FamilyId)
            .ToListAsync(cancellationToken);

        foreach (var @event in events)
        {
            await _knowledgeService.IndexEventData(@event.Id);
            _logger.LogInformation("Event data indexed for EventId: {EventId}", @event.Id);
        }

        _logger.LogInformation("Successfully generated knowledge base for FamilyId: {FamilyId}", request.FamilyId);
        return Result.Success();
    }
}
