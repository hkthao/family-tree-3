using backend.Application.Members.Queries.GetMembersByIds;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;
using backend.Application.UnitTests.Common;

namespace backend.Application.UnitTests.Members.Queries.GetMembersByIds;

public class GetMembersByIdsQueryHandlerTests : TestBase
{
    private readonly GetMembersByIdsQueryHandler _handler;

    public GetMembersByIdsQueryHandlerTests()
    {
        _handler = new GetMembersByIdsQueryHandler(_context, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoIdsProvided()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về danh sách rỗng khi không có ID nào được cung cấp.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một GetMembersByIdsQuery với danh sách ID rỗng.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và danh sách rỗng.
        var query = new GetMembersByIdsQuery(new List<Guid>());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
        // 💡 Giải thích: Nếu không có ID nào được cung cấp, không có thành viên nào được trả về.
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoMembersFoundForIds()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về danh sách rỗng khi không tìm thấy thành viên nào cho các ID đã cung cấp.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một GetMembersByIdsQuery với các ID không tồn tại trong Context.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và danh sách rỗng.
        var query = new GetMembersByIdsQuery(new List<Guid> { Guid.NewGuid(), Guid.NewGuid() });

        // No members added to _context

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
        // 💡 Giải thích: Nếu không có thành viên nào khớp với các ID, danh sách trả về sẽ rỗng.
    }

    [Fact]
    public async Task Handle_ShouldReturnMembers_WhenMembersFoundForIds()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về các thành viên khi tìm thấy chúng cho các ID đã cung cấp.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thêm các thành viên vào Context. Tạo một GetMembersByIdsQuery với ID của các thành viên này.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và chứa các MemberListDto mong đợi.
        var family = new Family { Id = Guid.NewGuid(), Name = "Test Family", Code = "TF001" };
        _context.Families.Add(family);

        var member1 = new Member { Id = Guid.NewGuid(), FamilyId = family.Id, FirstName = "John", LastName = "Doe", Code = "M001" };
        var member2 = new Member { Id = Guid.NewGuid(), FamilyId = family.Id, FirstName = "Jane", LastName = "Smith", Code = "M002" };
        _context.Members.AddRange(member1, member2);
        await _context.SaveChangesAsync();

        var query = new GetMembersByIdsQuery(new List<Guid> { member1.Id, member2.Id });

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(m => m.Id == member1.Id);
        result.Value.Should().Contain(m => m.Id == member2.Id);
        // 💡 Giải thích: Handler phải trả về tất cả các thành viên khớp với các ID đã cung cấp.
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyRequestedMembers_WhenSomeMembersFoundForIds()
    {
        // 🎯 Mục tiêu của test: Xác minh handler chỉ trả về các thành viên được yêu cầu khi một số ID không khớp.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Thêm nhiều thành viên vào Context. Tạo một GetMembersByIdsQuery với một tập hợp con các ID.
        // 2. Act: Gọi phương thức Handle.
        // 3. Assert: Kiểm tra kết quả trả về là thành công và chỉ chứa các MemberListDto được yêu cầu.
        var family = new Family { Id = Guid.NewGuid(), Name = "Test Family", Code = "TF001" };
        _context.Families.Add(family);

        var member1 = new Member { Id = Guid.NewGuid(), FamilyId = family.Id, FirstName = "John", LastName = "Doe", Code = "M001" };
        var member2 = new Member { Id = Guid.NewGuid(), FamilyId = family.Id, FirstName = "Jane", LastName = "Smith", Code = "M002" };
        var member3 = new Member { Id = Guid.NewGuid(), FamilyId = family.Id, FirstName = "Peter", LastName = "Pan", Code = "M003" };
        _context.Members.AddRange(member1, member2, member3);
        await _context.SaveChangesAsync();

        var query = new GetMembersByIdsQuery(new List<Guid> { member1.Id, member3.Id, Guid.NewGuid() }); // member1, member3, and one non-existent ID

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2); // Only member1 and member3
        result.Value.Should().Contain(m => m.Id == member1.Id);
        result.Value.Should().Contain(m => m.Id == member3.Id);
        result.Value.Should().NotContain(m => m.Id == member2.Id);
        // 💡 Giải thích: Handler chỉ nên trả về các thành viên có ID khớp với danh sách yêu cầu.
    }
}
