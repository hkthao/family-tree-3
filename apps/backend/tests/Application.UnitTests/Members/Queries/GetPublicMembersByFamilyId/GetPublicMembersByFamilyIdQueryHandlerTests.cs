using AutoMapper;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Members.Queries.GetMembers;
using backend.Application.Members.Queries.GetPublicMembersByFamilyId;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Members.Queries.GetPublicMembersByFamilyId;

public class GetPublicMembersByFamilyIdQueryHandlerTests : TestBase
{
    private readonly GetPublicMembersByFamilyIdQueryHandler _handler;
    private readonly Mock<IPrivacyService> _mockPrivacyService;

    public GetPublicMembersByFamilyIdQueryHandlerTests()
    {
        _mockPrivacyService = new Mock<IPrivacyService>();
        _handler = new GetPublicMembersByFamilyIdQueryHandler(_context, _mapper, _mockPrivacyService.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnMembers_WhenFamilyExistsAndIsPublic()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var publicFamily = new Family { Id = familyId, Name = "Public Family", Code = "PUB1", Visibility = FamilyVisibility.Public.ToString() };
        _context.Families.Add(publicFamily);

        var member1 = new Member("Doe", "John", "M1", familyId) { Id = Guid.NewGuid() };
        var member2 = new Member("Doe", "Jane", "M2", familyId) { Id = Guid.NewGuid() };
        _context.Members.AddRange(member1, member2);
        await _context.SaveChangesAsync();

        // Mock privacy service to return members as is for this test
        _mockPrivacyService.Setup(x => x.ApplyPrivacyFilter(It.IsAny<List<MemberListDto>>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<MemberListDto> members, Guid fId, CancellationToken ct) => members);

        var query = new GetPublicMembersByFamilyIdQuery(familyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(2);
        result.Value.Should().Contain(m => m.Id == member1.Id);
        result.Value.Should().Contain(m => m.Id == member2.Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFamilyDoesNotExist()
    {
        // Arrange
        var nonExistentFamilyId = Guid.NewGuid();
        var query = new GetPublicMembersByFamilyIdQuery(nonExistentFamilyId);

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
        var privateFamily = new Family { Id = familyId, Name = "Private Family", Code = "PRIV1", Visibility = FamilyVisibility.Private.ToString() };
        _context.Families.Add(privateFamily);
        await _context.SaveChangesAsync();

        var query = new GetPublicMembersByFamilyIdQuery(familyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }

    [Fact]
    public async Task Handle_ShouldApplyPrivacyFilters()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var publicFamily = new Family { Id = familyId, Name = "Public Family", Code = "PUB1", Visibility = FamilyVisibility.Public.ToString() };
        _context.Families.Add(publicFamily);

        var member1 = new Member("Doe", "John", "M1", familyId) { Id = Guid.NewGuid() };
        _context.Members.Add(member1);
        await _context.SaveChangesAsync();

        var filteredMemberDto = new MemberListDto { Id = member1.Id, FirstName = "John", LastName = "" }; // Example of filtered data
        _mockPrivacyService.Setup(x => x.ApplyPrivacyFilter(It.IsAny<List<MemberListDto>>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<MemberListDto> members, Guid fId, CancellationToken ct) =>
            {
                members.ForEach(m =>
                {
                    m.LastName = "";
                });
                return members;
            });

        var query = new GetPublicMembersByFamilyIdQuery(familyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull().And.NotBeEmpty(); // Ensure not null and not empty
        var actualMember = result.Value!.First(); // Assign to a variable after null/empty check
        actualMember.Id.Should().Be(member1.Id);
        actualMember.FirstName.Should().Be("John");
        actualMember.LastName.Should().Be(""); // Assert that privacy filter was applied
        _mockPrivacyService.Verify(x => x.ApplyPrivacyFilter(It.IsAny<List<MemberListDto>>(), familyId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
