using backend.Application.Common.Models;
using backend.Application.Members.Commands.CreateMember;
using backend.Application.Members.Queries;
using FluentValidation.Results;

namespace backend.Application.Members.Commands.CreateMembers;

public class CreateMembersCommandHandler : IRequestHandler<CreateMembersCommand, Result<List<Guid>>>
{
    private readonly IValidator<MemberDto> _memberDtoValidator;
    private readonly IMediator _mediator;

    public CreateMembersCommandHandler(IValidator<MemberDto> memberDtoValidator, IMediator mediator)
    {
        _memberDtoValidator = memberDtoValidator;
        _mediator = mediator;
    }

    public async Task<Result<List<Guid>>> Handle(CreateMembersCommand request, CancellationToken cancellationToken)
    {
        var createdMemberIds = new List<Guid>();

        foreach (var memberDto in request.Members)
        {
            ValidationResult validationResult = await _memberDtoValidator.ValidateAsync(memberDto, cancellationToken);
            if (!validationResult.IsValid)
            {
                memberDto.ValidationErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                continue; // Skip this member if validation fails
            }

            // Create a CreateMemberCommand from the memberDto
            var createMemberCommand = new CreateMemberCommand
            {
                FirstName = memberDto.FirstName,
                LastName = memberDto.LastName,
                Nickname = memberDto.Nickname,
                Gender = memberDto.Gender,
                DateOfBirth = memberDto.DateOfBirth,
                DateOfDeath = memberDto.DateOfDeath,
                PlaceOfBirth = memberDto.PlaceOfBirth,
                PlaceOfDeath = memberDto.PlaceOfDeath,
                Occupation = memberDto.Occupation,
                AvatarUrl = memberDto.AvatarUrl,
                Biography = memberDto.Biography,
                FamilyId = memberDto.FamilyId,
                IsRoot = memberDto.IsRoot
            };

            // Send the CreateMemberCommand using mediator
            var createResult = await _mediator.Send(createMemberCommand, cancellationToken);

            if (createResult.IsSuccess)
            {
                createdMemberIds.Add(createResult.Value);
            }
            else
            {
                // If individual member creation fails, add error to memberDto for feedback
                memberDto.ValidationErrors = memberDto.ValidationErrors ?? new List<string>();
                if (createResult.Error != null)
                {
                    memberDto.ValidationErrors.Add(createResult.Error);
                }
            }
        }

        // No need to call SaveChangesAsync here, as CreateMemberCommand will handle it

        return Result<List<Guid>>.Success(createdMemberIds);
    }
}