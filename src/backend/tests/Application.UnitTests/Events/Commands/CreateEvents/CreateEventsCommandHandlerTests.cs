using backend.Application.Common.Interfaces;
using backend.Application.Events.Commands.CreateEvents;
using backend.Application.Events.Commands.Inputs;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Events.Commands.CreateEvents;

public class CreateEventsCommandHandlerTests : TestBase
{
    private readonly CreateEventsCommandHandler _handler;

    public CreateEventsCommandHandlerTests()
    {
        _handler = new CreateEventsCommandHandler(_context, _mockUser.Object, _mockAuthorizationService.Object);
    }

    private UserProfile SetupUserAuthorization(bool isAdmin)
    {
        var userProfileId = Guid.NewGuid();
        var currentUserProfile = new UserProfile { Id = userProfileId, ExternalId = Guid.NewGuid().ToString(), Email = "test@example.com", Name = "Test User" };
        _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUserProfile);
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(isAdmin);
        _mockUser.Setup(x => x.Id).Returns(currentUserProfile.ExternalId);
        return currentUserProfile;
    }

    private async Task<(Family family1, Family family2, Member member1, Member member2, Member member3)> SeedDefaultData(UserProfile currentUserProfile, CancellationToken cancellationToken)
    {
        var family1 = new Family { Id = Guid.NewGuid(), Name = "Family A", CreatedBy = currentUserProfile.ExternalId, Code = "FAM001" };
        var family2 = new Family { Id = Guid.NewGuid(), Name = "Family B", CreatedBy = currentUserProfile.ExternalId, Code = "FAM002" };
        _context.Families.Add(family1);
        _context.Families.Add(family2);

        _context.FamilyUsers.Add(new FamilyUser { FamilyId = family1.Id, UserProfileId = currentUserProfile.Id, Role = FamilyRole.Manager });
        _context.FamilyUsers.Add(new FamilyUser { FamilyId = family2.Id, UserProfileId = currentUserProfile.Id, Role = FamilyRole.Manager });

        var member1 = new Member { Id = Guid.NewGuid(), FamilyId = family1.Id, FirstName = "Member", LastName = "1", Code = "MEM001" };
        var member2 = new Member { Id = Guid.NewGuid(), FamilyId = family1.Id, FirstName = "Member", LastName = "2", Code = "MEM002" };
        var member3 = new Member { Id = Guid.NewGuid(), FamilyId = family2.Id, FirstName = "Member", LastName = "3", Code = "MEM003" };
        _context.Members.Add(member1);
        _context.Members.Add(member2);
        _context.Members.Add(member3);

        _context.Relationships.Add(new Relationship { Id = Guid.NewGuid(), SourceMember = member1, TargetMember = member2, Type = RelationshipType.Father, FamilyId = family1.Id });
        _context.Relationships.Add(new Relationship { Id = Guid.NewGuid(), SourceMember = member2, TargetMember = member3, Type = RelationshipType.Mother, FamilyId = family1.Id });

        await _context.SaveChangesAsync(cancellationToken);

        return (family1, family2, member1, member2, member3);
    }

    private CreateEventDto CreateValidEventDto(Guid familyId, List<Guid>? relatedMembers = null)
    {
        return new CreateEventDto
        {
            Name = "Test Event",
            Code = "EVT" + Guid.NewGuid().ToString().Substring(0, 5).ToUpper(), // Generate a unique code
            Type = EventType.Birth,
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(-9),
            Location = "Test Location",
            Description = "Test Description",
            FamilyId = familyId,
            RelatedMembers = relatedMembers?.Select(m => m.ToString()).ToList() ?? new List<string>()
        };
    }

    /// <summary>
    /// Kiểm tra xem CreateEventsCommandHandler có tạo sự kiện thành công khi được ủy quyền.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldCreateEventsSuccessfully_WhenAuthorized()
    {
        // Arrange
        var currentUserProfile = SetupUserAuthorization(false);
        var (family1, _, member1, member2, _) = await SeedDefaultData(currentUserProfile, CancellationToken.None);

        var eventsToCreate = new List<CreateEventDto>
        {
            CreateValidEventDto(family1.Id, new List<Guid> { member1.Id, member2.Id }),
            CreateValidEventDto(family1.Id)
        };
        var command = new CreateEventsCommand(eventsToCreate);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value?.Count.Should().Be(2);
        _context.Events.Count().Should().Be(2);
        _context.Events.First().RelatedMembers.Count.Should().Be(2);
    }

    /// <summary>
    /// Kiểm tra xem CreateEventsCommandHandler có trả về lỗi khi không tìm thấy hồ sơ người dùng.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserProfileNotFound()
    {
        // Arrange
        _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfile)null!);

        var eventsToCreate = new List<CreateEventDto>
        {
            CreateValidEventDto(Guid.NewGuid())
        };
        var command = new CreateEventsCommand(eventsToCreate);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User profile not found.");
    }

    /// <summary>
    /// Kiểm tra xem CreateEventsCommandHandler có trả về lỗi khi không tìm thấy gia đình.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyNotFound()
    {
        // Arrange
        SetupUserAuthorization(false);

        var eventsToCreate = new List<CreateEventDto>
        {
            CreateValidEventDto(Guid.NewGuid())
        };
        var command = new CreateEventsCommand(eventsToCreate);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Family with ID");
    }

    /// <summary>
    /// Kiểm tra xem CreateEventsCommandHandler có trả về lỗi khi người dùng không được ủy quyền.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserIsNotAuthorized()
    {
        // Arrange
        var currentUserProfile = SetupUserAuthorization(false);
        var (family1, _, _, _, _) = await SeedDefaultData(currentUserProfile, CancellationToken.None);

        // Mock authorization service to return false for CanManageFamily
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(x => x.CanManageFamily(family1.Id, currentUserProfile)).Returns(false);

        // Remove user from family managers to ensure unauthorized access
        var familyUser = _context.FamilyUsers.FirstOrDefault(fu => fu.FamilyId == family1.Id && fu.UserProfileId == currentUserProfile.Id);
        if (familyUser != null)
        {
            _context.FamilyUsers.Remove(familyUser);
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        var eventsToCreate = new List<CreateEventDto>
        {
            CreateValidEventDto(family1.Id)
        };
        var command = new CreateEventsCommand(eventsToCreate);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"User is not authorized to create events in family {family1.Name}.");
    }

    /// <summary>
    /// Kiểm tra xem CreateEventsCommandHandler có tạo sự kiện thành công ngay cả khi một số thành viên liên quan không tìm thấy.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldCreateEventSuccessfully_WhenSomeRelatedMembersNotFound()
    {
        // Arrange
        var currentUserProfile = SetupUserAuthorization(false);
        var (family1, _, member1, _, _) = await SeedDefaultData(currentUserProfile, CancellationToken.None);

        var nonExistentMemberId = Guid.NewGuid();
        var eventsToCreate = new List<CreateEventDto>
        {
            CreateValidEventDto(family1.Id, new List<Guid> { member1.Id, nonExistentMemberId })
        };
        var command = new CreateEventsCommand(eventsToCreate);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value?.Count.Should().Be(1);
        _context.Events.Count().Should().Be(1);
        _context.Events.First().RelatedMembers.Count.Should().Be(1); // Only existing member should be added
    }
}
