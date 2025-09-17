# Ước tính User Story (Story Point Estimation)

Tài liệu này cung cấp các ước tính ban đầu cho các user story trong product backlog bằng cách sử dụng đơn vị **Story Point**.

## 1. Giới thiệu về Story Point

**Story Point** là một đơn vị đo lường tương đối được sử dụng trong Agile để ước tính nỗ lực cần thiết để hoàn thành một user story. Một Story Point phản ánh ba yếu tố:

-   **Độ phức tạp (Complexity):** Mức độ khó khăn của yêu cầu về mặt kỹ thuật.
-   **Khối lượng công việc (Volume):** Có bao nhiêu việc phải làm.
-   **Sự không chắc chắn (Uncertainty/Risk):** Mức độ mơ hồ hoặc rủi ro của yêu cầu.

Chúng tôi sử dụng thang đo **Fibonacci (1, 2, 3, 5, 8, 13, ...)** để thể hiện rằng sự không chắc chắn tăng lên theo cấp số nhân khi story trở nên lớn hơn.

**Lưu ý:** Đây là ước tính sơ bộ. Đội ngũ nên tổ chức một buổi **Planning Poker** để thống nhất về số điểm cuối cùng cho mỗi story.

---

## 2. Bảng Ước tính

### a. Must-have (Bắt buộc có cho MVP)

| User Story | Epic | Ước tính (Story Points) | Ghi chú (Rationale) |
| :--- | :--- | :--- | :--- |
| **Đăng nhập hệ thống** | `accounts` | **3** | Công việc tiêu chuẩn, bao gồm API, UI và quản lý token. |
| **Quản lý tài khoản và vai trò (cơ bản)** | `accounts` | **3** | Yêu cầu thiết kế schema cho người dùng/vai trò và API cơ bản. |
| **Tạo Dòng họ/Gia đình** | `core-data` | **2** | CRUD cơ bản, form đơn giản. |
| **Thêm thành viên** | `core-data` | **2** | CRUD cơ bản, form có nhiều trường hơn một chút. |
| **Chỉnh sửa thành viên** | `core-data` | **2** | Tương tự "Thêm thành viên", có thể tái sử dụng nhiều thành phần. |
| **Xem chi tiết thành viên** | `core-data` | **1** | Chỉ hiển thị dữ liệu đã có, ít logic phức tạp. |
| **Quản lý mối quan hệ (cơ bản)** | `core-data` | **5** | Phức tạp hơn CRUD thông thường. Cần xử lý logic liên kết và validation ở cả backend và frontend. |
| **Xem cây gia phả (một kiểu hiển thị chuẩn)** | `visualization` | **8** | Yêu cầu kỹ thuật cao nhất trong MVP. Cần nghiên cứu thư viện vẽ biểu đồ, xử lý layout và tương tác (zoom/pan). Độ không chắc chắn cao. |
| **Xác thực dữ liệu cơ bản** | `core-data` | **(0)** | Đây là một phần của "Definition of Done" và đã được tính vào các story CRUD (Thêm/Sửa). |
| **Tổng cộng cho MVP** | | **26** | |

### b. Should-have (Nên có ngay sau MVP)

| User Story | Epic | Ước tính (Story Points) | Ghi chú (Rationale) |
| :--- | :--- | :--- | :--- |
| **Tìm kiếm thành viên** | `visualization` | **2** | Tìm kiếm theo tên là tương đối đơn giản. Các tùy chọn mở rộng sẽ làm tăng điểm. |
| **Mời thành viên** | `accounts` | **3** | Yêu cầu tích hợp dịch vụ gửi email và quản lý token mời. |
| **Quản lý chế độ xem thông tin (Công khai/Riêng tư)** | `accounts` | **5** | Ảnh hưởng đến logic truy vấn ở nhiều nơi, cần kiểm thử cẩn thận để tránh lộ dữ liệu. |
| **Xem dòng thời gian sự kiện** | `visualization` | **5** | Yêu cầu xử lý dữ liệu và trình bày trên một giao diện mới. |
| **Xuất/Nhập cây gia phả (GEDCOM)** | `import-export` | **13** | Rất phức tạp. Định dạng GEDCOM có đặc tả lớn và nhiều biến thể. Cần nghiên cứu kỹ lưỡng hoặc tìm thư viện phù hợp. Đây có thể là một Epic riêng. |

---

## 3. Kế hoạch Sprint (Dự kiến)

Với tổng số **26 story points** cho MVP, chúng ta có thể dự kiến hoàn thành trong 2 sprint (mỗi sprint 2 tuần), giả sử vận tốc (velocity) của đội là khoảng **13-15 points/sprint**.

-   **Sprint 1 (Khoảng 13 points):** Tập trung vào nền tảng và nhập liệu.
    -   Đăng nhập (3), Quản lý tài khoản (3), Tạo Dòng họ (2), Thêm/Sửa/Xem thành viên (2+2+1 = 5).
-   **Sprint 2 (Khoảng 13 points):** Tập trung vào trực quan hóa và các tính năng cốt lõi còn lại.
    -   Quản lý mối quan hệ (5), Xem cây gia phả (8).