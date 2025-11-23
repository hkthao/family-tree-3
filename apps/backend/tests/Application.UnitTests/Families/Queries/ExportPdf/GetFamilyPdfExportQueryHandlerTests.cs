using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Families.Queries.ExportPdf;
using backend.Application.UnitTests.Common;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Moq;
using System.Net.Http; // Added
using System.Threading; // Added
using Xunit;

namespace backend.Application.UnitTests.Families.Queries.ExportPdf;

public class GetFamilyPdfExportQueryHandlerTests : TestBase
{
    private readonly Mock<ICurrentUser> _mockCurrentUser;
    private readonly GetFamilyPdfExportQueryHandler _handler;
    private readonly string _dummyHtmlContent = "<html><body><h1>Test Family Tree</h1></body></html>";

    public GetFamilyPdfExportQueryHandlerTests()
    {
        _mockCurrentUser = new Mock<ICurrentUser>();
        _handler = new GetFamilyPdfExportQueryHandler(
            _context,
            _mockAuthorizationService.Object,
            _mockCurrentUser.Object,
            _mockHttpClient.Object // Pass the mocked HttpClient
        );

        // Setup mock for HttpClient PostAsync
        _mockHttpClient
            .Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new ByteArrayContent(new byte[] { 1, 2, 3, 4 }) // Dummy PDF bytes
            });
    }

    [Fact]
    public async Task Handle_ShouldReturnPdfFile_WhenFamilyExistsAndUserIsAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var creatorUserId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();

        _mockCurrentUser.Setup(x => x.UserId).Returns(currentUserId);
        _mockAuthorizationService.Setup(x => x.CanAccessFamily(familyId)).Returns(true);

        var family = Family.Create("Test Family", "TF1", "Description", "Address", "AvatarUrl", "Private", creatorUserId);
        family.Id = familyId;

        // Populate family with some members for the test
        var member1 = new Member("Doe", "John", "JOHNDOE", familyId, "Johnny", "Male", DateTime.Now.AddYears(-30), null, "Place1", null, null, null, null, "Occupation1", "Avatar1", "Bio1", 1, false);
        member1.SetId(Guid.NewGuid());
        var member2 = new Member("Doe", "Jane", "JANEDOE", familyId, "Janie", "Female", DateTime.Now.AddYears(-28), null, "Place2", null, null, null, null, "Occupation2", "Avatar2", "Bio2", 2, false);
        member2.SetId(Guid.NewGuid());

        // Add members and event to family via public methods or directly to the in-memory context's DbSet
        family.AddMember(member1);
        family.AddMember(member2);

        var event1 = new Event("Birthday", "BIRTHDAY", EventType.Birth, familyId, DateTime.Now.AddYears(-30));
        event1.Id = Guid.NewGuid();
        event1.AddEventMember(member1.Id);
        event1.AddEventMember(member2.Id);

        _context.Families.Add(family);
        _context.Members.Add(member1);
        _context.Members.Add(member2);
        _context.Events.Add(event1);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetFamilyPdfExportQuery(familyId, _dummyHtmlContent); // Pass dummy HTML content

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Content.Should().NotBeEmpty();
        result.Value!.FileName.Should().Contain($"FamilyTree_{family.Name.Replace(" ", "_")}.pdf");
        result.Value!.ContentType.Should().Be("application/pdf");
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenFamilyDoesNotExist()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _mockAuthorizationService.Setup(x => x.CanAccessFamily(familyId)).Returns(true);

        var query = new GetFamilyPdfExportQuery(familyId, _dummyHtmlContent); // Pass dummy HTML content

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be($"Family with ID {familyId} not found.");
        result.ErrorSource.Should().Be(ErrorSources.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnAccessDenied_WhenUserIsNotAuthorized()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var creatorUserId = Guid.NewGuid();
        var family = Family.Create("Test Family", "TF1", "Description", "Address", "AvatarUrl", "Private", creatorUserId);
        family.Id = familyId;

        _context.Families.Add(family);
        await _context.SaveChangesAsync(CancellationToken.None);

        _mockAuthorizationService.Setup(x => x.CanAccessFamily(familyId)).Returns(false);

        var query = new GetFamilyPdfExportQuery(familyId, _dummyHtmlContent); // Pass dummy HTML content

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.AccessDenied);
        result.ErrorSource.Should().Be(ErrorSources.Forbidden);
    }

    [Fact]
    public async Task Handle_ShouldIncludeMembersAndEventsInPdfContent()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var creatorUserId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();

        _mockCurrentUser.Setup(x => x.UserId).Returns(currentUserId);
        _mockAuthorizationService.Setup(x => x.CanAccessFamily(familyId)).Returns(true);

        var family = Family.Create("Test Family", "TF1", "Description", "Address", "AvatarUrl", "Private", creatorUserId);
        family.Id = familyId;

        var member1 = new Member("Doe", "John", "JOHNDOE", familyId, "Johnny", "Male", DateTime.Now.AddYears(-30), null, "Place1", null, null, null, null, "Occupation1", "Avatar1", "Bio1", 1, false);
        member1.SetId(Guid.NewGuid());
        var member2 = new Member("Doe", "Jane", "JANEDOE", familyId, "Janie", "Female", DateTime.Now.AddYears(-28), null, "Place2", null, null, null, null, "Occupation2", "Avatar2", "Bio2", 2, false);
        member2.SetId(Guid.NewGuid());

        // Add members and event to family via public methods
        family.AddMember(member1);
        family.AddMember(member2);

        var event1 = new Event("Birthday", "BIRTHDAY", EventType.Birth, familyId, DateTime.Now.AddYears(-30));
        event1.Id = Guid.NewGuid();
        event1.AddEventMember(member1.Id);
        event1.AddEventMember(member2.Id);

        _context.Families.Add(family);
        _context.Members.Add(member1);
        _context.Members.Add(member2);
        _context.Events.Add(event1);
        await _context.SaveChangesAsync(CancellationToken.None);

        var query = new GetFamilyPdfExportQuery(familyId, _dummyHtmlContent); // Pass dummy HTML content

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Content.Should().NotBeEmpty();

        // For a unit test, verifying non-empty content is sufficient.
        // Detailed content verification (e.g., checking for specific text within the PDF)
        // would require a PDF parsing library and is more appropriate for integration tests.
    }
}