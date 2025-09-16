# Thiết Kế API (API Design)

API được thiết kế theo chuẩn RESTful. Chi tiết sẽ được cập nhật qua Swagger/OpenAPI.

- `POST /api/auth/login`: Đăng nhập
- `GET /api/families`: Lấy danh sách dòng họ
- `POST /api/families`: Tạo dòng họ mới
- `GET /api/families/{id}`: Lấy chi tiết dòng họ
- `PUT /api/families/{id}`: Cập nhật dòng họ
- `DELETE /api/families/{id}`: Xóa dòng họ
- `GET /api/members`: Lấy danh sách thành viên (hỗ trợ filter, search)
- `POST /api/members`: Thêm thành viên mới
- `GET /api/members/{id}`: Lấy chi tiết thành viên
- `PUT /api/members/{id}`: Cập nhật thành viên
- `DELETE /api/members/{id}`: Xóa thành viên
- `POST /api/relationships`: Tạo quan hệ mới
- `DELETE /api/relationships/{id}`: Xóa quan hệ
- `GET /api/familytree/{familyId}`: Lấy dữ liệu cây gia phả (JSON)
- `GET /api/familytree/{familyId}/pdf`: Xuất cây gia phả (PDF)
