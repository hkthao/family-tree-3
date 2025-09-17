Phần mềm quản lý gia phả
Chức năng chính
* Quản lý thông tin dòng họ / gia đình
    * Tên dòng họ/gia đình
    * Địa chỉ (quê quán, nơi ở chính)
    * Số lượng thành viên
    * Hình đại diện/logo
* Quản lý thông tin thành viên
    * Họ, tên đầy đủ
    * Ngày sinh, ngày mất
    * Trạng thái: còn sống / đã mất
    * Hình đại diện (avatar)
    * Thông tin liên lạc: email, số điện thoại
    * Mô tả về cuộc đời, sự nghiệp, thành tựu (có thể rich-text/markdown)
    * Thế hệ thứ mấy trong dòng họ
    * Vị trí trong gia đình (con thứ mấy)
    * Thành viên thuộc vào một dòng họ và một gia đình nhất định (có ràng buộc dữ liệu)
* Quản lý quan hệ giữa các thành viên
    * Cha – Mẹ
    * Vợ/Chồng (có thể nhiều nếu cho phép nhập lịch sử hôn nhân)
    * Con cái
* Hiển thị cây gia phả
    * Tự động dựng sơ đồ dựa trên thông tin thành viên và quan hệ
    * Thông tin hiển thị: hình đại diện, họ tên, năm sinh – năm mất, quan hệ với thành viên khác
    * Cho phép zoom, kéo thả, lọc theo thế hệ
    * Xuất cây gia phả ra ảnh/PDF

Công nghệ
* Backend: ASP.NET Core (RESTful API, JWT Authentication, Swagger)
* Database: MongoDB (ưu tiên thiết kế schema linh hoạt cho mối quan hệ)
* Frontend: Vue.js + Vuetify 3
* Triển khai: Docker, CI/CD (GitHub Actions/GitLab CI), có thể deploy lên Azure/AWS

Yêu cầu phát triển
* Tạo project chuẩn chuyên nghiệp như các team IBM/Google:
    * Clean Architecture, SOLID, Coding convention, CI/CD pipeline
    * Logging, error handling, validation, i18n (đa ngôn ngữ: tiếng Việt, tiếng Anh)
    * Unit test, integration test đầy đủ
    * Bảo mật: Authentication (JWT/OAuth2), Authorization (role-based)
* Tài liệu cần có:
    * Requirement Specification (chi tiết tính năng, use case)
    * System Design (kiến trúc hệ thống, database schema, sequence diagram, component diagram)
    * Component Design Document (thiết kế module, API contract)
    * User Guide (hướng dẫn sử dụng cho end-user)
    * Developer Guide (hướng dẫn setup môi trường, coding convention, CI/CD pipeline)
* Quản lý công việc:
    * Xây dựng đầy đủ Product backlog và User story
    * Áp dụng Kanban board (GitHub Project/Jira/Trello)
    * Có acceptance criteria rõ ràng cho từng story
    * Review định kỳ + retrospective meeting