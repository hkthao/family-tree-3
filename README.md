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
Database: MySQL (via Entity Framework Core)
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

### Cấu hình MySQL

Để chạy ứng dụng với MySQL, bạn cần đảm bảo rằng chuỗi kết nối trong `appsettings.json` của backend được cấu hình đúng. Ví dụ:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3306;Database=familytree_db;Uid=root;Pwd=password;"
}
```

Bạn cần thay thế `localhost`, `3306`, `familytree_db`, `root`, và `password` bằng thông tin chi tiết máy chủ MySQL thực tế của bạn.

Sau khi cấu hình, bạn có thể chạy các migrations để tạo schema database:

```bash
dotnet ef database update --project backend/src/Infrastructure --startup-project backend/src/Web
```

### Tối ưu hóa Docker Build (Docker Build Optimization)

Để tăng tốc độ build Docker, đặc biệt trên macOS, chúng tôi đã thực hiện các tối ưu hóa sau:

- **Build Context Tối thiểu:** Mỗi dịch vụ (backend và frontend) giờ đây chỉ gửi thư mục mã nguồn của riêng nó làm build context cho Docker daemon. Điều này giảm đáng kể lượng dữ liệu cần truyền tải, giúp build nhanh hơn.
- **Tệp `.dockerignore` chuyên biệt:** Mỗi thư mục `backend/` và `frontend/` hiện có một tệp `.dockerignore` riêng. Các tệp này đảm bảo rằng chỉ những tệp cần thiết mới được đưa vào build context, loại bỏ các tệp tạm thời, thư mục `node_modules`, `bin/obj`, và các tệp không liên quan khác.
- **Tận dụng Cache hiệu quả:** Các Dockerfile được cấu trúc để tận dụng tối đa Docker cache. Các bước cài đặt dependency (`npm install`, `dotnet restore`) được đặt ở các layer riêng biệt, chỉ chạy lại khi các tệp cấu hình dependency (ví dụ: `package.json`, `*.csproj`) thay đổi. Điều này giúp tiết kiệm thời gian đáng kể cho các lần build tiếp theo.

### Truy cập ứng dụng:**
   - **Frontend:** [http://localhost](http://localhost)
   - **Backend API (Swagger):** [http://localhost:8080/swagger](http://localhost:8080/swagger)

## 📚 Tài liệu Dự án (Project Documentation)

Toàn bộ tài liệu của dự án được tổ chức trong thư mục [`docs/`](./docs/). Dưới đây là các tài liệu quan trọng bạn nên tham khảo:

### 1. Tài liệu Sản phẩm
-   **Lộ trình Phát triển Sản phẩm (Roadmap)**: Định hướng phát triển cho các quý tiếp theo.
-   **Phân loại Ưu tiên**: Phương pháp MoSCoW để xác định phạm vi cho MVP.
-   **Product Backlog**: Danh sách chi tiết các User Story.
-   **Phân nhóm Epic**: Nhóm các User Story thành các tính năng lớn.

### 2. Tài liệu Kỹ thuật
-   **Thiết kế Hệ thống**: Kiến trúc tổng quan, sơ đồ và thiết kế database.
-   **Thiết kế API**: Mô tả chi tiết các API endpoint.
-   **Hướng dẫn cho Developer**: Hướng dẫn cài đặt, chạy dự án và các quy trình kỹ thuật.

### 3. Quy trình & Hướng dẫn
-   **Kế hoạch Sprint**: Phân rã công việc chi tiết cho từng Sprint.
-   **Thiết lập Bảng Kanban**: Hướng dẫn thiết lập cột và nhãn trên GitHub Projects.
-   **Hướng dẫn Đóng góp**: Quy tắc và quy trình để đóng góp vào dự án.
-   **Quy tắc Ứng xử (Code of Conduct)**: Các quy tắc ứng xử trong cộng đồng dự án.
-   **Hướng dẫn Sử dụng**: Các bước cơ bản để sử dụng sản phẩm.

---

## ️ Hướng dẫn phát triển (Development Guide)

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

Chúng tôi hoan nghênh mọi sự đóng góp! Vui lòng đọc file [Hướng dẫn Đóng góp](./docs/3_process/contribution.md) để biết chi tiết về quy trình đóng góp. Mọi người tham gia dự án này đều được kỳ vọng sẽ tuân theo [Quy tắc Ứng xử](./docs/3_process/CODE_OF_CONDUCT.md) của chúng tôi.

## 📄 Giấy Phép (License)

Dự án này được cấp phép dưới giấy phép MIT. Xem file [LICENSE](./LICENSE) để biết thêm chi tiết.