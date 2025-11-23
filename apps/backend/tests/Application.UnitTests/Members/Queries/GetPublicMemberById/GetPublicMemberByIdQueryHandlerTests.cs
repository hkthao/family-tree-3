using AutoMapper;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Members.Queries.GetMemberById;
using backend.Application.Members.Queries.GetPublicMemberById;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Members.Queries.GetPublicMemberById;

public class GetPublicMemberByIdQueryHandlerTests : TestBase
{
    private readonly GetPublicMemberByIdQueryHandler _handler;
    private readonly Mock<IPrivacyService> _mockPrivacyService;

    public GetPublicMemberByIdQueryHandlerTests()
    {
        _mockPrivacyService = new Mock<IPrivacyService>();
        _handler = new GetPublicMemberByIdQueryHandler(_context, _mapper, _mockPrivacyService.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnMember_WhenFamilyExistsAndIsPublicAndMemberBelongsToFamily()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var publicFamily = new Family { Id = familyId, Name = "Public Family", Code = "PUB1", Visibility = FamilyVisibility.Public.ToString() };
        var member = new Member("Doe", "John", "M1", familyId, null, null, null, null, null, null, null, null, null, null, null, null, null, false) { Id = memberId };
        publicFamily.AddMember(member); // Add member to family
        _context.Families.Add(publicFamily);
        await _context.SaveChangesAsync();

        // Mock privacy service to return member as is for this test
        _mockPrivacyService.Setup(x => x.ApplyPrivacyFilter(It.IsAny<MemberDetailDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MemberDetailDto m, Guid fId, CancellationToken ct) => m);

        var query = new GetPublicMemberByIdQuery(memberId, familyId);

        // Debugging assertion
        var memberFromContext = await _context.Members.FindAsync(memberId);
        memberFromContext.Should().NotBeNull("The member should be present in the context.");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(memberId);
        result.Value.FamilyId.Should().Be(familyId);
        result.Value.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMemberDoesNotExist()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var publicFamily = new Family { Id = familyId, Name = "Public Family", Code = "PUB1", Visibility = FamilyVisibility.Public.ToString() };
        _context.Families.Add(publicFamily);
        await _context.SaveChangesAsync();

        var nonExistentMemberId = Guid.NewGuid();
        var query = new GetPublicMemberByIdQuery(nonExistentMemberId, familyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.NotFound, $"Member with ID {nonExistentMemberId} in Family ID {familyId}"));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyDoesNotExist()
    {
        // Arrange
        var nonExistentFamilyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var query = new GetPublicMemberByIdQuery(memberId, nonExistentFamilyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.FamilyNotFound, nonExistentFamilyId));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyExistsButIsPrivate()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var privateFamily = new Family { Id = familyId, Name = "Private Family", Code = "PRIV1", Visibility = FamilyVisibility.Private.ToString() };
        _context.Families.Add(privateFamily);

        var member = new Member("Doe", "John", "M1", familyId, null, null, null, null, null, null, null, null, null, null, null, null, null, false) { Id = memberId };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        var query = new GetPublicMemberByIdQuery(memberId, familyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMemberDoesNotBelongToSpecifiedFamily()
    {
        // Arrange
        var publicFamilyId = Guid.NewGuid();
        var otherFamilyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        var publicFamily = new Family { Id = publicFamilyId, Name = "Public Family", Code = "PUB1", Visibility = FamilyVisibility.Public.ToString() };
        var otherFamily = new Family { Id = otherFamilyId, Name = "Other Family", Code = "OTH1", Visibility = FamilyVisibility.Public.ToString() };
        _context.Families.AddRange(publicFamily, otherFamily);

        var member = new Member("Doe", "John", "M1", otherFamilyId, null, null, null, null, null, null, null, null, null, null, null, null, null, false) { Id = memberId };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        var query = new GetPublicMemberByIdQuery(memberId, publicFamilyId); // Member exists but in a different family

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(string.Format(ErrorMessages.NotFound, $"Member with ID {memberId} in Family ID {publicFamilyId}"));
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldApplyPrivacyFilters()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var publicFamily = new Family { Id = familyId, Name = "Public Family", Code = "PUB1", Visibility = FamilyVisibility.Public.ToString() };
        _context.Families.Add(publicFamily);

        var member = new Member("Doe", "John", "M1", familyId, null, null, null, null, null, null, null, null, null, null, null, "Some biography", null, false) { Id = memberId };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        var filteredMemberDto = new MemberDetailDto { Id = memberId, FirstName = "John", LastName = "", Biography = null }; // Example of filtered data
        _mockPrivacyService.Setup(x => x.ApplyPrivacyFilter(It.IsAny<MemberDetailDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MemberDetailDto m, Guid fId, CancellationToken ct) =>
            {
                m.LastName = "";
                m.Biography = null;
                return m;
            });

        var query = new GetPublicMemberByIdQuery(memberId, familyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(memberId);
        result.Value.FirstName.Should().Be("John");
        result.Value.LastName.Should().Be(""); // Assert that privacy filter was applied
        result.Value.Biography.Should().BeNull(); // Assert that privacy filter was applied
        _mockPrivacyService.Verify(x => x.ApplyPrivacyFilter(It.IsAny<MemberDetailDto>(), familyId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
