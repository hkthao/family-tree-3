# Tài liệu Thiết kế API (API Design)

Tài liệu này mô tả các endpoint của Backend API, bao gồm định dạng request, response và các quy ước chung.

## 1. Tổng quan

-   **Base URL:** `/api`
-   **Authentication:** Sử dụng JWT Bearer Token trong header `Authorization`.
-   **Định dạng:** JSON
-   **Swagger UI:** Giao diện tài liệu API tương tác có tại `http://localhost:8080/swagger`.

## 2. Authentication & Authorization (`/api/auth`)

-   `POST /login`: Đăng nhập, trả về JWT Token.
-   `POST /register`: Đăng ký tài khoản người dùng mới.

## 3. Các Endpoint chính

### a. Quản lý Dòng họ (`/api/families`)
- `GET /api/families`: Lấy danh sách tất cả dòng họ.
  - Response: `200 OK`, `List<FamilyDto>`
- `GET /api/families/{id}`: Lấy chi tiết một dòng họ theo ID.
  - Response: `200 OK`, `FamilyDto` hoặc `404 Not Found`
- `POST /api/families`: Tạo dòng họ mới.
  - Request: `CreateFamilyCommand { name, description, address, visibility }`
  - Response: `201 Created`, `string` (ID của dòng họ mới)
- `PUT /api/families/{id}`: Cập nhật thông tin dòng họ.
  - Request: `UpdateFamilyCommand { id, name, description, address, visibility }`
  - Response: `204 No Content` hoặc `404 Not Found`
- `DELETE /api/families/{id}`: Xóa một dòng họ.
  - Response: `204 No Content` hoặc `404 Not Found`

### b. Quản lý Thành viên (`/api/members`)
- `GET /api/members`: Lấy danh sách tất cả thành viên (có thể hỗ trợ filter, search).
  - Response: `200 OK`, `List<MemberDto>`
- `GET /api/members/{id}`: Lấy chi tiết một thành viên theo ID.
  - Response: `200 OK`, `MemberDto` hoặc `404 Not Found`
- `POST /api/members`: Thêm thành viên mới.
  - Request: `CreateMemberCommand { familyId, fullName, nicknames, gender, dob, placeOfBirth, ... }`
  - Response: `201 Created`, `string` (ID của thành viên mới)
- `PUT /api/members/{id}`: Cập nhật thông tin thành viên.
  - Request: `UpdateMemberCommand { id, fullName, nicknames, gender, ... }`
  - Response: `204 No Content` hoặc `404 Not Found`
- `DELETE /api/members/{id}`: Xóa một thành viên.
  - Response: `204 No Content` hoặc `404 Not Found`

### c. Quản lý Quan hệ (`/api/relationships`)
- `POST /api/relationships`: Tạo quan hệ mới.
  - Request: `CreateRelationshipCommand { sourceMemberId, targetMemberId, type, subType, startDate }`
  - Response: `201 Created`, `string` (ID của quan hệ mới)
- `GET /api/relationships/member/{memberId}`: Lấy tất cả các mối quan hệ của một thành viên.
  - Response: `200 OK`, `List<RelationshipDto>`
- `PUT /api/relationships/{id}`: Cập nhật thông tin một mối quan hệ (ví dụ: ngày cưới).
  - Request: `UpdateRelationshipCommand { id, startDate, endDate }`
  - Response: `204 No Content` hoặc `404 Not Found`
- `DELETE /api/relationships/{id}`: Xóa một quan hệ.
  - Response: `204 No Content` hoặc `404 Not Found`

### d. Cây Gia Phả (`/api/family-trees`)
- `GET /api/family-trees/{familyId}`: Lấy dữ liệu cây gia phả dạng JSON để frontend vẽ.
  - Response: `200 OK`, `FamilyTreeDto`

### e. Quản lý File đính kèm (`/api/members/{memberId}/attachments`)
- `POST /api/members/{memberId}/attachments`: Tải lên một file đính kèm cho thành viên.
  - Request: `multipart/form-data` chứa file.
  - Response: `201 Created`, `AttachmentDto { name, url, type }`
- `DELETE /api/members/{memberId}/attachments/{attachmentId}`: Xóa một file đính kèm.
  - Response: `204 No Content` hoặc `404 Not Found`

## 4. Ví dụ chi tiết

### Tạo thành viên mới

**Request:** `POST /api/members`

```json
{
  "familyId": "60c72b2f9b1d8c001f8e4a3c",
  "fullName": "Nguyễn Văn A",
  "nicknames": ["Tí"],
  "gender": "Male",
  "dob": "1990-01-15T00:00:00Z",
  "placeOfBirth": "Hà Nội",
  "biography": "Tiểu sử về cuộc đời và sự nghiệp...",
  "visibility": "FamilyOnly"
}
```