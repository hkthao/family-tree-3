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

- Docker & Docker Compose (để chạy ứng dụng)
- .NET 8 SDK (chỉ cần cho phát triển backend)
- Node.js 20+ (chỉ cần cho phát triển frontend)

### Cài đặt và Chạy

1. **Clone a repository:**
   ```bash
   git clone https://github.com/your-username/family-tree-3.git
   cd family-tree-3
   ```

3. **Chạy ứng dụng với Docker Compose:**
   Lệnh này sẽ build (nếu cần) và chạy backend, frontend, và database.
   ```bash
   docker-compose -f infra/docker-compose.yml up --build
   ```

### Tối ưu hóa Docker Build (Docker Build Optimization)

Để tăng tốc độ build Docker, đặc biệt trên macOS, chúng tôi đã thực hiện các tối ưu hóa sau:

- **Build Context Tối thiểu:** Mỗi dịch vụ (backend và frontend) giờ đây chỉ gửi thư mục mã nguồn của riêng nó làm build context cho Docker daemon. Điều này giảm đáng kể lượng dữ liệu cần truyền tải, giúp build nhanh hơn.
- **Tệp `.dockerignore` chuyên biệt:** Mỗi thư mục `backend/` và `frontend/` hiện có một tệp `.dockerignore` riêng. Các tệp này đảm bảo rằng chỉ những tệp cần thiết mới được đưa vào build context, loại bỏ các tệp tạm thời, thư mục `node_modules`, `bin/obj`, và các tệp không liên quan khác.
- **Tận dụng Cache hiệu quả:** Các Dockerfile được cấu trúc để tận dụng tối đa Docker cache. Các bước cài đặt dependency (`npm install`, `dotnet restore`) được đặt ở các layer riêng biệt, chỉ chạy lại khi các tệp cấu hình dependency (ví dụ: `package.json`, `*.csproj`) thay đổi. Điều này giúp tiết kiệm thời gian đáng kể cho các lần build tiếp theo.

### Truy cập ứng dụng:**
   - **Frontend:** [http://localhost](http://localhost)
   - **Backend API (Swagger):** [http://localhost:8080/swagger](http://localhost:8080/swagger)

## 🛠️ Hướng dẫn phát triển (Development Guide)

### Cấu hình Linting Frontend

Để đảm bảo chất lượng mã nguồn frontend, dự án sử dụng ESLint.
- Lệnh `lint` sẽ kiểm tra lỗi mà không tự động sửa:
  ```bash
  npm run lint --prefix frontend
  ```
- Lệnh `lint:fix` sẽ tự động sửa các lỗi có thể sửa được:
  ```bash
  npm run lint:fix --prefix frontend
  ```
- **Lưu ý về TypeScript:** Dự án sử dụng TypeScript phiên bản `~5.5.0` để đảm bảo tương thích với các công cụ linting. Nếu bạn gặp lỗi liên quan đến phiên bản TypeScript không được hỗ trợ, hãy đảm bảo phiên bản TypeScript của bạn nằm trong khoảng `>=4.7.4 <5.6.0`.

## 🤝 Đóng Góp (Contributing)

Chúng tôi hoan nghênh mọi sự đóng góp! Vui lòng đọc file [docs/contribution.md](./docs/contribution.md) để biết chi tiết về quy trình đóng góp, quy tắc đặt tên branch, và quy trình code review.

## 📄 Giấy Phép (License)

Dự án này được cấp phép dưới giấy phép MIT. Xem file [LICENSE](./LICENSE) để biết thêm chi tiết.