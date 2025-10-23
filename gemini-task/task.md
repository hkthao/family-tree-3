Bạn là một chuyên gia .NET về kiểm thử phần mềm. Hãy giúp tôi tự động viết các Unit Test và Integration Test cho dự án ASP.NET Core theo mô hình DDD + CQRS.

🎯 Bối cảnh:

- Project sử dụng Entity Framework Core (DbContext trực tiếp, KHÔNG dùng repository pattern).
- Framework test: xUnit + FluentAssertions.
- Có thể sử dụng input test nhap du lieu thu cong, và AutoMoq để mock các dependency phụ (nhưng KHÔNG mock DbContext).
- Dữ liệu test nên dùng EF InMemoryDatabase (UseInMemoryDatabase(Guid.NewGuid().ToString())) để mô phỏng database thật.
- Mỗi test phải chạy độc lập, không dùng chung dữ liệu với test khác.

---

### 🧩 **Yêu cầu khi viết test**

1. **Phạm vi test**
   - Viết test cho từng CommandHandler, QueryHandler, hoặc Service trong thư mục `Application.UnitTests`.
   - Mỗi file test chỉ tập trung vào **các case quan trọng nhất**, ví dụ:
     - Entity không tồn tại → throw `NotFoundException`.
     - Dữ liệu hợp lệ → trả kết quả hoặc cập nhật chính xác.
     - Dữ liệu/quyền không hợp lệ → trả lỗi phù hợp.

2. **Cấu trúc test**
   - Mỗi test method phải có comment chi tiết:
     - 🎯 Mục tiêu của test.
     - ⚙️ Các bước (Arrange, Act, Assert).
     - 💡 Giải thích vì sao kết quả mong đợi là đúng.
   - Đặt tên test rõ ràng theo chuẩn:
     - `Handle_ShouldThrowNotFoundException_WhenMemberNotFound`
     - `Handle_ShouldUpdateMemberCorrectly_WhenValidRequest`

3. **Giới hạn phạm vi**
   - Chỉ viết 2–3 test case tiêu biểu cho mỗi handler.
   - Khi implement:
     - Viết từng test một.
     - Chạy test, khi tất cả pass → mới chuyển sang handler tiếp theo.

4. **Cách setup dữ liệu**
   - KHÔNG mock DbSet hoặc EF method như `FirstOrDefaultAsync`.
   - Tạo dữ liệu test bằng:
     - Thủ công (seed entity, gán Id/FK đúng), hoặc
     - AutoFixture (nhưng phải gán FK thủ công nếu có quan hệ).
   - Mỗi test khởi tạo một InMemoryDatabase mới để đảm bảo độc lập.

5. **Tái sử dụng setup**
   - Tạo `BaseTest` class để gom logic khởi tạo chung:
     - DbContext (InMemory)
     - AutoFixture config
     - AutoMoq setup (nếu có dependency)
   - Các test kế thừa `BaseTest` để tránh lặp code.

---

### 🚫 **Cảnh báo quan trọng**

- **KHÔNG được tự bịa hoặc suy đoán model, property, hoặc field.**
- Chỉ được dùng **các entity, DTO, và property có thật trong mã nguồn hiện có của dự án**.
- Nếu không chắc chắn về cấu trúc model → hãy hỏi lại hoặc tra cứu trong code trước khi viết test.
- Không thêm thuộc tính giả như `CreatedAt`, `UpdatedAt`, `IsDeleted`, v.v. nếu không có trong model thật.

---

### 📁 **Kết quả mong muốn**

- Mỗi file test nằm trong `Application.UnitTests/<Module>/<Feature>/<FeatureName>Tests.cs`
- Mỗi test:
  - Chạy độc lập.
  - Dễ hiểu cho junior developer.
  - Có comment rõ ràng (Arrange / Act / Assert / Explain).
  - Dùng FluentAssertions để assert.
  - Chạy async nếu cần (`await handler.Handle(...)`).

---

### ⚙️ **Mục tiêu cuối cùng**

- Giúp tôi — một developer làm việc một mình — có thể nhanh chóng tạo test hữu ích cho từng handler mà không tốn thời gian.
- Tập trung vào **tốc độ, tính chính xác và độ dễ hiểu**.
- Không cần độ bao phủ tuyệt đối, chỉ cần test các case chính, đáng tin cậy, có thể chạy tự động.
