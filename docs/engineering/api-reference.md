# Hướng dẫn API

## Mục lục

- [1. Tổng quan](#1-tổng-quan)
- [2. Xác thực (Authentication)](#2-xác-thực-authentication)
- [3. Phân trang (Pagination)](#3-phân-trang-pagination)
- [4. Lọc và Tìm kiếm](#4-lọc-và-tìm-kiếm)
- [5. Cấu trúc Phản hồi Lỗi (Error Response)](#5-cấu-trúc-phản-hồi-lỗi-error-response)
- [6. Các Endpoint chính](#6-các-endpoint-chính)
  - [6.1. Quản lý Dòng họ (`/api/families`)](#61-quản-lý-dòng-họ-apifamilies)
  - [6.2. Quản lý Thành viên (`/api/members`)](#62-quản-lý-thành-viên-apimembers)
- [7. Mô hình Dữ liệu (Response Models)](#7-mô-hình-dữ-liệu-response-models)
  - [7.1. Family](#71-family)
  - [7.2. Member](#72-member)

---

## 1. Tổng quan

- **Base URL**: `/api`
- **Định dạng**: JSON
- **Swagger UI**: Tài liệu tương tác có tại `http://localhost:8080/swagger`.

## 2. Xác thực (Authentication)

Hệ thống sử dụng **JWT Bearer Token** để xác thực các yêu cầu API. Cơ chế này được thiết kế để không phụ thuộc vào nhà cung cấp xác thực cụ thể (provider-agnostic).

### Luồng xác thực

1.  **Client lấy Token**: Client (ví dụ: Frontend app) chịu trách nhiệm lấy JWT từ một nhà cung cấp xác thực (ví dụ: Auth0, Keycloak, Firebase Auth).
2.  **Gửi Token trong Header**: Với mỗi yêu cầu đến các endpoint được bảo vệ, client phải gửi token trong header `Authorization`.

    ```http
    GET /api/families/some-family-id
    Host: localhost:8080
    Authorization: Bearer <YOUR_JWT_TOKEN>
    ```

## 3. Phân trang (Pagination)

Các endpoint trả về danh sách (ví dụ: `GET /api/families`, `GET /api/members`) đều hỗ trợ phân trang qua các query parameter sau:

-   `pageNumber` (int, optional, default: 1): Số trang muốn lấy.
-   `pageSize` (int, optional, default: 10): Số lượng mục trên mỗi trang.

**Ví dụ:**

```http
GET /api/families?pageNumber=2&pageSize=20
```

Phản hồi sẽ có cấu trúc `PaginatedList<T>`:

```json
{
  "items": [ ... ],
  "pageNumber": 2,
  "totalPages": 8,
  "totalCount": 150
}
```

## 4. Lọc và Tìm kiếm

Các endpoint danh sách hỗ trợ lọc và tìm kiếm qua query parameter. Ví dụ, `GET /api/members` có thể hỗ trợ:

-   `search`: Chuỗi ký tự để tìm kiếm theo tên, nghề nghiệp, v.v.
-   `gender`: Lọc theo giới tính.

**Ví dụ:**

```http
GET /api/members?search=Văn&gender=Male
```

## 5. Cấu trúc Phản hồi Lỗi (Error Response)

Khi có lỗi xảy ra, API sẽ trả về một response body chuẩn với cấu trúc sau, được bao gói trong `Result Pattern`:

```json
{
  "isSuccess": false,
  "value": null,
  "error": "string",
  "errorCode": number,
  "source": "string"
}
```

**Ví dụ lỗi:**

```json
{
  "isSuccess": false,
  "value": null,
  "error": "Family with ID {id} not found.",
  "errorCode": 404,
  "source": "FamilyService.GetFamilyByIdAsync"
}
```

## 6. Các Endpoint chính

### 6.1. Quản lý Dòng họ (`/api/family`)

-   `GET /api/family`: Lấy danh sách dòng họ (hỗ trợ [phân trang](#3-phân-trang-pagination)).
-   `GET /api/family/{id}`: Lấy thông tin dòng họ theo ID.
-   `GET /api/family?ids=id1,id2`: Lấy thông tin nhiều dòng họ theo danh sách ID.
-   `GET /api/family/search?keyword=...&page=...&itemsPerPage=...`: Tìm kiếm dòng họ theo từ khóa và hỗ trợ phân trang.
-   `POST /api/family`: Tạo dòng họ mới.
-   `PUT /api/family/{id}`: Cập nhật thông tin dòng họ.
-   `DELETE /api/family/{id}`: Xóa dòng họ.

### 6.2. Quản lý Thành viên (`/api/members`)

-   `GET /api/members`: Lấy danh sách thành viên (hỗ trợ [phân trang](#3-phân-trang-pagination) và [lọc](#4-lọc-và-tìm-kiếm)).
-   `GET /api/members/{id}`: Lấy thông tin thành viên theo ID.
-   `GET /api/members?ids=id1,id2`: Lấy thông tin nhiều thành viên theo danh sách ID.
-   `POST /api/members`: Thêm thành viên mới.
-   `PUT /api/members/{id}`: Cập nhật thông tin thành viên.
-   `DELETE /api/members/{id}`: Xóa thành viên.

### 6.3. Tìm kiếm chung (`/api/search`)

-   `GET /api/search?keyword=...`: Tìm kiếm chung trên cả dòng họ và thành viên theo từ khóa.

## 7. Mô hình Dữ liệu (Response Models)

### 7.1. Family

```json
{
  "id": "string (uuid)",
  "name": "string",
  "description": "string",
  "address": "string"
}
```

### 7.2. Member

```json
{
  "id": "string (uuid)",
  "familyId": "string (uuid)",
  "fullName": "string",
  "gender": "string (Male/Female/Other)",
  "dateOfBirth": "string (date-time)",
  "placeOfBirth": "string",
  "fatherId": "string (uuid, nullable)",
  "motherId": "string (uuid, nullable)",
  "spouseId": "string (uuid, nullable)",
  "childrenIds": "array of string (uuid)"
}
```
