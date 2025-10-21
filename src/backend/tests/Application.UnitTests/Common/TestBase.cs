using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using backend.Application.Common.Interfaces;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;

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

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated(); // Đảm bảo database được tạo

        // Mock ICurrentUserService
        _mockUser = _fixture.Freeze<Mock<IUser>>();

        // Cấu hình AutoMapper
        var mapperConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
            cfg.AddProfile<UserPreferenceMappingProfile>();
            // Add other profiles if needed
        });
        _mapper = mapperConfiguration.CreateMapper();
    }

    /// <summary>
    /// Giải phóng tài nguyên.
    /// </summary>
    public void Dispose()
    {
        _context.Dispose();
    }

    /// <summary>
    /// Tạo một instance ApplicationDbContext mới với cùng cấu hình InMemoryDatabase.
    /// Hữu ích cho việc kiểm tra các thay đổi đã được lưu vào DB.
    /// </summary>
    protected ApplicationDbContext CreateNewContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName) // Sử dụng cùng tên DB
            .Options;
        var newContext = new ApplicationDbContext(options);
        newContext.Database.EnsureCreated();
        return newContext;
    }
}