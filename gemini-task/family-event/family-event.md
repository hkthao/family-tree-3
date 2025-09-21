gemini generate ui --framework vue --library vuetify --task "
Thiết kế và triển khai các màn hình/quy trình quản lý sự kiện trong phần mềm FamilyTree.  

### 1. Danh sách sự kiện (Event List View)
- Hiển thị dạng bảng (`v-data-table-server`) hoặc timeline (dòng thời gian) hoặc chế độ xem theo lịch (Calendar View).
- Các cột chính:
  - 🗓️ Ngày
  - 📌 Tên sự kiện (đám cưới, sinh nhật, lễ giỗ, di cư, v.v.)
  - 👥 Thành viên liên quan (có avatar + tên, multi-chip)
  - 🏠 Địa điểm
  - ✏️ Actions (Xem, Sửa, Xóa).
- Có phân trang, tìm kiếm, lọc theo loại sự kiện.
- 👉 Trường hợp nhiều sự kiện, người dùng có thể switch view: Table <-> Timeline <-> Calendar.

### 2. Form thêm/chỉnh sửa sự kiện
- Biểu mẫu cần đơn giản nhưng đầy đủ:
  - Tên sự kiện (Text field).
  - Loại sự kiện (Select: Sinh, Cưới, Mất, Di cư, Khác...).
  - Gia đình/Dòng họ (autocomplete, bắt buộc).
  - Ngày bắt đầu – Ngày kết thúc (date picker, có option chỉ chọn năm nếu không rõ).
  - Địa điểm (text hoặc Google Maps autocomplete).
  - Mô tả/ghi chú (textarea).
  - Màu sắc (Color picker).
  - Thành viên liên quan (multi-select từ danh sách thành viên, tùy chọn → hiển thị chip avatar).
- 📌 UX chuyên nghiệp:
  - Các field bắt buộc có dấu *.
  - Group form theo 2 cột trên desktop, 1 cột trên mobile.
  - Dùng stepper nếu sự kiện phức tạp (ví dụ nhập nhiều người liên quan + upload ảnh).

### Kỹ thuật chung
- Vue 3 + Composition API.
- Vuetify 3: `v-app`, `v-dialog`, `v-data-table-server`, `v-form`, `v-text-field`, `v-select`, `v-autocomplete`, `DateInputField`, `v-avatar`, `v-card`, `v-tabs`, `v-timeline`, `v-timeline-item`, `v-pagination`, `v-color-picker`, `v-calendar`.
- Routing: `/events` (danh sách & tìm kiếm), `/events/add` (thêm mới), `/events/edit/:id` (chỉnh sửa). Xem chi tiết sự kiện được hiển thị trong dialog trên trang `/events`.
- Mock data mẫu trong `src/data/events.ts` (JSON array).
- Code chia component:
  - `EventList.vue`
  - `EventForm.vue`
  - `EventSearch.vue`
  - `EventTimeline.vue`

### Yêu cầu UI/UX
- Phong cách hiện đại, spacing thoáng, giống Google/IBM.
- Validation hiển thị gọn gàng, dễ hiểu.
- Loading state (skeleton loader) khi fetch dữ liệu.
- Snackbar hiển thị thông báo khi thêm/sửa/xóa thành công/thất bại.
- Responsive: tối ưu cho cả desktop & mobile.

### Kết quả mong muốn
- Source code Vue + Vuetify.
- Chạy được bằng `npm run dev`.
- Có mock data và đầy đủ flow thêm, chỉnh sửa, tìm kiếm, xem chi tiết sự kiện.
- Đã cấu hình i18n cho các component của Vuetify."