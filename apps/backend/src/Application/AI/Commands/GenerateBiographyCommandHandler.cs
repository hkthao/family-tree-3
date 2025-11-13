using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.AI.Prompts;
using backend.Domain.Enums;
using backend.Application.Members.Specifications; // Import the new specification
using Ardalis.Specification.EntityFrameworkCore; // For WithSpecification

namespace backend.Application.AI.Commands;

public class GenerateBiographyCommandHandler : IRequestHandler<GenerateBiographyCommand, Result<string>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IN8nService _n8nService;

    public GenerateBiographyCommandHandler(IApplicationDbContext dbContext, IN8nService n8nService)
    {
        _dbContext = dbContext;
        _n8nService = n8nService;
    }

    public async Task<Result<string>> Handle(GenerateBiographyCommand request, CancellationToken cancellationToken)
    {
        var spec = new MemberWithDetailsForBiographySpecification(request.MemberId);
        var member = await _dbContext.Members.AsNoTracking().WithSpecification(spec).FirstOrDefaultAsync(cancellationToken);

        if (member == null)
        {
            return Result<string>.Failure($"Member with ID {request.MemberId} not found.");
        }

        // Family is already included in the member object
        var family = member.Family;

        // Extract parents from relationships
        var parents = member.TargetRelationships
            .Where(r => r.Type == RelationshipType.Father || r.Type == RelationshipType.Mother)
            .Select(r => r.SourceMember)
            .ToList();

        var father = parents.FirstOrDefault(p => Enum.TryParse<Gender>(p?.Gender, out var genderEnum) && genderEnum == Gender.Male);
        var mother = parents.FirstOrDefault(p => Enum.TryParse<Gender>(p?.Gender, out var genderEnum) && genderEnum == Gender.Female);

        // Extract spouses from relationships
        var spouses = member.SourceRelationships
            .Where(r => r.Type == RelationshipType.Husband || r.Type == RelationshipType.Wife)
            .Select(r => r.TargetMember)
            .Concat(member.TargetRelationships
                .Where(r => r.Type == RelationshipType.Husband || r.Type == RelationshipType.Wife)
                .Select(r => r.SourceMember))
            .Where(s => s != null)
            .ToList();

        var message = PromptBuilder.BuildBiographyPrompt(request, member, family, father, mother, spouses!);

        var result = await _n8nService.CallChatWebhookAsync(request.MemberId.ToString(), message, cancellationToken);

        if (result.IsSuccess)
        {
            return Result<string>.Success(result.Value ?? string.Empty);
        }
        else
        {
            return Result<string>.Failure(result.Error ?? "Unknown error from N8nService.");
        }
    }
}
