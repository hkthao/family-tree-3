# Hướng dẫn Frontend

- [Giới thiệu](#giới-thiệu)
- [Cấu trúc thư mục](#cấu-trúc-thư-mục)
- [Quản lý trạng thái với Pinia](#quản-lý-trạng-thái-với-pinia)
- [Coding Style](#coding-style)
- [Hướng dẫn Kiểm thử](#hướng-dẫn-kiểm-thử)

---



## Giới thiệu

Frontend của dự án được xây dựng bằng **Vue 3** với **TypeScript**, sử dụng **Vite** làm công cụ build và phát triển. Giao diện người dùng được xây dựng bằng **Vuetify 3** (một framework UI dựa trên Material Design), và quản lý trạng thái ứng dụng bằng **Pinia** (thư viện quản lý state nhẹ và mạnh mẽ cho Vue). **ESLint** và **Prettier** được sử dụng để đảm bảo chất lượng code và định dạng nhất quán.

## Cấu trúc thư mục

Cấu trúc thư mục của Frontend được tổ chức một cách rõ ràng để dễ dàng tìm kiếm và quản lý code:

```
frontend/
├── src/
│   ├── App.vue         # Component gốc của ứng dụng Vue.
│   ├── main.ts         # File khởi tạo ứng dụng Vue và các plugin.
│   ├── assets/         # Chứa các tài sản tĩnh như hình ảnh, icons, fonts, và các file CSS/SCSS toàn cục.
│   ├── components/     # Chứa các Vue component có thể tái sử dụng trên nhiều trang hoặc trong các component lớn hơn (ví dụ: `BaseButton.vue`, `FamilyCard.vue`).
│   ├── constants/      # Chứa các hằng số được sử dụng trong toàn bộ ứng dụng (ví dụ: `API_ENDPOINTS.ts`, `APP_CONFIG.ts`).
│   ├── data/           # Chứa dữ liệu mock được sử dụng trong môi trường phát triển hoặc cho các service mock, ví dụ: `menuItems.ts`.
│   ├── layouts/        # Chứa các layout chính của ứng dụng (ví dụ: `DefaultLayout.vue`, `AuthLayout.vue`).
│   ├── locales/        # Chứa các file dịch (i18n) cho các ngôn ngữ khác nhau.
│   ├── plugins/        # Chứa các plugin của Vue hoặc các thư viện bên thứ ba được khởi tạo (ví dụ: `vuetify.ts`, `pinia.ts`, `axios.ts`).
│   ├── router/         # Chứa cấu hình routing của Vue Router, định nghĩa các đường dẫn và component tương ứng.
│   ├── services/       # Chứa các service giao tiếp với Backend API hoặc các dịch vụ bên ngoài khác (ví dụ: `api.family.service.ts`, `api.ai.service.ts`). Các service này trả về kết quả theo `Result Pattern` để xử lý lỗi và thành công một cách nhất quán.
│   ├── stores/         # Chứa các Pinia store để quản lý trạng thái toàn cục của ứng dụng (ví dụ: `auth.store.ts`, `userSettings.store.ts`).
│   ├── styles/         # Chứa các style chung, biến CSS, hoặc các file SCSS/CSS được import toàn cục.
│   ├── types/          # Chứa định nghĩa các TypeScript type và interface cho dữ liệu (ví dụ: `Family.d.ts`, `UserPreference.d.ts`).
│   ├── utils/          # Chứa các hàm tiện ích (utility functions) không liên quan đến Vue component hoặc logic nghiệp vụ cụ thể (ví dụ: `dateUtils.ts`, `stringUtils.ts`).
│   └── views/          # Chứa các page components, mỗi component đại diện cho một trang hoặc một route chính của ứng dụng (ví dụ: `FamilyListView.vue`, `MemberDetailView.vue`).
├── tests/              # Chứa các file test cho Frontend (Unit Tests, Component Tests).
└── ...                 # Các file cấu hình khác (package.json, tsconfig.json, vite.config.ts, v.v.).
```

## Quản lý trạng thái với Pinia

**Pinia** là thư viện quản lý trạng thái (state management) được khuyến nghị cho các ứng dụng Vue 3. Nó cung cấp một cách tiếp cận đơn giản, mạnh mẽ và có thể mở rộng để quản lý trạng thái toàn cục của ứng dụng. Các store của Pinia được định nghĩa trong thư mục `src/stores`.

#### Cấu trúc Store

Mỗi store được định nghĩa bằng `defineStore()` và bao gồm:

*   **`state`**: Nơi lưu trữ dữ liệu trạng thái của store.
*   **`getters`**: Các hàm tính toán dữ liệu từ `state` (tương tự computed properties).
*   **`actions`**: Các phương thức để thay đổi `state` hoặc thực hiện các thao tác bất đồng bộ (ví dụ: gọi API).

#### Các quy ước khi sử dụng Pinia trong dự án

*   **Sử dụng Options API**: Các store nên được định nghĩa theo kiểu Options API của Pinia (sử dụng `state`, `getters`, `actions` làm thuộc tính của đối tượng truyền vào `defineStore`).
*   **Truy cập Service**: Các service nên được truy cập thông qua `this.services.[tên_service]` (ví dụ: `this.services.family.loadItems()`). Điều này được thực hiện thông qua `src/frontend/src/plugins/services.plugin.ts`.
*   **Xử lý lỗi và Loading**: Các hành động (actions) trong store nên cập nhật trạng thái `loading` và `error` một cách nhất quán để các component có thể hiển thị trạng thái tải hoặc thông báo lỗi cho người dùng.
*   **Dịch hóa thông báo lỗi**: Thông báo lỗi nên được dịch hóa bằng `i18n.global.t()` (ví dụ: `i18n.global.t('family.errors.load')`).
*   **`userSettingsStore`**: `userSettingsStore` (`frontend/src/stores/userSettings.store.ts`) là nguồn đáng tin cậy duy nhất cho các cài đặt và tùy chọn cá nhân của người dùng hiện tại (chủ đề, ngôn ngữ, cài đặt thông báo). Các component UI cần hiển thị hoặc thay đổi cài đặt người dùng nên lấy dữ liệu từ store này.

## Coding Style

Để đảm bảo code Frontend luôn sạch, dễ đọc và nhất quán, dự án sử dụng **ESLint** để phân tích mã tĩnh (static analysis) và **Prettier** để định dạng code tự động. Các công cụ này được tích hợp vào quy trình phát triển và có thể chạy tự động.

#### ESLint

*   **Mục đích**: Phát hiện các lỗi cú pháp, lỗi lập trình tiềm ẩn, và các vấn đề về code style không tuân thủ quy tắc đã định nghĩa.
*   **Cấu hình**: File `.eslintrc.cjs` trong thư mục `frontend/`.
*   **Cách chạy**: 

    ```bash
    # Kiểm tra lỗi linting
    npm run lint --prefix frontend
    
    # Tự động sửa các lỗi có thể sửa được
    npm run lint:fix --prefix frontend
    ```

#### Prettier

*   **Mục đích**: Tự động định dạng code theo một bộ quy tắc nhất quán, giúp loại bỏ các tranh cãi về code style trong nhóm.
*   **Cấu hình**: File `.prettierrc.cjs` trong thư mục `frontend/`.
*   **Tích hợp**: Prettier thường được tích hợp với ESLint và chạy cùng với lệnh `npm run lint:fix`.

#### Quy tắc chung

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

#### Cấu hình IDE

*   Nên cài đặt các extension ESLint và Prettier cho VS Code (hoặc IDE tương tự) và cấu hình chúng để tự động định dạng và sửa lỗi khi lưu file. Điều này giúp duy trì code style mà không cần chạy lệnh thủ công.






## Hướng dẫn Kiểm thử

Kiểm thử Frontend là rất quan trọng để đảm bảo giao diện người dùng hoạt động đúng như mong đợi và cung cấp trải nghiệm tốt. Dự án này sử dụng **Vitest** làm test runner và **Vue Test Utils** để kiểm thử các component Vue.

#### Component Tests

*   **Mục đích**: Kiểm tra giao diện và tương tác của từng component Vue một cách độc lập. Đảm bảo component hiển thị đúng dữ liệu, phản ứng chính xác với các sự kiện của người dùng và tương tác đúng với các props/emits.
*   **Công cụ**: `Vitest` và `Vue Test Utils`.
*   **Vị trí**: Các file test thường nằm cùng thư mục với component hoặc trong thư mục `tests/unit/components`.
*   **Cách chạy**: 

    ```bash
    # Chạy tất cả component tests
    npm run test:unit --prefix frontend
    ```

#### Store Tests (Pinia)

*   **Mục đích**: Kiểm tra logic của các Pinia store, bao gồm `state`, `getters`, và `actions`. Đảm bảo rằng store quản lý trạng thái đúng cách và các action thực hiện các thay đổi hoặc gọi API một cách chính xác.
*   **Công cụ**: `Vitest`.
*   **Vị trí**: Các file test thường nằm trong thư mục `tests/unit/stores`.
*   **Cách chạy**: 

    ```bash
    # Chạy tất cả store tests
    npm run test:unit --prefix frontend
    ```

#### Test Coverage

*   **Mục đích**: Đo lường tỷ lệ phần trăm mã nguồn Frontend được thực thi bởi các bài kiểm thử. Test Coverage cao giúp tăng cường sự tự tin vào chất lượng mã nguồn Frontend.
*   **Cách tạo báo cáo**: 

    ```bash
    # Chạy test và tạo báo cáo coverage
    npm run test:coverage --prefix frontend
    ```

    Báo cáo coverage thường được tạo ra trong thư mục `frontend/coverage`.

*   **Ngưỡng Coverage**: Đặt mục tiêu coverage hợp lý (ví dụ: 80% cho logic quan trọng) nhưng không nên coi coverage là mục tiêu duy nhất. Chất lượng test quan trọng hơn số lượng.