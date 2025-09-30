gemini generate ui --framework vue --library vuetify --task "
Thiết kế và triển khai các màn hình/quy trình quản lý thành viên trong phần mềm FamilyTree.  
Yêu cầu theo backlog: **Thêm thành viên, Chỉnh sửa thành viên, Tìm kiếm thành viên (mở rộng), Xem chi tiết thành viên, Dòng thời gian**.  

### 1. Thêm thành viên
- Màn hình riêng `MemberForm` (từ `@/components/members`) (được dùng chung cho cả thêm mới và chỉnh sửa) để nhập thông tin:
  - Trường: Họ tên, Biệt danh (optional), Ngày sinh, Ngày mất (optional), Giới tính (select), Gia đình/Dòng họ (autocomplete), Cha (autocomplete), Mẹ (autocomplete), Vợ/Chồng (autocomplete).
  - Validation: Họ tên, Ngày sinh, Giới tính, Gia đình/Dòng họ là bắt buộc. Ngày mất (nếu có) phải sau Ngày sinh.
- Nút 'Thêm thành viên' trên trang quản lý sẽ điều hướng đến màn hình thêm mới (`/members/add`).
- Submit → thêm vào store/mock data và điều hướng về trang danh sách.
- UI: dùng `v-form`, `v-text-field`, `GenderSelect` (từ `@/components/common`), `FamilyAutocomplete` (từ `@/components/common`), `v-autocomplete`, `DateInputField` (từ `@/components/common`).

### 2. Chỉnh sửa thành viên
- Màn hình riêng `MemberForm` (từ `@/components/members`) (chế độ chỉnh sửa).
- Khi chọn 'Chỉnh sửa' từ bảng danh sách hoặc nút actions → điều hướng đến màn hình chỉnh sửa với dữ liệu prefilled (`/members/edit/:id`).
- Cho phép cập nhật tất cả trường, validate như khi thêm mới.
- Sau khi lưu → dữ liệu cập nhật trong store và điều hướng về trang danh sách.

### 3. Tìm kiếm thành viên (Mở rộng)
- Màn hình `MemberSearch` (từ `@/components/members`).
- Thanh tìm kiếm nâng cao (Advanced Search):
  - Các trường filter: Họ tên, Ngày sinh, Ngày mất, Nơi sinh, Nơi mất, Giới tính, Nghề nghiệp, Gia đình/Dòng họ.
  - Cho phép kết hợp nhiều filter cùng lúc.
  - Các dropdown (Giới tính, Gia đình/Dòng họ) có chức năng tìm kiếm.
- Kết quả:
  - Hiển thị dưới dạng bảng (`v-data-table`) + phân trang.
  - Có cột 'Gia đình/Dòng họ' hiển thị tên gia đình của thành viên.
- Thêm navigation giữa các kết quả (next/prev).

### 4. Xem chi tiết thành viên
- Dialog `MemberForm` (từ `@/components/members`) (chế độ chỉ đọc).
- Hiển thị thông tin đầy đủ qua 2 tab: "Thông Tin Chung" và "Dòng Thời Gian".
- **Tab "Thông Tin Chung"**:
  - `AvatarInput` component (từ `@/components/common`).
  - Họ tên, biệt danh, ngày sinh, ngày mất.
  - Nơi sinh, nơi mất, giới tính, nghề nghiệp.
  - Gia đình/Dòng họ, Cha, Mẹ, Vợ/Chồng.
  - Tiểu sử (textarea rich text).
- **Tab "Dòng Thời Gian"**:
  - Hiển thị các sự kiện trong cuộc đời của thành viên dưới dạng timeline.
  - Có phân trang nếu có nhiều hơn 5 sự kiện.
  - Có các nút thêm, sửa, xóa sự kiện.
- Action: nút 'Đóng'.
- Layout card style hiện đại, giống sản phẩm Google/IBM.

### Kỹ thuật chung
- Vue 3 + Composition API.
- Vuetify 3: `v-app`, `v-dialog`, `v-data-table`, `v-form`, `v-text-field`, `v-autocomplete`, `DateInputField` (từ `@/components/common`), `v-avatar`, `v-card`, `GenderSelect` (từ `@/components/common`), `FamilyAutocomplete` (từ `@/components/common`), `v-tabs`, `v-timeline`, `v-timeline-item`, `v-pagination`.
- Routing: `/members` (danh sách & tìm kiếm), `/members/add` (thêm mới), `/members/edit/:id` (chỉnh sửa). Xem chi tiết thành viên được hiển thị trong dialog trên trang `/members`.
- Mock data mẫu trong `src/data/members.ts` (JSON array).
- Code chia component:
  - `MemberForm` (từ `@/components/members`)
  - `MemberSearch` (từ `@/components/members`)
  - `MemberList` (từ `@/components/members`)
  - `MemberListView` (từ `@/views/members`)
  - `MemberTimeline` (từ `@/components/events`)
  - `TimelineEventForm` (từ `@/components/events`)
  - `FamilyAutocomplete` (từ `@/components/common`)
  - `GenderSelect` (từ `@/components/common`)

### Yêu cầu UI/UX
- Phong cách hiện đại, spacing thoáng, giống Google/IBM.
- Validation hiển thị gọn gàng, dễ hiểu.
- Loading state (skeleton loader) khi fetch dữ liệu.
- Snackbar hiển thị thông báo khi thêm/sửa/xóa thành công/thất bại.
- Responsive: tối ưu cho cả desktop & mobile.
- Các dropdown cho Cha, Mẹ, Vợ/Chồng hiển thị thêm biệt danh, năm sinh, năm mất để dễ phân biệt.
- Dropdown Gia đình/Dòng họ hiển thị thêm địa chỉ để dễ phân biệt.

### Kết quả mong muốn
- Source code Vue + Vuetify.
- Chạy được bằng `npm run dev`.
- Có mock data và đầy đủ flow thêm, chỉnh sửa, tìm kiếm, xem chi tiết thành viên.
- Đã cấu hình i18n cho các component của Vuetify."