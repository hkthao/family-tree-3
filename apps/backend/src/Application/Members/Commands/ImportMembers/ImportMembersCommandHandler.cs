using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.ValueObjects; // Add this using statement

namespace backend.Application.Members.Commands.ImportMembers;

public class ImportMembersCommandHandler : IRequestHandler<ImportMembersCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;

    public ImportMembersCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService)
    {
        _context = context;
        _authorizationService = authorizationService;
    }

    public async Task<Result<Unit>> Handle(ImportMembersCommand request, CancellationToken cancellationToken)
    {
        // Check if family exists
        var familyExists = await _context.Families.AnyAsync(f => f.Id == request.FamilyId, cancellationToken);
        if (!familyExists)
        {
            return Result<Unit>.Failure($"Family with ID {request.FamilyId} not found.");
        }

        // Authorization check: Only family managers or admins can import members
        if (!_authorizationService.CanManageFamily(request.FamilyId))
        {
            return Result<Unit>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        var importedMembers = new List<Member>();
        var originalIdToNewIdMap = new Dictionary<Guid, Guid>();

        // First pass: Create new Member entities with new IDs and map old IDs to new IDs
        foreach (var memberDto in request.Members)
        {
            var newMemberId = Guid.NewGuid();
            originalIdToNewIdMap[memberDto.Id] = newMemberId; // Map original DTO ID to new entity ID

            var member = new Member(
                memberDto.LastName,
                memberDto.FirstName,
                memberDto.Code ?? Guid.NewGuid().ToString().Substring(0, 8), // Generate if not provided
                request.FamilyId
            );
            member.Id = newMemberId; // Assign new ID
            member.Update(
                memberDto.FirstName,
                memberDto.LastName,
                memberDto.Code ?? Guid.NewGuid().ToString().Substring(0, 8), // Re-generate code if null here
                memberDto.Nickname,
                memberDto.Gender, // Gender is string?
                memberDto.DateOfBirth,
                memberDto.DateOfDeath,
                memberDto.LunarDateOfDeath == null ? null : new LunarDate(
                    memberDto.LunarDateOfDeath.Day,
                    memberDto.LunarDateOfDeath.Month,
                    memberDto.LunarDateOfDeath.IsLeapMonth,
                    memberDto.LunarDateOfDeath.IsEstimated
                ),
                memberDto.PlaceOfBirth,
                memberDto.PlaceOfDeath,
                memberDto.Phone,
                memberDto.Email,
                memberDto.Address,
                memberDto.Occupation,
                memberDto.AvatarUrl,
                memberDto.Biography,
                memberDto.Order,
                memberDto.IsDeceased
            );
            if (memberDto.IsRoot)
            {
                member.SetAsRoot();
            }
            importedMembers.Add(member);
        }

        // Second pass: Establish relationships among newly imported members or existing members
        foreach (var memberDto in request.Members)
        {
            var currentMember = importedMembers.First(m => m.Id == originalIdToNewIdMap[memberDto.Id]);

            // Father relationship
            if (memberDto.FatherId.HasValue)
            {
                if (originalIdToNewIdMap.TryGetValue(memberDto.FatherId.Value, out var newFatherId))
                {
                    currentMember.FatherId = newFatherId;
                }
                else
                {
                    // If father is not in the imported list, try to find in existing family members
                    var existingFather = await _context.Members
                        .Where(m => m.Id == memberDto.FatherId.Value && m.FamilyId == request.FamilyId)
                        .FirstOrDefaultAsync(cancellationToken);
                    if (existingFather != null)
                    {
                        currentMember.FatherId = existingFather.Id;
                    }
                    // Else, father not found, relationship remains null
                }
            }

            // Mother relationship
            if (memberDto.MotherId.HasValue)
            {
                if (originalIdToNewIdMap.TryGetValue(memberDto.MotherId.Value, out var newMotherId))
                {
                    currentMember.MotherId = newMotherId;
                }
                else
                {
                    var existingMother = await _context.Members
                        .Where(m => m.Id == memberDto.MotherId.Value && m.FamilyId == request.FamilyId)
                        .FirstOrDefaultAsync(cancellationToken);
                    if (existingMother != null)
                    {
                        currentMember.MotherId = existingMother.Id;
                    }
                }
            }

            // Husband relationship
            if (memberDto.HusbandId.HasValue)
            {
                if (originalIdToNewIdMap.TryGetValue(memberDto.HusbandId.Value, out var newHusbandId))
                {
                    currentMember.HusbandId = newHusbandId;
                }
                else
                {
                    var existingHusband = await _context.Members
                        .Where(m => m.Id == memberDto.HusbandId.Value && m.FamilyId == request.FamilyId)
                        .FirstOrDefaultAsync(cancellationToken);
                    if (existingHusband != null)
                    {
                        currentMember.HusbandId = existingHusband.Id;
                    }
                }
            }

            // Wife relationship
            if (memberDto.WifeId.HasValue)
            {
                if (originalIdToNewIdMap.TryGetValue(memberDto.WifeId.Value, out var newWifeId))
                {
                    currentMember.WifeId = newWifeId;
                }
                else
                {
                    var existingWife = await _context.Members
                        .Where(m => m.Id == memberDto.WifeId.Value && m.FamilyId == request.FamilyId)
                        .FirstOrDefaultAsync(cancellationToken);
                    if (existingWife != null)
                    {
                        currentMember.WifeId = existingWife.Id;
                    }
                }
            }
        }

        _context.Members.AddRange(importedMembers);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
