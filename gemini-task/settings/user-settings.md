## Task: User Settings Page

**Status:** Hoàn thành

Thiết kế và triển khai trang "Cài đặt người dùng" (User Settings Page) cho ứng dụng quản lý cây gia phả.

### 1. Yêu cầu
- Tạo một trang cài đặt người dùng với các tab điều hướng cho các phần khác nhau.

### 2. Layout
- Thiết kế responsive với điều hướng dạng sidebar cho các tab "Hồ sơ", "Tùy chọn", "Bảo mật".
- Khu vực nội dung chính thay đổi theo tab được chọn.

### 3. Tab Hồ sơ (Profile Tab)
- Biểu mẫu với các trường: Họ và tên, Email, Tải ảnh đại diện.
- Nút "Lưu" với xác thực biểu mẫu.

### 4. Tab Tùy chọn (Preferences Tab)
- Lựa chọn chủ đề (Sáng / Tối).
- Tùy chọn thông báo (checkboxes: Email, SMS, Trong ứng dụng).
- Lựa chọn ngôn ngữ (Tiếng Việt / Tiếng Anh).
- Nút "Lưu".

### 5. Tab Bảo mật (Security Tab)
- Biểu mẫu thay đổi mật khẩu: Mật khẩu hiện tại, Mật khẩu mới, Xác nhận mật khẩu.
- Nút "Lưu" với xác thực: mật khẩu khớp, độ dài tối thiểu 8 ký tự.

### 6. Chung
- Sử dụng các thành phần Vuetify.
- Cung cấp trạng thái reactive với Vue 3 `ref` hoặc `reactive`.
- Bao gồm phản hồi xác thực cơ bản (trường bắt buộc, mật khẩu không khớp).
- Giữ thiết kế sạch sẽ và đơn giản, tương tự như hướng dẫn kiểu Vuetify.
- Sử dụng các hình ảnh/biểu tượng có sẵn trong Vuetify (không cần tài sản tùy chỉnh).

### 7. Bonus
- Bao gồm thông báo snackbar sau khi lưu thay đổi.
- Cấu trúc code với các component cho mỗi tab.

### 8. Kỹ thuật chung
- **Component:** `UserSettingsPage.vue` (từ `@/views/settings`)
- **Thành phần con:**
  - `ProfileSettings.vue` (từ `@/components/settings`)
  - `PreferencesSettings.vue` (từ `@/components/settings`)
  - `SecuritySettings.vue` (từ `@/components/settings`)
- **Pinia Store:** `useUserSettingsStore.ts` (từ `@/stores`)
- **Routing:** `/settings`
- **i18n:** Sử dụng i18n keys cho tất cả các label.
