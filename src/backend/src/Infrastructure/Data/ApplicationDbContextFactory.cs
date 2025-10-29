using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.IO;
using System.Reflection;

namespace backend.Infrastructure.Data
{
    /// <summary>
    /// Factory để tạo ApplicationDbContext tại thời điểm thiết kế (design-time).
    /// Điều này cần thiết cho các công cụ EF Core như 'dotnet ef migrations add' hoặc 'dotnet ef database update'
    /// để có thể tạo một instance của DbContext mà không cần chạy toàn bộ ứng dụng.
    /// </summary>
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Sử dụng chuỗi kết nối cứng cho mục đích thiết kế (design-time)
            // Điều này giúp tránh các vấn đề phụ thuộc DI phức tạp khi chạy các lệnh EF Core.
            // Đảm bảo chuỗi kết nối này khớp với môi trường phát triển cục bộ của bạn.
            // Cập nhật mật khẩu từ 'password' thành 'root_password' dựa trên docker-compose.yml
            var connectionString = "Server=localhost;Port=3306;Database=family_tree_db;Uid=root;Pwd=root_password;";

            builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));

            return new ApplicationDbContext(builder.Options);
        }
    }
}
