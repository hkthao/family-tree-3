using backend.Domain.Entities; // Add this using directive
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.IntegrationTests;

/// <summary>
/// Lớp cơ sở trừu tượng cho các bài kiểm thử tích hợp.
/// Cung cấp môi trường cơ sở dữ liệu trong bộ nhớ và các phương thức trợ giúp chung.
/// </summary>
public abstract class TestBase : IDisposable
{
    /// <summary>
    /// Ngữ cảnh cơ sở dữ liệu của ứng dụng.
    /// </summary>
    protected ApplicationDbContext Context { get; }
    /// <summary>
    /// Gia đình mặc định được tạo cho các bài kiểm thử.
    /// </summary>
    protected Family DefaultFamily { get; } // Add DefaultFamily property

    /// <summary>
    /// Khởi tạo một phiên bản mới của lớp TestBase.
    /// Thiết lập cơ sở dữ liệu SQLite trong bộ nhớ và seed dữ liệu ban đầu.
    /// </summary>
    protected TestBase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite($"DataSource=file:{Guid.NewGuid()}.db?mode=memory&cache=shared")
            .Options;

        Context = new ApplicationDbContext(options);
        Context.Database.OpenConnection();
        Context.Database.EnsureCreated();

        // Seed default family
        DefaultFamily = new Family { Name = "Default Family", Code = "DEFAULT" };
        Context.Families.Add(DefaultFamily);
        Context.SaveChanges(); // Use SaveChanges() for synchronous seeding

        // Seed data here if needed
        // Example: SeedData(Context);
    }

    /// <summary>
    /// Giải phóng các tài nguyên được sử dụng bởi lớp TestBase.
    /// Đóng kết nối cơ sở dữ liệu và giải phóng ngữ cảnh.
    /// </summary>
    public void Dispose()
    {
        Context.Database.CloseConnection();
        Context.Dispose();
    }

    /// <summary>
    /// Phương thức tùy chọn để seed dữ liệu.
    /// Các lớp dẫn xuất có thể ghi đè phương thức này để cung cấp logic seed dữ liệu cụ thể.
    /// </summary>
    /// <param name="context">Ngữ cảnh cơ sở dữ liệu.</param>
    protected virtual void SeedData(ApplicationDbContext context)
    {
        // Implement seeding logic in derived classes or here
    }

    /// <summary>
    /// Phương thức trợ giúp để tạo một thành viên với gia đình mặc định.
    /// </summary>
    /// <param name="firstName">Tên của thành viên.</param>
    /// <param name="lastName">Họ của thành viên.</param>
    /// <param name="gender">Giới tính của thành viên.</param>
    /// <param name="dateOfBirth">Ngày sinh của thành viên.</param>
    /// <returns>Một đối tượng Member mới.</returns>
    protected Member CreateMember(string firstName, string lastName, string gender, DateTime dateOfBirth)
    {
        return new Member
        {
            FirstName = firstName,
            LastName = lastName,
            Gender = gender,
            DateOfBirth = dateOfBirth,
            FamilyId = DefaultFamily.Id,
            Code = Guid.NewGuid().ToString() // Assign a unique code
        };
    }
}
