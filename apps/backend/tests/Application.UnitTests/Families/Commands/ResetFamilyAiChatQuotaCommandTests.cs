using backend.Application.Common.Constants; // ADDED
using backend.Application.Common.Models;
using backend.Application.Families.Commands.ResetFamilyAiChatQuota; // Updated namespace
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.Families.Commands;

public class ResetFamilyAiChatQuotaCommandTests : TestBase
{
    public ResetFamilyAiChatQuotaCommandTests()
    {
        _mockAuthorizationService.Setup(s => s.AuthorizeAsync(It.IsAny<string>())) // ADDED
                                 .ReturnsAsync(Result.Success()); // ADDED
    }

    // Test thành công khi đặt lại hạn mức trò chuyện AI cho gia đình tồn tại.
    [Fact]
    public async Task Handle_GivenExistingFamily_ShouldResetAiChatUsage()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = Family.Create("Test Family", "TF001", null, null, "Public", Guid.NewGuid());
        family.Id = familyId; // Gán ID đã tạo
        family.FamilyLimitConfiguration!.Update(50, 1024, 100); // Đặt giới hạn
        family.FamilyLimitConfiguration.IncrementAiChatUsage(); // Tăng sử dụng
        family.FamilyLimitConfiguration.IncrementAiChatUsage(); // Tăng sử dụng
        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new ResetFamilyAiChatQuotaCommand { FamilyId = familyId };
        var handler = new ResetFamilyAiChatQuotaCommandHandler(_context, _mockAuthorizationService.Object); // MODIFIED

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var updatedFamily = await _context.Families
            .Include(f => f.FamilyLimitConfiguration)
            .FirstOrDefaultAsync(f => f.Id == familyId);

        updatedFamily.Should().NotBeNull();
        updatedFamily!.FamilyLimitConfiguration.Should().NotBeNull();
        updatedFamily.FamilyLimitConfiguration!.AiChatMonthlyUsage.Should().Be(0); // Kiểm tra hạn mức đã được đặt lại
    }

    // Test thất bại khi gia đình không tồn tại.
    [Fact]
    public async Task Handle_GivenNonExistingFamily_ShouldReturnNotFound()
    {
        // Arrange
        var familyId = Guid.NewGuid(); // Một ID không tồn tại
        var command = new ResetFamilyAiChatQuotaCommand { FamilyId = familyId };
        var handler = new ResetFamilyAiChatQuotaCommandHandler(_context, _mockAuthorizationService.Object); // MODIFIED

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Không tìm thấy gia đình với ID '{familyId}'.");
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    // Test thất bại khi FamilyLimitConfiguration không tồn tại (trường hợp hiếm nhưng cần kiểm tra).
    [Fact]
    public async Task Handle_GivenFamilyWithoutLimitConfiguration_ShouldReturnNotFound()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = Family.Create("Test Family No Limit", "TF002", null, null, "Public", Guid.NewGuid());
        family.Id = familyId;
        // Loại bỏ FamilyLimitConfiguration để kiểm tra trường hợp này
        family.FamilyLimitConfiguration = null;
        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new ResetFamilyAiChatQuotaCommand { FamilyId = familyId };
        var handler = new ResetFamilyAiChatQuotaCommandHandler(_context, _mockAuthorizationService.Object); // MODIFIED

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain($"Gia đình với ID '{familyId}' không có cấu hình giới hạn.");
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }
}
