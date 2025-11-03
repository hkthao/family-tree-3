using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Members.Queries.GetMembers;
using backend.Application.Members.Queries.GetMembersByIds;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.UnitTests.Members.Queries.GetMembersByIds;

public class GetMembersByIdsQueryHandlerTests : TestBase
{
    public GetMembersByIdsQueryHandlerTests()
    {
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về các thành viên khi các thành viên tồn tại.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnMembers_WhenMembersExist()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var member1Id = Guid.NewGuid();
        var member2Id = Guid.NewGuid();

        var existingFamily = new Family { Id = familyId, Name = "Test Family", Code = "TF001" };

        var member1 = new Member("Doe", "John", "JD001", familyId) { Id = member1Id };
        var member2 = new Member("Smith", "Jane", "JS001", familyId) { Id = member2Id };
        var member3 = new Member("Brown", "Peter", "PB001", familyId) { Id = Guid.NewGuid() };

        _context.Families.Add(existingFamily);
        _context.Members.AddRange(member1, member2, member3);
        await _context.SaveChangesAsync();

        var query = new GetMembersByIdsQuery(new List<Guid> { member1Id, member2Id });

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new GetMembersByIdsQueryHandler(handlerContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(m => m.Id == member1Id);
        result.Value.Should().Contain(m => m.Id == member2Id);
        result.Value.Should().NotContain(m => m.Id == member3.Id);
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về danh sách rỗng khi không có thành viên nào tồn tại cho các ID đã cho.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoMembersExistForGivenIds()
    {
        // Arrange
        var member1Id = Guid.NewGuid();
        var member2Id = Guid.NewGuid();

        var query = new GetMembersByIdsQuery(new List<Guid> { member1Id, member2Id });

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new GetMembersByIdsQueryHandler(handlerContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về danh sách một phần khi một số thành viên tồn tại và một số không tồn tại.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnPartialList_WhenSomeMembersExistAndSomeDoNotExist()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var member1Id = Guid.NewGuid();
        var nonExistentMemberId = Guid.NewGuid();

        var existingFamily = new Family { Id = familyId, Name = "Test Family", Code = "TF001" };

        var member1 = new Member("Doe", "John", "JD001", familyId) { Id = member1Id };

        _context.Families.Add(existingFamily);
        _context.Members.Add(member1);
        await _context.SaveChangesAsync();

        var query = new GetMembersByIdsQuery(new List<Guid> { member1Id, nonExistentMemberId });

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new GetMembersByIdsQueryHandler(handlerContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value.Should().Contain(m => m.Id == member1Id);
        result.Value.Should().NotContain(m => m.Id == nonExistentMemberId);
    }
}
