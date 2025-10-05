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
- [6.3. Quản lý Sự kiện (`/api/events`)](#63-quản-lý-sự-kiện-apievents)
- [6.4. Tìm kiếm chung (`/api/search`)](#64-tìm-kiếm-chung-apisearch)
- [6.5. Quản lý Quan hệ (`/api/relationships`)](#65-quản-lý-quan-hệ-apirelationships)
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

### 🔄 Áp dụng xác thực trên Endpoint

*(Updated to match current refactor: Authentication enforcement)*

Các endpoint yêu cầu xác thực sẽ được đánh dấu bằng attribute `[Authorize]` trong các Controller hoặc trên từng action method. Điều này đảm bảo rằng chỉ những request có JWT hợp lệ mới có thể truy cập tài nguyên.

**Ví dụ:**

```csharp
// backend/src/Web/Controllers/FamilyController.cs

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FamilyController : ApiControllerBase
{
    // ... các action methods ...

    [HttpGet("{id}")]
    public async Task<ActionResult<Result<FamilyDto>>> GetFamilyById(Guid id)
    {
        return await Mediator.Send(new GetFamilyByIdQuery(id));
    }

    // ...
}
```

Trong ví dụ trên, toàn bộ `FamilyController` yêu cầu xác thực. Nếu một request không có hoặc có JWT không hợp lệ, API sẽ trả về lỗi `401 Unauthorized` hoặc `403 Forbidden`.

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

Các endpoint danh sách hỗ trợ lọc và tìm kiếm qua query parameter. Các tham số lọc cụ thể sẽ phụ thuộc vào từng tài nguyên (resource).

**Ví dụ với `GET /api/members`:**

-   `searchQuery`: Chuỗi ký tự để tìm kiếm theo tên, nghề nghiệp, v.v. (ví dụ: `searchQuery=Văn`)
-   `gender`: Lọc theo giới tính (ví dụ: `gender=Male`)
-   `familyId`: Lọc theo ID của dòng họ (ví dụ: `familyId=some-uuid`)

**Ví dụ:**

```http
GET /api/members?searchQuery=Văn&gender=Male&pageNumber=1&pageSize=10
```

**Ví dụ với `GET /api/family/search`:**

-   `keyword`: Từ khóa để tìm kiếm theo tên dòng họ, mô tả, v.v.

```http
GET /api/family/search?keyword=Royal&pageNumber=1&pageSize=5
```

## 5. Cấu trúc Phản hồi Lỗi (Error Response)

Khi có lỗi xảy ra hoặc một thao tác hoàn tất, API sẽ trả về một phản hồi chuẩn sử dụng **Result Pattern**. `Result Pattern` là một cách tiếp cận để xử lý kết quả của các thao tác (thành công hoặc thất bại) một cách nhất quán, tránh việc throw exceptions không cần thiết và làm rõ ràng luồng xử lý lỗi.

#### Mục đích của Result Pattern

*   **Minh bạch:** Rõ ràng chỉ ra một thao tác có thành công hay không.
*   **Thông tin lỗi chi tiết:** Cung cấp thông tin cụ thể về lỗi (thông báo, mã lỗi, nguồn gốc) mà không cần throw exception.
*   **Dễ kiểm soát:** Giúp client dễ dàng kiểm tra kết quả và xử lý các trường hợp thành công/thất bại.

#### Cấu trúc Phản hồi

Phản hồi sẽ có cấu trúc sau:

```json
{
  "isSuccess": boolean, // true nếu thao tác thành công, false nếu thất bại
  "value": any | null,  // Dữ liệu trả về nếu thành công, null nếu thất bại
  "error": string | null, // Thông báo lỗi nếu thất bại, null nếu thành công
  "errorCode": number | null, // Mã lỗi HTTP (ví dụ: 400, 404, 500) nếu thất bại, null nếu thành công
  "source": string | null // Nguồn gốc của lỗi (ví dụ: tên phương thức, class) để dễ debug
}
```

#### Ví dụ Phản hồi Thành công

```json
HTTP/1.1 200 OK
Content-Type: application/json

{
  "isSuccess": true,
  "value": {
    "id": "16905e2b-5654-4ed0-b118-bbdd028df6eb",
    "name": "Royal Family",
    "description": "The British Royal Family",
    "address": "Buckingham Palace"
  },
  "error": null,
  "errorCode": null,
  "source": null
}
```

#### Ví dụ Phản hồi Lỗi

```json
HTTP/1.1 404 Not Found
Content-Type: application/json

{
  "isSuccess": false,
  "value": null,
  "error": "Family with ID 'some-invalid-id' not found.",
  "errorCode": 404,
  "source": "FamilyService.GetFamilyByIdAsync"
}
```

**Giải thích các trường lỗi:**

*   `isSuccess`: Luôn là `false` khi có lỗi.
*   `value`: Luôn là `null` khi có lỗi.
*   `error`: Một chuỗi mô tả lỗi, thường là thông báo thân thiện với người dùng hoặc thông tin chi tiết cho nhà phát triển.
*   `errorCode`: Mã trạng thái HTTP tương ứng với lỗi (ví dụ: 400 Bad Request, 401 Unauthorized, 403 Forbidden, 404 Not Found, 500 Internal Server Error).
*   `source`: Cho biết nơi lỗi phát sinh trong mã nguồn (ví dụ: tên class và phương thức), rất hữu ích cho việc debug.

## 6. Các Endpoint chính

### 6.1. Quản lý Dòng họ (`/api/Family`) (updated after refactor)

-   `GET /api/Family`: Lấy danh sách dòng họ (hỗ trợ [phân trang](#3-phân-trang-pagination)).
    *   **Phản hồi:** `Result<PaginatedList<Family>>`
-   `GET /api/Family/{id}`: Lấy thông tin dòng họ theo ID.
    *   **Phản hồi:** `Result<Family>`
-   `GET /api/Family/by-ids?ids=id1,id2`: Lấy thông tin nhiều dòng họ theo danh sách ID.
    *   **Phản hồi:** `Result<List<Family>>`
-   `GET /api/Family/search?keyword=...&page=...&itemsPerPage=...`: Tìm kiếm dòng họ theo từ khóa và hỗ trợ phân trang.
    *   **Phản hồi:** `Result<PaginatedList<Family>>`
-   `POST /api/Family`: Tạo dòng họ mới.
    *   **Request Body:** `CreateFamilyCommand` (ví dụ: `{ "name": "Tên dòng họ", "description": "Mô tả" }`)
    *   **Phản hồi:** `Result<Guid>` (ID của dòng họ vừa tạo)
-   `PUT /api/Family/{id}`: Cập nhật thông tin dòng họ.
    *   **Request Body:** `UpdateFamilyCommand` (ví dụ: `{ "id": "uuid", "name": "Tên mới", "description": "Mô tả mới" }`)
    *   **Phản hồi:** `Result<bool>` (true nếu cập nhật thành công)
-   `DELETE /api/Family/{id}`: Xóa dòng họ.
    *   **Phản hồi:** `Result<bool>` (true nếu xóa thành công)

### 6.2. Quản lý Thành viên (`/api/members`)

-   `GET /api/members`: Lấy danh sách thành viên (hỗ trợ [phân trang](#3-phân-trang-pagination) và [lọc](#4-lọc-và-tìm-kiếm)).
    *   **Phản hồi:** `Result<PaginatedList<Member>>`
-   `GET /api/members/{id}`: Lấy thông tin thành viên theo ID.
    *   **Phản hồi:** `Result<Member>`
-   `GET /api/members?ids=id1,id2`: Lấy thông tin nhiều thành viên theo danh sách ID.
    *   **Phản hồi:** `Result<List<Member>>`
-   `POST /api/members`: Thêm thành viên mới.
    *   **Request Body:** `CreateMemberCommand` (ví dụ: `{ "firstName": "Tên", "lastName": "Họ", "familyId": "uuid" }`)
    *   **Phản hồi:** `Result<Guid>` (ID của thành viên vừa tạo)
-   `PUT /api/members/{id}`: Cập nhật thông tin thành viên.
    *   **Request Body:** `UpdateMemberCommand` (ví dụ: `{ "id": "uuid", "firstName": "Tên mới", "lastName": "Họ mới" }`)
    *   **Phản hồi:** `Result<bool>` (true nếu cập nhật thành công)
-   `DELETE /api/members/{id}`: Xóa thành viên.
    *   **Phản hồi:** `Result<bool>` (true nếu xóa thành công)

### 6.3. Quản lý Sự kiện (`/api/events`)

-   `GET /api/events`: Lấy danh sách sự kiện (hỗ trợ [phân trang](#3-phân-trang-pagination) và [lọc](#4-lọc-và-tìm-kiếm)).
    *   **Phản hồi:** `Result<PaginatedList<Event>>`
-   `GET /api/events/{id}`: Lấy thông tin sự kiện theo ID.
    *   **Phản hồi:** `Result<Event>`
-   `POST /api/events`: Tạo sự kiện mới.
    *   **Request Body:** `CreateEventCommand` (ví dụ: `{ "name": "Tên sự kiện", "startDate": "2023-01-01T00:00:00Z", "familyId": "uuid" }`)
    *   **Phản hồi:** `Result<Guid>` (ID của sự kiện vừa tạo)
-   `PUT /api/events/{id}`: Cập nhật thông tin sự kiện.
    *   **Request Body:** `UpdateEventCommand` (ví dụ: `{ "id": "uuid", "name": "Tên sự kiện mới" }`)
    *   **Phản hồi:** `Result<bool>` (true nếu cập nhật thành công)
-   `DELETE /api/events/{id}`: Xóa sự kiện.
    *   **Phản hồi:** `Result<bool>` (true nếu xóa thành công)

### 6.4. Tìm kiếm chung (`/api/search`)

-   `GET /api/search?keyword=...`: Tìm kiếm chung trên cả dòng họ và thành viên theo từ khóa.
    *   **Phản hồi:** `Result<SearchResultsDto>` (chứa danh sách Family và Member tìm được)

### 6.5. Quản lý Quan hệ (`/api/relationships`)

-   `GET /api/relationships`: Lấy danh sách quan hệ (hỗ trợ [phân trang](#3-phân-trang-pagination)).
    *   **Phản hồi:** `Result<PaginatedList<RelationshipListDto>>`
-   `GET /api/relationships/{id}`: Lấy thông tin quan hệ theo ID.
    *   **Phản hồi:** `Result<RelationshipDto>`
-   `GET /api/relationships/search?sourceMemberId=...&targetMemberId=...&type=...&page=...&itemsPerPage=...`: Tìm kiếm quan hệ theo các tiêu chí và hỗ trợ phân trang.
    *   **Phản hồi:** `Result<PaginatedList<RelationshipListDto>>`
-   `POST /api/relationships`: Tạo quan hệ mới.
    *   **Request Body:** `CreateRelationshipCommand` (ví dụ: `{ "sourceMemberId": "uuid", "targetMemberId": "uuid", "type": "Parent" }`)
    *   **Phản hồi:** `Result<Guid>` (ID của quan hệ vừa tạo)
-   `PUT /api/relationships/{id}`: Cập nhật thông tin quan hệ.
    *   **Request Body:** `UpdateRelationshipCommand` (ví dụ: `{ "id": "uuid", "sourceMemberId": "uuid", "targetMemberId": "uuid", "type": "Spouse" }`)
    *   **Phản hồi:** `Result<bool>` (true nếu cập nhật thành công)
-   `DELETE /api/relationships/{id}`: Xóa quan hệ.
    *   **Phản hồi:** `Result<bool>` (true nếu xóa thành công)

## 7. Mô hình Dữ liệu (Response Models)

### 7.1. Family

```json
{
  "id": "string (uuid)",
  "name": "string",
  "description": "string",
  "address": "string",
  "avatarUrl": "string (url, nullable)",
  "visibility": "string (Public/Private)",
  "totalMembers": "number"
}
```

### 7.2. Member

```json
{
  "id": "string (uuid)",
  "familyId": "string (uuid)",
  "firstName": "string",
  "lastName": "string",
  "fullName": "string",
  "gender": "string (Male/Female/Other)",
  "dateOfBirth": "string (date-time, nullable)",
  "dateOfDeath": "string (date-time, nullable)",
  "birthDeathYears": "string (nullable)",
  "avatarUrl": "string (url, nullable)",
  "nickname": "string (nullable)",
  "placeOfBirth": "string (nullable)",
  "placeOfDeath": "string (nullable)",
  "occupation": "string (nullable)",
  "fatherId": "string (uuid, nullable)",
  "motherId": "string (uuid, nullable)",
  "spouseId": "string (uuid, nullable)",
  "biography": "string (nullable)"
}
```

### 7.5. Relationship

```json
{
  "id": "string (uuid)",
  "sourceMemberId": "string (uuid)",
  "targetMemberId": "string (uuid)",
  "type": "string (Parent/Child/Spouse/Sibling)",
  "order": "number (nullable)"
}
```

### 7.6. RelationshipListDto

```json
{
  "id": "string (uuid)",
  "sourceMemberId": "string (uuid)",
  "targetMemberId": "string (uuid)",
  "type": "string (Parent/Child/Spouse/Sibling)",
  "order": "number (nullable)",
  "sourceMemberFullName": "string",
  "targetMemberFullName": "string"
}
```

### 7.3. Event

```json
{
  "id": "string (uuid)",
  "name": "string",
  "description": "string (nullable)",
  "startDate": "string (date-time)",
  "endDate": "string (date-time, nullable)",
  "location": "string (nullable)",
  "familyId": "string (uuid, nullable)",
  "type": "string (Birth, Marriage, Death, Other)",
  "color": "string (nullable)",
  "relatedMembers": "array of string (uuid)"
}
```

### 7.4. SearchResultsDto

```json
{
  "families": [
    // ... Family objects ...
  ],
  "members": [
    // ... Member objects ...
  ]
}
```
```
