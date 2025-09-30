gemini generate ui --framework vue --library vuetify --task "
Thiết kế và triển khai UserMenu (avatar dropdown).

### 1. Yêu cầu
- Tạo một component UserMenu hiển thị avatar người dùng và mở ra một menu khi click.
- Menu bao gồm thông tin người dùng, các link điều hướng, và nút đăng xuất.
- Component phải responsive và accessible.

### 2. Thiết kế
- **Activator:**
  - Avatar tròn (v-avatar) 36px.
  - Có chấm xanh online nếu người dùng đang online.
- **Popup (v-menu):**
  - **Header:** Avatar lớn, tên người dùng, vai trò (role).
  - **Menu items:** Profile, Settings, Pricing, FAQ. Mỗi item có icon.
  - **Divider và Logout:**
    - Có divider ngăn cách.
    - Nút Logout có icon và màu đỏ, yêu cầu xác nhận trước khi đăng xuất.
- **Responsive:**
  - Trên màn hình nhỏ, menu hiển thị dưới dạng bottom sheet.

### 3. Kỹ thuật chung
- **Component:** `UserMenu` (từ `@/components/layout`)
- **Framework:** Vue 3 + Composition API
- **UI Library:** Vuetify 3
- **Props:**
  - `currentUser`: Thông tin người dùng (tên, vai trò, avatar, online status).
  - `notificationsCount`: Số lượng thông báo.
- **Emits:**
  - `navigate`: Khi chọn một menu item.
  - `logout`: Khi xác nhận đăng xuất.
- **i18n:** Sử dụng i18n keys cho tất cả các label.

### Kết quả mong muốn
- Source code cho `UserMenu`.
- Component UserMenu hiển thị đúng và hoạt động đầy đủ chức năng."