gemini generate ui --framework vue --library vuetify --task "
Thiết kế và triển khai các màn hình/quy trình quản lý thành viên trong phần mềm FamilyTree.  
Yêu cầu theo backlog: **Thêm thành viên, Chỉnh sửa thành viên, Tìm kiếm thành viên (mở rộng), Xem chi tiết thành viên**.  

### 1. Thêm thành viên
- Màn hình riêng `MemberForm.vue` (được dùng chung cho cả thêm mới và chỉnh sửa) để nhập thông tin:
  - Trường: Họ tên, Ngày sinh, Ngày mất (optional), Giới tính (select), Cha mẹ (autocomplete), Vợ/Chồng (autocomplete), Con cái (autocomplete).
  - Validation: Họ tên, Ngày sinh, Giới tính là bắt buộc.
- Nút 'Thêm thành viên' trên trang quản lý sẽ điều hướng đến màn hình thêm mới.
- Submit → thêm vào store/mock data và điều hướng về trang danh sách.
- UI: dùng `v-form`, `v-text-field`, `v-select`, `v-autocomplete`, `DateInputField`.

### 2. Chỉnh sửa thành viên
- Màn hình riêng `MemberForm.vue` (chế độ chỉnh sửa).
- Khi chọn 'Chỉnh sửa' từ bảng danh sách hoặc nút actions → điều hướng đến màn hình chỉnh sửa với dữ liệu prefilled.
- Cho phép cập nhật tất cả trường, validate như khi thêm mới.
- Sau khi lưu → dữ liệu cập nhật trong store và điều hướng về trang danh sách.

### 3. Tìm kiếm thành viên (Mở rộng)
- Màn hình `MemberSearch.vue`.
- Thanh tìm kiếm nâng cao (Advanced Search):
  - Các trường filter: Họ tên, Ngày sinh, Ngày mất, Nơi sinh, Nơi mất, Giới tính, Nghề nghiệp, Quan hệ.
  - Cho phép kết hợp nhiều filter cùng lúc.
- Kết quả:
  - Hiển thị dưới dạng bảng (`v-data-table`) + phân trang.
  - Hoặc highlight trên cây gia phả (mock UI → highlight icon/avatar).
- Thêm navigation giữa các kết quả (next/prev).

### 4. Xem chi tiết thành viên
- Component `MemberDetail.vue` (Dialog).
- Hiển thị thông tin đầy đủ:
  - Ảnh đại diện (có nút upload/change).
  - Họ tên, ngày sinh, ngày mất.
  - Nơi sinh, nơi mất, giới tính, nghề nghiệp.
  - Tiểu sử (textarea rich text).
- Action: nút 'Chỉnh sửa', 'Xóa', 'Đóng'.
- Layout card style hiện đại, giống sản phẩm Google/IBM.

### Kỹ thuật chung
- Vue 3 + Composition API.
- Vuetify 3: `v-app`, `v-dialog`, `v-data-table`, `v-form`, `v-text-field`, `v-select`, `v-avatar`, `v-card`, `DateInputField`.
- Routing: `/members` (danh sách & tìm kiếm), `/members/add` (thêm mới), `/members/edit/:id` (chỉnh sửa), `/members/:id` (chi tiết - nếu là route riêng).
- Mock data mẫu trong `src/data/members.ts` (JSON array).
- Code chia component:
  - `MemberForm.vue`
  - `MemberSearch.vue`
  - `MemberDetail.vue`
  - `MemberList.vue`

### Yêu cầu UI/UX
- Phong cách hiện đại, spacing thoáng, giống Google/IBM.
- Validation hiển thị gọn gàng, dễ hiểu.
- Loading state (skeleton loader) khi fetch dữ liệu.
- Snackbar hiển thị thông báo khi thêm/sửa/xóa thành công/thất bại.
- Responsive: tối ưu cho cả desktop & mobile.

### Kết quả mong muốn
- Source code Vue + Vuetify.
- Chạy được bằng `npm run dev`.
- Có mock data và đầy đủ flow thêm, chỉnh sửa, tìm kiếm, xem chi tiết thành viên."