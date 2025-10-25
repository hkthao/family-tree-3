
### **Implement Integration Tests cho ASP.NET Core DDD với SQLite In-Memory**

> ⚠️ **Scope:**
>
> * Integration Test kiểm tra luồng thực giữa `API (Web)` → `Application` → `Infrastructure` → `Database`.
> * Dùng **SQLite In-Memory Database**, **không dùng SQL Server hoặc DB thật**, **không mock repository hoặc DbContext**.

---

## 🧩 Cấu trúc chuẩn Integration Test Project

```
tests/
└── Infrastructure.IntegrationTests/
    ├── Common/
    │   ├── IntegrationTestBase.cs        ← setup WebApplicationFactory + SQLite InMemory
    │   ├── TestDatabaseFixture.cs        ← quản lý database lifecycle
    │   └── HttpClientExtensions.cs       ← helper gọi API
    │
    ├── Controllers/
    │   ├── FamilyControllerTests.cs
    │   ├── MemberControllerTests.cs
    │   └── AuthControllerTests.cs
    │
    ├── Services/
    │   ├── FileStorageIntegrationTests.cs
    │   ├── ChatProviderIntegrationTests.cs
    │   └── VectorStoreIntegrationTests.cs
    │
    ├── Infrastructure/
    │   ├── ConfigurationProviderTests.cs
    │   └── DateTimeServiceTests.cs
    │
    └── IntegrationTests.csproj
```

---

## ⚙️ Các yêu cầu Gemini cần setup chính xác

### 1️⃣ WebApplicationFactory

* Dùng `WebApplicationFactory<Program>` để **khởi chạy API thật** (Startup pipeline).
* Tạo `HttpClient` để gọi endpoint thật (`/api/...`), không mock controller.

### 2️⃣ Dùng SQLite In-Memory Database

* Thay bằng `UseSqlite("DataSource=:memory:")` + giữ connection mở suốt vòng đời test.
* Tạo fixture:

  ```csharp
  public class TestDatabaseFixture : IAsyncLifetime
  {
      public SqliteConnection Connection { get; private set; } = default!;
      public AppDbContext DbContext { get; private set; } = default!;

      public async Task InitializeAsync()
      {
          Connection = new SqliteConnection("DataSource=:memory:");
          await Connection.OpenAsync();

          var options = new DbContextOptionsBuilder<AppDbContext>()
              .UseSqlite(Connection)
              .Options;

          DbContext = new AppDbContext(options);
          await DbContext.Database.EnsureCreatedAsync();
      }

      public async Task DisposeAsync()
      {
          await Connection.CloseAsync();
      }
  }
  ```

### 3️⃣ Dependency Injection

* Giữ nguyên toàn bộ module `Application` và `Infrastructure`.
* Gắn SQLite connection vào DI trong `IntegrationTestBase`:

  ```csharp
  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
      builder.ConfigureServices(services =>
      {
          services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
          services.AddDbContext<AppDbContext>(options =>
              options.UseSqlite(_fixture.Connection));
      });
  }
  ```

### 4️⃣ Viết test theo luồng thực

* Test API thực sự (CRUD, Auth, Upload...).
* Gọi `POST → GET → DELETE`, kiểm tra response và DB state.

  ```csharp
  /// <summary>✅ Tạo mới Family và xác thực tồn tại trong DB</summary>
  /// <remarks>
  /// ⚙️ B1: Gửi POST /api/family
  /// ⚙️ B2: Gửi GET /api/family/{id}
  /// </remarks>
  [Fact]
  public async Task CreateFamily_ShouldPersistInSQLiteMemory()
  {
      // Arrange
      var request = new CreateFamilyRequest("Huynh");

      // Act
      var response = await _client.PostAsJsonAsync("/api/family", request);
      var family = await _fixture.DbContext.Families.FirstOrDefaultAsync(f => f.Name == "Huynh");

      // Assert
      response.StatusCode.Should().Be(HttpStatusCode.Created);
      family.Should().NotBeNull();
  }
  ```

### 5️⃣ Bình luận bằng tiếng Việt

* Mỗi test có cấu trúc:

  ```csharp
  /// <summary>✅ Mục tiêu test...</summary>
  /// <remarks>⚙️ Các bước thực hiện...</remarks>
  /// <explanation>💡 Giải thích logic hoặc mục tiêu business...</explanation>
  ```
