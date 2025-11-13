# Hướng dẫn Phát triển Frontend

Tài liệu này cung cấp hướng dẫn chi tiết về cách thiết lập, phát triển và duy trì phần frontend của ứng dụng Family Tree.

## 1. Tổng quan

Frontend của ứng dụng Family Tree là giao diện người dùng tương tác, cho phép người dùng xem, quản lý và trực quan hóa cây gia phả. Mục tiêu chính là cung cấp trải nghiệm người dùng mượt mà, trực quan và phản hồi nhanh.

**Công nghệ chính:**
*   **Framework:** Vue.js 3 (TypeScript)
*   **UI Framework:** Vuetify 3 (Material Design)
*   **State Management:** Pinia
*   **Build Tool:** Vite
*   **Routing:** Vue Router

## 2. Cài đặt và Chạy

### Yêu cầu

*   Node.js 20+ (hoặc phiên bản tương thích)
*   npm (thường đi kèm với Node.js)

### Chạy cục bộ (Development Mode)

1.  **Điều hướng đến thư mục frontend:**
    ```bash
    cd apps/admin
    ```
2.  **Cài đặt các phụ thuộc:**
    ```bash
    npm install
    ```
3.  **Cấu hình biến môi trường:**
    Tạo một tệp `.env` trong thư mục `apps/admin` dựa trên `apps/admin/.env.example`. Cấu hình `VITE_APP_API_BASE_URL` trỏ đến địa chỉ backend API của bạn (ví dụ: `http://localhost:5000`).
4.  **Chạy ứng dụng:**
    ```bash
    npm run dev
    ```
    Ứng dụng sẽ chạy trên `http://localhost:5173` (hoặc một cổng khác nếu 5173 đã được sử dụng).

### Xây dựng (Build for Production)

Để tạo bản build sẵn sàng cho môi trường production:

1.  **Điều hướng đến thư mục frontend:**
    ```bash
    cd apps/admin
    ```
2.  **Chạy lệnh build:**
    ```bash
    npm run build
    ```
    Các tệp tĩnh đã được build sẽ nằm trong thư mục `apps/admin/dist/`.

## 3. Cấu trúc Thư mục

Dự án frontend được tổ chức một cách rõ ràng để dễ dàng quản lý và mở rộng:

*   **`apps/admin/src/`**:
    *   **`assets/`**: Chứa các tài nguyên tĩnh như hình ảnh, font, JSON data.
    *   **`components/`**: Chứa các Vue components có thể tái sử dụng (ví dụ: `buttons`, `cards`, `forms`).
    *   **`constants/`**: Định nghĩa các hằng số của ứng dụng.
    *   **`data/`**: Dữ liệu mock hoặc dữ liệu tĩnh.
    *   **`layouts/`**: Các layout chính của ứng dụng (ví dụ: `DefaultLayout.vue`).
    *   **`locales/`**: Các tệp dịch thuật cho tính năng đa ngôn ngữ.
    *   **`plugins/`**: Các plugin Vue.js (ví dụ: `vuetify.ts`, `i18n.ts`, `services.plugin.ts`).
    *   **`router/`**: Cấu hình Vue Router và định nghĩa các route.
    *   **`services/`**: Chứa các service để tương tác với backend API hoặc các dịch vụ bên ngoài. Được tổ chức theo từng module (ví dụ: `family`, `member`).
    *   **`stores/`**: Chứa các Pinia stores để quản lý trạng thái ứng dụng. Được tổ chức theo từng module (ví dụ: `family.store.ts`, `auth.store.ts`).
    *   **`styles/`**: Các tệp CSS/SCSS tùy chỉnh.
    *   **`types/`**: Định nghĩa các TypeScript interfaces và types chung của ứng dụng.
    *   **`utils/`**: Các hàm tiện ích chung.
    *   **`views/`**: Chứa các Vue components cấp cao nhất, đại diện cho các trang hoặc màn hình chính của ứng dụng.

## 4. Quy ước (Conventions)

### 4.1. Code Style & Linting

*   Sử dụng ESLint và Prettier để duy trì code style nhất quán. Cấu hình được định nghĩa trong `.eslintrc.cjs` và `.prettierrc.cjs`.
*   Để kiểm tra và tự động sửa lỗi định dạng:
    ```bash
    cd apps/admin
    npm run lint
    npm run lint:fix
    ```

### 4.2. Commit Messages

Tuân thủ Conventional Commits để có các commit message rõ ràng và có cấu trúc. Ví dụ:

*   `feat: add new user registration feature`
*   `fix: correct typo in login form`
*   `docs: update frontend development guide`

### 4.3. Import Paths

Luôn sử dụng alias `@/` cho các import trong thư mục `apps/admin/src/` để giữ cho đường dẫn ngắn gọn và dễ đọc. Ví dụ:

```typescript
import { useAuthStore } from '@/stores/auth.store';
import MyComponent from '@/components/MyComponent.vue';
```

## 5. Kiểm thử

Dự án frontend sử dụng Vitest để chạy unit tests và kiểm tra độ bao phủ mã.

### 5.1. Chạy Tests

1.  **Điều hướng đến thư mục frontend:**
    ```bash
    cd apps/admin
    ```
2.  **Chạy tất cả các tests:**
    ```bash
    npm run test
    ```
3.  **Chạy tests với độ bao phủ mã (Code Coverage):**
    ```bash
    npm run test:coverage
    ```
    Kết quả độ bao phủ mã sẽ được tạo ra trong thư mục `apps/admin/coverage/`.

### 5.2. Hướng dẫn Viết Tests

*   **Framework**: Sử dụng Vitest làm test runner và thư viện testing. Vue Test Utils để mount và tương tác với các Vue components.
*   **Cấu trúc**: Các tệp test thường được đặt cùng thư mục với tệp mã nguồn hoặc trong thư mục `apps/admin/tests/unit/`.
*   **Mocking**: Sử dụng Vitest's mocking utilities hoặc thư viện như `vi.mock` để mock các dependencies (ví dụ: API services, Pinia stores).
*   **Tham khảo**: Xem các tệp test hiện có trong `apps/admin/tests/unit/` để hiểu rõ hơn về cách viết test.

## 6. CI/CD

Frontend được tích hợp vào quy trình CI/CD của GitHub Actions thông qua workflow `ci.yml`.

*   **`build-and-test` job**: Trong job này, các bước sau được thực hiện cho frontend:
    *   Cài đặt Node.js 20.
    *   Cài đặt các phụ thuộc (`npm install`).
    *   Chạy linting (`npm run lint`).
    *   Chạy unit tests và kiểm tra độ bao phủ mã (`npm run test:coverage`).
*   **`docker-build` job**: Sau khi các kiểm tra và test thành công, một Docker image cho frontend sẽ được xây dựng bằng `infra/Dockerfile.admin` và gắn thẻ `hkthao/family-tree-admin:latest`.

## 7. Cấu hình Biến môi trường

Frontend sử dụng các biến môi trường để cấu hình các cài đặt khác nhau tùy thuộc vào môi trường (development, production).

*   **Tệp `.env`**: Khi chạy cục bộ, bạn có thể định nghĩa các biến môi trường trong tệp `.env` (hoặc `.env.development`, `.env.production`).
*   **Tiền tố `VITE_APP_`**: Vite yêu cầu các biến môi trường được sử dụng trong mã nguồn frontend phải có tiền tố `VITE_APP_` (ví dụ: `VITE_APP_API_BASE_URL`).
*   **Truy cập trong mã nguồn**: Các biến này có thể được truy cập trong mã nguồn bằng `import.meta.env.VITE_APP_API_BASE_URL`.

## 8. Quy trình Phát triển Tính năng

Khi thêm một tính năng mới vào frontend, hãy tuân thủ quy trình sau:

1.  **Tạo nhánh mới:** Luôn làm việc trên một nhánh tính năng mới (ví dụ: `feature/ten-tinh-nang-moi`).
2.  **Phát triển:**
    *   Tạo hoặc cập nhật các components, views, services, stores liên quan.
    *   Đảm bảo tuân thủ code style và các quy ước đã định nghĩa.
    *   Viết unit tests cho các phần logic mới.
3.  **Kiểm tra cục bộ:** Chạy `npm run dev` để kiểm tra tính năng trong trình duyệt.
4.  **Chạy lint và tests:** Đảm bảo không có lỗi lint và tất cả các tests đều vượt qua (`npm run lint`, `npm run test:coverage`).
5.  **Tạo Pull Request:** Khi hoàn thành, tạo một Pull Request lên nhánh `develop`.

## 9. Xử lý sự cố (Troubleshooting)

*   **Lỗi `npm install`**: Kiểm tra phiên bản Node.js của bạn có tương thích không. Thử xóa `node_modules` và `package-lock.json` rồi chạy lại `npm install`.
*   **Lỗi API không phản hồi**: Đảm bảo backend API đang chạy và `VITE_APP_API_BASE_URL` trong tệp `.env` của frontend được cấu hình đúng.
*   **Lỗi hiển thị UI**: Kiểm tra console của trình duyệt để tìm lỗi JavaScript hoặc CSS. Đảm bảo Vuetify đã được cài đặt và cấu hình đúng.
*   **Lỗi Linting**: Chạy `npm run lint:fix` để tự động sửa các lỗi định dạng.

## 10. Tài liệu liên quan

*   [Vue.js Documentation](https://vuejs.org/)
*   [Vuetify Documentation](https://vuetifyjs.com/)
*   [Pinia Documentation](https://pinia.vuejs.org/)
*   [Vite Documentation](https://vitejs.dev/)
*   [Vitest Documentation](https://vitest.dev/)
*   [Novu Documentation](https://docs.novu.co/)
