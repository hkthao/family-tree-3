using AutoMapper;
using backend.Application.Common.Constants;
using backend.Application.Families.Queries; // For Any()
using backend.Application.MemberFaces.Common;
using backend.Application.MemberFaces.Queries.ExportMemberFaces;
using backend.Application.Members.Queries;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.MemberFaces.Queries;

public class ExportMemberFacesQueryHandlerTests : TestBase
{
    private readonly ExportMemberFacesQueryHandler _handler;
    private readonly Mock<ILogger<ExportMemberFacesQueryHandler>> _mockLogger;
    private readonly Mock<IMapper> _mockMapper;

    private readonly Guid _testFamilyId = Guid.NewGuid();
    private readonly Guid _testMemberId1 = Guid.NewGuid();
    private readonly Guid _testMemberId2 = Guid.NewGuid();

    public ExportMemberFacesQueryHandlerTests()
    {
        _mockLogger = new Mock<ILogger<ExportMemberFacesQueryHandler>>();
        _mockMapper = new Mock<IMapper>();

        // Setup the mapper for ProjectTo
        _mockMapper.Setup(m => m.ConfigurationProvider).Returns(new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<MemberFace, MemberFaceDto>();
            cfg.CreateMap<Member, MemberDto>(); // Ensure member is mapped for enriched data
            cfg.CreateMap<Family, FamilyDto>(); // Ensure family is mapped for enriched data
            cfg.CreateMap<backend.Domain.ValueObjects.BoundingBox, backend.Application.MemberFaces.Common.BoundingBoxDto>();
        }));

        _handler = new ExportMemberFacesQueryHandler(_context, _mockAuthorizationService.Object, _mockLogger.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_ShouldExportAllMemberFaces_WhenUserIsAdminAndFacesExist()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);

        var family = new Family { Id = _testFamilyId, Name = "Test Family", Code = "TEST_FAMILY_CODE" };
        var member1 = new Member(_testMemberId1, "Doe", "John", "MEMBER_JOHN_1", _testFamilyId, family);
        var member2 = new Member(_testMemberId2, "Doe", "Jane", "MEMBER_JANE_1", _testFamilyId, family);

        _context.Families.Add(family);
        _context.Members.Add(member1);
        _context.Members.Add(member2);
        _context.MemberFaces.Add(new MemberFace { Id = Guid.NewGuid(), MemberId = _testMemberId1, Member = member1, FaceId = "FACE_1" });
        _context.MemberFaces.Add(new MemberFace { Id = Guid.NewGuid(), MemberId = _testMemberId2, Member = member2, FaceId = "FACE_2" });
        _context.MemberFaces.Add(new MemberFace { Id = Guid.NewGuid(), MemberId = Guid.NewGuid(), FaceId = "OTHER_FAMILY_FACE" }); // Other family face
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new ExportMemberFacesQuery(_testFamilyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty().And.HaveCount(2);
        result.Value.Should().Contain(mf => mf.FaceId == "FACE_1");
        result.Value.Should().Contain(mf => mf.FaceId == "FACE_2");
        result.Value.Should().NotBeNull().And.NotBeEmpty(); // Add null check before Any()
        result.Value!.Any(mf => mf.FaceId == "OTHER_FAMILY_FACE").Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldExportAllMemberFaces_WhenUserIsFamilyManager()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(x => x.CanManageFamily(_testFamilyId)).Returns(true);

        var family = new Family { Id = _testFamilyId, Name = "Test Family", Code = "TEST_FAMILY_CODE" };
        var member1 = new Member(_testMemberId1, "Doe", "John", "MEMBER_JOHN_1", _testFamilyId, family);

        _context.Families.Add(family);
        _context.Members.Add(member1);
        _context.MemberFaces.Add(new MemberFace { Id = Guid.NewGuid(), MemberId = _testMemberId1, Member = member1, FaceId = "FACE_3" });
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new ExportMemberFacesQuery(_testFamilyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty().And.HaveCount(1);
        result.Value.Should().Contain(mf => mf.FaceId == "FACE_3");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoMemberFacesExistForFamily()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);
        // No faces added for _testFamilyId

        var query = new ExportMemberFacesQuery(_testFamilyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnAccessDenied_WhenUserIsNotAdminAndNotFamilyManager()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(x => x.CanManageFamily(_testFamilyId)).Returns(false);

        var family = new Family { Id = _testFamilyId, Name = "Test Family", Code = "TEST_FAMILY_CODE" };
        var member1 = new Member(_testMemberId1, "Doe", "John", "MEMBER_JOHN_1", _testFamilyId, family);

        _context.Families.Add(family);
        _context.Members.Add(member1);
        _context.MemberFaces.Add(new MemberFace { Id = Guid.NewGuid(), MemberId = _testMemberId1, Member = member1, FaceId = "FACE_4" });
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new ExportMemberFacesQuery(_testFamilyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }
}
