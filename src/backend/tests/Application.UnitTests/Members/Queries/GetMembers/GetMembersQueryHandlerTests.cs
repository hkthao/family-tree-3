using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Members.Queries.GetMembers;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.UnitTests.Members.Queries.GetMembers;

public class GetMembersQueryHandlerTests : TestBase
{
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;

    public GetMembersQueryHandlerTests()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về tất cả các thành viên khi là admin và không chỉ định FamilyId.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnAllMembers_WhenAdminAndNoFamilyIdSpecified()
    {
        // Arrange
        var family1Id = Guid.NewGuid();
        var family2Id = Guid.NewGuid();

        var family1 = new Family { Id = family1Id, Name = "Family A", Code = "FA" };
        var family2 = new Family { Id = family2Id, Name = "Family B", Code = "FB" };

        var member1 = new Member("Doe", "John", "JD001", family1Id);
        var member2 = new Member("Smith", "Jane", "JS001", family1Id);
        var member3 = new Member("Brown", "Peter", "PB001", family2Id);

        _context.Families.AddRange(family1, family2);
        _context.Members.AddRange(member1, member2, member3);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(true);

        var query = new GetMembersQuery { FamilyId = Guid.Empty }; // No specific family ID

        var handler = new GetMembersQueryHandler(_context, _mapper, _currentUserMock.Object, _authorizationServiceMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(3);
        result.Value.Should().Contain(m => m.FullName == "John Doe");
        result.Value.Should().Contain(m => m.FullName == "Jane Smith");
        result.Value.Should().Contain(m => m.FullName == "Peter Brown");
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về thành viên cho một gia đình cụ thể khi là admin và FamilyId được chỉ định.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnMembersForSpecificFamily_WhenAdminAndFamilyIdSpecified()
    {
        // Arrange
        var family1Id = Guid.NewGuid();
        var family2Id = Guid.NewGuid();

        var family1 = new Family { Id = family1Id, Name = "Family A", Code = "FA" };
        var family2 = new Family { Id = family2Id, Name = "Family B", Code = "FB" };

        var member1 = new Member("Doe", "John", "JD001", family1Id);
        var member2 = new Member("Smith", "Jane", "JS001", family1Id);
        var member3 = new Member("Brown", "Peter", "PB001", family2Id);

        _context.Families.AddRange(family1, family2);
        _context.Members.AddRange(member1, member2, member3);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(true);

        var query = new GetMembersQuery { FamilyId = family1Id }; // Specific family ID

        var handler = new GetMembersQueryHandler(_context, _mapper, _currentUserMock.Object, _authorizationServiceMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(m => m.FullName == "John Doe");
        result.Value.Should().Contain(m => m.FullName == "Jane Smith");
        result.Value.Should().NotContain(m => m.FullName == "Peter Brown");
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về thành viên cho các gia đình mà người dùng có quyền truy cập khi không phải admin và không chỉ định FamilyId.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnMembersForAccessibleFamilies_WhenNonAdminAndNoFamilyIdSpecified()
    {
        // Arrange
        var family1Id = Guid.NewGuid();
        var family2Id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var family1 = new Family { Id = family1Id, Name = "Family A", Code = "FA" };
        var family2 = new Family { Id = family2Id, Name = "Family B", Code = "FB" };

        var member1 = new Member("Doe", "John", "JD001", family1Id);
        var member2 = new Member("Smith", "Jane", "JS001", family1Id);
        var member3 = new Member("Brown", "Peter", "PB001", family2Id);

        var familyUser1 = new FamilyUser(family1Id, userId, FamilyRole.Manager);

        _context.Families.AddRange(family1, family2);
        _context.Members.AddRange(member1, member2, member3);
        _context.FamilyUsers.Add(familyUser1);
        await _context.SaveChangesAsync();

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(false);

        var query = new GetMembersQuery { FamilyId = Guid.Empty }; // No specific family ID

        var handler = new GetMembersQueryHandler(_context, _mapper, _currentUserMock.Object, _authorizationServiceMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(m => m.FullName == "John Doe");
        result.Value.Should().Contain(m => m.FullName == "Jane Smith");
        result.Value.Should().NotContain(m => m.FullName == "Peter Brown");
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về thành viên cho một gia đình cụ thể mà người dùng có quyền truy cập khi không phải admin và FamilyId được chỉ định.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnMembersForSpecificAccessibleFamily_WhenNonAdminAndFamilyIdSpecified()
    {
        // Arrange
        var family1Id = Guid.NewGuid();
        var family2Id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var family1 = new Family { Id = family1Id, Name = "Family A", Code = "FA" };
        var family2 = new Family { Id = family2Id, Name = "Family B", Code = "FB" };

        var member1 = new Member("Doe", "John", "JD001", family1Id);
        var member2 = new Member("Smith", "Jane", "JS001", family1Id);
        var member3 = new Member("Brown", "Peter", "PB001", family2Id);

        var familyUser1 = new FamilyUser(family1Id, userId, FamilyRole.Manager);

        _context.Families.AddRange(family1, family2);
        _context.Members.AddRange(member1, member2, member3);
        _context.FamilyUsers.Add(familyUser1);
        await _context.SaveChangesAsync();

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(false);

        var query = new GetMembersQuery { FamilyId = family1Id }; // Specific accessible family ID

        var handler = new GetMembersQueryHandler(_context, _mapper, _currentUserMock.Object, _authorizationServiceMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(m => m.FullName == "John Doe");
        result.Value.Should().Contain(m => m.FullName == "Jane Smith");
        result.Value.Should().NotContain(m => m.FullName == "Peter Brown");
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về lỗi AccessDenied khi không phải admin và FamilyId được chỉ định nhưng không thể truy cập.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnAccessDenied_WhenNonAdminAndFamilyIdSpecifiedButNotAccessible()
    {
        // Arrange
        var family1Id = Guid.NewGuid();
        var family2Id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var family1 = new Family { Id = family1Id, Name = "Family A", Code = "FA" };
        var family2 = new Family { Id = family2Id, Name = "Family B", Code = "FB" };

        var member1 = new Member("Doe", "John", "JD001", family1Id);
        var member2 = new Member("Smith", "Jane", "JS001", family1Id);
        var member3 = new Member("Brown", "Peter", "PB001", family2Id);

        var familyUser1 = new FamilyUser(family1Id, userId, FamilyRole.Manager);

        _context.Families.AddRange(family1, family2);
        _context.Members.AddRange(member1, member2, member3);
        _context.FamilyUsers.Add(familyUser1);
        await _context.SaveChangesAsync();

        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(false);

        var query = new GetMembersQuery { FamilyId = family2Id }; // Specific inaccessible family ID

        var handler = new GetMembersQueryHandler(_context, _mapper, _currentUserMock.Object, _authorizationServiceMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(ErrorMessages.AccessDenied);
    }

    /// <summary>
    /// Kiểm tra xem handler có lọc thành viên theo thuật ngữ tìm kiếm.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldFilterMembersBySearchTerm()
    {
        // Arrange
        var familyId = Guid.NewGuid();

        var family = new Family { Id = familyId, Name = "Family A", Code = "FA" };

        var member1 = new Member("Doe", "John", "JD001", familyId);
        var member2 = new Member("Smith", "Jane", "JS001", familyId);
        var member3 = new Member("Brown", "Peter", "PB001", familyId);

        _context.Families.Add(family);
        _context.Members.AddRange(member1, member2, member3);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.IsAdmin()).Returns(true);

        var query = new GetMembersQuery { SearchTerm = "john" };

        var handler = new GetMembersQueryHandler(_context, _mapper, _currentUserMock.Object, _authorizationServiceMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value.Should().Contain(m => m.FullName == "John Doe");
    }
}
