gemini generate ui --framework vue --library vuetify --task "
Thiết kế layout dashboard hiện đại.

### 1. Yêu cầu
- Tạo một layout dashboard hiện đại, thân thiện, và responsive.
- Layout bao gồm Sidebar (Navigation Drawer), Top App Bar, và Main Content.

### 2. Thiết kế
- **Sidebar (Navigation Drawer):**
  - Nền trắng, border-right mảnh.
  - Logo và tên ứng dụng ở đầu.
  - Menu items được nhóm theo chức năng.
  - Menu có icon và text.
- **Top App Bar:**
  - Nền trắng, elevation nhẹ.
  - Hộp tìm kiếm.
  - Nút chuyển đổi Dark/Light mode.
  - Nút thông báo có badge.
  - Avatar người dùng.
- **Main Content:**
  - Sử dụng `v-container fluid` với `v-row` và `v-col`.
  - Grid layout với các `v-card` bo góc.

### 3. Kỹ thuật chung
- **Component:** `DashboardLayout` (từ `@/layouts/dashboard`)
- **Framework:** Vue 3 + Composition API
- **UI Library:** Vuetify 3
- **Components con:**
  - `Sidebar` (từ `@/components/layout`)
  - `TopBar` (từ `@/components/layout`)
- **Theme:**
  - Primary: #696CFF
  - Secondary: #8592A3
  - Success: #71DD37
  - Error: #FF3E1D
  - Warning: #FFAB00

### Kết quả mong muốn
- Source code cho `DashboardLayout` và các component con.
- Layout dashboard hiển thị đúng và responsive."