# Thiết Kế API (API Design)

API được thiết kế theo chuẩn RESTful. Chi tiết sẽ được cập nhật qua Swagger/OpenAPI.

## 1. Authentication & Authorization
- `POST /api/auth/login`: Đăng nhập, trả về JWT Token.
- Các API khác yêu cầu JWT Token trong header `Authorization: Bearer <token>`.
- Phân quyền dựa trên vai trò (Admin, Member, Guest).

## 2. API Endpoints

### 2.1. Quản lý Dòng họ (Families)
- `GET /api/families`: Lấy danh sách tất cả dòng họ.
  - Response: `200 OK`, `List<FamilyDto>`
- `GET /api/families/{id}`: Lấy chi tiết một dòng họ theo ID.
  - Response: `200 OK`, `FamilyDto` hoặc `404 Not Found`
- `POST /api/families`: Tạo dòng họ mới.
  - Request: `CreateFamilyCommand`
  - Response: `201 Created`, `string` (ID của dòng họ mới)
- `PUT /api/families/{id}`: Cập nhật thông tin dòng họ.
  - Request: `UpdateFamilyCommand`
  - Response: `204 No Content` hoặc `404 Not Found`
- `DELETE /api/families/{id}`: Xóa một dòng họ.
  - Response: `204 No Content` hoặc `404 Not Found`

### 2.2. Quản lý Thành viên (Members)
- `GET /api/members`: Lấy danh sách tất cả thành viên (có thể hỗ trợ filter, search).
  - Response: `200 OK`, `List<MemberDto>`
- `GET /api/members/{id}`: Lấy chi tiết một thành viên theo ID.
  - Response: `200 OK`, `MemberDto` hoặc `404 Not Found`
- `POST /api/members`: Thêm thành viên mới.
  - Request: `CreateMemberCommand`
  - Response: `201 Created`, `string` (ID của thành viên mới)
- `PUT /api/members/{id}`: Cập nhật thông tin thành viên.
  - Request: `UpdateMemberCommand`
  - Response: `204 No Content` hoặc `404 Not Found`
- `DELETE /api/members/{id}`: Xóa một thành viên.
  - Response: `204 No Content` hoặc `404 Not Found`

### 2.3. Quản lý Quan hệ (Relationships)
- `POST /api/relationships`: Tạo quan hệ mới.
  - Request: `CreateRelationshipCommand`
  - Response: `201 Created`, `string` (ID của quan hệ mới)
- `DELETE /api/relationships/{id}`: Xóa một quan hệ.
  - Response: `204 No Content` hoặc `404 Not Found`

### 2.4. Cây Gia Phả (Family Tree)
- `GET /api/familytree/{familyId}`: Lấy dữ liệu cây gia phả dạng JSON.
  - Response: `200 OK`, `FamilyTreeDto`
- `GET /api/familytree/{familyId}/pdf`: Xuất cây gia phả dạng PDF.
  - Response: `200 OK`, `application/pdf` (file PDF)
  - **TODO**: Server-side PDF generation. Hiện tại trả về dummy content. Cần tích hợp thư viện như QuestPDF hoặc iTextSharp.

## 3. OpenAPI/Swagger UI
Truy cập Swagger UI tại: `http://localhost:8080/swagger`

## 4. Postman/Insomnia Collection
**TODO**: Tạo và cung cấp file Postman/Insomnia collection mẫu để dễ dàng kiểm thử các API.