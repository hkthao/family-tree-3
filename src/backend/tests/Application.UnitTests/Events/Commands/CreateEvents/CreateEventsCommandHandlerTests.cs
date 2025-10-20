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
        _context.UserProfiles.Add(currentUserProfile);
        _context.SaveChanges();

        _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUserProfile);
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(isAdmin);
        _mockUser.Setup(x => x.Id).Returns(currentUserProfile.ExternalId);
        return currentUserProfile;
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
        var royalFamilyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb"); // Royal Family ID from SeedSampleData
        var williamId = Guid.Parse("a1b2c3d4-e5f6-7890-1234-567890abcdef"); // Prince William ID from SeedSampleData
        var catherineId = Guid.Parse("b2c3d4e5-f6a1-8901-2345-67890abcdef0"); // Catherine ID from SeedSampleData

        // Ensure the current user is a manager of the royal family
        _context.FamilyUsers.Add(new FamilyUser { FamilyId = royalFamilyId, UserProfileId = currentUserProfile.Id, Role = FamilyRole.Manager });
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockAuthorizationService.Setup(x => x.CanManageFamily(royalFamilyId, currentUserProfile)).Returns(true);

        var eventsToCreate = new List<CreateEventDto>
        {
            CreateValidEventDto(royalFamilyId, new List<Guid> { williamId, catherineId }),
            CreateValidEventDto(royalFamilyId)
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
        var royalFamilyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb"); // Royal Family ID from SeedSampleData

        // Mock authorization service to return false for CanManageFamily
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(x => x.CanManageFamily(royalFamilyId, currentUserProfile)).Returns(false);

        // Remove user from family managers to ensure unauthorized access
        var familyUser = _context.FamilyUsers.FirstOrDefault(fu => fu.FamilyId == royalFamilyId && fu.UserProfileId == currentUserProfile.Id);
        if (familyUser != null)
        {
            _context.FamilyUsers.Remove(familyUser);
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        var eventsToCreate = new List<CreateEventDto>
        {
            CreateValidEventDto(royalFamilyId)
        };
        var command = new CreateEventsCommand(eventsToCreate);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    /// <summary>
    /// Kiểm tra xem CreateEventsCommandHandler có tạo sự kiện thành công ngay cả khi một số thành viên liên quan không tìm thấy.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldCreateEventSuccessfully_WhenSomeRelatedMembersNotFound()
    {
        // Arrange
        var currentUserProfile = SetupUserAuthorization(false);
        var royalFamilyId = Guid.Parse("16905e2b-5654-4ed0-b118-bbdd028df6eb"); // Royal Family ID from SeedSampleData
        var williamId = Guid.Parse("a1b2c3d4-e5f6-7890-1234-567890abcdef"); // Prince William ID from SeedSampleData

        // Ensure the current user is a manager of the royal family
        _context.FamilyUsers.Add(new FamilyUser { FamilyId = royalFamilyId, UserProfileId = currentUserProfile.Id, Role = FamilyRole.Manager });
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockAuthorizationService.Setup(x => x.CanManageFamily(royalFamilyId, currentUserProfile)).Returns(true);

        var nonExistentMemberId = Guid.NewGuid();
        var eventsToCreate = new List<CreateEventDto>
        {
            CreateValidEventDto(royalFamilyId, new List<Guid> { williamId, nonExistentMemberId })
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
