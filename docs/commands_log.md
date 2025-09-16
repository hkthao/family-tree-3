# Log các lệnh đã thực thi để scaffold dự án

Đây là danh sách các lệnh shell đã được chạy để khởi tạo và cấu hình dự án này.

## 1. Tạo cấu trúc thư mục
```bash
mkdir -p backend frontend docs tests infra .github/workflows
```

## 2. Scaffold Frontend (Vue.js)
```bash
# Tạo sườn project Vue.js với TypeScript, Router, Pinia, Vitest
npm create vue@latest frontend -- --ts --router --pinia --vitest --force

# Cài đặt các dependencies
npm install --prefix frontend

# Cài đặt Vuetify và Material Design Icons
npm install --prefix frontend vuetify@^3.5.8 @mdi/font
npm install --prefix frontend -D vite-plugin-vuetify

# Dọn dẹp các component và view mặc định
rm -rf frontend/src/components/* frontend/src/assets/*
rm frontend/src/views/HomeView.vue frontend/src/views/AboutView.vue

# Cài đặt i18n
npm install --prefix frontend vue-i18n
mkdir -p frontend/src/locales
```
*Lưu ý: Các bước tạo và chỉnh sửa file cấu hình (`vite.config.ts`, `main.ts`,...), file component (`App.vue`, các views,...), và file tài liệu (`docs/*.md`) được thực hiện bằng tool `write_file` và `replace`, không được liệt kê ở đây.*

## 3. Backend (Chưa hoàn thành)
Các lệnh sau cần được thực thi để hoàn tất phần backend.

```bash
# 1. Cài đặt template Clean Architecture
dotnet new --install JasonTaylor.CleanArchitecture

# 2. Tạo project từ template
dotnet new ca-sln -o backend --use-program-main
```
