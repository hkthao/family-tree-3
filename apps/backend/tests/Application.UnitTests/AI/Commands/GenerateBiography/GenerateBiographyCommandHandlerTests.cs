using backend.Application.AI.Commands;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.Application.UnitTests.AI.Commands;

public class GenerateBiographyCommandHandlerTests : TestBase
{
    private readonly Mock<IN8nService> _n8nServiceMock;
    private readonly GenerateBiographyCommandHandler _handler;

    public GenerateBiographyCommandHandlerTests() : base()
    {
        _n8nServiceMock = new Mock<IN8nService>();
        _handler = new GenerateBiographyCommandHandler(_context, _n8nServiceMock.Object);
    }




    [Fact]
    public async Task Handle_ShouldReturnBiography_WhenMemberExistsAndN8nServiceSucceeds()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var fatherId = Guid.NewGuid();
        var motherId = Guid.NewGuid();
        var spouseId = Guid.NewGuid();

        var family = new Family { Id = familyId, Name = "Test Family", Code = "TF1" };
        var father = new Member("Father", "Test", "FT", familyId, null, "Male", null, null, null, null, null, null, null, null, null, null, null, true) { Id = fatherId };
        var mother = new Member("Mother", "Test", "MT", familyId, null, "Female", null, null, null, null, null, null, null, null, null, null, null, true) { Id = motherId };
        var spouse = new Member("Spouse", "Test", "ST", familyId, null, "Female", null, null, null, null, null, null, null, null, null, null, null, true) { Id = spouseId };

        var member = new Member(memberId, "Child", "Test", "CT", familyId, family, false);
        member.Update("Child", "Test", "CT", null, "Male", new DateTime(1990, 1, 1), null, null, null, null, null, null, null, null, null, null, false);

        // Manually create relationships using the correct constructor
        var fatherRelationship = new Relationship(familyId, fatherId, memberId, RelationshipType.Father);
        var motherRelationship = new Relationship(familyId, motherId, memberId, RelationshipType.Mother);
        var spouseRelationship = new Relationship(familyId, memberId, spouseId, RelationshipType.Husband);

        // Add relationships to the members
        member.TargetRelationships.Add(fatherRelationship);
        member.TargetRelationships.Add(motherRelationship);
        member.SourceRelationships.Add(spouseRelationship);
        father.SourceRelationships.Add(fatherRelationship);
        mother.SourceRelationships.Add(motherRelationship);
        spouse.TargetRelationships.Add(spouseRelationship);

        // Add member to the in-memory database to be found by the real _context
        _context.Families.Add(family);
        _context.Members.Add(father);
        _context.Members.Add(mother);
        _context.Members.Add(spouse);
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        var command = new GenerateBiographyCommand
        {
            MemberId = memberId,
            Style = BiographyStyle.Formal,
            UserPrompt = "Focus on his early life.",
            GeneratedFromDB = false
        };

        _n8nServiceMock.Setup(x => x.CallChatWebhookAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success("Generated biography content."));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Generated biography content.");
        _n8nServiceMock.Verify(x => x.CallChatWebhookAsync(
            memberId.ToString(), It.Is<string>(msg =>
                msg.Contains("Generate a biography for the following family member.") &&
                msg.Contains("Style: Formal") &&
                msg.Contains("Additional instructions: Focus on his early life.") &&
                msg.Contains("Father: Father Test") &&
                msg.Contains("Mother: Mother Test") &&
                msg.Contains("Spouses:") && msg.Contains("Spouse Test")
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMemberNotFound()
    {
        // Arrange
        var nonExistentMemberId = Guid.NewGuid();
        var command = new GenerateBiographyCommand
        {
            MemberId = nonExistentMemberId,
            Style = BiographyStyle.Formal
        };

        // No member added to context, so it won't be found

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be($"Member with ID {nonExistentMemberId} not found.");
        _n8nServiceMock.Verify(x => x.CallChatWebhookAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenN8nServiceCallFails()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var member = new Member(memberId, "Test", "Member", "TM1", Guid.NewGuid(), new Family { Id = Guid.NewGuid(), Name = "Test Family", Code = "TF1" }, false);
        member.Update("Test", "Member", "TM1", null, null, null, null, null, null, null, null, null, null, null, null, null, false);
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        var command = new GenerateBiographyCommand
        {
            MemberId = memberId,
            Style = BiographyStyle.Formal
        };

        _n8nServiceMock.Setup(x => x.CallChatWebhookAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Failure("N8n error from AI", "N8nService"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("N8n error from AI");
    }

    [Fact]
    public async Task Handle_ShouldIncludeExistingBiography_WhenGeneratedFromDBIsTrueAndBiographyExists()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var existingBiography = "This is an existing biography.";
        var member = new Member(memberId, "Test", "Member", "TM1", Guid.NewGuid(), new Family { Id = Guid.NewGuid(), Name = "Test Family", Code = "TF1" }, false);
        member.Update("Test", "Member", "TM1", null, null, null, null, null, null, null, null, null, null, null, existingBiography, null, false);
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        var command = new GenerateBiographyCommand
        {
            MemberId = memberId,
            Style = BiographyStyle.Emotional,
            GeneratedFromDB = true
        };

        _n8nServiceMock.Setup(x => x.CallChatWebhookAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success("New biography based on existing."));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _n8nServiceMock.Verify(x => x.CallChatWebhookAsync(
            memberId.ToString(), It.Is<string>(msg => msg.Contains($"- Existing Biography: {existingBiography}")),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotIncludeExistingBiography_WhenGeneratedFromDBIsFalse()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var existingBiography = "This is an existing biography.";
        var member = new Member(memberId, "Test", "Member", "TM1", Guid.NewGuid(), new Family { Id = Guid.NewGuid(), Name = "Test Family", Code = "TF1" }, false);
        member.Update("Test", "Member", "TM1", null, null, null, null, null, null, null, null, null, null, null, existingBiography, null, false);
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        var command = new GenerateBiographyCommand
        {
            MemberId = memberId,
            Style = BiographyStyle.Emotional,
            GeneratedFromDB = false
        };

        _n8nServiceMock.Setup(x => x.CallChatWebhookAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success("New biography."));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _n8nServiceMock.Verify(x => x.CallChatWebhookAsync(
            memberId.ToString(), It.Is<string>(msg => !msg.Contains($"- Existing Biography: {existingBiography}")),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}
