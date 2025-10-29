using AutoFixture;
using backend.Application.Families;
using backend.Application.Families.Commands.CreateFamilies;
using backend.Application.UnitTests.Common;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.Families.Commands.CreateFamilies;

public class CreateFamiliesCommandHandlerTests : TestBase
{
    private readonly CreateFamiliesCommandHandler _handler;

    public CreateFamiliesCommandHandlerTests()
    {
        _handler = new CreateFamiliesCommandHandler(
            _context,
            _mockUser.Object
        );
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler tạo thành công nhiều gia đình từ một danh sách FamilyDto.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Thiết lập _mockUser với một User ID hợp lệ. Tạo một danh sách FamilyDto.
    ///               Tạo một CreateFamiliesCommand với danh sách FamilyDto.
    ///    - Act: Gọi phương thức Handle của handler.
    ///    - Assert: Kiểm tra xem kết quả trả về là thành công. Kiểm tra xem số lượng gia đình được tạo
    ///              trong DB khớp với số lượng FamilyDto đã cung cấp. Kiểm tra xem các gia đình đã tạo
    ///              có UserProfileId của người dùng hiện tại với vai trò Manager.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Test này đảm bảo rằng handler có thể xử lý
    /// việc tạo hàng loạt gia đình một cách chính xác và gán người dùng tạo làm quản lý.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldCreateMultipleFamiliesSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUser.Setup(u => u.Id).Returns(userId);

        var familyDtos = new List<FamilyDto>
        {
            _fixture.Build<FamilyDto>()
                .With(f => f.Name, "Family One")
                .With(f => f.Visibility, "Public")
                .With(f => f.Code, "FAM001")
                .Without(f => f.Id)
                .Without(f => f.ValidationErrors)
                .Create(),
            _fixture.Build<FamilyDto>()
                .With(f => f.Name, "Family Two")
                .With(f => f.Visibility, "Private")
                .With(f => f.Code, "FAM002")
                .Without(f => f.Id)
                .Without(f => f.ValidationErrors)
                .Create()
        };

        var command = new CreateFamiliesCommand(familyDtos);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);

        _context.Families.Should().HaveCount(2);
        _context.FamilyUsers.Should().HaveCount(2);

        foreach (var familyDto in familyDtos)
        {
            var createdFamily = _context.Families.FirstOrDefault(f => f.Name == familyDto.Name);
            createdFamily.Should().NotBeNull();
            createdFamily!.FamilyUsers.Should().ContainSingle(fu => fu.UserProfileId == userId && fu.Role == FamilyRole.Manager);
        }
    }
}
