
using backend.Application.Common.Exceptions;
using backend.Application.Members.Commands.DeleteMember;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Application.Common.Interfaces;

namespace backend.Application.UnitTests.Members.Commands.DeleteMember;

public class DeleteMemberCommandHandlerTests
{
    private readonly Mock<IMemberRepository> _mockMemberRepository;

    public DeleteMemberCommandHandlerTests()
    {
        _mockMemberRepository = new Mock<IMemberRepository>();
    }

    [Fact]
    public async Task Handle_GivenValidId_ShouldDeleteMember()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var member = new Member { Id = memberId, FirstName = "Test", LastName = "Member", FamilyId = Guid.NewGuid() };
        _mockMemberRepository.Setup(repo => repo.GetByIdAsync(memberId)).ReturnsAsync(member);
        _mockMemberRepository.Setup(repo => repo.DeleteAsync(memberId)).Returns(Task.CompletedTask);

        var command = new DeleteMemberCommand(memberId);
        var handler = new DeleteMemberCommandHandler(_mockMemberRepository.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mockMemberRepository.Verify(repo => repo.DeleteAsync(memberId), Times.Once);
    }

    [Fact]
    public async Task Handle_GivenInvalidId_ShouldThrowNotFoundException()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        _mockMemberRepository.Setup(repo => repo.GetByIdAsync(invalidId)).ReturnsAsync((Member?)null);

        var command = new DeleteMemberCommand(invalidId);
        var handler = new DeleteMemberCommandHandler(_mockMemberRepository.Object);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
