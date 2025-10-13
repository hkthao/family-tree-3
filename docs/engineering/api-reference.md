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
- [6.11. Xử lý Dữ liệu và Tải lên Chunk (`/api/chunk/upload`)](#611-xử-lý-dữ-liệu-và-tải-lên-chunk-apichunkupload)
- [7. Mô hình Dữ liệu (Response Models)](#7-mô-hình-dữ-liệu-response-models)
  - [7.1. Family](#71-family)
  - [7.2. Member](#72-member)
  - [7.5. Relationship](#75-relationship)
  - [7.6. RelationshipListDto](#76-relationshiplistdto)
  - [7.3. Event](#73-event)
  - [7.4. SearchResultsDto](#74-searchresultsdto)
  - [7.7. UserProfile](#77-userprofile)
  - [7.8. UserActivity](#78-useractivity)
  - [7.9. AIBiographyDto](#79-aibiographydto)
  - [7.10. BiographyResultDto](#710-biographyresultdto)
  - [7.11. AIProvider](#711-aiprovider)
  - [7.12. UserPreference](#712-userpreference)
  - [7.13. FileMetadata](#713-filemetadata)
  - [7.14. TextChunk](#714-textchunk)

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

### 2.1. Cấu hình JWT và Xác thực Backend

Backend được cấu hình để xác thực JWT dựa trên các thiết lập trong `JwtSettings` (được đọc từ `appsettings.json` hoặc `appsettings.Development.json`). Các thông số quan trọng bao gồm `Authority` (URL của nhà phát hành token) và `Audience` (đối tượng mà token được cấp cho).

Khi Backend nhận được một JWT, nó sẽ thực hiện các bước xác thực sau:

1.  **Xác minh chữ ký**: Đảm bảo token không bị giả mạo.
2.  **Kiểm tra thời hạn**: Đảm bảo token chưa hết hạn.
3.  **Kiểm tra `Issuer` (Authority)**: Đảm bảo token được phát hành bởi `Authority` đã cấu hình.
4.  **Kiểm tra `Audience`**: Đảm bảo token được cấp cho `Audience` đã cấu hình.
5.  **Xử lý Claims**: Sau khi xác thực thành công, các claims trong token sẽ được xử lý và ánh xạ thành `ClaimsPrincipal` của người dùng trong ứng dụng.

Để biết thêm chi tiết về cấu hình Backend, vui lòng tham khảo [Hướng dẫn Phát triển Backend](./backend-guide.md#7-xác-thực--phân-quyền).

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

## 3. Phân quyền (Authorization)

Hệ thống sử dụng cơ chế phân quyền chi tiết dựa trên vai trò của người dùng trong từng gia đình (Family-specific roles) và vai trò toàn cục (Global roles). Các vai trò này được quản lý thông qua `FamilyRole` enum và được kiểm tra bởi `IAuthorizationService` ở Backend để đảm bảo người dùng chỉ có thể thực hiện các hành động được phép.

### 3.1. Cơ chế RBAC

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

-   `GET /api/events/upcoming?familyId=...`: Lấy danh sách các sự kiện sắp tới (trong 30 ngày tới).
    *   **Phản hồi:** `Result<List<EventDto>>`

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

### 6.6. Quản lý Hồ sơ Người dùng (`/api/UserProfiles`)

-   `GET /api/UserProfiles/me`: Lấy thông tin hồ sơ của người dùng hiện tại.
    *   **Mô tả:** Endpoint này không yêu cầu ID người dùng trong URL; ID được lấy từ ngữ cảnh xác thực của server.
    *   **Phản hồi:** `Result<UserProfileDto>`
-   `GET /api/UserProfiles`: Lấy danh sách tất cả hồ sơ người dùng.
    *   **Phản hồi:** `Result<List<UserProfileDto>>`
-   `GET /api/UserProfiles/{id}`: Lấy thông tin hồ sơ người dùng theo ID nội bộ (GUID).
    *   **Phản hồi:** `Result<UserProfileDto>`
-   `GET /api/UserProfiles/byExternalId/{externalId}`: Lấy thông tin hồ sơ người dùng theo External ID (ví dụ: Auth0 User ID).
    *   **Phản hồi:** `Result<UserProfileDto>`

### 6.7. Quản lý Hoạt động Người dùng (`/api/activities`)

-   `GET /api/activities/recent?limit=...&targetType=...&targetId=...&familyId=...`: Lấy danh sách các hoạt động gần đây của người dùng.
    *   **Phản hồi:** `Result<List<UserActivityDto>>`

### 6.8. Quản lý AI (`/api/ai`)

-   `POST /api/ai/biography`: Sinh tiểu sử cho thành viên bằng AI.
    *   **Request Body:** `GenerateBiographyCommand` (ví dụ: `{ "memberId": "uuid", "style": "Emotional", "useDBData": "true", "userPrompt": "string (nullable)", "language": "string" }`)
    *   **Phản hồi:** `Result<BiographyResultDto>`
-   `GET /api/ai/biography/last/{memberId}`: Lấy tiểu sử AI gần nhất cho thành viên.
    *   **Phản hồi:** `Result<AIBiographyDto?>`
-   `POST /api/ai/biography/save`: Lưu tiểu sử AI đã tạo cho thành viên.
    *   **Request Body:** `SaveAIBiographyCommand` (ví dụ: `{ "memberId": "uuid", "style": "Emotional", "content": "string", "provider": "Gemini", "userPrompt": "string", "generatedFromDB": "boolean", "tokensUsed": "number" }`)
    *   **Phản hồi:** `Result<Guid>` (ID của tiểu sử AI vừa lưu)
-   `GET /api/ai/biography/providers`: Liệt kê các nhà cung cấp AI hiện có và trạng thái sử dụng.
    *   **Phản hồi:** `Result<List<AIProviderDto>>`

### 6.9. Quản lý Tùy chọn Người dùng (`/api/UserPreferences`)

-   `GET /api/UserPreferences`: Lấy tùy chọn của người dùng hiện tại.
    *   **Phản hồi:** `Result<UserPreferenceDto>`
-   `PUT /api/UserPreferences`: Cập nhật tùy chọn của người dùng hiện tại.
    *   **Request Body:** `SaveUserPreferencesCommand` (ví dụ: `{ "theme": "Light", "language": "Vietnamese", "emailNotificationsEnabled": "true", "smsNotificationsEnabled": "false", "inAppNotificationsEnabled": "true" }`)
    *   **Phản hồi:** `Result`

### 6.10. Quản lý Tải lên Tệp (`/api/upload`)

-   `POST /api/upload`: Tải lên một tệp (hình ảnh, tài liệu) lên hệ thống lưu trữ đã cấu hình.
    *   **Request Body:** `multipart/form-data` (chứa `IFormFile`)
    *   **Phản hồi:** `Result<string>` (URL công khai của tệp đã tải lên)
-   `GET /api/upload/preview/{fileName}`: Lấy nội dung của một tệp đã tải lên để xem trước. Yêu cầu xác thực.
    *   **Phản hồi:** `FileContentResult` (nội dung tệp với `Content-Type` phù hợp)

### 6.11. Xử lý Dữ liệu và Tải lên Chunk (`/api/chunk/upload`)

-   `POST /api/chunk/upload`: Tải lên một tệp (PDF hoặc TXT) để trích xuất văn bản, làm sạch và chia thành các chunk.
    *   **Request Body:** `multipart/form-data` (chứa `IFormFile file`, `string fileId`, `string familyId`, `string category`, `string createdBy`)
    *   **Phản hồi:** `Result<List<TextChunk>>`

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

### 7.7. UserProfile

```json
{
  "id": "string (uuid)",
  "externalId": "string",
  "email": "string",
  "name": "string",
  "avatar": "string (url, nullable)",
  "roles": [
    "string"
  ]
}
```

### 7.8. UserActivity

```json
{
  "id": "string (uuid)",
  "userProfileId": "string (uuid)",
  "actionType": "string (enum: Login, CreateFamily, UpdateMember, etc.)",
  "targetType": "string (enum: Family, Member, UserProfile, etc.)",
  "targetId": "string (uuid)",
  "metadata": "object (json, nullable)",
  "activitySummary": "string",
  "created": "string (date-time)"
}
```

### 7.9. AIBiographyDto

```json
{
  "id": "string (uuid)",
  "memberId": "string (uuid)",
  "style": "string (enum: Emotional, Historical, Storytelling, Formal, Informal)",
  "content": "string",
  "provider": "string (enum: Gemini, OpenAI, LocalAI)",
  "userPrompt": "string",
  "generatedFromDB": "boolean",
  "tokensUsed": "number",
  "metadata": "object (json, nullable)",
  "created": "string (date-time)"
}
```

### 7.10. BiographyResultDto

```json
{
  "content": "string",
  "provider": "string (enum: Gemini, OpenAI, LocalAI)",
  "tokensUsed": "number",
  "generatedAt": "string (date-time)",
  "userPrompt": "string"
}
```

### 7.11. AIProvider

```json
{
  "providerType": "string (enum: Gemini, OpenAI, LocalAI)",
  "name": "string",
  "isEnabled": "boolean",
  "dailyUsageLimit": "number",
  "currentDailyUsage": "number",
  "maxTokensPerRequest": "number"
}
```

### 7.12. UserPreference

```json
{
  "id": "string (uuid)",
  "userId": "string (uuid)",
  "theme": "string (enum: Light, Dark)",
  "language": "string (enum: English, Vietnamese)",
  "emailNotificationsEnabled": "boolean",
  "smsNotificationsEnabled": "boolean",
  "inAppNotificationsEnabled": "boolean"
}
```

### 7.13. FileMetadata

```json
{
  "id": "string (uuid)",
  "fileName": "string",
  "url": "string",
  "storageProvider": "string (enum: Local, Cloudinary, S3, AzureBlob)",
  "contentType": "string",
  "fileSize": "number",
  "uploadedBy": "string (uuid)",
  "usedByEntity": "string (nullable)",
  "usedById": "string (uuid, nullable)",
  "isActive": "boolean",
  "created": "string (date-time)",
  "lastModified": "string (date-time, nullable)"
}
```

### 7.14. TextChunk

```json
{
  "id": "string (uuid)",
  "content": "string",
  "metadata": { 
    "fileName": "string", 
    "fileId": "string", 
    "familyId": "string", 
    "page": "string", 
    "category": "string", 
    "createdBy": "string", 
    "createdAt": "string (date-time)" 
  }
}
```

```
