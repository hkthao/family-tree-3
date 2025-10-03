Bạn là senior backend engineer. Hãy viết bộ Infrastructure.IntegrationTests cho project Clean Architecture + DDD, backend .NET 8, sử dụng:

- EF Core (SQLite in-memory và InMemory provider cho test).
- Auth0 làm identity provider, backend validate JWT để authorize user.
- CQRS + Specification pattern cho query, có projection sang DTO.

Yêu cầu:

1. Cấu trúc test project:
   - Thư mục `Infrastructure.IntegrationTests/Database`
   - Thư mục `Infrastructure.IntegrationTests/Authentication`
   - Có `TestBase` để khởi tạo DbContext với SQLite InMemory, seed data.
   - Sử dụng xUnit.

2. Viết test case chi tiết cho EF Core:
   - Đảm bảo entity mapping chạy đúng (Member, Relationships).
   - Relationship Father/Mother/Children/Spouse lưu và query đúng.
   - Projection sang DTO hoạt động trong LINQ (test với `Select`).
   - Migration tạo được schema hợp lệ.
   - Constraint/validation (Required, MaxLength) có hiệu lực.
   - Query với Specification filter (ví dụ lọc theo Gender, DateOfBirth range).
   - Trường hợp thường fail: InMemory pass nhưng SQLite fail → viết test confirm (ví dụ `Contains`, `GroupBy`, `DateTime` functions).

3. Viết test case cho Authentication (Auth0):
   - Validate JWT hợp lệ: signature đúng, token chưa hết hạn.
   - Token expired → reject.
   - Token thiếu claim `sub` → reject.
   - Token có role `Admin` → authorize thành công.
   - Token với scope không đủ → bị từ chối.
   - Mock JWT để test offline.
   - (Optional) Integration test với sandbox Auth0 (chạy thật) → verify config (audience, issuer).

4. Ngoài ra, thêm test cho External Service (có thể placeholder):
   - Fake EmailSender → gửi email được log lại.
   - Fake FileStorage → upload/download file thành công.
   - RedisCache → set/get key-value.

5. Mỗi test cần có:
   - Arrange → seed data hoặc tạo token.
   - Act → chạy handler hoặc query.
   - Assert → verify kết quả đúng như mong đợi.

6. Tạo code test mẫu minh họa:
   - `DbContextTests` với seed 2 Member (Father + Child) và query lại quan hệ.
   - `JwtValidationTests` với token hợp lệ/không hợp lệ.
   - `ProjectionTests` dùng LINQ Select sang DTO.

Hãy viết toàn bộ test project mẫu với xUnit và FluentAssertions.
