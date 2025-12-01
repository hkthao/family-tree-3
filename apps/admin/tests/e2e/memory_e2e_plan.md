# Kế hoạch kiểm thử E2E cho tính năng Story Memory

## Mục tiêu
Đảm bảo luồng người dùng chính của tính năng Story Memory hoạt động đúng đắn trên giao diện người dùng và tích hợp chính xác với backend API.

## Các kịch bản kiểm thử

### Kịch bản 1: Tạo ký ức mới với phân tích ảnh
1.  **Đăng nhập** với tư cách người dùng hợp lệ.
2.  **Truy cập** trang chi tiết thành viên (`/members/:memberId`).
3.  **Nhấp** vào nút "AI Memorial Studio" hoặc "Thêm ký ức" liên quan đến thành viên.
4.  **Chọn** tùy chọn "Story Memory".
5.  **Tại bước "Chọn ảnh":**
    *   **Tải lên** một tệp ảnh hợp lệ (JPG/PNG).
    *   **Nhấp** vào nút "Phân tích ảnh".
    *   **Xác minh** rằng kết quả phân tích ảnh (tóm tắt, cảnh, đối tượng, người, ước tính năm) được hiển thị chính xác trong `PhotoAnalyzerPreview`.
6.  **Tại bước "Nhập văn bản thô":**
    *   **Nhấp** vào nút "Sử dụng ngữ cảnh này" từ `PhotoAnalyzerPreview`.
    *   **Xác minh** rằng văn bản thô được điền trước với tóm tắt từ phân tích ảnh.
    *   **Nhập** thêm văn bản mô tả ký ức (nếu cần).
    *   **Chọn** một phong cách câu chuyện từ dropdown.
    *   **Nhập** tiêu đề tùy chọn, năm, và thẻ.
    *   **Nhấp** vào nút "Tạo câu chuyện".
    *   **Xác minh** rằng khung tải hiển thị, sau đó câu chuyện được tạo bởi AI hiển thị trong `StoryEditor`.
7.  **Tại bước "Xem lại & Lưu":**
    *   **Chỉnh sửa** tiêu đề và/hoặc nội dung câu chuyện trong `StoryEditor` (nếu cần).
    *   **Nhấp** vào nút "Lưu".
    *   **Xác minh** rằng thông báo thành công hiển thị và ký ức được lưu thành công.
    *   **Xác minh** rằng trang chuyển hướng đến trang chi tiết ký ức hoặc trở lại danh sách.

### Kịch bản 2: Tạo ký ức mới chỉ với văn bản thô (bỏ qua phân tích ảnh)
1.  **Đăng nhập** với tư cách người dùng hợp lệ.
2.  **Truy cập** trang chi tiết thành viên.
3.  **Nhấp** vào nút "Thêm ký ức".
4.  **Chọn** tùy chọn "Story Memory".
5.  **Tại bước "Chọn ảnh":**
    *   **Nhấp** vào nút "Bỏ qua phân tích ảnh" hoặc "Đi thẳng đến văn bản".
6.  **Tại bước "Nhập văn bản thô":**
    *   **Nhập** văn bản mô tả ký ức (phải dài hơn 10 ký tự).
    *   **Chọn** một phong cách câu chuyện.
    *   **Nhấp** vào nút "Tạo câu chuyện".
    *   **Xác minh** rằng câu chuyện được tạo và hiển thị.
7.  **Tại bước "Xem lại & Lưu":**
    *   **Nhấp** vào nút "Lưu".
    *   **Xác minh** rằng ký ức được lưu thành công.

### Kịch bản 3: Xem chi tiết ký ức
1.  **Đăng nhập** với tư cách người dùng hợp lệ.
2.  **Truy cập** trang danh sách ký ức của thành viên.
3.  **Nhấp** vào nút "Xem" trên một ký ức cụ thể.
4.  **Xác minh** rằng trang chi tiết ký ức hiển thị đầy đủ thông tin (tiêu đề, câu chuyện, thẻ, từ khóa, ảnh, kết quả phân tích ảnh nếu có).

### Kịch bản 4: Chỉnh sửa ký ức
1.  **Đăng nhập** với tư cách người dùng hợp lệ.
2.  **Truy cập** trang chi tiết ký ức.
3.  **Nhấp** vào nút "Chỉnh sửa".
4.  **Thay đổi** tiêu đề và/hoặc nội dung câu chuyện trong `StoryEditor`.
5.  **Nhấp** vào nút "Lưu".
6.  **Xác minh** rằng thông báo thành công hiển thị và các thay đổi được phản ánh trên trang chi tiết ký ức.

### Kịch bản 5: Xóa ký ức
1.  **Đăng nhập** với tư cách người dùng hợp lệ.
2.  **Truy cập** trang chi tiết ký ức.
3.  **Nhấp** vào nút "Xóa".
4.  **Xác nhận** hành động xóa trong hộp thoại xác nhận.
5.  **Xác minh** rằng thông báo thành công hiển thị và ký ức không còn xuất hiện trong danh sách.

### Kịch bản 6: Xử lý lỗi
1.  **Kiểm thử** tải lên tệp ảnh không hợp lệ (kích thước quá lớn, định dạng sai) khi tạo ký ức.
2.  **Kiểm thử** tạo câu chuyện với văn bản thô quá ngắn hoặc thiếu thông tin cần thiết.
3.  **Xác minh** rằng thông báo lỗi phù hợp hiển thị cho người dùng.
4.  **Kiểm thử** các trường hợp không tìm thấy tài nguyên (ví dụ: cố gắng xem chi tiết ký ức không tồn tại).

## Yêu cầu bổ sung
*   **Hiển thị loading/spinner** khi các thao tác AI (phân tích ảnh, tạo câu chuyện) hoặc lưu dữ liệu đang diễn ra.
*   **Hiển thị thông báo Snackbar** cho các thao tác thành công hoặc thất bại.
*   **Kiểm tra Breadcrumbs** và tiêu đề trang phải chính xác.
*   **Kiểm tra ủy quyền (Authorization):** Đảm bảo chỉ người dùng được ủy quyền mới có thể tạo/xem/sửa/xóa ký ức cho thành viên của gia đình họ.
