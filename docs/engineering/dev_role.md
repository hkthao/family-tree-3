# Vai trò của Nhà phát triển (Developer Role)

Tài liệu này phác thảo vai trò, trách nhiệm và kỳ vọng đối với một Nhà phát triển trong dự án "Cây Gia Phả".

## 1. Trách nhiệm Chính

*   **Phân tích Yêu cầu:** Hiểu rõ các yêu cầu nghiệp vụ và kỹ thuật, tham gia vào quá trình phân tích và thiết kế giải pháp.
*   **Phát triển Mã nguồn:** Viết mã nguồn chất lượng cao, tuân thủ các quy ước mã hóa (xem `code-convention.md`) và kiến trúc hệ thống (xem `architecture.md`).
*   **Kiểm thử Đơn vị & Tích hợp:** Viết và duy trì các bài kiểm thử đơn vị (unit tests) và kiểm thử tích hợp (integration tests) để đảm bảo chất lượng mã.
*   **Gỡ lỗi & Sửa lỗi:** Nhanh chóng xác định và sửa chữa các lỗi phát sinh trong quá trình phát triển hoặc được báo cáo từ các môi trường kiểm thử/sản phẩm.
*   **Đánh giá Mã (Code Review):** Tham gia vào quá trình đánh giá mã của đồng nghiệp, cung cấp phản hồi mang tính xây dựng và đảm bảo chất lượng mã tổng thể của đội.
*   **Tài liệu Hóa:** Đóng góp vào việc tạo và cập nhật tài liệu kỹ thuật, bao gồm kiến trúc, quy ước, tài liệu API và các hướng dẫn phát triển.
*   **Triển khai & Vận hành:** Hỗ trợ quá trình triển khai (deployment) và giám sát ứng dụng trong môi trường sản phẩm.
*   **Cải tiến Liên tục:** Tìm kiếm cơ hội để cải thiện hiệu suất, bảo mật, khả năng mở rộng và khả năng bảo trì của hệ thống.

## 2. Kỹ năng Chính

*   **Backend Developer:**
    *   Thành thạo C# và ASP.NET Core 8.
    *   Kinh nghiệm với Clean Architecture, DDD, CQRS, MediatR.
    *   Hiểu biết sâu về Entity Framework Core và MySQL.
    *   Kiến thức về bảo mật API (JWT Authentication).
    *   Có khả năng thiết kế và triển khai API RESTful hiệu quả.
*   **Frontend Developer:**
    *   Thành thạo TypeScript, JavaScript (ES6+).
    *   Kinh nghiệm với Vue.js 3 (Composition API), Pinia, Vue Router.
    *   Sử dụng thành thạo Vuetify 3 hoặc các framework UI tương tự.
    *   Kiến thức về tối ưu hóa hiệu suất frontend và trải nghiệm người dùng (UX).
*   **DevOps & Infrastructure (cơ bản):**
    *   Hiểu biết cơ bản về Docker và Docker Compose.
    *   Kiến thức về Git và GitHub Actions cho CI/CD.
*   **Kỹ năng mềm:**
    *   Khả năng giải quyết vấn đề.
    *   Kỹ năng giao tiếp và làm việc nhóm hiệu quả.
    *   Tư duy phản biện và chủ động.
    *   Ham học hỏi và thích nghi với công nghệ mới.

## 3. Quy trình Phát triển

Nhà phát triển sẽ tuân thủ quy trình phát triển như sau:

1.  **Lấy yêu cầu:** Nhận các User Story/Tasks từ công cụ quản lý dự án.
2.  **Thiết kế & Phân tích:** Phân tích yêu cầu, thiết kế giải pháp kỹ thuật, có thể thảo luận với các thành viên khác trong nhóm.
3.  **Phát triển:** Viết mã, bao gồm cả unit tests.
4.  **Kiểm thử nội bộ:** Chạy unit tests, integration tests cục bộ để đảm bảo chức năng cơ bản hoạt động đúng.
5.  **Tạo Pull Request (PR):** Gửi mã lên repository thông qua Pull Request, đảm bảo tuân thủ các quy ước Git (xem `code-convention.md`).
6.  **Code Review:** Tham gia vào quá trình code review cho PR của mình và của đồng nghiệp.
7.  **Sửa lỗi & Tích hợp:** Sửa các vấn đề phát hiện trong code review hoặc từ quá trình kiểm thử tự động (CI).
8.  **Triển khai:** Sau khi PR được chấp thuận và CI/CD thành công, mã sẽ được tích hợp và triển khai.

## 4. Công cụ Sử dụng

*   **IDE:** Visual Studio / Visual Studio Code
*   **Version Control:** Git, GitHub
*   **Containerization:** Docker Desktop
*   **Database Client:** MySQL Workbench / DBeaver
*   **API Testing:** Postman / Swagger UI
*   **Công cụ Build:** dotnet CLI, npm / yarn
*   **CI/CD:** GitHub Actions

## 5. Cộng tác

*   **Thường xuyên giao tiếp:** Sử dụng các công cụ giao tiếp của nhóm để cập nhật tiến độ, thảo luận về các vấn đề kỹ thuật và đưa ra giải pháp.
*   **Tham gia các cuộc họp:** Tham gia các cuộc họp Scrum (Daily Scrum, Sprint Planning, Sprint Review, Sprint Retrospective) để đồng bộ hóa và đóng góp vào kế hoạch chung của nhóm.
*   **Hỗ trợ lẫn nhau:** Giúp đỡ các thành viên khác trong nhóm khi họ gặp khó khăn.

---
**Lưu ý:** Vai trò này có thể phát triển và thay đổi để phù hợp với nhu cầu của dự án.
