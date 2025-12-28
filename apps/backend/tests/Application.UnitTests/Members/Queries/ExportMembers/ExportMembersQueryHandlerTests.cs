using Moq;
using AutoMapper;
using backend.Application.Common.Models;
using backend.Application.Members.Queries; // MemberDto is here
using backend.Application.Members.Queries.ExportMembers;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;
using backend.Application.Common.Interfaces; // Add this using statement

namespace backend.Application.UnitTests.Members.Queries.ExportMembers;

public class ExportMembersQueryHandlerTests : TestBase
{
    private readonly ExportMembersQueryHandler _handler;
    private readonly Mock<IPrivacyService> _mockPrivacyService;

    public ExportMembersQueryHandlerTests()
    {
        _mockPrivacyService = new Mock<IPrivacyService>();
        // Default setup for privacy service to return the DTO as is (no filtering for basic tests)
        _mockPrivacyService.Setup(x => x.ApplyPrivacyFilter(It.IsAny<MemberDto>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MemberDto dto, Guid familyId, CancellationToken token) => dto);
        _mockPrivacyService.Setup(x => x.ApplyPrivacyFilter(It.IsAny<List<MemberDto>>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<MemberDto> dtos, Guid familyId, CancellationToken token) => dtos);

        _handler = new ExportMembersQueryHandler(_context, _mapper, _mockPrivacyService.Object);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler có thể xuất thành viên thành công với đầy đủ MemberDto.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Chuẩn bị family và một số thành viên trong database.
    ///    - Act: Gửi ExportMembersQuery.
    ///    - Assert: Kiểm tra kết quả thành công, nội dung JSON có chứa các thành viên đã tạo với dữ liệu chính xác.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Handler phải xuất đúng dữ liệu thành viên.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldExportMembersSuccessfully()
    {
        // Arrange
        var family = new Family { Id = Guid.NewGuid(), Name = "Test Family", Code = "TF" };
        _context.Families.Add(family);

        var member1 = new Member(
            "John", "Doe", "JD1", family.Id, "Johnny", "Male",
            new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc), null,
            "Place A", null, "123-456-7890", "john@example.com", "Address A", "Engineer", "avatar1.png", "Biography 1", 1, false
        );
        var member2 = new Member(
            "Jane", "Doe", "JD2", family.Id, "Janey", "Female",
            new DateTime(1985, 5, 10, 0, 0, 0, DateTimeKind.Utc), null,
            "Place B", null, "987-654-3210", "jane@example.com", "Address B", "Doctor", "avatar2.png", "Biography 2", 2, false
        );
        _context.Members.AddRange(member1, member2);
        await _context.SaveChangesAsync();

        var query = new ExportMembersQuery(family.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNullOrEmpty();

        var exportedMembers = JsonConvert.DeserializeObject<List<MemberDto>>(result.Value!)!;
        exportedMembers.Should().HaveCount(2);

        var exportedMember1 = exportedMembers.FirstOrDefault(m => m.FirstName == member1.FirstName)!;
        exportedMember1.Should().NotBeNull();
        exportedMember1.Id.Should().Be(member1.Id);
        exportedMember1.LastName.Should().Be(member1.LastName);
        exportedMember1.FirstName.Should().Be(member1.FirstName);
        exportedMember1.FamilyId.Should().Be(member1.FamilyId);
        exportedMember1.DateOfBirth.Should().Be(member1.DateOfBirth);
        exportedMember1.Occupation.Should().Be(member1.Occupation);
        exportedMember1.Biography.Should().Be(member1.Biography);

        var exportedMember2 = exportedMembers.FirstOrDefault(m => m.FirstName == member2.FirstName)!;
        exportedMember2.Should().NotBeNull();
        exportedMember2.Id.Should().Be(member2.Id);
        exportedMember2.LastName.Should().Be(member2.LastName);
        exportedMember2.FirstName.Should().Be(member2.FirstName);
        exportedMember2.FamilyId.Should().Be(member2.FamilyId);
        exportedMember2.DateOfBirth.Should().Be(member2.DateOfBirth);
        exportedMember2.Occupation.Should().Be(member2.Occupation);
        exportedMember2.Biography.Should().Be(member2.Biography);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về lỗi khi không tìm thấy thành viên nào cho familyId.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Chuẩn bị một family nhưng không có thành viên nào.
    ///    - Act: Gửi ExportMembersQuery.
    ///    - Assert: Kiểm tra kết quả thất bại với thông báo lỗi phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Không có thành viên nào để xuất.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailureWhenNoMembersFound()
    {
        // Arrange
        var family = new Family { Id = Guid.NewGuid(), Name = "Empty Family", Code = "EF" };
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var query = new ExportMembersQuery(family.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Không tìm thấy thành viên nào cho gia đình này.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về lỗi khi familyId không tồn tại.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Không có family nào trong database với familyId được cung cấp.
    ///    - Act: Gửi ExportMembersQuery với một familyId ngẫu nhiên.
    ///    - Assert: Kiểm tra kết quả thất bại với thông báo lỗi phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Query trả về một lỗi nếu familyId không tồn tại.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailureWhenFamilyIdDoesNotExist()
    {
        // Arrange
        var nonExistentFamilyId = Guid.NewGuid();
        var query = new ExportMembersQuery(nonExistentFamilyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Không tìm thấy thành viên nào cho gia đình này.");
    }
}
