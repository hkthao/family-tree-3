using backend.Application.MemberStories.Commands.DeleteMemberStory;
using FluentValidation.TestHelper;
using Xunit;

namespace backend.Application.UnitTests.MemberStories.Commands.DeleteMemberStory;

public class DeleteMemberStoryCommandValidatorTests
{
    private readonly DeleteMemberStoryCommandValidator _validator;

    public DeleteMemberStoryCommandValidatorTests()
    {
        _validator = new DeleteMemberStoryCommandValidator();
    }

    [Fact]
    public void ShouldHaveError_WhenIdIsEmpty()
    {
        var command = new DeleteMemberStoryCommand { Id = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("MemberStory ID is required.");
    }

    [Fact]
    public void ShouldNotHaveError_WhenCommandIsValid()
    {
        var command = new DeleteMemberStoryCommand
        {
            Id = Guid.NewGuid()
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
