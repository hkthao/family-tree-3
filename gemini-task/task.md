update family-event.md các cấu trúc giống file member.md theo mô tả dưới đây, tham khảo thêm backlog.md 
Danh sách sự kiện (Event List View)
Hiển thị dạng bảng (v-data-table), dòng thời gian (Timeline View) hoặc chế độ xem theo lịch (Calendar View).
Các cột chính:
🗓️ Ngày
📌 Tên sự kiện (đám cưới, sinh nhật, lễ giỗ, di cư, v.v.)
👥 Thành viên liên quan (có avatar + tên, multi-chip)
🏠 Địa điểm
✏️ Actions (Xem, Sửa, Xóa).
Có phân trang, tìm kiếm, lọc theo loại sự kiện.
👉 Trường hợp nhiều sự kiện, người dùng có thể switch view: Table <-> Timeline <-> Calendar.


Form thêm/chỉnh sửa sự kiện
Biểu mẫu cần đơn giản nhưng đầy đủ:
Tên sự kiện (Text field).
Loại sự kiện (Select: Sinh, Cưới, Mất, Di cư, Khác...).
Ngày bắt đầu – Ngày kết thúc (date picker, có option chỉ chọn năm nếu không rõ).
Địa điểm (text hoặc Google Maps autocomplete).
Mô tả/ghi chú (textarea).
Thành viên liên quan (multi-select từ danh sách thành viên → hiển thị chip avatar).
📌 UX chuyên nghiệp:
Các field bắt buộc có dấu *.
Group form theo 2 cột trên desktop, 1 cột trên mobile.
Dùng stepper nếu sự kiện phức tạp (ví dụ nhập nhiều người liên quan + upload ảnh).
