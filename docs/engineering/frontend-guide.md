# Hướng dẫn Frontend

## Mục lục

- [1. Giới thiệu](#1-giới-thiệu)
- [2. Cấu trúc thư mục](#2-cấu-trúc-thư-mục)
- [3. Quản lý trạng thái với Pinia](#3-quản-lý-trạng-thái-với-pinia)
- [4. Quy tắc đặt tên Component](#4-quy-tắc-đặt-tên-component)
- [5. Coding Style](#5-coding-style)
- [6. Bất đồng bộ](#6-bất-đồng-bộ)
- [7. Mapping Dữ liệu từ API](#7-mapping-dữ-liệu-từ-api)
- [8. Hướng dẫn Kiểm thử](#8-hướng-dẫn-kiểm-thử)

---

## 1. Giới thiệu

Frontend của dự án được xây dựng bằng **Vue 3** với **TypeScript**, sử dụng **Vite** làm công cụ build. Giao diện người dùng được xây dựng bằng **Vuetify 3**, và quản lý trạng thái bằng **Pinia**. **ESLint** và **Prettier** được sử dụng để đảm bảo chất lượng code.

## 2. Cấu trúc thư mục

```
frontend/
├── src/
│   ├── assets/         # Tài sản tĩnh (hình ảnh, fonts)
│   ├── components/     # Component có thể tái sử dụng
│   ├── composables/    # Logic tái sử dụng (Vue 3 composables)
│   ├── constants/      # Hằng số
│   ├── data/           # Dữ liệu mock
│   ├── layouts/        # Layout chính của ứng dụng
│   ├── locales/        # File dịch (i18n)
│   ├── plugins/        # Plugin của Vue (vuetify, pinia, ...)
│   ├── router/         # Cấu hình routing
│   ├── services/       # Service giao tiếp với API
│   ├── stores/         # Store của Pinia
│   ├── styles/         # Style chung
│   ├── types/          # Định nghĩa type/interface
│   ├── utils/          # Hàm tiện ích
│   └── views/          # Page components
├── tests/              # File test
└── ...
```

## 3. Quản lý trạng thái với Pinia

Pinia được sử dụng để quản lý trạng thái toàn cục. Các store được đặt trong `src/stores`.

**Lifecycle Hooks & API Service:**

```typescript
// stores/family.store.ts
import { defineStore } from 'pinia';
import { useFamilyService } from '@/services';

export const useFamilyStore = defineStore('family', {
  state: () => ({ families: [], loading: false }),
  actions: {
    async fetchFamilies() {
      this.loading = true;
      const familyService = useFamilyService();
      try {
        this.families = await familyService.getAll();
      } catch (error) {
        console.error("Failed to fetch families:", error);
      } finally {
        this.loading = false;
      }
    },
  },
});
```

## 4. Quy tắc đặt tên Component

-   **PascalCase**: `FamilyTree.vue`.
-   **Hierarchy**: Component con nên có tên component cha làm tiền tố. Ví dụ: `FamilyTreeMember.vue` là con của `FamilyTree.vue`.
-   **Slots**: Khi sử dụng slot, đặt tên slot rõ ràng. Ví dụ: `<slot name="header"></slot>`.

## 5. Coding Style

-   **Import Order**: Nhóm các import theo thứ tự: thư viện bên ngoài, alias của project (`@/`), import tương đối (`./`, `../`).
-   **Props Order**: `defineProps` nên được định nghĩa ở đầu `<script setup>`.
-   **CSS**: Sử dụng `<style scoped>` để CSS chỉ ảnh hưởng đến component hiện tại. Các style toàn cục đặt trong `src/styles`.
-   **Vuetify**: Sử dụng các component của Vuetify một cách nhất quán.

## 6. Bất đồng bộ

-   **Mocking**: Trong môi trường development, các service có thể được mock để trả về dữ liệu giả lập từ `src/data`.
-   **Error Handling**: Sử dụng `try...catch` trong các action của Pinia hoặc trong component để xử lý lỗi từ API.
-   **Retry Strategy**: (Chưa áp dụng) Có thể xem xét sử dụng thư viện như `axios-retry` nếu cần.

## 7. Mapping Dữ liệu từ API

Sử dụng TypeScript interfaces trong `src/types` để định nghĩa cấu trúc dữ liệu từ API.

```typescript
// src/types/member.ts
export interface Member {
  id: string;
  familyId: string;
  firstName: string;
  lastName: string;
  fullName?: string;
  gender?: 'Male' | 'Female' | 'Other';
  dateOfBirth?: string; // ISO 8601 format
}
```

## 8. Hướng dẫn Kiểm thử

-   **Component Tests**: Kiểm tra giao diện và tương tác của component. Sử dụng `Vue Test Utils` và `Vitest`.
-   **Store Tests**: Kiểm tra logic của Pinia store.
-   **API Service Tests**: Mock API để kiểm tra các service.

-   **Chạy test:**

    ```bash
    npm run test:unit --prefix frontend
    ```