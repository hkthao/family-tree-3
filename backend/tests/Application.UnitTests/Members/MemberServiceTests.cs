using Xunit;
using Moq;
using backend.Application.Common.Interfaces;
using backend.Application.Members.Commands.CreateMember;
using System.Threading;
using System.Threading.Tasks;
using backend.Domain.Entities;
using FluentAssertions;
using backend.Application.Common.Exceptions;
using FluentValidation;
using MongoDB.Driver;

namespace backend.tests.Application.UnitTests.Members;

public class MemberServiceTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;

    public MemberServiceTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _contextMock.Setup(x => x.Members).Returns(new Mock<IMongoCollection<Member>>().Object);
    }

    [Fact]
    public async Task CreateMember_ShouldCreateMember_WhenRequestIsValid()
    {
        // Arrange
        var command = new CreateMemberCommand { FullName = "Test Member", FamilyId = "60d5ec49e04a4a5c8c8b4567" };
        var handler = new CreateMemberCommandHandler(_contextMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        _contextMock.Verify(x => x.Members.InsertOneAsync(It.IsAny<Member>(), null, CancellationToken.None), Times.Once);
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateMember_ShouldThrowValidationException_WhenNameIsMissing()
    {
        // Arrange
        var command = new CreateMemberCommand { FamilyId = "60d5ec49e04a4a5c8c8b4567" };
        var validator = new CreateMemberCommandValidator();

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FullName");
    }
}
