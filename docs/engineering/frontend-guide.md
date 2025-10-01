# Hướng dẫn Frontend

Tài liệu này cung cấp hướng dẫn chi tiết về cấu trúc, quy tắc và các phương pháp tốt nhất cho việc phát triển Frontend của dự án.

## 1. Cấu trúc thư mục

```
frontend/
├── src/
│   ├── assets/         # Chứa các tài sản tĩnh (hình ảnh, fonts)
│   ├── components/     # Chứa các component có thể tái sử dụng
│   ├── composables/    # Chứa các hàm logic có thể tái sử dụng (Vue 3)
│   ├── constants/      # Chứa các hằng số
│   ├── data/           # Chứa dữ liệu mock
│   ├── layouts/        # Chứa các layout chính của ứng dụng
│   ├── locales/        # Chứa các file dịch (i18n)
│   ├── plugins/        # Chứa các plugin của Vue (vuetify, pinia, ...)
│   ├── router/         # Cấu hình routing
│   ├── services/       # Chứa các service giao tiếp với API
│   ├── stores/         # Chứa các store của Pinia
│   ├── styles/         # Chứa các file style chung
│   ├── types/          # Chứa các định nghĩa type/interface
│   ├── utils/          # Chứa các hàm tiện ích
│   └── views/          # Chứa các trang (page components)
├── tests/              # Chứa các file test
└── ...
```

## 2. Quản lý trạng thái với Pinia

Dự án sử dụng Pinia để quản lý trạng thái. Các store được đặt trong `src/stores`.

### Quy tắc đặt tên

-   Tên file: `[name].store.ts` (ví dụ: `family.store.ts`)
-   Tên store (id): `[name]` (ví dụ: `family`)

### Cấu trúc một store

```typescript
import { defineStore } from 'pinia';

export const useMyStore = defineStore('myStore', {
  state: () => ({
    // ... state properties
  }),
  getters: {
    // ... getters
  },
  actions: {
    // ... actions
  },
});
```

## 3. Quy tắc đặt tên Component

-   **PascalCase**: Tên component luôn ở dạng PascalCase (ví dụ: `FamilyTree.vue`).
-   **Tiền tố `Base`**: Các component cơ bản, có thể tái sử dụng cao nên có tiền tố `Base` (ví dụ: `BaseButton.vue`).
-   **Tên component cha**: Component con nên có tên component cha làm tiền tố (ví dụ: `FamilyTreeMember.vue` là con của `FamilyTree.vue`).

## 4. Coding Style

-   Sử dụng **ESLint** và **Prettier** để đảm bảo code style nhất quán. Chạy `npm run lint` để kiểm tra.
-   Luôn sử dụng `<script setup lang="ts">` cho các component Vue 3.
-   Sử dụng TypeScript cho tất cả các file `.ts` và `.vue`.
-   Viết comment rõ ràng cho các logic phức tạp.

## 5. Bất đồng bộ

-   Sử dụng `async/await` cho tất cả các tác vụ bất đồng bộ (ví dụ: gọi API).
-   Xử lý lỗi bằng `try...catch` hoặc trong các service.
