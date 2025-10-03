using backend.Application.Members.Commands.CreateMember;
using FluentAssertions;
using Moq;
using Xunit;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.UnitTests.Members.Commands.CreateMember;

public class CreateMemberCommandHandlerTests
{
    private readonly Mock<IMemberRepository> _mockMemberRepository;

    public CreateMemberCommandHandlerTests()
    {
        _mockMemberRepository = new Mock<IMemberRepository>();
    }

    [Fact]
    public async Task Handle_ShouldCreateMemberAndReturnId()
    {
        // Arrange
        var command = new CreateMemberCommand
        {
            FirstName = "Test",
            LastName = "Member",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Male",
            FamilyId = Guid.NewGuid()
        };

        _mockMemberRepository.Setup(repo => repo.AddAsync(It.IsAny<Member>()))
            .ReturnsAsync((Member member) =>
            {
                member.Id = Guid.NewGuid(); // Simulate ID generation
                return member;
            });

        var handler = new CreateMemberCommandHandler(_mockMemberRepository.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _mockMemberRepository.Verify(repo => repo.AddAsync(It.Is<Member>(m => m.FirstName == command.FirstName && m.LastName == command.LastName)), Times.Once);
    }
}
