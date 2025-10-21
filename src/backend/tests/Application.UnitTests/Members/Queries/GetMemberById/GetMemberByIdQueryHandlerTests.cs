using AutoFixture.Xunit2;
using backend.Application.Members.Queries.GetMemberById;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Members.Queries;

public class GetMemberByIdQueryHandlerTests : TestBase
{
    private readonly GetMemberByIdQueryHandler _handler;

    public GetMemberByIdQueryHandlerTests()
    {
        _handler = new GetMemberByIdQueryHandler(_context, _mapper);
    }

    [Theory, AutoData]
    public async Task Handle_ShouldReturnFailureResult_WhenMemberNotFound(GetMemberByIdQuery query)
    {
        // 🎯 Mục tiêu của test: Đảm bảo handler trả về lỗi khi không tìm thấy Member với ID được yêu cầu.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange: Đảm bảo không có Member nào trong Context với ID của query.
        // (Mặc định Context sẽ trống rỗng, không cần thêm Member nào có ID trùng với query.Id)

        // Act: Gọi handler để xử lý query.
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Member with ID {query.Id} not found.");

        // 💡 Giải thích: Khi không tìm thấy Member trong cơ sở dữ liệu với ID đã cho,
        // handler sẽ trả về Result.Failure với thông báo lỗi tương ứng.
    }

    [Theory, AutoData]
    public async Task Handle_ShouldReturnMemberDetailDto_WhenMemberFound(GetMemberByIdQuery query)
    {
        // 🎯 Mục tiêu của test: Đảm bảo handler trả về MemberDetailDto chính xác khi tìm thấy Member.

        // ⚙️ Các bước (Arrange, Act, Assert):
        // Arrange: Tạo một Family và một Member, sau đó thêm vào Context.
        var family = new Family
        {
            Id = Guid.NewGuid(),
            Name = "Test Family",
            Code = "TF123",
            Description = "A test family description",
            Address = "123 Test St",
            AvatarUrl = "http://example.com/family_avatar.jpg",
            Visibility = "Public",
            TotalMembers = 1
        };
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var member = new Member
        {
            Id = query.Id,
            FamilyId = family.Id,
            FirstName = "John",
            LastName = "Doe",
            Code = "MEMBER123",
            Gender = "Male",
            DateOfBirth = new DateTime(1990, 1, 1),
            PlaceOfBirth = "Test City",
            AvatarUrl = "http://example.com/member_avatar.jpg",
            Occupation = "Engineer",
            Biography = "A test biography."
        };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        // Act: Gọi handler để xử lý query.
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert: Kiểm tra kết quả trả về là thành công và chứa MemberDetailDto đã ánh xạ chính xác.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(member.Id);
        result.Value!.FirstName.Should().Be(member.FirstName);
        result.Value!.LastName.Should().Be(member.LastName);
        result.Value!.Gender.Should().Be(member.Gender);
        result.Value!.DateOfBirth.Should().Be(member.DateOfBirth);
        result.Value!.PlaceOfBirth.Should().Be(member.PlaceOfBirth);
        result.Value!.AvatarUrl.Should().Be(member.AvatarUrl);
        result.Value!.Occupation.Should().Be(member.Occupation);
        result.Value!.FamilyId.Should().Be(member.FamilyId);
        result.Value!.Biography.Should().Be(member.Biography);

        // 💡 Giải thích: Khi tìm thấy Member trong cơ sở dữ liệu với ID đã cho,
        // handler sẽ ánh xạ nó sang MemberDetailDto và trả về Result.Success.
    }
}
