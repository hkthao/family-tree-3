using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
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

    public CreateMembersCommandHandlerTests()
    {
        _aiMemberDtoValidatorMock = new Mock<IValidator<AIMemberDto>>();
        _mediatorMock = new Mock<IMediator>();
    }

    /// <summary>
    /// Kiểm tra xem handler có tạo tất cả các thành viên thành công khi tất cả các thành viên đều hợp lệ và việc tạo thành công.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldCreateAllMembers_WhenAllMembersAreValidAndCreationSucceeds()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var member1Id = Guid.NewGuid();
        var member2Id = Guid.NewGuid();

        var memberDto1 = new AIMemberDto
        {
            FamilyId = familyId,
            FirstName = "John",
            LastName = "Doe",
            FamilyName = "Test Family"
        };
        var memberDto2 = new AIMemberDto
        {
            FamilyId = familyId,
            FirstName = "Jane",
            LastName = "Doe",
            FamilyName = "Test Family"
        };

        var members = new List<AIMemberDto> { memberDto1, memberDto2 };
        var command = new CreateMembersCommand(members);

        _aiMemberDtoValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<AIMemberDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult()); // All members are valid

        _mediatorMock.SetupSequence(x => x.Send(It.IsAny<CreateMemberCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Success(member1Id))
            .ReturnsAsync(Result<Guid>.Success(member2Id));

        var handler = new CreateMembersCommandHandler(_aiMemberDtoValidatorMock.Object, _mediatorMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(member1Id);
        result.Value.Should().Contain(member2Id);

        _mediatorMock.Verify(x => x.Send(It.IsAny<CreateMemberCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    /// <summary>
    /// Kiểm tra xem handler có bỏ qua các thành viên không hợp lệ và chỉ tạo các thành viên hợp lệ.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldSkipInvalidMembers_WhenSomeMembersAreInvalid()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var member1Id = Guid.NewGuid();

        var memberDto1 = new AIMemberDto
        {
            FamilyId = familyId,
            FirstName = "John",
            LastName = "Doe",
            FamilyName = "Test Family"
        };
        var memberDto2 = new AIMemberDto // Invalid member (e.g., missing first name)
        {
            FamilyId = familyId,
            LastName = "Invalid",
            FamilyName = "Test Family"
        };

        var members = new List<AIMemberDto> { memberDto1, memberDto2 };
        var command = new CreateMembersCommand(members);

        var validationResultForInvalid = new ValidationResult(new List<ValidationFailure>
        {
            new("FirstName", "First name is required.")
        });

        _aiMemberDtoValidatorMock.SetupSequence(x => x.ValidateAsync(It.IsAny<AIMemberDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult()) // memberDto1 is valid
            .ReturnsAsync(validationResultForInvalid); // memberDto2 is invalid

        _mediatorMock.Setup(x => x.Send(It.Is<CreateMemberCommand>(cmd => cmd.FirstName == "John"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Success(member1Id));

        var handler = new CreateMembersCommandHandler(_aiMemberDtoValidatorMock.Object, _mediatorMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.Should().Contain(member1Id);

        memberDto2.ValidationErrors.Should().NotBeNull();
        memberDto2.ValidationErrors.Should().Contain("First name is required.");

        _mediatorMock.Verify(x => x.Send(It.IsAny<CreateMemberCommand>(), It.IsAny<CancellationToken>()), Times.Once); // Only one member created
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về danh sách rỗng khi tất cả các thành viên đều không hợp lệ.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenAllMembersAreInvalid()
    {
        // Arrange
        var familyId = Guid.NewGuid();

        var memberDto1 = new AIMemberDto // Invalid member
        {
            FamilyId = familyId,
            LastName = "Invalid1",
            FamilyName = "Test Family"
        };
        var memberDto2 = new AIMemberDto // Invalid member
        {
            FamilyId = familyId,
            LastName = "Invalid2",
            FamilyName = "Test Family"
        };

        var members = new List<AIMemberDto> { memberDto1, memberDto2 };
        var command = new CreateMembersCommand(members);

        var validationResultForInvalid = new ValidationResult(new List<ValidationFailure>
        {
            new("FirstName", "First name is required.")
        });

        _aiMemberDtoValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<AIMemberDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResultForInvalid); // All members are invalid

        var handler = new CreateMembersCommandHandler(_aiMemberDtoValidatorMock.Object, _mediatorMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        memberDto1.ValidationErrors.Should().NotBeNull();
        memberDto1.ValidationErrors.Should().Contain("First name is required.");
        memberDto2.ValidationErrors.Should().NotBeNull();
        memberDto2.ValidationErrors.Should().Contain("First name is required.");

        _mediatorMock.Verify(x => x.Send(It.IsAny<CreateMemberCommand>(), It.IsAny<CancellationToken>()), Times.Never); // No members created
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về thành công một phần khi một số lệnh tạo thành viên thất bại.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnPartialSuccess_WhenSomeCreateMemberCommandsFail()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var member1Id = Guid.NewGuid();

        var memberDto1 = new AIMemberDto
        {
            FamilyId = familyId,
            FirstName = "John",
            LastName = "Doe",
            FamilyName = "Test Family"
        };
        var memberDto2 = new AIMemberDto
        {
            FamilyId = familyId,
            FirstName = "Jane",
            LastName = "Doe",
            FamilyName = "Test Family"
        };

        var members = new List<AIMemberDto> { memberDto1, memberDto2 };
        var command = new CreateMembersCommand(members);

        _aiMemberDtoValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<AIMemberDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult()); // All members are valid according to AIMemberDtoValidator

        _mediatorMock.SetupSequence(x => x.Send(It.IsAny<CreateMemberCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Guid>.Success(member1Id)) // First member creation succeeds
            .ReturnsAsync(Result<Guid>.Failure("Failed to create member 2")); // Second member creation fails

        var handler = new CreateMembersCommandHandler(_aiMemberDtoValidatorMock.Object, _mediatorMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue(); // Overall success because some members were created
        result.Value.Should().HaveCount(1);
        result.Value.Should().Contain(member1Id);

        memberDto2.ValidationErrors.Should().NotBeNull();
        memberDto2.ValidationErrors.Should().Contain("Failed to create member 2");

        _mediatorMock.Verify(x => x.Send(It.IsAny<CreateMemberCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
