gemini generate ui --framework vue --library vuetify --task "
Thiết kế và triển khai các màn hình/quy trình quản lý gia đình trong phần mềm FamilyTree.  
Yêu cầu theo backlog: **Thêm gia đình, Chỉnh sửa gia đình, Tìm kiếm gia đình, Xem chi tiết gia đình**.  

### 1. Quản lý Gia đình (FamilyListView.vue)
- Màn hình chính hiển thị danh sách các gia đình.
- Tích hợp `FamilySearch.vue` để tìm kiếm và lọc.
- Tích hợp `FamilyList.vue` để hiển thị bảng danh sách các gia đình: Avatar (nếu có), Name, Visibility.
- Có phân trang (pagination).
- Có nút 'Thêm mới Family' điều hướng đến màn hình thêm mới (`/family/add`).
- Mỗi dòng có action: Xem chi tiết (mở dialog), Chỉnh sửa (điều hướng đến màn hình chỉnh sửa), Xoá (xoá phải confirm).

### 2. Thêm Gia đình
- Màn hình riêng `FamilyAddView.vue` sử dụng `FamilyForm.vue`.
- `FamilyForm.vue` (được dùng chung cho cả thêm mới, chỉnh sửa và xem chi tiết) để nhập thông tin:
  - Trường: Name (bắt buộc), Description (textarea), Avatar URL (text field), Visibility (select: Private, Public).
  - Hiển thị ảnh đại diện (avatar) ngay trên form.
  - Validation: Name không để trống.
- Submit → thêm vào store/mock data và điều hướng về trang danh sách.

### 3. Chỉnh sửa Gia đình
- Màn hình riêng `FamilyEditView.vue` sử dụng `FamilyForm.vue`.
- Khi chọn 'Chỉnh sửa' từ bảng danh sách hoặc nút actions → điều hướng đến màn hình chỉnh sửa với dữ liệu prefilled (`/family/edit/:id`).
- Cho phép cập nhật tất cả trường, validate như khi thêm mới.
- Sau khi lưu → dữ liệu cập nhật trong store và điều hướng về trang danh sách.

### 4. Xem chi tiết Gia đình
- Dialog `FamilyForm.vue` (chế độ chỉ đọc).
- Hiển thị thông tin đầy đủ:
  - Ảnh đại diện.
  - Name, Description, Visibility.
- Action: nút 'Đóng'.
- Layout card style hiện đại, giống sản phẩm Google/IBM.

### Kỹ thuật chung
- Vue 3 + Composition API.
- Vuetify 3: `v-app`, `v-dialog`, `v-data-table`, `v-form`, `v-text-field`, `v-select`, `v-avatar`, `v-card`, `DateInputField`.
- Routing: `/family` (danh sách & tìm kiếm), `/family/add` (thêm mới), `/family/edit/:id` (chỉnh sửa). Xem chi tiết gia đình được hiển thị trong dialog trên trang `/family`.
- Mock data mẫu trong `src/data/families.ts` (JSON array).
- Code chia component:
  - `FamilyForm.vue`
  - `FamilySearch.vue`
  - `FamilyList.vue`
  - `FamilyListView.vue`
  - `FamilyAddView.vue`
  - `FamilyEditView.vue`

### Yêu cầu UI/UX
- Phong cách hiện đại, spacing thoáng, giống Google/IBM.
- Validation hiển thị gọn gàng, dễ hiểu.
- Loading state (skeleton loader) khi fetch dữ liệu.
- Snackbar hiển thị thông báo khi thêm/sửa/xóa thành công/thất bại.
- Responsive: tối ưu cho cả desktop & mobile.

### Kết quả mong muốn
- Source code Vue + Vuetify.
- Chạy được bằng `npm run dev`.
- Có mock data và đầy đủ flow thêm, chỉnh sửa, tìm kiếm, xem chi tiết gia đình."