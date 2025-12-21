using backend.Application.AI.Chat;
using backend.Application.AI.DTOs;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.UnitTests.Common; // Corrected
using backend.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.AI.Commands.Chat;

public class ChatWithAssistantCommandHandlerTests : TestBase
{
    private readonly Mock<IAiChatService> _aiChatServiceMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ILogger<ChatWithAssistantCommandHandler>> _loggerMock;
    private readonly ChatWithAssistantCommandHandler _handler;

    public ChatWithAssistantCommandHandlerTests()
    {
        _aiChatServiceMock = new Mock<IAiChatService>();
        _currentUserMock = new Mock<ICurrentUser>();
        _loggerMock = new Mock<ILogger<ChatWithAssistantCommandHandler>>();
        _handler = new ChatWithAssistantCommandHandler(
            _aiChatServiceMock.Object,
            _context,
            _currentUserMock.Object,
            _loggerMock.Object);

        // Thiết lập người dùng hiện tại mặc định
        _currentUserMock.Setup(u => u.UserId).Returns(Guid.NewGuid());
        // Thiết lập người dùng hiện tại là đã xác thực
        _currentUserMock.Setup(u => u.IsAuthenticated).Returns(true);
    }

    // Test khi gia đình không tồn tại
    [Fact]
    public async Task Handle_GivenNonExistingFamily_ShouldReturnNotFound()
    {
        // Arrange
        var command = new ChatWithAssistantCommand
        {
            FamilyId = Guid.NewGuid(), // ID gia đình không tồn tại
            SessionId = "session1",
            ChatInput = "Hello AI"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
        result.Error.Should().Contain(string.Format(ErrorMessages.FamilyNotFound, command.FamilyId));
        _aiChatServiceMock.Verify(x => x.CallChatWebhookAsync(It.IsAny<ChatRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // Test khi người dùng không được xác thực
    [Fact]
    public async Task Handle_GivenUnauthenticatedUser_ShouldReturnUnauthorized()
    {
        // Arrange
        _currentUserMock.Setup(u => u.IsAuthenticated).Returns(false); // Người dùng không được xác thực

        var command = new ChatWithAssistantCommand
        {
            FamilyId = Guid.NewGuid(),
            SessionId = "session1",
            ChatInput = "Hello AI"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be(ErrorSources.Authentication);
        result.Error.Should().Be(ErrorMessages.Unauthorized);
        _aiChatServiceMock.Verify(x => x.CallChatWebhookAsync(It.IsAny<ChatRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // Test khi hạn mức AI chat chưa bị vượt quá
    [Fact]
    public async Task Handle_GivenQuotaNotExceeded_ShouldIncrementUsageAndCallAiService()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = Family.Create("Test Family", "TF001", null, null, "Public", Guid.NewGuid());
        family.Id = familyId;
        family.FamilyLimitConfiguration!.Update(50, 1024, 5); // Giới hạn 5 lượt
        family.FamilyLimitConfiguration.IncrementAiChatUsage(); // Sử dụng 1 lượt
        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new ChatWithAssistantCommand
        {
            FamilyId = familyId,
            SessionId = "session1",
            ChatInput = "Hello AI"
        };

        _aiChatServiceMock.Setup(x => x.CallChatWebhookAsync(It.IsAny<ChatRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ChatResponse>.Success(new ChatResponse { Output = "AI Response" }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Output.Should().Be("AI Response");

        // Kiểm tra xem hạn mức đã được tăng
        var updatedFamily = await _context.Families
            .Include(f => f.FamilyLimitConfiguration)
            .FirstOrDefaultAsync(f => f.Id == familyId);
        updatedFamily!.FamilyLimitConfiguration!.AiChatMonthlyUsage.Should().Be(2); // Đã tăng từ 1 lên 2

        _aiChatServiceMock.Verify(x => x.CallChatWebhookAsync(It.IsAny<ChatRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // Test khi hạn mức AI chat bị vượt quá
    [Fact]
    public async Task Handle_GivenQuotaExceeded_ShouldReturnQuotaExceededError()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = Family.Create("Test Family", "TF001", null, null, "Public", Guid.NewGuid());
        family.Id = familyId;
        family.FamilyLimitConfiguration!.Update(50, 1024, 1); // Giới hạn 1 lượt
        family.FamilyLimitConfiguration.IncrementAiChatUsage(); // Sử dụng 1 lượt (đã đạt giới hạn)
        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new ChatWithAssistantCommand
        {
            FamilyId = familyId,
            SessionId = "session1",
            ChatInput = "Hello AI"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorSource.Should().Be(ErrorSources.QuotaExceeded);
        result.Error.Should().Be(ErrorMessages.AiChatQuotaExceeded);
        _aiChatServiceMock.Verify(x => x.CallChatWebhookAsync(It.IsAny<ChatRequest>(), It.IsAny<CancellationToken>()), Times.Never);

        // Kiểm tra xem hạn mức sử dụng không bị tăng thêm
        var updatedFamily = await _context.Families
            .Include(f => f.FamilyLimitConfiguration)
            .FirstOrDefaultAsync(f => f.Id == familyId);
        updatedFamily!.FamilyLimitConfiguration!.AiChatMonthlyUsage.Should().Be(1);
    }

    // Test khi FamilyLimitConfiguration ban đầu là null (trường hợp ngoại lệ)
    [Fact]
    public async Task Handle_GivenNullFamilyLimitConfiguration_ShouldInitializeAndProceed()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = Family.Create("Test Family Null Config", "TF003", null, null, "Public", Guid.NewGuid());
        family.Id = familyId;
        family.FamilyLimitConfiguration = null; // Cố tình đặt là null
        await _context.Families.AddAsync(family);
        await _context.SaveChangesAsync(CancellationToken.None);

        var command = new ChatWithAssistantCommand
        {
            FamilyId = familyId,
            SessionId = "session1",
            ChatInput = "Hello AI"
        };

        _aiChatServiceMock.Setup(x => x.CallChatWebhookAsync(It.IsAny<ChatRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ChatResponse>.Success(new ChatResponse { Output = "AI Response Initialized" }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Output.Should().Be("AI Response Initialized");

        var updatedFamily = await _context.Families
            .Include(f => f.FamilyLimitConfiguration)
            .FirstOrDefaultAsync(f => f.Id == familyId);
        updatedFamily.Should().NotBeNull();
        updatedFamily!.FamilyLimitConfiguration.Should().NotBeNull();
        updatedFamily.FamilyLimitConfiguration!.AiChatMonthlyUsage.Should().Be(1); // Đã được khởi tạo và tăng lên 1

        _aiChatServiceMock.Verify(x => x.CallChatWebhookAsync(It.IsAny<ChatRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
