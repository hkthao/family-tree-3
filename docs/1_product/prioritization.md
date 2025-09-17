# Phân loại Ưu tiên Product Backlog

Tài liệu này xác định mức độ ưu tiên cho các user story trong backlog của dự án, sử dụng phương pháp MoSCoW để định hình lộ trình phát triển và xác định phạm vi cho các phiên bản sản phẩm.

## 1. Giới thiệu về Phương pháp MoSCoW

MoSCoW là một kỹ thuật quản lý ưu tiên giúp phân loại các yêu cầu thành bốn nhóm:

-   **Must-have (Bắt buộc có):** Các yêu cầu không thể thiếu để sản phẩm có thể hoạt động và được coi là thành công.
-   **Should-have (Nên có):** Các yêu cầu quan trọng nhưng không phải là cốt lõi. Sản phẩm vẫn hoạt động nếu thiếu chúng, nhưng giá trị sẽ giảm đi.
-   **Could-have (Có thể có):** Các yêu cầu "nice-to-have", ít quan trọng hơn và thường được xem xét khi còn thời gian và nguồn lực.
-   **Won't-have (Sẽ không có):** Các yêu cầu đã được thống nhất là sẽ không thực hiện trong phiên bản này.

Tài liệu này tập trung vào ba nhóm đầu để định hướng phát triển.

---

## 2. Danh sách User Story theo Mức độ Ưu tiên

### a. Must-have (Bắt buộc có cho MVP)

Đây là những tính năng cốt lõi nhất để tạo ra một Sản phẩm Khả thi Tối thiểu (MVP). Nếu thiếu bất kỳ tính năng nào trong nhóm này, sản phẩm sẽ không thể ra mắt.

-   **Tạo Dòng họ/Gia đình:** Chức năng khởi tạo không gian làm việc đầu tiên cho người dùng.
-   **Thêm thành viên:** Chức năng cốt lõi để bắt đầu xây dựng cây gia phả.
-   **Chỉnh sửa thành viên:** Cần thiết để người dùng sửa lỗi và cập nhật thông tin.
-   **Xem chi tiết thành viên:** Cho phép người dùng xem lại thông tin họ đã nhập.
-   **Quản lý mối quan hệ (cơ bản):** Thiết lập các mối quan hệ cốt lõi (Cha-Mẹ, Vợ-Chồng) để dựng cây.
-   **Xem cây gia phả (một kiểu hiển thị chuẩn):** Tính năng trực quan hóa chính, ban đầu chỉ cần một sơ đồ cây dọc, rõ ràng.
-   **Xác thực dữ liệu cơ bản:** Đảm bảo các trường quan trọng như `Họ tên thành viên` và `Tên dòng họ` không bị bỏ trống.

### b. Should-have (Nên có ngay sau MVP)

Đây là những tính năng quan trọng, giúp tăng cường đáng kể giá trị của sản phẩm và nâng cao trải nghiệm người dùng. Chúng nên được phát triển ngay sau khi MVP được phát hành và ổn định.

-   **Tích hợp xác thực Auth0 (Đăng nhập, Đăng ký):** Thay thế cho chức năng đăng nhập tự xây dựng, cho phép người dùng truy cập và bảo mật dữ liệu.
-   **Tìm kiếm thành viên:** Nhanh chóng tìm người trong cây gia phả.
-   **Xuất/Nhập cây gia phả (GEDCOM):** Thu hút người dùng từ các nền tảng khác và cho phép họ lưu trữ dữ liệu.
-   **Mời thành viên:** Bắt đầu cho phép tính năng cộng tác.
-   **Quản lý chế độ xem thông tin (Công khai/Riêng tư):** Cung cấp quyền kiểm soát riêng tư cho người dùng.
-   **Xem dòng thời gian sự kiện:** Cung cấp một góc nhìn trực quan và thú vị về lịch sử gia đình.
-   **Thay đổi nút gốc cây gia phả:** Tăng tính linh hoạt khi xem các nhánh khác nhau của cây.
-   **Đính kèm ghi chú/tài liệu:** Làm phong phú thêm dữ liệu của từng thành viên.
-   **Quản lý ảnh hồ sơ:** Tăng tính cá nhân hóa cho tài khoản và thành viên.

### c. Could-have (Có thể có trong tương lai)

Đây là những tính năng "nice-to-have", có thể làm sản phẩm trở nên khác biệt và hấp dẫn hơn nhưng không ảnh hưởng đến chức năng cốt lõi.

-   **Toàn bộ Epic "Tích hợp Trí tuệ Nhân tạo (AI)"**:
    -   Gợi ý tiểu sử bằng AI
    -   Nhận diện khuôn mặt
    -   Tìm kiếm bằng khuôn mặt
    -   Chatbot AI hỗ trợ
    -   AI kể chuyện (Storytelling)
-   **Báo cáo thống kê & Kỷ lục gia đình:** Cung cấp các phân tích sâu hơn về dữ liệu.
-   **Cộng tác chỉnh sửa trong thời gian thực:** Nâng cao trải nghiệm làm việc nhóm.
-   **Thông báo ngày giỗ/ngày kỷ niệm:** Một tính năng hữu ích để gắn kết người dùng.
-   **In poster gia phả / In 3D:** Các tùy chọn xuất nâng cao và độc đáo.
-   **Phát hiện trùng lặp thành viên:** Hữu ích cho các cây gia phả lớn và phức tạp.
-   **Hỗ trợ đa ngôn ngữ:** Mở rộng đối tượng người dùng quốc tế.