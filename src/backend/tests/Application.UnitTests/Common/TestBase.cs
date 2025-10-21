using AutoFixture;
using AutoFixture.AutoMoq;
using backend.Application.Common.Interfaces;
using backend.Application.Identity.UserProfiles.Queries;
using backend.Domain.Entities;
using backend.Infrastructure.Data;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore.Metadata;

namespace backend.Application.UnitTests.Common;

public abstract class TestBase : IDisposable
{
    protected readonly IFixture _fixture;
    protected readonly ApplicationDbContext _context;
    protected readonly Mock<IAuthorizationService> _mockAuthorizationService;
    protected readonly Mock<IMediator> _mockMediator;
    protected readonly Mock<IFamilyTreeService> _mockFamilyTreeService;

    protected TestBase()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        // Cấu hình AutoFixture để xử lý các tham chiếu vòng lặp trong biểu đồ đối tượng.
        // Điều này ngăn chặn lỗi 'circular reference' khi tạo các đối tượng phức tạp có quan hệ qua lại.
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        // Setup In-Memory DbContext
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated(); // Ensure the database is created for each test

        // Mock services
        _mockAuthorizationService = _fixture.Freeze<Mock<IAuthorizationService>>();
        _mockMediator = _fixture.Freeze<Mock<IMediator>>();
        _mockFamilyTreeService = _fixture.Freeze<Mock<IFamilyTreeService>>();

        // Default mock for GetCurrentUserProfileAsync to return a valid UserProfile
        // This can be overridden in specific tests if needed
        _mockAuthorizationService.Setup(x => x.GetCurrentUserProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_fixture.Create<UserProfile>());

        // Default mock for IsAdmin to return true for most tests, can be overridden
        _mockAuthorizationService.Setup(x => x.IsAdmin()).Returns(true);

        // Default mock for CanManageFamily to return true for most tests, can be overridden
        _mockAuthorizationService.Setup(x => x.CanManageFamily(It.IsAny<Guid>(), It.IsAny<UserProfile>()))
            .Returns(true);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted(); // Clean up the database after each test
        _context.Dispose();
    }
}
