gemini generate ui --framework vue --library vuetify --task "
Thiết kế và triển khai các màn hình/quy trình quản lý sự kiện trong phần mềm FamilyTree.  

### 1. Danh sách sự kiện (Event List View)
- Hiển thị dạng bảng (`v-data-table-server`), dòng thời gian (Timeline View) hoặc chế độ xem theo lịch (Calendar View).
- Người dùng có thể chuyển đổi giữa các chế độ xem này (Table <-> Timeline <-> Calendar) thông qua các tab.
- Trong chế độ bảng, click vào tên sự kiện để xem chi tiết sự kiện trong dialog (chế độ chỉ đọc).
- Đối với chế độ dòng thời gian và lịch, bắt buộc phải chọn một gia đình để hiển thị dữ liệu.
- Chế độ xem lịch có thanh công cụ với các nút điều hướng (trước, sau, hôm nay) và lựa chọn chế độ xem (tháng, tuần, ngày, 4 ngày).

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
- Vuetify 3: `v-app`, `v-dialog`, `v-data-table-server`, `v-form`, `v-text-field`, `v-select`, `v-autocomplete`, `DateInputField`, `v-avatar`, `v-card`, `v-tabs`, `v-timeline`, `v-timeline-item`, `v-pagination`, `v-color-picker`, `v-calendar` (từ labs), `v-ripple` (directive).
- Các components và directives của Vuetify được đăng ký toàn cục trong `plugins/vuetify.ts`.
- Routing: `/events` (danh sách & tìm kiếm), `/events/add` (thêm mới), `/events/edit/:id` (chỉnh sửa). Xem chi tiết sự kiện được hiển thị trong dialog trên trang `/events`.
- Mock data mẫu trong `src/data/events.ts` (JSON array), với các gia đình 'Smith Family' (ID 1) và 'Johnson Family' (ID 2) có sẵn sự kiện để kiểm tra.
- Code chia component:
  - `EventList.vue`
  - `EventForm.vue`
  - `EventSearch.vue`
  - `EventTimeline.vue`
  - `EventCalendar.vue`

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