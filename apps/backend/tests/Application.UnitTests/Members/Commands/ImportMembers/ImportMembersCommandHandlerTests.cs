using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Members.Commands.ImportMembers;
using backend.Application.Members.DTOs; // MemberImportDto is here
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Members.Commands.ImportMembers;

public class ImportMembersCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly ImportMembersCommandHandler _handler;

    public ImportMembersCommandHandlerTests()
    {
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _handler = new ImportMembersCommandHandler(_context, _authorizationServiceMock.Object);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler có thể nhập các thành viên thành công, bao gồm cả các thuộc tính đầy đủ và mối quan hệ giữa các thành viên được nhập.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Chuẩn bị family và một số thành viên (nếu cần cho mối quan hệ). Tạo ImportMembersCommand với danh sách MemberImportDto.
    ///    - Act: Gửi ImportMembersCommand.
    ///    - Assert: Kiểm tra kết quả thành công, các thành viên được thêm vào database đúng cách với dữ liệu đầy đủ.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Handler phải nhập đúng dữ liệu thành viên và thiết lập các mối quan hệ (nếu có).
    /// </summary>
    [Fact]
    public async Task Handle_ShouldImportMembersSuccessfully()
    {
        // Arrange
        var family = new Family { Id = Guid.NewGuid(), Name = "Test Family", Code = "TF" };
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(family.Id)).Returns(true);

        var memberDtos = new List<MemberImportDto>
        {
            new MemberImportDto
            {
                Id = Guid.NewGuid(), // Original ID for relationship mapping
                FirstName = "Imported",
                LastName = "Father",
                Code = "IMF",
                Gender = "Male",
                DateOfBirth = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsRoot = true,
                Biography = "Biography of Imported Father"
            },
            new MemberImportDto
            {
                Id = Guid.NewGuid(), // Original ID for relationship mapping
                FirstName = "Imported",
                LastName = "Child",
                Code = "IMC",
                Gender = "Female",
                DateOfBirth = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                FatherId = default // This will be updated later with the actual ID
            }
        };
        // Manually set FatherId for the child to reference the father within the imported list
        memberDtos[1].FatherId = memberDtos[0].Id;


        var command = new ImportMembersCommand(family.Id, memberDtos);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _context.Members.Should().HaveCount(2);

        var importedFather = _context.Members.FirstOrDefault(m => m.FirstName == "Imported" && m.LastName == "Father");
        importedFather.Should().NotBeNull();
        importedFather!.IsRoot.Should().BeTrue();
        importedFather.Biography.Should().Be("Biography of Imported Father");

        var importedChild = _context.Members.FirstOrDefault(m => m.FirstName == "Imported" && m.LastName == "Child");
        importedChild.Should().NotBeNull();
        importedChild!.FatherId.Should().Be(importedFather.Id);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về lỗi khi familyId không tồn tại.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Tạo ImportMembersCommand với familyId không tồn tại.
    ///    - Act: Gửi ImportMembersCommand.
    ///    - Assert: Kiểm tra kết quả thất bại với thông báo lỗi phù hợp.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Thành viên phải thuộc về một family tồn tại.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailureWhenFamilyNotFound()
    {
        // Arrange
        var nonExistentFamilyId = Guid.NewGuid();
        _authorizationServiceMock.Setup(x => x.CanManageFamily(nonExistentFamilyId)).Returns(true);

        var command = new ImportMembersCommand(nonExistentFamilyId, new List<MemberImportDto>
        {
            new MemberImportDto { Id = Guid.NewGuid(), FirstName = "Test", LastName = "Member", Code = "TM" }
        });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Family with ID {nonExistentFamilyId} not found.");
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler trả về lỗi khi người dùng không được ủy quyền.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Chuẩn bị family và thiết lập ủy quyền trả về false.
    ///    - Act: Gửi ImportMembersCommand.
    ///    - Assert: Kiểm tra kết quả thất bại với thông báo lỗi AccessDenied.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Người dùng phải có quyền quản lý family để nhập thành viên.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailureWhenUserNotAuthorized()
    {
        // Arrange
        var family = new Family { Id = Guid.NewGuid(), Name = "Unauthorized Family", Code = "UF" };
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(family.Id)).Returns(false);

        var command = new ImportMembersCommand(family.Id, new List<MemberImportDto>
        {
            new MemberImportDto { Id = Guid.NewGuid(), FirstName = "Test", LastName = "Member", Code = "TM" }
        });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }

    /// <summary>
    /// 🎯 Mục tiêu của test: Xác minh rằng handler xử lý đúng các mối quan hệ đến các thành viên đã tồn tại trong database.
    /// ⚙️ Các bước (Arrange, Act, Assert):
    ///    - Arrange: Chuẩn bị family và một thành viên cha đã tồn tại. Tạo ImportMembersCommand với thành viên con có FatherId là ID của cha đã tồn tại.
    ///    - Act: Gửi ImportMembersCommand.
    ///    - Assert: Kiểm tra kết quả thành công và thành viên con có FatherId trỏ đúng đến cha đã tồn tại.
    /// 💡 Giải thích vì sao kết quả mong đợi là đúng: Handler phải liên kết thành viên được nhập với thành viên đã tồn tại.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldHandleRelationshipsToExistingMembersCorrectly()
    {
        // Arrange
        var family = new Family { Id = Guid.NewGuid(), Name = "Test Family", Code = "TF" };
        var existingFather = new Member(
            "Existing", "Father", "EF", family.Id,
            null, // Nickname
            "Male", // Gender
            null, // DateOfBirth
            null, // DateOfDeath
            null, // PlaceOfBirth
            null, // PlaceOfDeath
            null, // Phone
            null, // Email
            null, // Address
            null, // Occupation
            null, // AvatarUrl
            null, // Biography
            null, // Order
            false // IsDeceased
        );
        _context.Families.Add(family);
        _context.Members.Add(existingFather);
        await _context.SaveChangesAsync();

        _authorizationServiceMock.Setup(x => x.CanManageFamily(family.Id)).Returns(true);

        var memberDtos = new List<MemberImportDto>
        {
            new MemberImportDto
            {
                Id = Guid.NewGuid(),
                FirstName = "Imported",
                LastName = "Child",
                Code = "IC",
                Gender = "Female",
                FatherId = existingFather.Id // Father is an existing member
            }
        };

        var command = new ImportMembersCommand(family.Id, memberDtos);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _context.Members.Should().HaveCount(2); // Existing father + new child

        var importedChild = _context.Members.FirstOrDefault(m => m.FirstName == "Imported");
        importedChild.Should().NotBeNull();
        importedChild!.FatherId.Should().Be(existingFather.Id);
    }
}
