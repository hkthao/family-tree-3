# Hướng Dẫn cho Developer

## 1. Cài đặt môi trường phát triển
Để phát triển dự án, bạn cần cài đặt các công cụ sau:
- **Docker & Docker Compose**: Để chạy các dịch vụ (backend API, frontend, MongoDB) một cách nhất quán.
- **.NET 8 SDK**: Cho việc phát triển và build phần backend.
- **Node.js 20+**: Cho việc phát triển và build phần frontend.

## 2. Khởi tạo dự án (Scaffolding)
Các lệnh đã được sử dụng để khởi tạo và cấu hình dự án này được ghi lại chi tiết tại file [docs/commands_log.md](./commands_log.md).

## 3. Chạy dự án local với Docker Compose
Dự án được cấu hình để chạy toàn bộ stack (backend API, frontend, MongoDB) bằng Docker Compose.
1.  Đảm bảo Docker Desktop đang chạy trên máy của bạn.
2.  Mở terminal tại thư mục gốc của dự án (`family-tree-3`).
3.  Chạy lệnh sau để build và khởi động tất cả các dịch vụ:
    ```bash
    docker-compose -f infra/docker-compose.yml up --build -d
    ```
    *Lưu ý về Dockerfile Backend:* Nếu bạn gặp lỗi `lstat /backend/src/Api: no such file or directory` khi build Docker image cho backend, hãy kiểm tra `infra/Dockerfile.backend`. Tệp này đã được cập nhật để sử dụng đường dẫn dự án `Web` thay vì `Api` (ví dụ: `COPY backend/src/Web/*.csproj ./src/Web/`, `WORKDIR /source/src/Web`, `ENTRYPOINT ["dotnet", "Web.dll"]`). Đảm bảo rằng các đường dẫn trong Dockerfile khớp với cấu trúc thư mục dự án thực tế của bạn.
    - `--build`: Buộc Docker Compose build lại các image từ Dockerfile (hữu ích khi có thay đổi code).
    - `-d`: Chạy các dịch vụ ở chế độ nền (detached mode).
4.  **Truy cập ứng dụng**:
    - **Frontend**: `http://localhost`
    - **Backend API (Swagger UI)**: `http://localhost:8080/swagger`

## 4. Biến môi trường (Environment Variables)
Các biến môi trường quan trọng được quản lý thông qua file `appsettings.json` (cho backend) và `.env` files (cho frontend).
- **Backend**: `backend/src/Web/appsettings.json` chứa cấu hình kết nối MongoDB.
  ```json
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "family-tree"
  }
  ```
- **Frontend**: Cấu hình API endpoint có thể được quản lý qua biến môi trường trong `.env` file.

## 5. Chạy Tests và Coverage
### Backend Tests
Để chạy unit tests và kiểm tra coverage cho backend:
```bash
# Chạy tests và xem kết quả coverage
dotnet test backend/backend.sln /p:CollectCoverage=true /p:CoverletOutput=./tests/coverage/backend/ /p:CoverletOutputFormat=lcov

# Để kiểm tra coverage với ngưỡng 80% (sẽ fail nếu thấp hơn)
dotnet test backend/backend.sln /p:CollectCoverage=true /p:CoverletOutput=./tests/coverage/backend/ /p:CoverletOutputFormat=lcov /p:Threshold=80
```
### Frontend Tests
Để chạy unit/component tests và kiểm tra coverage cho frontend:
```bash
# Chạy tests và xem kết quả coverage
npm run test:coverage --prefix frontend
```

## 6. Code Style và Linting
### Backend
Để kiểm tra và định dạng code backend:
```bash
# Kiểm tra định dạng code
dotnet format backend/ --verify-no-changes --include-generated

# Tự động định dạng code
dotnet format backend/ --include-generated
```
### Frontend
Để kiểm tra và định dạng code frontend:
```bash
# Kiểm tra định dạng code
npm run lint --prefix frontend

# Tự động định dạng code
npm run lint:fix --prefix frontend
```
- **Lưu ý về TypeScript:** Dự án sử dụng TypeScript phiên bản `~5.5.0` để đảm bảo tương thích với các công cụ linting. Nếu bạn gặp lỗi liên quan đến phiên bản TypeScript không được hỗ trợ, hãy đảm bảo phiên bản TypeScript của bạn nằm trong khoảng `>=4.7.4 <5.6.0`.

## 7. Quy trình Pull Request (PR Checklist)
Khi tạo Pull Request, hãy đảm bảo các điểm sau:
- [ ] Code đã được định dạng (`dotnet format` và `npm run lint`).
- [ ] Tất cả các unit tests đều pass.
- [ ] Test coverage đạt ngưỡng yêu cầu (>=80%).
- [ ] Đã cập nhật tài liệu liên quan (nếu có).
- [ ] Branch được đặt tên theo quy tắc (`feature/`, `bugfix/`, `hotfix/`, `docs/`).
- [ ] Commit message tuân thủ Conventional Commits.

## 8. Branch Strategy
Dự án sử dụng chiến lược nhánh cơ bản:
- `main`: Nhánh ổn định, chứa code sẵn sàng triển khai lên môi trường Production.
- `develop`: Nhánh phát triển chính, nơi các tính năng mới được tích hợp.
- `feature/<tên-tính-năng>`: Nhánh con được tạo từ `develop` để phát triển một tính năng cụ thể.
- `bugfix/<tên-lỗi>`: Nhánh con được tạo từ `main` (hoặc `develop` tùy mức độ) để sửa lỗi.
- `docs/<tên-tài-liệu>`: Nhánh con để cập nhật tài liệu.
- `hotfix/<tên-fix>`: Nhánh con được tạo từ `main` để sửa lỗi khẩn cấp trên Production.

Tất cả các thay đổi phải được merge vào `main` thông qua Pull Request.

## 9. CI/CD Pipeline
Pipeline được cấu hình tại `.github/workflows/ci.yml`. Các bước chính bao gồm:
- Build và Test backend (với coverage check).
- Build và Test frontend (với coverage check).
- Lint code (backend và frontend).
- Build và Push Docker images lên Docker Hub.

## 10. Logging và Xử lý lỗi (Logging and Error Handling)
- **Logging**: Hệ thống sử dụng Serilog để ghi log.
  - **TODO**: Hướng dẫn cấu hình Serilog và các sink (ví dụ: console, file, Elasticsearch).
- **Global Error Handling**: Middleware xử lý lỗi tập trung.
  - **TODO**: Mô tả cách CustomExceptionHandler hoạt động và cách mở rộng.

## 11. Quản lý Schema Database (Schema Versioning)
- **Hướng dẫn**: Hiện tại, việc quản lý thay đổi schema trong MongoDB được thực hiện thủ công. Khi có thay đổi về cấu trúc document, cần cập nhật các entity tương ứng trong code và đảm bảo tính tương thích ngược.
- **TODO**: Nghiên cứu và đề xuất một giải pháp quản lý schema tự động hoặc bán tự động cho MongoDB trong tương lai (ví dụ: sử dụng các thư viện như `migrate-mongo` hoặc xây dựng cơ chế versioning riêng).

## 12. Chạy Seed Data
Để populate database với dữ liệu mẫu, bạn có thể chạy script seed data:
1.  Đảm bảo MongoDB đang chạy (ví dụ: thông qua `docker-compose up -d mongo`).
2.  Mở terminal tại thư mục `infra/seeds`.
3.  Cài đặt các dependencies cho seed script:
    ```bash
    npm install
    ```
4.  Chạy script seed data:
    ```bash
    npm run seed
    ```
