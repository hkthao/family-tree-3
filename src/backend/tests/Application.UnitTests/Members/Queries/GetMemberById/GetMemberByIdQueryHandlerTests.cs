using AutoFixture.Xunit2;
using backend.Application.Common.Constants;
using backend.Application.Members.Queries.GetMemberById;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Members.Queries.GetMemberById;

public class GetMemberByIdQueryHandlerTests : TestBase
{
    private readonly GetMemberByIdQueryHandler _handler;

    public GetMemberByIdQueryHandlerTests()
    {
        _handler = new GetMemberByIdQueryHandler(_context, _mapper);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về một kết quả thất bại
    /// khi không tìm thấy thành viên với ID được yêu cầu.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Đảm bảo không có thành viên nào trong Context với ID của query.
    ///    - Act: Gọi phương thức Handle của handler với query đã tạo.
    ///    - Assert: Kiểm tra xem kết quả trả về là thất bại và có thông báo lỗi phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Khi không tìm thấy thành viên trong cơ sở dữ liệu với ID đã cho,
    /// handler sẽ trả về Result.Failure với thông báo lỗi tương ứng.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailureResult_WhenMemberNotFound()
    {
        // Arrange
        var nonExistentMemberId = Guid.NewGuid();
        var query = new GetMemberByIdQuery(nonExistentMemberId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(string.Format(ErrorMessages.NotFound, $"Member with ID {nonExistentMemberId}"));
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về MemberDetailDto chính xác khi tìm thấy thành viên.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một Family và một Member, sau đó thêm vào Context.
    ///    - Act: Gọi phương thức Handle của handler với query đã tạo.
    ///    - Assert: Kiểm tra kết quả trả về là thành công và chứa MemberDetailDto đã ánh xạ chính xác.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Khi tìm thấy thành viên trong cơ sở dữ liệu với ID đã cho,
    /// handler sẽ ánh xạ nó sang MemberDetailDto và trả về Result.Success.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnMemberDetailDto_WhenMemberFound()
    {
        // Arrange
        var memberId = Guid.NewGuid();
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
            Id = memberId,
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

        var query = new GetMemberByIdQuery(memberId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
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
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Đảm bảo handler trả về MemberDetailDto với các mối quan hệ được bao gồm.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo một Family, hai Member và một Relationship giữa chúng. Thêm tất cả vào Context.
    ///               Tạo một GetMemberByIdQuery cho một trong các thành viên.
    ///    - Act: Gọi handler để xử lý query.
    ///    - Assert: Kiểm tra kết quả trả về là thành công và MemberDetailDto chứa mối quan hệ đã tạo.
    /// 💡 Giải thích: Test này xác minh rằng truy vấn bao gồm dữ liệu mối quan hệ khi ánh xạ sang DTO.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnMemberDetailDtoWithRelationships_WhenMemberFound()
    {
        // Arrange
        var member1Id = Guid.NewGuid();
        var member2Id = Guid.NewGuid();
        var family = new Family
        {
            Id = Guid.NewGuid(),
            Name = "Test Family",
            Code = "TF123",
            Description = "A test family description",
            Address = "123 Test St",
            AvatarUrl = "http://example.com/family_avatar.jpg",
            Visibility = "Public",
            TotalMembers = 2
        };
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var member1 = new Member
        {
            Id = member1Id,
            FamilyId = family.Id,
            FirstName = "John",
            LastName = "Doe",
            Code = "MEMBER1",
            Gender = "Male",
            DateOfBirth = new DateTime(1990, 1, 1),
            PlaceOfBirth = "Test City",
            AvatarUrl = "http://example.com/member1_avatar.jpg",
            Occupation = "Engineer",
            Biography = "A test biography for John."
        };
        var member2 = new Member
        {
            Id = member2Id,
            FamilyId = family.Id,
            FirstName = "Jane",
            LastName = "Doe",
            Code = "MEMBER2",
            Gender = "Female",
            DateOfBirth = new DateTime(1992, 2, 2),
            PlaceOfBirth = "Test City",
            AvatarUrl = "http://example.com/member2_avatar.jpg",
            Occupation = "Doctor",
            Biography = "A test biography for Jane."
        };
        _context.Members.Add(member1);
        _context.Members.Add(member2);
        await _context.SaveChangesAsync();

        var relationship = new Relationship
        {
            Id = Guid.NewGuid(),
            SourceMemberId = member1.Id,
            TargetMemberId = member2.Id,
            Type = RelationshipType.Husband,
            FamilyId = family.Id
        };
        _context.Relationships.Add(relationship);
        await _context.SaveChangesAsync();

        var query = new GetMemberByIdQuery(member1Id);

        // Act: Gọi handler để xử lý query.
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert: Kiểm tra kết quả trả về là thành công và MemberDetailDto với mối quan hệ.
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(member1.Id);
        result.Value!.Relationships.Should().NotBeEmpty();
        result.Value!.Relationships.Should().ContainSingle(r => r.SourceMemberId == member1.Id && r.TargetMemberId == member2.Id && r.Type == RelationshipType.Husband);
    }
}
