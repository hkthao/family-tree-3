# Thiết lập Bảng Kanban trên GitHub Projects

Hướng dẫn này mô tả cách thiết lập các cột (columns) và nhãn (labels) cho bảng Kanban của dự án trên GitHub Projects. Việc chuẩn hóa này giúp cả nhóm dễ dàng theo dõi tiến độ, quản lý công việc và lập kế hoạch cho các sprint.

## 1. Cấu hình các Cột (Columns)

Các cột thể hiện các giai đoạn trong quy trình phát triển. Một quy trình Kanban cơ bản bao gồm các cột sau:

-   **Backlog**: Chứa toàn bộ user story và ý tưởng chưa được sắp xếp ưu tiên. Đây là nơi bắt đầu của mọi công việc.
-   **To Do (Sprint Backlog)**: Chứa các story đã được làm rõ và ưu tiên để thực hiện trong sprint hiện tại.
-   **In Progress**: Các story đang được developer tích cực phát triển. Một người chỉ nên có 1-2 story ở cột này để tập trung.
-   **In Review**: Story đã hoàn thành code và đang chờ được review (code review) hoặc kiểm thử (QA).
-   **Done**: Story đã hoàn thành, vượt qua tất cả các bài kiểm tra, được review và đáp ứng "Định nghĩa Hoàn thành" (Definition of Done).

## 2. Cấu hình các Nhãn (Labels)

Các nhãn giúp phân loại, lọc và nhanh chóng xác định bản chất của từng story.

### a. Nhãn theo Epic (Epic Labels)

Dùng để nhóm các user story liên quan đến một tính năng lớn.

-   `epic: core-data` (Màu đề xuất: `#1D76DB`) - Quản lý dữ liệu cốt lõi (thành viên, gia đình).
-   `epic: visualization` (Màu đề xuất: `#006B75`) - Trực quan hóa cây gia phả.
-   `epic: accounts` (Màu đề xuất: `#B60205`) - Quản lý tài khoản và phân quyền.
-   `epic: import-export` (Màu đề xuất: `#D93F0B`) - Nhập/Xuất dữ liệu và Báo cáo.
-   `epic: advanced` (Màu đề xuất: `#5319E7`) - Các tính năng nâng cao và cộng tác.
-   `epic: ai-integration` (Màu đề xuất: `#0E8A16`) - Tích hợp trí tuệ nhân tạo.

### b. Nhãn theo Mức độ Ưu tiên (Priority Labels)

Dùng để xác định thứ tự quan trọng của công việc, đặc biệt khi lập kế hoạch cho MVP.

-   `priority: must-have` (Màu đề xuất: `#B60205`) - Bắt buộc có cho MVP.
-   `priority: should-have` (Màu đề xuất: `#FBCA04`) - Rất quan trọng, nên làm sớm sau MVP.
-   `priority: nice-to-have` (Màu đề xuất: `#0E8A16`) - Có thì tốt nhưng có thể làm sau.

### c. Nhãn theo Loại Công việc (Type Labels)

Dùng để phân biệt bản chất của công việc.

-   `type: feature` (Màu đề xuất: `#1D76DB`) - Một tính năng mới cho người dùng.
-   `type: bug` (Màu đề xuất: `#D73A4A`) - Sửa lỗi trong chức năng hiện có.
-   `type: chore` (Màu đề xuất: `#CFD3D7`) - Công việc kỹ thuật không trực tiếp mang lại tính năng mới (refactor, nâng cấp thư viện).
-   `type: documentation` (Màu đề xuất: `#0075CA`) - Viết hoặc cập nhật tài liệu.

### d. Nhãn theo Trạng thái (Status Labels)

Dùng để cung cấp thêm thông tin về trạng thái của một story.

-   `status: blocked` (Màu đề xuất: `#000000`) - Bị chặn bởi một vấn đề khác.
-   `status: needs-info` (Màu đề xuất: `#F8E5B4`) - Cần thêm thông tin hoặc quyết định từ Product Owner/khách hàng.

## 3. Cách sử dụng

Mỗi issue (user story) trên bảng Kanban nên được gắn ít nhất:

-   1 nhãn `epic:*`
-   1 nhãn `priority:*`
-   1 nhãn `type:*`

Điều này giúp việc lọc, xem báo cáo và quản lý backlog trở nên dễ dàng và trực quan hơn.