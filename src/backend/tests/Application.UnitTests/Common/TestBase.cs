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
    protected readonly Mock<IMapper> _mapperMock;

    protected TestBase()
    {
        // Cấu hình AutoFixture với AutoMoq để tự động tạo đối tượng và mock interface
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        // Thiết lập InMemoryDatabase cho mỗi test để đảm bảo tính độc lập
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated(); // Đảm bảo database được tạo

        // Mock ICurrentUserService
        _mockUser = _fixture.Freeze<Mock<IUser>>();

        // Cấu hình AutoMapper (cần một cấu hình mapper thực tế nếu có)
        // Đối với các unit test, thường mock IMapper hoặc cung cấp một cấu hình tối thiểu.
        // Ở đây, chúng ta sẽ mock nó và thiết lập hành vi cụ thể khi cần.
        _mapperMock = _fixture.Freeze<Mock<IMapper>>();
    }

    /// <summary>
    /// Giải phóng tài nguyên.
    /// </summary>
    public void Dispose()
    {
        _context.Dispose();
    }
}