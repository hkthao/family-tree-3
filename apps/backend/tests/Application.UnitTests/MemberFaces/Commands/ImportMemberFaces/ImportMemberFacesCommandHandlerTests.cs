using backend.Application.Common.Constants;
using backend.Application.MemberFaces.Commands.ImportMemberFaces;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.UnitTests.MemberFaces.Commands.ImportMemberFaces;

public class ImportMemberFacesCommandHandlerTests : TestBase
{
    private readonly Mock<ILogger<ImportMemberFacesCommandHandler>> _mockLogger;
    private readonly ImportMemberFacesCommandHandler _handler;

    private readonly Guid _testFamilyId = Guid.NewGuid();
    private readonly Guid _testMemberId1 = Guid.NewGuid();

    public ImportMemberFacesCommandHandlerTests()
    {
        _mockLogger = new Mock<ILogger<ImportMemberFacesCommandHandler>>();
        _handler = new ImportMemberFacesCommandHandler(_context, _mockAuthorizationService.Object, _mockLogger.Object, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldImportNewMemberFaces_WhenUserIsAdminAndFacesAreNew()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);

        var family = new Family { Id = _testFamilyId, Name = "Test Family", Code = "TEST_FAMILY_CODE" };
        var member1 = new Member(_testMemberId1, "Doe", "John", "MEMBER_JOHN_1", _testFamilyId, family);
        _context.Families.Add(family);
        _context.Members.Add(member1);
        await _context.SaveChangesAsync(CancellationToken.None);


        var importItems = new List<ImportMemberFaceItemDto>
        {
            new() { MemberId = _testMemberId1, Confidence = 0.9, Emotion = "Happy" },
            new() { MemberId = _testMemberId1, Confidence = 0.8, Emotion = "Neutral" },
        };
        var command = new ImportMemberFacesCommand(_testFamilyId, importItems);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty().And.HaveCount(2);

        _context.MemberFaces.Should().HaveCount(2);
        _context.MemberFaces.Should().Contain(mf => mf.Confidence == 0.9 && mf.Emotion == "Happy");
        _context.MemberFaces.Should().Contain(mf => mf.Confidence == 0.8 && mf.Emotion == "Neutral");
    }

    [Fact]
    public async Task Handle_ShouldImportNewMemberFaces_WhenUserIsFamilyManager()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(x => x.CanManageFamily(_testFamilyId)).Returns(true);

        var family = new Family { Id = _testFamilyId, Name = "Test Family", Code = "TEST_FAMILY_CODE" };
        var member1 = new Member(_testMemberId1, "Doe", "John", "MEMBER_JOHN_2", _testFamilyId, family);
        _context.Families.Add(family);
        _context.Members.Add(member1);
        await _context.SaveChangesAsync(CancellationToken.None);

        var importItems = new List<ImportMemberFaceItemDto>
        {
            new() { MemberId = _testMemberId1, Confidence = 0.7, Emotion = "Sad" },
        };
        var command = new ImportMemberFacesCommand(_testFamilyId, importItems);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty().And.HaveCount(1);
        _context.MemberFaces.Should().HaveCount(1);
        _context.MemberFaces.First().Confidence.Should().Be(0.7);
        _context.MemberFaces.First().Emotion.Should().Be("Sad");
    }

    // Removed Handle_ShouldSkipExistingMemberFaces_WhenFacesAlreadyExist test
    // as the underlying FaceId-based skipping logic has been removed.

    [Fact]
    public async Task Handle_ShouldReturnAccessDenied_WhenUserIsNotAdminAndNotFamilyManager()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(x => x.CanManageFamily(_testFamilyId)).Returns(false);

        var family = new Family { Id = _testFamilyId, Name = "Test Family", Code = "TEST_FAMILY_CODE" };
        var member1 = new Member(_testMemberId1, "Doe", "John", "MEMBER_JOHN_4", _testFamilyId, family);
        _context.Families.Add(family);
        _context.Members.Add(member1);
        await _context.SaveChangesAsync(CancellationToken.None);

        var importItems = new List<ImportMemberFaceItemDto>
        {
            new() { MemberId = _testMemberId1, Confidence = 0.6, Emotion = "Angry" },
        };
        var command = new ImportMemberFacesCommand(_testFamilyId, importItems);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
        _context.MemberFaces.Should().BeEmpty(); // No faces should be imported
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessWithEmptyList_WhenImportingEmptyList()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);
        var command = new ImportMemberFacesCommand(_testFamilyId, new List<ImportMemberFaceItemDto>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
        _context.MemberFaces.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldSkipFaceIfMemberNotFoundInFamily()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);

        var family = new Family { Id = _testFamilyId, Name = "Test Family", Code = "TEST_FAMILY_CODE" };
        var member1 = new Member(_testMemberId1, "John", "Doe", "JDO", _testFamilyId, family); // Add a valid member
        _context.Families.Add(family);
        _context.Members.Add(member1); // Add the member to the context
        await _context.SaveChangesAsync(CancellationToken.None);

        var importItems = new List<ImportMemberFaceItemDto>
        {
            new() { MemberId = Guid.NewGuid(), Confidence = 0.9 }, // MemberId not in family - this face should be skipped
            new() { MemberId = _testMemberId1, Confidence = 0.8 }, // This face should be imported
        };
        var command = new ImportMemberFacesCommand(_testFamilyId, importItems);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty().And.HaveCount(1); // Only the valid face should be imported
        _context.MemberFaces.Should().HaveCount(1);
        _context.MemberFaces.First().Confidence.Should().Be(0.8);
    }
}
