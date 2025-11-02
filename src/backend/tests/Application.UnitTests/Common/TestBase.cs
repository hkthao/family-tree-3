using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using backend.Application.Common.Interfaces;
using backend.Application.Identity.UserProfiles.Queries;
using backend.Domain.Entities;
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
    protected readonly IFixture _fixture;
    protected readonly Mock<IUser> _mockUser;
    protected readonly Mock<IDateTime> _mockDateTime;
    protected readonly Mock<IAuthorizationService> _mockAuthorizationService;
    protected readonly IMapper _mapper;
    protected readonly string _databaseName;

    protected TestBase()
    {
        // Cấu hình AutoFixture với AutoMoq để tự động tạo đối tượng và mock interface
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        // Thiết lập InMemoryDatabase cho mỗi test để đảm bảo tính độc lập
        _databaseName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;

        // Mock ICurrentUserService and IDateTime
        _mockUser = _fixture.Freeze<Mock<IUser>>();
        _mockDateTime = _fixture.Freeze<Mock<IDateTime>>();

        _context = new ApplicationDbContext(options, _mockUser.Object, _mockDateTime.Object);
        _context.Database.EnsureCreated(); // Đảm bảo database được tạo

        // Mock IAuthorizationService
        _mockAuthorizationService = _fixture.Freeze<Mock<IAuthorizationService>>();

        // Cấu hình AutoMapper
        var mapperConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(backend.Application.Common.Mappings.MappingProfile).Assembly);
            cfg.CreateMap<UserProfile, UserProfileDto>();
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
