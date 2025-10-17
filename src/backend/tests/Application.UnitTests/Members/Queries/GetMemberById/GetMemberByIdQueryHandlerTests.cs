using backend.Application.Members.Queries.GetMemberById;
using FluentAssertions;
using Xunit;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;

namespace backend.Application.UnitTests.Members.Queries.GetMemberById;

public class GetMemberByIdQueryHandlerTests : TestBase
{
    private readonly GetMemberByIdQueryHandler _handler;

    public GetMemberByIdQueryHandlerTests()
    {
        _handler = new GetMemberByIdQueryHandler(_context, _mapper);
    }

    private async Task ClearDatabaseAndSetupData(Guid familyId)
    {
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        _context.Members.RemoveRange(_context.Members);
        _context.Events.RemoveRange(_context.Events);
        _context.Families.RemoveRange(_context.Families);
        _context.UserProfiles.RemoveRange(_context.UserProfiles);
        await _context.SaveChangesAsync(CancellationToken.None);

        _context.Families.Add(new Family { Id = familyId, Name = "Test Family" });
        await _context.SaveChangesAsync(CancellationToken.None);
    }

    /// <summary>
    /// Kiểm tra xem handler có trả về đúng thành viên khi tìm thấy bằng ID.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi một ID thành viên hợp lệ được cung cấp,
    /// handler sẽ truy xuất và trả về thông tin chi tiết của thành viên đó.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_Return_Member_When_Found()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupData(familyId);

        var memberId = Guid.NewGuid();
        var memberFirstName = "Thành viên";
        var memberLastName = "Tìm thấy";
        _context.Members.Add(new Member { Id = memberId, FirstName = memberFirstName, LastName = memberLastName, FamilyId = familyId });
        await _context.SaveChangesAsync(CancellationToken.None);

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(new GetMemberByIdQuery(memberId), CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.Should().NotBeNull();
        result.Value!.Id.Should().Be(memberId);
        result.Value.FullName.Should().Be($"{memberLastName} {memberFirstName}");
    }

    /// <summary>
    /// Kiểm tra xem có trả về lỗi khi không tìm thấy thành viên.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi một ID thành viên không tồn tại được cung cấp,
    /// handler sẽ trả về kết quả thất bại với thông báo lỗi "không tìm thấy".
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnFailure_When_NotFound()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupData(familyId);

        var nonExistentMemberId = Guid.NewGuid();

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(new GetMemberByIdQuery(nonExistentMemberId), CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Member with ID {nonExistentMemberId} not found.");
    }
}
