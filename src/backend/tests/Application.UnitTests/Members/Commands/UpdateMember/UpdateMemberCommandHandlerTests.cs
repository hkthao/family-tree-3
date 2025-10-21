using AutoFixture;
using AutoFixture.AutoMoq;
using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Members.Commands.UpdateMember;
using backend.Application.UserActivities.Commands.RecordActivity;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

using backend.Application.UnitTests.Common;

namespace backend.Application.UnitTests.Members.Commands.UpdateMember;

public class UpdateMemberCommandHandlerTests : TestBase
{
    private readonly Mock<IAuthorizationService> _mockAuthorizationService;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IFamilyTreeService> _mockFamilyTreeService;
    private readonly UpdateMemberCommandHandler _handler;

    public UpdateMemberCommandHandlerTests()
    {
        _mockAuthorizationService = new Mock<IAuthorizationService>();
        _mockMediator = new Mock<IMediator>();
        _mockFamilyTreeService = new Mock<IFamilyTreeService>();

        _handler = new UpdateMemberCommandHandler(
            _context,
            _mockAuthorizationService.Object,
            _mockMediator.Object,
            _mockFamilyTreeService.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserProfileNotFound()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi không tìm thấy UserProfile.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Mock GetCurrentUserProfileAsync trả về null.
        // 2. Act: Gọi phương thức Handle với một UpdateMemberCommand bất kỳ.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        _mockAuthorizationService.Setup(a => a.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>())).ReturnsAsync((UserProfile)null!);

        var command = _fixture.Create<UpdateMemberCommand>();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User profile not found.");
        // 💡 Giải thích: Handler phải kiểm tra UserProfile trước khi thực hiện các thao tác khác.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMemberNotFound()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi không tìm thấy thành viên.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Mock GetCurrentUserProfileAsync trả về profile hợp lệ. Thêm một thành viên hiện có vào DB.
        // 2. Act: Gọi phương thức Handle với một UpdateMemberCommand có Id không tồn tại.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        _mockAuthorizationService.Setup(a => a.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_fixture.Create<UserProfile>());
        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(true);

        // Add an existing member to the database
        var existingMember = _fixture.Create<Member>();
        _context.Members.Add(existingMember);
        await _context.SaveChangesAsync();

        // Ensure the database contains the existing member
        _context.Members.Any(m => m.Id == existingMember.Id).Should().BeTrue();

        var nonExistentMemberId = Guid.NewGuid();
        var command = _fixture.Build<UpdateMemberCommand>()
            .With(c => c.Id, nonExistentMemberId)
            .Create();

        // Assert that Find also returns null for the non-existent ID
        _context.Members.Find(command.Id).Should().BeNull();

        // Now, try to handle the command and expect a failure result
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Member with ID {command.Id} not found.");
        result.ErrorSource.Should().Be("NotFound");
        // 💡 Giải thích: Handler phải kiểm tra sự tồn tại của thành viên trước khi cập nhật.
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserCannotManageFamily()
    {
        // 🎯 Mục tiêu của test: Xác minh handler trả về lỗi khi người dùng không có quyền quản lý gia đình.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Mock GetCurrentUserProfileAsync trả về profile hợp lệ, IsAdmin trả về false, CanManageFamily trả về false.
        // 2. Act: Gọi phương thức Handle với một UpdateMemberCommand bất kỳ.
        // 3. Assert: Kiểm tra kết quả trả về là thất bại và có thông báo lỗi phù hợp.
        var userProfile = _fixture.Create<UserProfile>();
        _mockAuthorizationService.Setup(a => a.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>())).ReturnsAsync(userProfile);
        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(a => a.CanManageFamily(It.IsAny<Guid>(), It.IsAny<UserProfile>())).Returns(false);

        var command = _fixture.Create<UpdateMemberCommand>();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Access denied. Only family managers can update members.");
        result.ErrorSource.Should().Be("Forbidden");
        // 💡 Giải thích: Người dùng phải có quyền quản lý gia đình để cập nhật thành viên.
    }

    [Fact]
    public async Task Handle_ShouldUpdateMemberSuccessfully_WhenAdminUser()
    {
        // 🎯 Mục tiêu của test: Xác minh handler cập nhật thành viên thành công khi người dùng là admin.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một thành viên, mock GetCurrentUserProfileAsync trả về profile hợp lệ, IsAdmin trả về true.
        // 2. Act: Gọi phương thức Handle với UpdateMemberCommand của thành viên đó.
        // 3. Assert: Kiểm tra kết quả trả về là thành công, thành viên được cập nhật trong context, và các service khác được gọi.
        var userProfile = _fixture.Create<UserProfile>();
        var existingMember = _fixture.Create<Member>();
        _context.Members.Add(existingMember);
        await _context.SaveChangesAsync();

        _mockAuthorizationService.Setup(a => a.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>())).ReturnsAsync(userProfile);
        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(true);
        _mockMediator.Setup(m => m.Send(It.IsAny<RecordActivityCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid()));
        _mockFamilyTreeService.Setup(f => f.UpdateFamilyStats(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                              .Returns(Task.CompletedTask);

        var command = _fixture.Build<UpdateMemberCommand>()
                               .With(c => c.Id, existingMember.Id)
                               .With(c => c.FirstName, "UpdatedFirstName")
                               .With(c => c.LastName, "UpdatedLastName")
                               .With(c => c.Nickname, "UpdatedNickname")
                               .With(c => c.Gender, "Female")
                               .With(c => c.DateOfBirth, new DateTime(1990, 1, 1))
                               .With(c => c.DateOfDeath, new DateTime(2050, 1, 1))
                               .With(c => c.PlaceOfBirth, "UpdatedPlaceOfBirth")
                               .With(c => c.PlaceOfDeath, "UpdatedPlaceOfDeath")
                               .With(c => c.Occupation, "UpdatedOccupation")
                               .With(c => c.Biography, "UpdatedBiography")
                               .With(c => c.FamilyId, existingMember.FamilyId) // Keep same family ID
                               .With(c => c.IsRoot, true)
                               .Create();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(existingMember.Id);

        var updatedMember = await _context.Members.FindAsync(existingMember.Id);
        updatedMember.Should().NotBeNull();
        updatedMember!.FirstName.Should().Be(command.FirstName);
        updatedMember.LastName.Should().Be(command.LastName);
        updatedMember.Nickname.Should().Be(command.Nickname);
        updatedMember.Gender.Should().Be(command.Gender);
        updatedMember.DateOfBirth.Should().Be(command.DateOfBirth);
        updatedMember.DateOfDeath.Should().Be(command.DateOfDeath);
        updatedMember.PlaceOfBirth.Should().Be(command.PlaceOfBirth);
        updatedMember.PlaceOfDeath.Should().Be(command.PlaceOfDeath);
        updatedMember.Occupation.Should().Be(command.Occupation);
        updatedMember.Biography.Should().Be(command.Biography);
        updatedMember.FamilyId.Should().Be(command.FamilyId);
        updatedMember.IsRoot.Should().Be(command.IsRoot);

        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(existingMember.FamilyId, It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Send(It.IsAny<RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        // 💡 Giải thích: Người dùng admin có quyền cập nhật thành viên và các thay đổi được phản ánh chính xác.
    }

    [Fact]
    public async Task Handle_ShouldUpdateMemberSuccessfully_WhenManagerUser()
    {
        // 🎯 Mục tiêu của test: Xác minh handler cập nhật thành viên thành công khi người dùng có quyền quản lý gia đình.
        // ⚙️ Các bước (Arrange, Act, Assert):
        // 1. Arrange: Tạo một thành viên, mock GetCurrentUserProfileAsync trả về profile hợp lệ, IsAdmin trả về false, CanManageFamily trả về true.
        // 2. Act: Gọi phương thức Handle với UpdateMemberCommand của thành viên đó.
        // 3. Assert: Kiểm tra kết quả trả về là thành công, thành viên được cập nhật trong context, và các service khác được gọi.
        var userProfile = _fixture.Create<UserProfile>();
        var existingMember = _fixture.Create<Member>();
        _context.Members.Add(existingMember);
        await _context.SaveChangesAsync();

        _mockAuthorizationService.Setup(a => a.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>())).ReturnsAsync(userProfile);
        _mockAuthorizationService.Setup(a => a.IsAdmin()).Returns(false);
        _mockAuthorizationService.Setup(a => a.CanManageFamily(existingMember.FamilyId, userProfile)).Returns(true);
        _mockMediator.Setup(m => m.Send(It.IsAny<RecordActivityCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid()));
        _mockFamilyTreeService.Setup(f => f.UpdateFamilyStats(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                              .Returns(Task.CompletedTask);

        var command = _fixture.Build<UpdateMemberCommand>()
                               .With(c => c.Id, existingMember.Id)
                               .With(c => c.FirstName, "UpdatedFirstNameByManager")
                               .With(c => c.LastName, "UpdatedLastNameByManager")
                               .With(c => c.Nickname, "UpdatedNicknameByManager")
                               .With(c => c.Gender, "Male")
                               .With(c => c.DateOfBirth, new DateTime(1985, 5, 10))
                               .With(c => c.DateOfDeath, (DateTime?)null)
                               .With(c => c.PlaceOfBirth, "UpdatedPlaceOfBirthByManager")
                               .With(c => c.PlaceOfDeath, (string?)null)
                               .With(c => c.Occupation, "UpdatedOccupationByManager")
                               .With(c => c.Biography, "UpdatedBiographyByManager")
                               .With(c => c.FamilyId, existingMember.FamilyId) // Keep same family ID
                               .With(c => c.IsRoot, false)
                               .Create();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(existingMember.Id);

        var updatedMember = await _context.Members.FindAsync(existingMember.Id);
        updatedMember.Should().NotBeNull();
        updatedMember!.FirstName.Should().Be(command.FirstName);
        updatedMember.LastName.Should().Be(command.LastName);
        updatedMember.Nickname.Should().Be(command.Nickname);
        updatedMember.Gender.Should().Be(command.Gender);
        updatedMember.DateOfBirth.Should().Be(command.DateOfBirth);
        updatedMember.DateOfDeath.Should().Be(command.DateOfDeath);
        updatedMember.PlaceOfBirth.Should().Be(command.PlaceOfBirth);
        updatedMember.PlaceOfDeath.Should().Be(command.PlaceOfDeath);
        updatedMember.Occupation.Should().Be(command.Occupation);
        updatedMember.Biography.Should().Be(command.Biography);
        updatedMember.FamilyId.Should().Be(command.FamilyId);
        updatedMember.IsRoot.Should().Be(command.IsRoot);

        _mockFamilyTreeService.Verify(f => f.UpdateFamilyStats(existingMember.FamilyId, It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Send(It.IsAny<RecordActivityCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        // 💡 Giải thích: Người dùng có quyền quản lý gia đình có thể cập nhật thành viên và các thay đổi được phản ánh chính xác.
    }
}
