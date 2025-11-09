using System.Text;
using System.Text.Json;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using backend.Application.AI.Prompts; // Import the new prompt builder
using backend.Domain.Enums; // Import Gender and RelationshipType

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
        var member = await _dbContext.Members.FindAsync(new object[] { request.MemberId }, cancellationToken);
        if (member == null)
        {
            return Result<string>.Failure($"Member with ID {request.MemberId} not found.");
        }

        // Fetch family details
        var family = await _dbContext.Families.FindAsync(new object[] { member.FamilyId }, cancellationToken);

        // Fetch parents
        var parents = await _dbContext.Relationships
            .Include(r => r.SourceMember)
            .Where(r => r.TargetMemberId == member.Id &&
                        (r.Type == RelationshipType.Father || r.Type == RelationshipType.Mother))
            .Select(r => r.SourceMember)
            .ToListAsync(cancellationToken);

        var father = parents.FirstOrDefault(p => Enum.TryParse<Gender>(p?.Gender, out var genderEnum) && genderEnum == Gender.Male);
        var mother = parents.FirstOrDefault(p => Enum.TryParse<Gender>(p?.Gender, out var genderEnum) && genderEnum == Gender.Female);

        // Fetch spouses
        var spouses = await _dbContext.Relationships
            .Include(r => r.SourceMember)
            .Include(r => r.TargetMember)
            .Where(r => (r.SourceMemberId == member.Id && (r.Type == RelationshipType.Wife || r.Type == RelationshipType.Husband)) ||
                        (r.TargetMemberId == member.Id && (r.Type == RelationshipType.Wife || r.Type == RelationshipType.Husband)))
            .Select(r => r.SourceMemberId == member.Id ? r.TargetMember : r.SourceMember)
            .Where(s => s != null)
            .ToListAsync(cancellationToken);

        var message = BiographyPromptBuilder.BuildPrompt(request, member, family, father, mother, spouses!);

        var result = await _n8nService.CallChatWebhookAsync(request.MemberId, message, cancellationToken);

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
