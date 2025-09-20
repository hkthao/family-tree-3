gemini generate ui --framework vue --library vuetify --task "
Thiết kế và triển khai Left Sidebar (Navigation Drawer).

### 1. Yêu cầu
- Tạo một Sidebar đầy đủ chức năng cho cả Admin và User.
- Sidebar phải responsive, có thể thu gọn, và hỗ trợ keyboard navigation.
- Quyền truy cập vào các menu item phải được kiểm soát dựa trên vai trò của người dùng.

### 2. Thiết kế
- **Layout:**
  - Nền trắng, border-right mảnh.
  - Logo và tên ứng dụng ở đầu.
  - Menu items được nhóm theo chức năng.
  - Menu có icon và text.
- **Tương tác:**
  - Hỗ trợ expand/collapse cho các nhóm menu.
  - Highlight menu item đang active.
  - Có ô tìm kiếm để lọc menu items.
  - Có thể thu gọn sidebar (mini-variant).
  - Hỗ trợ keyboard navigation.

### 3. Kỹ thuật chung
- **Component:** `Sidebar.vue`
- **Framework:** Vue 3 + Composition API
- **UI Library:** Vuetify 3
- **Dữ liệu menu:** `menuItems.ts`
- **Routing:** `sidebar-routes.ts`
- **Phân quyền:** `menu-permissions.ts`
- **i18n:** Sử dụng i18n keys cho tất cả các label.

### 4. Chi tiết Menu
- **Dashboards:**
  - Tổng quan
- **Gia phả:**
  - Xem cây gia phả
  - Thêm thành viên
  - Quản lý thành viên
  - Sự kiện gia đình (Timeline)
  - Thay đổi nút gốc cây
  - In / Xuất
- **Hồ sơ & Nội dung:**
  - Hồ sơ của tôi
  - Cập nhật thông tin
  - Ghi chú & Tài liệu
  - Ghi âm / Ký ức giọng nói
  - AI gợi ý tiểu sử
- **Quản trị (Admin):**
  - Quản lý người dùng
  - Quản lý vai trò & quyền
  - Nhật ký thay đổi (Audit log)
  - Mời thành viên
  - Multi-tree & Ghép nối
  - Phát hiện trùng lặp
- **Tiện ích & AI:**
  - Tìm kiếm thông minh
  - Tìm kiếm mối quan hệ
  - Nhận diện khuôn mặt (tagging)
  - Tìm kiếm bằng khuôn mặt
  - Chatbot AI
  - Cộng tác real-time
- **Văn hóa & Truyền thống:**
  - Truyền thống & lễ hội
  - Thông báo ngày giỗ/ kỷ niệm
- **Hệ thống:**
  - Cài đặt hệ thống
  - Cài đặt tài khoản
  - Xuất/Nhập dữ liệu
  - Báo cáo & Thống kê

### Kết quả mong muốn
- Source code cho `Sidebar.vue` và các file liên quan.
- Sidebar hiển thị đúng và hoạt động đầy đủ chức năng."