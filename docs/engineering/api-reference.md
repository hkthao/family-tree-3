# Tham chiếu API

Tài liệu này mô tả chi tiết về các endpoint của API theo chuẩn OpenAPI.

## 1. Chuẩn OpenAPI

API của chúng ta tuân thủ chuẩn [OpenAPI 3.0](https://swagger.io/specification/).

Bạn có thể xem tài liệu API trực quan qua Swagger UI tại địa chỉ: `http://localhost:8080/swagger`

## 2. Cấu trúc Endpoint

Mỗi endpoint được mô tả theo cấu trúc sau:

-   **Path**: Đường dẫn của endpoint (ví dụ: `/api/families`).
-   **Method**: Phương thức HTTP (GET, POST, PUT, DELETE).
-   **Description**: Mô tả ngắn về chức năng của endpoint.
-   **Request**: Mô tả về request body, query parameters, và headers.
-   **Response**: Mô tả về các response có thể có, bao gồm cả success và error.
-   **Error Codes**: Danh sách các mã lỗi và ý nghĩa.

## 3. Các Endpoint chính

### a. Quản lý Dòng họ (`/api/families`)
- `GET /api/families`: Lấy danh sách tất cả dòng họ.
- `GET /api/families/{id}`: Lấy chi tiết một dòng họ theo ID.
- `POST /api/families`: Tạo dòng họ mới.
- `PUT /api/families/{id}`: Cập nhật thông tin dòng họ.
- `DELETE /api/families/{id}`: Xóa một dòng họ.

### b. Quản lý Thành viên (`/api/members`)
- `GET /api/members`: Lấy danh sách tất cả thành viên (có thể hỗ trợ filter, search).
- `GET /api/members/{id}`: Lấy chi tiết một thành viên theo ID.
- `POST /api/members`: Thêm thành viên mới.
- `PUT /api/members/{id}`: Cập nhật thông tin thành viên.
- `DELETE /api/members/{id}`: Xóa một thành viên.

### c. Quản lý Quan hệ (`/api/relationships`)
- `POST /api/relationships`: Tạo quan hệ mới.
- `GET /api/relationships/member/{memberId}`: Lấy tất cả các mối quan hệ của một thành viên.
- `PUT /api/relationships/{id}`: Cập nhật thông tin một mối quan hệ.
- `DELETE /api/relationships/{id}`: Xóa một quan hệ.

### d. Cây Gia Phả (`/api/family-trees`)
- `GET /api/family-trees/{familyId}`: Lấy dữ liệu cây gia phả dạng JSON để frontend vẽ.

### e. Quản lý File đính kèm (`/api/members/{memberId}/attachments`)
- `POST /api/members/{memberId}/attachments`: Tải lên một file đính kèm cho thành viên.
- `DELETE /api/members/{memberId}/attachments/{attachmentId}`: Xóa một file đính kèm.

## 4. Ví dụ chi tiết

### Tạo thành viên mới

**Request:** `POST /api/members`

```json
{
  "familyId": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
  "fullName": "Nguyễn Văn A",
  "gender": "Male",
  "dateOfBirth": "1990-01-15T00:00:00Z",
  "placeOfBirth": "Hà Nội"
}
```

**Response (201 Created)**:

```json
{
  "id": "m1b2c3d4-e5f6-7890-1234-567890abcdef"
}
```