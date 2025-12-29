using backend.Application.MemberFaces.Commands.ImportMemberFaces;
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using backend.Application.UnitTests.Common;
using backend.Application.Common.Constants;

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
            new() { MemberId = _testMemberId1, FaceId = "FACE_NEW_1", Confidence = 0.9, Emotion = "Happy" },
            new() { MemberId = _testMemberId1, FaceId = "FACE_NEW_2", Confidence = 0.8, Emotion = "Neutral" },
        };
        var command = new ImportMemberFacesCommand(_testFamilyId, importItems);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty().And.HaveCount(2);

        _context.MemberFaces.Should().HaveCount(2);
        _context.MemberFaces.Should().Contain(mf => mf.FaceId == "FACE_NEW_1");
        _context.MemberFaces.Should().Contain(mf => mf.FaceId == "FACE_NEW_2");
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
            new() { MemberId = _testMemberId1, FaceId = "FACE_NEW_3", Confidence = 0.7, Emotion = "Sad" },
        };
        var command = new ImportMemberFacesCommand(_testFamilyId, importItems);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty().And.HaveCount(1);
        _context.MemberFaces.Should().HaveCount(1);
        _context.MemberFaces.First().FaceId.Should().Be("FACE_NEW_3");
    }

    [Fact]
    public async Task Handle_ShouldSkipExistingMemberFaces_WhenFacesAlreadyExist()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);

        var family = new Family { Id = _testFamilyId, Name = "Test Family", Code = "TEST_FAMILY_CODE" };
        var member1 = new Member(_testMemberId1, "Doe", "John", "MEMBER_JOHN_3", _testFamilyId, family);
        _context.Families.Add(family);
        _context.Members.Add(member1);
        _context.MemberFaces.Add(new MemberFace { MemberId = _testMemberId1, Member = member1, FaceId = "FACE_EXISTING", Confidence = 0.9 });
        await _context.SaveChangesAsync(CancellationToken.None);

        var importItems = new List<ImportMemberFaceItemDto>
        {
            new() { MemberId = _testMemberId1, FaceId = "FACE_EXISTING", Confidence = 0.8, Emotion = "Neutral" }, // Should be skipped
            new() { MemberId = _testMemberId1, FaceId = "FACE_NEW_4", Confidence = 0.7, Emotion = "Happy" },
        };
        var command = new ImportMemberFacesCommand(_testFamilyId, importItems);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty().And.HaveCount(1); // Only the new face should be imported

        _context.MemberFaces.Should().HaveCount(2); // Existing + 1 new
        _context.MemberFaces.Any(mf => mf.FaceId == "FACE_EXISTING").Should().BeTrue();
        _context.MemberFaces.Any(mf => mf.FaceId == "FACE_NEW_4").Should().BeTrue();
        _context.MemberFaces.First(mf => mf.FaceId == "FACE_EXISTING").Confidence.Should().Be(0.9); // Should not be updated
    }

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
            new() { MemberId = _testMemberId1, FaceId = "FACE_X", Confidence = 0.6, Emotion = "Angry" },
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
        _context.Families.Add(family);
        await _context.SaveChangesAsync(CancellationToken.None);

        var importItems = new List<ImportMemberFaceItemDto>
        {
            new() { MemberId = Guid.NewGuid(), FaceId = "FACE_NOT_FOUND", Confidence = 0.9 }, // MemberId not in family
            new() { MemberId = _testMemberId1, FaceId = "FACE_VALID", Confidence = 0.8 },
        };
        var command = new ImportMemberFacesCommand(_testFamilyId, importItems);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty(); // No face imported
        _context.MemberFaces.Should().BeEmpty();
    }
}
