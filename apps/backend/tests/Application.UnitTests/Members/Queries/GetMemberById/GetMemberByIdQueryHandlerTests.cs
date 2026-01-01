using backend.Application.Common.Interfaces; // Add this using statement
using backend.Application.Members.Queries.GetMemberById; // For MemberDetailDto
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Members.Queries.GetMemberById;

public class GetMemberByIdQueryHandlerTests : TestBase
{
    private readonly Mock<IPrivacyService> _mockPrivacyService;

    public GetMemberByIdQueryHandlerTests()
    {
        _mockPrivacyService = new Mock<IPrivacyService>();
        // Default setup for privacy service to return the DTO as is (no filtering for basic tests)
        _mockPrivacyService.Setup(x => x.ApplyPrivacyFilter(It.IsAny<MemberDetailDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MemberDetailDto dto, Guid familyId, CancellationToken token) => dto);

        // No need to initialize _mockAuthorizationService and _mockCurrentUser here, as TestBase does it.
        // We might want to override their behavior in specific tests if needed.
    }

    [Fact]
    public async Task Handle_ShouldReturnMember_WhenMemberExists()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF1" };
        var member = new Member("John", "Doe", "JD", familyId) { Id = memberId };
        _context.Families.Add(family);
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        var handler = new GetMemberByIdQueryHandler(_context, _mapper, _mockAuthorizationService.Object, _mockUser.Object, _mockPrivacyService.Object);
        var query = new GetMemberByIdQuery(memberId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(memberId);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMemberDoesNotExist()
    {
        // Arrange
        var handler = new GetMemberByIdQueryHandler(_context, _mapper, _mockAuthorizationService.Object, _mockUser.Object, _mockPrivacyService.Object);
        var query = new GetMemberByIdQuery(Guid.NewGuid());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}
