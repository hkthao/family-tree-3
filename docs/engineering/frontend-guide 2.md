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

Frontend của dự án được xây dựng bằng **Vue 3** với **TypeScript**, sử dụng **Vite** làm công cụ build và phát triển. Giao diện người dùng được xây dựng bằng **Vuetify 3** (một framework UI dựa trên Material Design), và quản lý trạng thái ứng dụng bằng **Pinia** (thư viện quản lý state nhẹ và mạnh mẽ cho Vue). **ESLint** và **Prettier** được sử dụng để đảm bảo chất lượng code và định dạng nhất quán.

## 2. Cấu trúc thư mục

Cấu trúc thư mục của Frontend được tổ chức một cách rõ ràng để dễ dàng tìm kiếm và quản lý code:

```
frontend/
├── src/
│   ├── assets/         # Chứa các tài sản tĩnh như hình ảnh, icons, fonts, và các file CSS/SCSS toàn cục.
│   ├── components/     # Chứa các Vue component có thể tái sử dụng trên nhiều trang hoặc trong các component lớn hơn (ví dụ: `BaseButton.vue`, `FamilyCard.vue`).
│   ├── composables/    # Chứa các hàm Composable (Vue 3 Composition API) để tái sử dụng logic có trạng thái (stateful logic) giữa các component (ví dụ: `useAuth.ts`, `usePagination.ts`).
│   ├── constants/      # Chứa các hằng số được sử dụng trong toàn bộ ứng dụng (ví dụ: `API_ENDPOINTS.ts`, `APP_CONFIG.ts`).
│   ├── data/           # Chứa dữ liệu mock được sử dụng trong môi trường phát triển hoặc cho các service mock.
│   ├── layouts/        # Chứa các layout chính của ứng dụng (ví dụ: `DefaultLayout.vue`, `AuthLayout.vue`).
│   ├── locales/        # Chứa các file dịch (i18n) cho các ngôn ngữ khác nhau.
│   ├── plugins/        # Chứa các plugin của Vue hoặc các thư viện bên thứ ba được khởi tạo (ví dụ: `vuetify.ts`, `pinia.ts`, `axios.ts`).
│   ├── router/         # Chứa cấu hình routing của Vue Router, định nghĩa các đường dẫn và component tương ứng.
│   ├── services/       # Chứa các service giao tiếp với Backend API hoặc các dịch vụ bên ngoài khác (ví dụ: `api.family.service.ts`). Các service này trả về kết quả theo `Result Pattern` để xử lý lỗi và thành công một cách nhất quán.
│   ├── stores/         # Chứa các Pinia store để quản lý trạng thái toàn cục của ứng dụng (ví dụ: `auth.store.ts`, `family.store.ts`).
│   ├── styles/         # Chứa các style chung, biến CSS, hoặc các file SCSS/CSS được import toàn cục.
│   ├── types/          # Chứa định nghĩa các TypeScript type và interface cho dữ liệu (ví dụ: `Family.ts`, `Member.ts`).
│   ├── utils/          # Chứa các hàm tiện ích (utility functions) không liên quan đến Vue component hoặc logic nghiệp vụ cụ thể (ví dụ: `dateUtils.ts`, `stringUtils.ts`).
│   └── views/          # Chứa các page components, mỗi component đại diện cho một trang hoặc một route chính của ứng dụng (ví dụ: `FamilyListView.vue`, `MemberDetailView.vue`).
├── tests/              # Chứa các file test cho Frontend (Unit Tests, Component Tests).
└── ...                 # Các file cấu hình khác (package.json, tsconfig.json, vite.config.ts, v.v.).
```

## 3. Quản lý trạng thái với Pinia

**Pinia** là thư viện quản lý trạng thái (state management) được khuyến nghị cho các ứng dụng Vue 3. Nó cung cấp một cách tiếp cận đơn giản, mạnh mẽ và có thể mở rộng để quản lý trạng thái toàn cục của ứng dụng. Các store của Pinia được định nghĩa trong thư mục `src/stores`.

#### Quản lý Hồ sơ Người dùng với `userProfileStore`

`userProfileStore` (`frontend/src/stores/userProfile.store.ts`) hiện là nguồn đáng tin cậy duy nhất (single source of truth) cho thông tin hồ sơ của người dùng hiện tại. Các component UI cần hiển thị thông tin người dùng (như tên, email, avatar, vai trò) nên lấy dữ liệu từ store này thay vì `authStore`.

-   **`fetchCurrentUserProfile()`**: Action này sẽ gọi API backend `GET /api/UserProfiles/me` để lấy hồ sơ của người dùng hiện tại, bao gồm cả các vai trò của họ.
-   **`userProfile`**: State chứa đối tượng `UserProfile` của người dùng hiện tại.

#### Cấu trúc Store

Mỗi store được định nghĩa bằng `defineStore()` và bao gồm:

*   **`state`**: Nơi lưu trữ dữ liệu trạng thái của store.
*   **`getters`**: Các hàm tính toán dữ liệu từ `state` (tương tự computed properties).
*   **`actions`**: Các phương thức để thay đổi `state` hoặc thực hiện các thao tác bất đồng bộ (ví dụ: gọi API).

#### Ví dụ về Family Store

Store này quản lý danh sách các dòng họ, trạng thái loading và các thao tác liên quan đến việc lấy dữ liệu dòng họ từ API.

```typescript
// frontend/src/stores/family.store.ts
import { defineStore } from 'pinia';
import { useFamilyService } from '@/services/family/api.family.service'; // Import service API
import { Family } from '@/types/family/family'; // Import type Family

export const useFamilyStore = defineStore('family', {
  id: 'family', // ID duy nhất của store
  state: () => ({
    families: [] as Family[], // Danh sách các dòng họ
    loading: false,          // Trạng thái loading khi gọi API
    error: null as string | null, // Lưu trữ thông báo lỗi
  }),
  getters: {
    // Ví dụ: Lấy tổng số dòng họ
    totalFamilies: (state) => state.families.length,
    // Ví dụ: Lấy dòng họ theo ID
    getFamilyById: (state) => (id: string) => state.families.find(f => f.id === id),
  },
  actions: {
    async fetchFamilies() {
      this.loading = true;
      this.error = null; // Reset lỗi trước khi gọi API
      const familyService = useFamilyService(); // Khởi tạo service
      try {
        // Gọi API để lấy danh sách dòng họ
        const result = await familyService.getAll();
        if (result.ok) {
          this.families = result.value; // Cập nhật state nếu thành công
        } else {
          this.error = result.error?.message || "Đã xảy ra lỗi khi lấy danh sách dòng họ."; // Lưu lỗi nếu thất bại
        }
      } catch (error: any) {
        this.error = error.message || "Đã xảy ra lỗi không xác định.";
        console.error("Failed to fetch families:", error);
      } finally {
        this.loading = false;
      }
    },

    // Ví dụ: Thêm một dòng họ mới
    async addFamily(newFamily: Omit<Family, 'id'>) {
      this.loading = true;
      this.error = null;
      const familyService = useFamilyService();
      try {
        const result = await familyService.create(newFamily);
        if (result.ok) {
          // Thêm dòng họ mới vào state sau khi tạo thành công
          // Lưu ý: API thường trả về đối tượng hoàn chỉnh với ID
          this.families.push({ ...newFamily, id: result.value }); 
        } else {
          this.error = result.error?.message || "Đã xảy ra lỗi khi thêm dòng họ.";
        }
      } catch (error: any) {
        this.error = error.message || "Đã xảy ra lỗi không xác định.";
        console.error("Failed to add family:", error);
      } finally {
        this.loading = false;
      }
    },
  },
});
```

#### Lifecycle Hooks & API Service (Sử dụng trong Component)

Bạn có thể sử dụng store trong các Vue component bằng cách gọi `use[StoreName]()` và các lifecycle hooks của Vue (ví dụ: `onMounted`) để gọi các action.

```typescript
// frontend/src/views/FamilyListView.vue
<script setup lang="ts">
import { onMounted } from 'vue';
import { useFamilyStore } from '@/stores/family.store';

const familyStore = useFamilyStore();

onMounted(() => {
  familyStore.fetchFamilies(); // Gọi action để lấy dữ liệu khi component được mount
});
</script>

<template>
  <div>
    <div v-if="familyStore.loading">Đang tải dòng họ...</div>
    <div v-else-if="familyStore.error">Lỗi: {{ familyStore.error }}</div>
    <ul v-else>
      <li v-for="family in familyStore.families" :key="family.id">
        {{ family.name }}
      </li>
    </ul>
  </div>
</template>


## 4. Quy tắc đặt tên Component

Việc đặt tên component một cách nhất quán và có ý nghĩa là rất quan trọng để dễ dàng đọc hiểu và bảo trì code. Dưới đây là các quy tắc và khuyến nghị:

*   **PascalCase**: Luôn sử dụng PascalCase cho tên file component và tên component khi import/export. Ví dụ: `FamilyTree.vue`, `MemberCard.vue`.

    ```html
    <!-- Tốt -->
    <template>
      <FamilyTree />
    </template>

    <!-- Không tốt -->
    <template>
      <family-tree />
    </template>
    ```

*   **Hierarchy (Phân cấp)**: Component con nên có tên component cha làm tiền tố để thể hiện mối quan hệ phân cấp và dễ dàng tìm kiếm. Ví dụ: `FamilyTreeMember.vue` là con của `FamilyTree.vue`.

    ```
    components/
    ├── FamilyTree/
    │   ├── FamilyTree.vue
    │   └── FamilyTreeMember.vue
    ├── Member/
    │   ├── MemberList.vue
    │   └── MemberListItem.vue
    ```

*   **Tên mô tả**: Tên component nên mô tả rõ ràng chức năng hoặc nội dung của nó. Tránh các tên quá chung chung như `Item.vue` hoặc `Detail.vue`.

*   **Slots**: Khi sử dụng slot, đặt tên slot rõ ràng và có ý nghĩa để dễ dàng hiểu mục đích của slot đó. Ví dụ: `<slot name="header"></slot>` thay vì `<slot></slot>`.

*   **Prefix cho Base Components**: Các component cơ bản, chung (ví dụ: button, input, card) nên có tiền tố `Base` hoặc `App` để phân biệt với các component nghiệp vụ. Ví dụ: `BaseButton.vue`, `AppCard.vue`.

## 5. Coding Style

Để đảm bảo code Frontend luôn sạch, dễ đọc và nhất quán, dự án sử dụng **ESLint** để phân tích mã tĩnh (static analysis) và **Prettier** để định dạng code tự động. Các công cụ này được tích hợp vào quy trình phát triển và có thể chạy tự động.

#### 1. ESLint

*   **Mục đích**: Phát hiện các lỗi cú pháp, lỗi lập trình tiềm ẩn, và các vấn đề về code style không tuân thủ quy tắc đã định nghĩa.
*   **Cấu hình**: File `.eslintrc.cjs` trong thư mục `frontend/`.
*   **Cách chạy**: 

    ```bash
    # Kiểm tra lỗi linting
    npm run lint --prefix frontend
    
    # Tự động sửa các lỗi có thể sửa được
    npm run lint:fix --prefix frontend
    ```

#### 2. Prettier

*   **Mục đích**: Tự động định dạng code theo một bộ quy tắc nhất quán, giúp loại bỏ các tranh cãi về code style trong nhóm.
*   **Cấu hình**: File `.prettierrc.cjs` trong thư mục `frontend/`.
*   **Tích hợp**: Prettier thường được tích hợp với ESLint và chạy cùng với lệnh `npm run lint:fix`.

#### 3. Quy tắc chung

*   **Import Order**: Nhóm các import theo thứ tự: thư viện bên ngoài, alias của project (`@/`), import tương đối (`./`, `../`).

    ```typescript
    // Tốt
    import { ref } from 'vue';
    import { useFamilyStore } from '@/stores/family.store';
    import MyComponent from './MyComponent.vue';
    ```

*   **Props Order**: `defineProps` nên được định nghĩa ở đầu `<script setup>` để dễ dàng nhìn thấy các props của component.

*   **CSS**: Sử dụng `<style scoped>` cho các style chỉ ảnh hưởng đến component hiện tại. Các style toàn cục hoặc biến CSS nên được đặt trong `src/styles`.

*   **Vuetify**: Sử dụng các component và utility classes của Vuetify một cách nhất quán để đảm bảo giao diện đồng bộ.

*   **TypeScript**: Luôn sử dụng TypeScript để định nghĩa rõ ràng các kiểu dữ liệu, giúp phát hiện lỗi sớm và cải thiện khả năng bảo trì code.

#### 4. Cấu hình IDE

*   Nên cài đặt các extension ESLint và Prettier cho VS Code (hoặc IDE tương tự) và cấu hình chúng để tự động định dạng và sửa lỗi khi lưu file. Điều này giúp duy trì code style mà không cần chạy lệnh thủ công.

## 6. Bất đồng bộ

Các thao tác bất đồng bộ trong Frontend chủ yếu liên quan đến việc gọi API để tương tác với Backend. Việc xử lý các thao tác này cần được thực hiện một cách cẩn thận để đảm bảo trải nghiệm người dùng mượt mà và xử lý lỗi hiệu quả.

#### 1. Gọi API thông qua Services

*   Các yêu cầu API được đóng gói trong các service (ví dụ: `api.family.service.ts`, `api.member.service.ts`) trong thư mục `src/services`. Các service này sử dụng thư viện `axios` để gửi các HTTP request.
*   Kết quả từ API được trả về dưới dạng `Result Pattern` (như đã mô tả trong tài liệu Backend API) để xử lý thành công/thất bại một cách nhất quán.

    **Ví dụ (`api.family.service.ts`):**

    ```typescript
    // frontend/src/services/family/api.family.service.ts
    import axios from 'axios';
    import { Family, FamilyFilter } from '@/types/family/family';
    import { PaginatedList, Result } from '@/types/common/result';

    const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';

    export function useFamilyService() {
      const apiUrl = `${API_BASE_URL}/family`;

      const getAll = async (filter?: FamilyFilter): Promise<Result<PaginatedList<Family>>> => {
        try {
          const response = await axios.get<Result<PaginatedList<Family>>>(apiUrl, { params: filter });
          return response.data;
        } catch (error: any) {
          return Result.Fail(error.message || "Lỗi khi lấy danh sách dòng họ.");
        }
      };

      const create = async (family: Omit<Family, 'id'>): Promise<Result<string>> => {
        try {
          const response = await axios.post<Result<string>>(apiUrl, family);
          return response.data;
        } catch (error: any) {
          return Result.Fail(error.message || "Lỗi khi tạo dòng họ.");
        }
      };

      return { getAll, create };
    }
    ```

#### 2. Xử lý lỗi (Error Handling)

*   Sử dụng `try...catch` trong các `action` của Pinia store hoặc trong các component để bắt và xử lý lỗi từ API. Thông báo lỗi nên được hiển thị cho người dùng một cách thân thiện.
*   Các lỗi từ Backend API sẽ tuân theo `Result Pattern`, giúp dễ dàng kiểm tra `isSuccess` và lấy thông tin `error`.

    **Ví dụ (trong Pinia store action):**

    ```typescript
    // frontend/src/stores/family.store.ts
    async fetchFamilies() {
      this.loading = true;
      this.error = null;
      try {
        const result = await familyService.getAll();
        if (result.ok) {
          this.families = result.value.items;
        } else {
          this.error = result.error?.message || "Đã xảy ra lỗi khi lấy danh sách dòng họ."; // Hiển thị lỗi từ Backend
        }
      } catch (error: any) {
        this.error = "Lỗi kết nối API: " + error.message; // Lỗi mạng hoặc lỗi không xác định
        console.error("Failed to fetch families:", error);
      } finally {
        this.loading = false;
      },
    ```
    ```

#### 3. Vite Proxy

*   Trong môi trường phát triển cục bộ, Frontend sử dụng **Vite Proxy** để chuyển tiếp các yêu cầu API từ đường dẫn `/api` (ví dụ: `http://localhost:5173/api/family`) đến địa chỉ của Backend (ví dụ: `http://localhost:8080/family`).
*   Điều này giúp giải quyết vấn đề CORS (Cross-Origin Resource Sharing) trong quá trình phát triển, vì trình duyệt sẽ coi các yêu cầu đến `/api` là cùng một origin với Frontend.
*   Để biết thêm chi tiết về cấu hình Vite Proxy, vui lòng tham khảo phần [Vite Proxy trong Kiến trúc tổng quan](./architecture.md#vite-proxy-trong-môi-trường-phát-triển).

#### 4. Mocking

*   Trong môi trường development, các service có thể được mock để trả về dữ liệu giả lập từ `src/data`. Điều này hữu ích khi Backend chưa hoàn thiện hoặc khi cần phát triển Frontend độc lập.
*   Việc bật/tắt mocking được điều khiển bởi biến môi trường `VITE_USE_MOCK` (ví dụ: trong `.env.development`).

## 7. Mapping Dữ liệu từ API

Việc định nghĩa rõ ràng cấu trúc dữ liệu nhận được từ API là rất quan trọng để đảm bảo tính nhất quán, dễ bảo trì và tận dụng tối đa sức mạnh của TypeScript. Frontend sử dụng các **TypeScript interfaces** được định nghĩa trong thư mục `src/types` để mô tả cấu trúc của các đối tượng dữ liệu (ví dụ: `Family`, `Member`, `Event`).

#### Mục đích

*   **Type Safety**: Đảm bảo rằng dữ liệu được sử dụng trong Frontend khớp với cấu trúc dữ liệu từ Backend, giúp phát hiện lỗi sớm trong quá trình phát triển.
*   **Tự động hoàn thành (Autocompletion)**: Cung cấp gợi ý code và tự động hoàn thành trong IDE, tăng năng suất lập trình.
*   **Dễ đọc và hiểu**: Giúp các nhà phát triển khác dễ dàng hiểu cấu trúc của dữ liệu mà không cần phải xem qua code Backend.

#### Ví dụ về Member Interface

```typescript
// frontend/src/types/family/member.ts
export interface Member {
  id: string;
  lastName: string; // Họ
  firstName: string; // Tên
  fullName?: string; // Họ và tên đầy đủ (có thể được tính toán ở Backend)
  familyId: string;
  gender?: 'Male' | 'Female' | 'Other';
  dateOfBirth?: Date | null; // Ngày sinh (có thể là null)
  dateOfDeath?: Date | null; // Ngày mất (có thể là null)
  birthDeathYears?: string; // Chuỗi hiển thị năm sinh - năm mất
  avatarUrl?: string; // URL ảnh đại diện
  nickname?: string; // Biệt danh
  placeOfBirth?: string; // Nơi sinh
  placeOfDeath?: string; // Nơi mất
  occupation?: string; // Nghề nghiệp
  fatherId?: string | null; // ID của cha (có thể là null)
  motherId?: string | null; // ID của mẹ (có thể là null)
  spouseId?: string | null; // ID của vợ/chồng (có thể là null)
  biography?: string; // Tiểu sử
}
```

**Lưu ý:** Các trường `Date` từ Backend thường được trả về dưới dạng chuỗi ISO 8601 (ví dụ: `"2023-01-01T00:00:00Z"`). Trong Frontend, bạn có thể cần chuyển đổi chúng thành đối tượng `Date` của JavaScript nếu cần thao tác với ngày tháng.

## 8. Hướng dẫn Kiểm thử

Kiểm thử Frontend là rất quan trọng để đảm bảo giao diện người dùng hoạt động đúng như mong đợi và cung cấp trải nghiệm tốt. Dự án này sử dụng **Vitest** làm test runner và **Vue Test Utils** để kiểm thử các component Vue.

#### 1. Component Tests

*   **Mục đích**: Kiểm tra giao diện và tương tác của từng component Vue một cách độc lập. Đảm bảo component hiển thị đúng dữ liệu, phản ứng chính xác với các sự kiện của người dùng và tương tác đúng với các props/emits.
*   **Công cụ**: `Vitest` và `Vue Test Utils`.
*   **Vị trí**: Các file test thường nằm cùng thư mục với component hoặc trong thư mục `tests/unit/components`.
*   **Cách chạy**: 

    ```bash
    # Chạy tất cả component tests
    npm run test:unit --prefix frontend
    ```

#### 2. Store Tests (Pinia)

*   **Mục đích**: Kiểm tra logic của các Pinia store, bao gồm `state`, `getters`, và `actions`. Đảm bảo rằng store quản lý trạng thái đúng cách và các action thực hiện các thay đổi hoặc gọi API một cách chính xác.
*   **Công cụ**: `Vitest`.
*   **Vị trí**: Các file test thường nằm trong thư mục `tests/unit/stores`.
*   **Cách chạy**: 

    ```bash
    # Chạy tất cả store tests
    npm run test:unit --prefix frontend
    ```

#### 3. API Service Tests

*   **Mục đích**: Kiểm tra các service giao tiếp với API Backend. Trong các test này, API Backend thường được mock để đảm bảo rằng service gửi request đúng định dạng và xử lý phản hồi một cách chính xác, mà không cần Backend thực sự chạy.
*   **Công cụ**: `Vitest` và các thư viện mocking (ví dụ: `msw` - Mock Service Worker).
*   **Vị trí**: Các file test thường nằm trong thư mục `tests/unit/services`.
*   **Cách chạy**: 

    ```bash
    # Chạy tất cả service tests
    npm run test:unit --prefix frontend
    ```

#### 4. Test Coverage

*   **Mục đích**: Đo lường tỷ lệ phần trăm mã nguồn Frontend được thực thi bởi các bài kiểm thử. Test Coverage cao giúp tăng cường sự tự tin vào chất lượng mã nguồn Frontend.
*   **Cách tạo báo cáo**: 

    ```bash
    # Chạy test và tạo báo cáo coverage
    npm run test:coverage --prefix frontend
    ```

    Báo cáo coverage thường được tạo ra trong thư mục `frontend/coverage`.

*   **Ngưỡng Coverage**: Đặt mục tiêu coverage hợp lý (ví dụ: 80% cho logic quan trọng) nhưng không nên coi coverage là mục tiêu duy nhất. Chất lượng test quan trọng hơn số lượng.