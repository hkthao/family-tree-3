using System.Reflection;
using AutoMapper;
using backend.Application.Common.Interfaces;
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
    protected readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;
    protected readonly Mock<ICurrentUser> _mockUser;
    protected readonly Mock<IDateTime> _mockDateTime;
    protected readonly Mock<IDomainEventDispatcher> _mockDomainEventDispatcher;
    protected readonly Mock<IAuthorizationService> _mockAuthorizationService;
    protected readonly Mock<Microsoft.AspNetCore.Authorization.IAuthorizationService> _mockAspNetCoreAuthorizationService; // Added
    protected readonly Mock<HttpClient> _mockHttpClient; // Added for HttpClient
    protected readonly IMapper _mapper;
    protected readonly string _databaseName;
    protected readonly Guid TestUserId; // NEW

    protected TestBase()
    {
        // Thiết lập InMemoryDatabase cho mỗi test để đảm bảo tính độc lập
        _databaseName = Guid.NewGuid().ToString();
        _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;

        // Mock ICurrentUser and IDateTime and IDomainEventDispatcher
        _mockUser = new Mock<ICurrentUser>();
        TestUserId = Guid.NewGuid(); // Initialize TestUserId
        _mockUser.Setup(x => x.UserId).Returns(TestUserId); // Use the consistent TestUserId
        _mockUser.Setup(x => x.IsAuthenticated).Returns(true); // Default to authenticated
        _mockDateTime = new Mock<IDateTime>();
        _mockDomainEventDispatcher = new Mock<IDomainEventDispatcher>();

        _context = new ApplicationDbContext(_dbContextOptions, _mockDomainEventDispatcher.Object, _mockUser.Object, _mockDateTime.Object);
        _context.Database.EnsureCreated(); // Đảm bảo database được tạo

        // Mock IAuthorizationService and HttpClient
        _mockAuthorizationService = new Mock<IAuthorizationService>();
        _mockAuthorizationService.Setup(x => x.CanAccessFamily(It.IsAny<Guid>())).Returns(true); // Default to authorized
        _mockAspNetCoreAuthorizationService = new Mock<Microsoft.AspNetCore.Authorization.IAuthorizationService>(); // Initialized
        _mockHttpClient = new Mock<HttpClient>(); // Initialized HttpClient

        // Cấu hình AutoMapper
        var mapperConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<backend.Application.Common.Mappings.MappingProfile>(); // Explicitly add the profile

            cfg.CreateMap<backend.Application.MemberFaces.Common.BoundingBoxDto, backend.Domain.ValueObjects.BoundingBox>();
            // Add other profiles if needed
        });
        _mapper = mapperConfiguration.CreateMapper();
    }

    /// <summary>
    /// Phương thức tiện ích để lấy một instance của ApplicationDbContext mới cho mỗi bài kiểm thử.
    /// </summary>
    protected ApplicationDbContext GetApplicationDbContext()
    {
        return new ApplicationDbContext(_dbContextOptions, _mockDomainEventDispatcher.Object, _mockUser.Object, _mockDateTime.Object);
    }
    /// <summary>
    /// Giải phóng tài nguyên.
    /// </summary>
    public virtual void Dispose()
    {
        _context.Dispose();
    }

    /// <summary>
    /// Helper method to create a Member instance with common defaults.
    /// </summary>
    protected Member CreateMember(string lastName, string firstName, string code, Guid familyId, bool isDeceased = false, Guid? id = null)
    {
        var member = new Member(lastName, firstName, code, familyId, null, null, null, null, null, null, null, null, null, null, null, null, null, null, isDeceased);
        if (id.HasValue)
        {
            member.SetId(id.Value);
        }
        return member;
    }

    protected Member CreateMember(string lastName, string firstName, string code, Guid familyId, bool isDeceased, Guid? id, string? gender, DateTime? dateOfBirth, DateTime? dateOfDeath, bool isDeleted = false)
    {
        var member = new Member(lastName, firstName, code, familyId, null, gender, dateOfBirth, dateOfDeath, null, null, null, null, null, null, null, null, null, null, isDeceased);
        if (id.HasValue)
        {
            member.SetId(id.Value);
        }
        member.IsDeleted = isDeleted; // Set IsDeleted explicitly
        return member;
    }

    /// <summary>
    /// Sử dụng Reflection để set giá trị cho một private property.
    /// </summary>
    /// <typeparam name="TEntity">Kiểu của entity chứa property.</typeparam>
    /// <typeparam name="TProperty">Kiểu của property.</typeparam>
    /// <param name="entity">Instance của entity.</param>
    /// <param name="propertyName">Tên của private property.</param>
    /// <param name="value">Giá trị muốn set.</param>
    protected static void SetPrivateProperty<TEntity, TProperty>(TEntity entity, string propertyName, TProperty value)
        where TEntity : class
    {
        typeof(TEntity)
            .GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(entity, value);
    }
}

