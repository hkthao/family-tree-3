gemini generate ui --framework vue --library vuetify --task "
Thiết kế các màn hình quản lý thông tin gia đình (Family Management UI) với model:

Family {
  Name: string (bắt buộc),
  Description: string?,
  AvatarUrl: string?,
  Visibility: string (Private hoặc Public, mặc định Private)
}

1. **Màn hình danh sách (FamilySearch.vue)**  
   - Hiển thị bảng danh sách các gia đình: Avatar (nếu có), Name, Visibility.  
   - Có ô tìm kiếm theo Name.  
   - Có bộ lọc Visibility (Private/Public).  
   - Có phân trang (pagination).  
   - Khi tìm kiếm hoặc phân trang thì hiển thị loading spinner.  
   - Có nút **Thêm mới Family**.  
   - Mỗi dòng có action: Xem chi tiết, Chỉnh sửa, Xoá (xoá phải confirm).

2. **Màn hình chi tiết (FamilyDetail.vue)**  
   - Hiển thị đầy đủ thông tin một Family: Avatar, Name, Description, Visibility.  
   - Có nút quay lại danh sách.

3. **Form thêm / chỉnh sửa (FamilyForm.vue)**  
   - Input:  
     - Name (bắt buộc)  
     - Description (textarea)  
     - AvatarUrl (text field / upload mock)  
     - Visibility (select: Private, Public)  
   - Validate: Name không để trống.  
   - Có nút Lưu và Hủy.

4. **Dialog xác nhận xoá (ConfirmDeleteDialog.vue)**  
   - Text: “Bạn có chắc muốn xoá Family này không?”.  
   - Nút Xoá (màu đỏ), nút Hủy.

5. **Yêu cầu kỹ thuật:**  
   - Dùng Vue 3 + Composition API.  
   - UI sử dụng Vuetify (v-data-table, v-pagination, v-text-field, v-select, v-dialog, v-btn, v-avatar).  
   - Có quản lý state loading, confirm.  
   - Mock data cứng (không cần API thực).  
   - Code tách component rõ ràng, đặt trong thư mục `/components/family`.  
   - Chạy được bằng `npm run dev`.

Kết quả mong muốn: source code đầy đủ các file Vue trên, có thể chạy mock data để test trực tiếp UI."
