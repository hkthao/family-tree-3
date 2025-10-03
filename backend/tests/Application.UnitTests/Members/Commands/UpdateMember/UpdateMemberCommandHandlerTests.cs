
using backend.Application.Common.Exceptions;
using backend.Application.Members.Commands.UpdateMember;
using backend.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Application.Common.Interfaces;

namespace backend.Application.UnitTests.Members.Commands.UpdateMember;

public class UpdateMemberCommandHandlerTests
{
    private readonly Mock<IMemberRepository> _mockMemberRepository;

    public UpdateMemberCommandHandlerTests()
    {
        _mockMemberRepository = new Mock<IMemberRepository>();
    }

    [Fact]
    public async Task Handle_GivenValidId_ShouldUpdateMember()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var member = new Member { Id = memberId, FirstName = "Test", LastName = "Member", FamilyId = Guid.NewGuid() };
        _mockMemberRepository.Setup(repo => repo.GetByIdAsync(memberId)).ReturnsAsync(member);
        _mockMemberRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Member>())).Returns(Task.CompletedTask);

        var command = new UpdateMemberCommand
        {
            Id = memberId,
            FirstName = "Updated",
            LastName = "Name",
            Gender = "Female"
        };
        var handler = new UpdateMemberCommandHandler(_mockMemberRepository.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mockMemberRepository.Verify(repo => repo.UpdateAsync(It.Is<Member>(m => m.Id == memberId && m.FirstName == command.FirstName && m.LastName == command.LastName)), Times.Once);
    }

    [Fact]
    public async Task Handle_GivenInvalidId_ShouldThrowNotFoundException()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        _mockMemberRepository.Setup(repo => repo.GetByIdAsync(invalidId)).ReturnsAsync((Member?)null);

        var command = new UpdateMemberCommand { Id = invalidId };
        var handler = new UpdateMemberCommandHandler(_mockMemberRepository.Object);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
