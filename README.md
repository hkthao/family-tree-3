# Dự án Cây Gia Phả (Family Tree Project)

Hệ thống quản lý gia phả chuyên nghiệp cho dòng họ và gia đình, cho phép xây dựng, quản lý và trực quan hóa cây gia phả một cách dễ dàng.

---

## ✨ Tính Năng Chính (Features)

- **Quản lý Dòng họ/Gia đình:** Tạo và quản lý thông tin nhiều dòng họ hoặc gia đình khác nhau.
- **Quản lý Thành viên:** Thêm, sửa, xóa thông tin chi tiết của từng thành viên (họ tên, ngày sinh/mất, thế hệ,...).
- **Quản lý Quan hệ:** Thiết lập các mối quan hệ (cha/mẹ, vợ/chồng, con) giữa các thành viên.
- **Trực quan hóa Cây Gia Phả:** Xem cây gia phả dưới dạng biểu đồ có thể tương tác (zoom, kéo, lọc).
- **Tìm kiếm & Lọc:** Dễ dàng tìm kiếm thành viên theo tên, thế hệ và các tiêu chí khác.
- **Đa ngôn ngữ:** Hỗ trợ giao diện tiếng Việt và tiếng Anh.

## 🛠️ Công Nghệ Sử Dụng (Tech Stack)

- **Backend:** ASP.NET 8, Clean Architecture, JWT Authentication
- **Frontend:** Vue.js 3, Vuetify 3, Pinia, Vue Router, Vite
- **Database:** MongoDB
- **Deployment:** Docker, Nginx
- **CI/CD:** GitHub Actions

## 🚀 Bắt Đầu Nhanh (Getting Started)

### Yêu cầu

- Docker & Docker Compose
- .NET 8 SDK (cho phát triển backend)
- Node.js 20+ (cho phát triển frontend)

### Cài đặt và Chạy

1. **Clone a repository:**
   ```bash
   git clone https://github.com/your-username/family-tree-3.git
   cd family-tree-3
   ```

2. **Tạo project Backend (nếu chưa có):**
   *Lưu ý: Bước này chỉ cần làm một lần duy nhất.*
   ```bash
   # Cài đặt template
   dotnet new --install JasonTaylor.CleanArchitecture
   # Tạo project
   dotnet new ca-sln -o backend --use-program-main
   ```

3. **Chạy ứng dụng với Docker Compose:**
   Lệnh này sẽ build và chạy backend, frontend, và database.
   ```bash
   docker-compose up -d
   ```

4. **Truy cập ứng dụng:**
   - **Frontend:** [http://localhost](http://localhost)
   - **Backend API (Swagger):** [http://localhost:8080/swagger](http://localhost:8080/swagger)

## 🤝 Đóng Góp (Contributing)

Chúng tôi hoan nghênh mọi sự đóng góp! Vui lòng đọc file [docs/contribution.md](./docs/contribution.md) để biết chi tiết về quy trình đóng góp, quy tắc đặt tên branch, và quy trình code review.

## 📄 Giấy Phép (License)

Dự án này được cấp phép dưới giấy phép MIT. Xem file [LICENSE](./LICENSE) để biết thêm chi tiết.