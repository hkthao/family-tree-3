using AutoMapper;
using backend.Application.Common.Interfaces;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace backend.Application.UnitTests.Common;

/// <summary>
/// Lớp cơ sở cho các bài kiểm thử đơn vị.
/// Cung cấp môi trường cơ sở dữ liệu trong bộ nhớ, AutoFixture và AutoMoq để thiết lập dữ liệu và mock các dependency.
/// </summary>
public abstract class TestBase : IDisposable
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;
    protected readonly Mock<ICurrentUser> _mockUser;
    protected readonly Mock<IDateTime> _mockDateTime;
    protected readonly Mock<IDomainEventDispatcher> _mockDomainEventDispatcher;
    protected readonly Mock<IAuthorizationService> _mockAuthorizationService;
    protected readonly Mock<Microsoft.AspNetCore.Authorization.IAuthorizationService> _mockAspNetCoreAuthorizationService; // Added
    protected readonly Mock<HttpClient> _mockHttpClient; // Added for HttpClient
    protected readonly IMapper _mapper;
    protected readonly string _databaseName;

    protected TestBase()
    {
        // Thiết lập InMemoryDatabase cho mỗi test để đảm bảo tính độc lập
        _databaseName = Guid.NewGuid().ToString();
        _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;

        // Mock ICurrentUser and IDateTime and IDomainEventDispatcher
        _mockUser = new Mock<ICurrentUser>();
        _mockUser.Setup(x => x.UserId).Returns(Guid.NewGuid()); // Provide a default mocked UserId
        _mockDateTime = new Mock<IDateTime>();
        _mockDomainEventDispatcher = new Mock<IDomainEventDispatcher>();

        _context = new ApplicationDbContext(_dbContextOptions, _mockDomainEventDispatcher.Object, _mockUser.Object, _mockDateTime.Object);
        _context.Database.EnsureCreated(); // Đảm bảo database được tạo

        // Mock IAuthorizationService and HttpClient
        _mockAuthorizationService = new Mock<IAuthorizationService>();
        _mockAspNetCoreAuthorizationService = new Mock<Microsoft.AspNetCore.Authorization.IAuthorizationService>(); // Initialized
        _mockHttpClient = new Mock<HttpClient>(); // Initialized HttpClient

        // Cấu hình AutoMapper
        var mapperConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<Application.Common.Mappings.MappingProfile>(); // Explicitly add the profile
            // Add other profiles if needed
        });
        _mapper = mapperConfiguration.CreateMapper();
    }

    /// <summary>
    /// Giải phóng tài nguyên.
    /// </summary>
    public virtual void Dispose()
    {
        _context.Dispose();
    }
}
