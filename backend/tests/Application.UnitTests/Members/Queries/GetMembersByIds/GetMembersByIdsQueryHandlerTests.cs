using backend.Application.Members.Queries.GetMembersByIds;
using FluentAssertions;
using Xunit;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;

namespace backend.Application.UnitTests.Members.Queries.GetMembersByIds;

public class GetMembersByIdsQueryHandlerTests : TestBase
{
    private readonly GetMembersByIdsQueryHandler _handler;

    public GetMembersByIdsQueryHandlerTests()
    {
        _handler = new GetMembersByIdsQueryHandler(_context, _mapper);
    }

    private async Task ClearDatabaseAndSetupData(Guid familyId)
    {
        _context.FamilyUsers.RemoveRange(_context.FamilyUsers);
        _context.Members.RemoveRange(_context.Members);
        _context.Events.RemoveRange(_context.Events);
        _context.Families.RemoveRange(_context.Families);
        _context.UserProfiles.RemoveRange(_context.UserProfiles);
        _context.AIBiographies.RemoveRange(_context.AIBiographies);
        await _context.SaveChangesAsync(CancellationToken.None);

        _context.Families.Add(new Family { Id = familyId, Name = "Test Family" });
        await _context.SaveChangesAsync(CancellationToken.None);
    }

    /// <summary>
    /// Kiểm tra xem có thể truy xuất các thành viên thành công bằng danh sách ID.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi cung cấp một danh sách các ID thành viên hợp lệ,
    /// handler sẽ trả về chính xác các thành viên tương ứng.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnMembers_When_IdsExist()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupData(familyId);

        // Thêm một số thành viên vào cơ sở dữ liệu.
        var member1 = new Member { Id = Guid.NewGuid(), FirstName = "Thành viên", LastName = "1", FamilyId = familyId };
        var member2 = new Member { Id = Guid.NewGuid(), FirstName = "Thành viên", LastName = "2", FamilyId = familyId };
        _context.Members.Add(member1);
        _context.Members.Add(member2);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Tạo truy vấn với các ID của thành viên.
        var query = new GetMembersByIdsQuery(new List<Guid> { member1.Id, member2.Id });

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        // Đảm bảo truy vấn thành công và trả về 2 thành viên.
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(2);
        result.Value.Should().Contain(m => m.Id == member1.Id && m.FullName == $"{member1.LastName} {member1.FirstName}");
        result.Value.Should().Contain(m => m.Id == member2.Id && m.FullName == $"{member2.LastName} {member2.FirstName}");
    }

    /// <summary>
    /// Kiểm tra xem có thể truy xuất các thành viên khi một số ID không tồn tại.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi cung cấp một danh sách ID bao gồm cả ID tồn tại và không tồn tại,
    /// handler sẽ chỉ trả về các thành viên có ID tồn tại.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnOnlyExistingMembers_When_SomeIdsDoNotExist()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupData(familyId);

        // Thêm một thành viên vào cơ sở dữ liệu.
        var existingMember = new Member { Id = Guid.NewGuid(), FirstName = "Thành viên", LastName = "Tồn Tại", FamilyId = familyId };
        _context.Members.Add(existingMember);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Tạo truy vấn với một ID tồn tại và một ID không tồn tại.
        var nonExistentId = Guid.NewGuid();
        var query = new GetMembersByIdsQuery(new List<Guid> { existingMember.Id, nonExistentId });

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        // Đảm bảo truy vấn thành công và chỉ trả về 1 thành viên (thành viên tồn tại).
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(1);
        result.Value!.First()!.Id.Should().Be(existingMember.Id);
    }

    /// <summary>
    /// Kiểm tra xem có trả về danh sách rỗng khi không có ID nào được cung cấp.
    /// </summary>
    /// <remarks>
    /// Test này đảm bảo rằng khi cung cấp một danh sách ID rỗng,
    /// handler sẽ trả về một danh sách thành viên rỗng.
    /// </remarks>
    [Fact]
    public async Task Handle_Should_ReturnEmptyList_When_NoIdsProvided()
    {
        // Arrange (Thiết lập môi trường cho bài kiểm tra)
        var familyId = Guid.NewGuid();
        await ClearDatabaseAndSetupData(familyId);

        // Tạo truy vấn với danh sách ID rỗng.
        var query = new GetMembersByIdsQuery(new List<Guid>());

        // Act (Thực hiện hành động cần kiểm tra)
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert (Kiểm tra kết quả)
        // Đảm bảo truy vấn thành công và trả về danh sách rỗng.
        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().BeEmpty();
    }
}