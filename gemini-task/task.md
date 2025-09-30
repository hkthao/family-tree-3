Tạo hệ thống Auth pluggable cho dự án Vue 3 + TypeScript + Pinia + Vuetify, kết hợp với các component sẵn có `LoginForm.vue` và `Register.vue`:

1. Tạo file `authService.ts`:
   - Interface `AuthService` với phương thức: `login(credentials)`, `logout()`, `register(data)`, `getUser(): Promise<AuthUser | null>`, `getAccessToken(): Promise<string | null>`.
   - Type `AuthUser` gồm: `id`, `name`, `email`, optional `avatar`, optional `roles` (array string).
   - Export hàm `setAuthService(service: AuthService)` và `useAuthService(): AuthService`.

2. Tạo file `auth0Service.ts`:
   - Implement `AuthService` dùng `@auth0/auth0-vue` SDK.
   - Các phương thức `login`, `logout`, `getUser`, `getAccessToken` map với Auth0 SDK.
   - Không tạo UI mới, chỉ service.

3. Tạo file `fakeAuthService.ts`:
   - Implement `AuthService` dùng mock data.
   - Trả về user mặc định (`roles: ['admin']`) cho development/testing.

4. Tạo Pinia store `useAuthStore.ts`:
   - State: `user: AuthUser | null`, `token: string | null`.
   - Actions: `login(credentials)`, `logout()`, `register(data)` gọi tương ứng từ `useAuthService()` và cập nhật state.
   - Store không quan tâm provider (Auth0/Fake).

5. Hướng dẫn tích hợp:
   - `LoginForm.vue` và `Register.vue` sẽ gọi store `useAuthStore().login()` hoặc `register()`.
   - Frontend không dính trực tiếp Auth0, chỉ dùng service interface.

6. Kiến trúc:
   - Có thể thay thế service bất kỳ (Auth0 ↔ Fake ↔ JWT) bằng `setAuthService()`.
   - Component UI chỉ tương tác store.
   - Dễ test, dev, và bảo trì.

7. Yêu cầu code:
   - Vue 3 + `<script setup lang="ts">`.
   - TypeScript đầy đủ type.
   - Comment giải thích rõ ràng.
