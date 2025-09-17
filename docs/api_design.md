# Tài liệu Thiết kế API (API Design)

Tài liệu này mô tả các endpoint của Backend API, bao gồm định dạng request, response và các quy ước chung.

## 1. Tổng quan

-   **Base URL:** `/api`
-   **Authentication:** Sử dụng JWT Bearer Token trong header `Authorization`.
-   **Định dạng:** JSON
-   **Swagger UI:** Giao diện tài liệu API tương tác có tại `http://localhost:8080/swagger`.

## 2. Các Endpoint chính

### a. Quản lý Dòng họ (`/api/families`)

-   `GET /`: Lấy danh sách tất cả dòng họ.
-   `GET /{id}`: Lấy thông tin chi tiết một dòng họ.
-   `POST /`: Tạo một dòng họ mới.
-   `PUT /{id}`: Cập nhật thông tin một dòng họ.
-   `DELETE /{id}`: Xóa một dòng họ.

### b. Quản lý Thành viên (`/api/members`)

-   `GET /`: Lấy danh sách thành viên (hỗ trợ phân trang, lọc theo `familyId`).
-   `GET /{id}`: Lấy thông tin chi tiết một thành viên.
-   `POST /`: Tạo một thành viên mới.
-   `PUT /{id}`: Cập nhật thông tin một thành viên.
-   `DELETE /{id}`: Xóa một thành viên.

### c. Quản lý Quan hệ (`/api/relationships`)

-   `POST /`: Tạo một mối quan hệ mới giữa hai thành viên.
-   `DELETE /{id}`: Xóa một mối quan hệ.

### d. Cây Gia Phả (`/api/family-trees`)

-   `GET /{familyId}`: Lấy dữ liệu cây gia phả của một dòng họ dưới dạng JSON để frontend vẽ.

## 3. Ví dụ chi tiết

### Tạo thành viên mới

**Request:** `POST /api/members`

```json
{
  "familyId": "60c72b2f9b1d8c001f8e4a3c",
  "fullName": "Nguyễn Văn A",
  "gender": "Male",
  "dateOfBirth": "1990-01-15T00:00:00Z",
  "placeOfBirth": "Hà Nội",
  "biography": "Tiểu sử về cuộc đời và sự nghiệp..."
}
```

**Response:** `201 Created`

```json
{
  "id": "60c72b3a9b1d8c001f8e4a3d",
  "familyId": "60c72b2f9b1d8c001f8e4a3c",
  "fullName": "Nguyễn Văn A",
  "gender": "Male",
  "dateOfBirth": "1990-01-15T00:00:00Z",
  "placeOfBirth": "Hà Nội",
  "biography": "Tiểu sử về cuộc đời và sự nghiệp..."
}
```

---

*Lưu ý: Đây là thiết kế ban đầu, các DTOs và endpoint có thể được điều chỉnh trong quá trình phát triển.*