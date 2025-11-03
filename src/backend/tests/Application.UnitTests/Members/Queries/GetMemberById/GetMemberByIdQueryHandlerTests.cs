using AutoMapper;
using AutoMapper.QueryableExtensions;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Members.Queries.GetMemberById;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.UnitTests.Members.Queries.GetMemberById;

public class GetMemberByIdQueryHandlerTests : TestBase
{
    public GetMemberByIdQueryHandlerTests()
    {
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về MemberDetailDto khi thành viên tồn tại.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnMemberDetailDto_WhenMemberExists()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        var existingFamily = new Family
        {
            Id = familyId,
            Name = "Test Family",
            Code = "TF001"
        };

        var existingMember = new Member("LastName", "FirstName", "MEM001", familyId)
        {
            Id = memberId,
            Gender = "Male",
            DateOfBirth = new DateTime(1990, 1, 1),
            Biography = "Some biography"
        };

        _context.Families.Add(existingFamily);
        _context.Members.Add(existingMember);
        await _context.SaveChangesAsync();

        var memberDetailDto = new MemberDetailDto
        {
            Id = memberId,
            FirstName = "FirstName",
            LastName = "LastName",
            Gender = "Male",
            DateOfBirth = new DateTime(1990, 1, 1),
            Biography = "Some biography",
            FamilyId = familyId
        };

        var query = new GetMemberByIdQuery(memberId);

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new GetMemberByIdQueryHandler(handlerContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(memberId);
        result.Value.FirstName.Should().Be(memberDetailDto.FirstName);
        result.Value.LastName.Should().Be(memberDetailDto.LastName);
        result.Value.FullName.Should().Be("LastName FirstName"); // FullName is derived
        result.Value.Gender.Should().Be(memberDetailDto.Gender);
        result.Value.DateOfBirth.Should().Be(memberDetailDto.DateOfBirth);
        result.Value.Biography.Should().Be(memberDetailDto.Biography);
        result.Value.FamilyId.Should().Be(memberDetailDto.FamilyId);
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về lỗi NotFound khi không tìm thấy thành viên.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenMemberDoesNotExist()
    {
        // Arrange
        var memberId = Guid.NewGuid(); // Member này sẽ không tồn tại

        var query = new GetMemberByIdQuery(memberId);

        var handlerContext = new ApplicationDbContext(_dbContextOptions);
        var handler = new GetMemberByIdQueryHandler(handlerContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Member with ID {memberId} not found.");
    }
}
