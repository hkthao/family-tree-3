using backend.Application.Common.Models;
using backend.Application.Members.Commands.CreateMember;
using backend.Application.Members.Commands.CreateMembers;
using backend.Application.Members.Queries;
using backend.Application.UnitTests.Common;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.CreateMembers;

public class CreateMembersCommandHandlerTests : TestBase
{
    private readonly Mock<IValidator<AIMemberDto>> _aiMemberDtoValidatorMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly CreateMembersCommandHandler _handler;

    public CreateMembersCommandHandlerTests()
    {
        _aiMemberDtoValidatorMock = new Mock<IValidator<AIMemberDto>>();
        _mediatorMock = new Mock<IMediator>();
        _handler = new CreateMembersCommandHandler(_aiMemberDtoValidatorMock.Object, _mediatorMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateMembers_WhenAllMembersAreValid()
    {
        // Arrange
        var membersToCreate = new List<AIMemberDto>
        {
            new() { FirstName = "John", LastName = "Doe", Gender = "Male", DateOfBirth = new DateTime(1990, 1, 1) },
            new() { FirstName = "Jane", LastName = "Doe", Gender = "Female", DateOfBirth = new DateTime(1992, 2, 2) }
        };
        var command = new CreateMembersCommand(membersToCreate);

        _aiMemberDtoValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<AIMemberDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateMemberCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreateMemberCommand cmd, CancellationToken ct) => Result<Guid>.Success(Guid.NewGuid()));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateMemberCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ShouldSkipInvalidMembers_AndCreateValidOnes()
    {
        // Arrange
        var validMember = new AIMemberDto { FirstName = "John", LastName = "Doe", Gender = "Male", DateOfBirth = new DateTime(1990, 1, 1) };
        var invalidMember = new AIMemberDto { FirstName = "", LastName = "Doe", Gender = "Male", DateOfBirth = new DateTime(1990, 1, 1) }; // Invalid FirstName
        var membersToCreate = new List<AIMemberDto> { validMember, invalidMember };
        var command = new CreateMembersCommand(membersToCreate);

        _aiMemberDtoValidatorMock.Setup(v => v.ValidateAsync(validMember, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _aiMemberDtoValidatorMock.Setup(v => v.ValidateAsync(invalidMember, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("FirstName", "First name is required.") }));

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateMemberCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid()));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateMemberCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        invalidMember.ValidationErrors.Should().NotBeEmpty();
        invalidMember.ValidationErrors.Should().Contain("First name is required.");
    }

    [Fact]
    public async Task Handle_ShouldCaptureErrors_FromIndividualMemberCreation()
    {
        // Arrange
        var memberToCreate = new AIMemberDto { FirstName = "John", LastName = "Doe", Gender = "Male", DateOfBirth = new DateTime(1990, 1, 1) };
        var command = new CreateMembersCommand(new List<AIMemberDto> { memberToCreate });
        var creationError = "Failed to create member.";

        _aiMemberDtoValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<AIMemberDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateMemberCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Failure(creationError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
        memberToCreate.ValidationErrors.Should().NotBeEmpty();
        memberToCreate.ValidationErrors.Should().Contain(creationError);
    }
}
