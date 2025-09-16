# Hướng Dẫn cho Developer

## 1. Cài đặt môi trường
- .NET 8 SDK
- Node.js 20+
- Docker

## 2. Chạy dự án local
```bash
# Chạy full-stack (backend + frontend + db)
docker-compose up -d
```

## 3. Khởi tạo dự án
Toàn bộ các lệnh đã được dùng để scaffold dự án này được ghi lại tại file [commands_log.md](./commands_log.md).

## 4. CI/CD Pipeline
Pipeline được cấu hình tại `.github/workflows/ci.yml`.
Các bước chính:
- Build
- Test
- Lint
- Docker Build & Push
