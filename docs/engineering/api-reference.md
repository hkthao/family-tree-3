# Hướng dẫn API

## Mục lục

- [1. Tổng quan](#1-tổng-quan)
- [2. Xác thực (Authentication)](#2-xác-thực-authentication)
- [3. Phân trang (Pagination)](#3-phân-trang-pagination)
- [4. Lọc và Tìm kiếm](#4-lọc-và-tìm-kiếm)
- [5. Cấu trúc Phản hồi Lỗi (Error Response)](#5-cấu-trúc-phản-hồi-lỗi-error-response)
- [6. Các Endpoint chính](#6-các-endpoint-chính)
  - [6.1. Quản lý Dòng họ (`/api/family`)](#61-quản-lý-dòng-họ-apifamilies)
  - [6.2. Quản lý Thành viên (`/api/member`)](#62-quản-lý-thành-viên-apimembers)
  - [6.3. Quản lý Sự kiện (`/api/event`)](#63-quản-lý-sự-kiện-apievents)
  - [6.4. Tìm kiếm chung (`/api/search`)](#64-tìm-kiếm-chung-apisearch)
  - [6.5. Quản lý Quan hệ (`/api/relationship`)](#65-quản-lý-quan-hệ-apirelationships)
  - [6.6. Quản lý Hồ sơ Người dùng (`/api/user-profile`)](#66-quản-lý-hồ-sơ-người-dùng-apiuserprofiles)
  - [6.7. Quản lý Hoạt động Người dùng (`/api/activity`)](#67-quản-lý-hoạt-động-người-dùng-apiactivities)
  - [6.8. Quản lý AI (`/api/ai`)](#68-quản-lý-ai-api-ai)
  - [6.9. Quản lý Tùy chọn Người dùng (`/api/UserPreferences`)](#69-quản-lý-tùy-chọn-người-dùng-apiuserpreferences)
  - [6.10. Quản lý Tải lên Tệp (`/api/upload`)](#610-quản-lý-tải-lên-tệp-apiupload)
  - [6.11. Xử lý Dữ liệu và Tải lên Chunk (`/api/chunk/upload`)](#611-xử-lý-dữ-liệu-và-tải-lên-chunk-apichunkupload)
- [7. Mô hình Dữ liệu (Response Models)](#7-mô-hình-dữ-liệu-response-models)
  - [7.1. Family](#71-family)
  - [7.2. Member](#72-member)
  - [7.3. Event](#73-event)
  - [7.4. SearchResultsDto](#74-searchresultsdto)
  - [7.5. Relationship](#75-relationship)
  - [7.6. RelationshipListDto](#76-relationshiplistdto)
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

-   **Phương thức:** `GET`
-   **Đường dẫn:** `/api/family/some-family-id`
-   **Header:** `Authorization: Bearer <YOUR_JWT_TOKEN>` (thay `<YOUR_JWT_TOKEN>` bằng JWT hợp lệ của bạn)

### 2.1. Cấu hình JWT và Xác thực Backend

Backend được cấu hình để xác thực JWT dựa trên các thiết lập trong `JwtSettings` (được đọc từ tệp `src/backend/.env`). Các thông số quan trọng bao gồm `Authority` (URL của nhà phát hành token) và `Audience` (đối tượng mà token được cấp cho).

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

-   **Ví dụ:** Một `FamilyController` được đánh dấu `[Authorize]` để yêu cầu xác thực cho tất cả các hành động của nó. Nếu một yêu cầu không có hoặc có JWT không hợp lệ, API sẽ trả về lỗi `401 Unauthorized` hoặc `403 Forbidden`.

Trong ví dụ trên, toàn bộ `FamilyController` yêu cầu xác thực. Nếu một request không có hoặc có JWT không hợp lệ, API sẽ trả về lỗi `401 Unauthorized` hoặc `403 Forbidden`.

## 3. Phân trang (Pagination)

Các endpoint trả về danh sách (ví dụ: `GET /api/family`, `GET /api/member`) đều hỗ trợ phân trang qua các query parameter sau:

-   `pageNumber` (int, optional, default: 1): Số trang muốn lấy.
-   `pageSize` (int, optional, default: 10): Số lượng mục trên mỗi trang.

**Ví dụ:**

-   **Ví dụ:** Để lấy trang thứ 2 với 20 mục trên mỗi trang:
    -   **Phương thức:** `GET`
    -   **Đường dẫn:** `/api/family?pageNumber=2&pageSize=20`

Phản hồi sẽ có cấu trúc `PaginatedList<T>`:

Phản hồi sẽ có cấu trúc `PaginatedList<T>` với các trường:
-   `items`: Một mảng chứa các đối tượng dữ liệu của trang hiện tại.
-   `pageNumber`: Số trang hiện tại.
-   `totalPages`: Tổng số trang có sẵn.
-   `totalCount`: Tổng số mục trên tất cả các trang.

## 4. Lọc và Tìm kiếm

Các endpoint danh sách hỗ trợ lọc và tìm kiếm qua query parameter. Các tham số lọc cụ thể sẽ phụ thuộc vào từng tài nguyên (resource).

**Ví dụ với `GET /api/member`:**

-   `searchQuery`: Chuỗi ký tự để tìm kiếm theo tên, nghề nghiệp, v.v. (ví dụ: `searchQuery=Văn`)
-   `gender`: Lọc theo giới tính (ví dụ: `gender=Male`)
-   `familyId`: Lọc theo ID của dòng họ (ví dụ: `familyId=some-uuid`)

**Ví dụ:**

-   **Ví dụ:** Để tìm kiếm thành viên có tên "Văn" và giới tính "Male" trên trang 1 với 10 mục mỗi trang:
    -   **Phương thức:** `GET`
    -   **Đường dẫn:** `/api/member?searchQuery=Văn&gender=Male&pageNumber=1&pageSize=10`

**Ví dụ với `GET /api/family/search`:**

-   `keyword`: Từ khóa để tìm kiếm theo tên dòng họ, mô tả, v.v.

-   **Ví dụ:** Để tìm kiếm dòng họ có từ khóa "Royal" trên trang 1 với 5 mục mỗi trang:
    -   **Phương thức:** `GET`
    -   **Đường dẫn:** `/api/family/search?keyword=Royal&pageNumber=1&pageSize=5`

## 5. Cấu trúc Phản hồi Lỗi (Error Response)

Khi có lỗi xảy ra hoặc một thao tác hoàn tất, API sẽ trả về một phản hồi chuẩn sử dụng **Result Pattern**. `Result Pattern` là một cách tiếp cận để xử lý kết quả của các thao tác (thành công hoặc thất bại) một cách nhất quán, tránh việc throw exceptions không cần thiết và làm rõ ràng luồng xử lý lỗi.

#### Mục đích của Result Pattern

*   **Minh bạch:** Rõ ràng chỉ ra một thao tác có thành công hay không.
*   **Thông tin lỗi chi tiết:** Cung cấp thông tin cụ thể về lỗi (thông báo, mã lỗi, nguồn gốc) mà không cần throw exception.
*   **Dễ kiểm soát:** Giúp client dễ dàng kiểm tra kết quả và xử lý các trường hợp thành công/thất bại.

#### Cấu trúc Phản hồi

Phản hồi sẽ có cấu trúc sau:

Phản hồi sẽ có cấu trúc sau:
-   `isSuccess`: `boolean` - `true` nếu thao tác thành công, `false` nếu thất bại.
-   `value`: `any | null` - Dữ liệu trả về nếu thành công, `null` nếu thất bại.
-   `error`: `string | null` - Thông báo lỗi nếu thất bại, `null` nếu thành công.
-   `errorCode`: `number | null` - Mã lỗi HTTP (ví dụ: 400, 404, 500) nếu thất bại, `null` nếu thành công.
-   `source`: `string | null` - Nguồn gốc của lỗi (ví dụ: tên phương thức, class) để dễ debug.

#### Ví dụ Phản hồi Thành công

Ví dụ Phản hồi Thành công:
-   **HTTP Status:** `200 OK`
-   **Content-Type:** `application/json`
-   **Body:**
    -   `isSuccess`: `true`
    -   `value`:
        -   `id`: `"16905e2b-5654-4ed0-b118-bbdd028df6eb"`
        -   `name`: `"Royal Family"`
        -   `description`: `"The British Royal Family"`
        -   `address`: `"Buckingham Palace"`
    -   `error`: `null`
    -   `errorCode`: `null`
    -   `source`: `null`

#### Ví dụ Phản hồi Lỗi

Ví dụ Phản hồi Lỗi:
-   **HTTP Status:** `404 Not Found`
-   **Content-Type:** `application/json`
-   **Body:**
    -   `isSuccess`: `false`
    -   `value`: `null`
    -   `error`: `"Family with ID 'some-invalid-id' not found."`
    -   `errorCode`: `404`
    -   `source`: `"FamilyService.GetFamilyByIdAsync"`

**Giải thích các trường lỗi:**

*   `isSuccess`: Luôn là `false` khi có lỗi.
*   `value`: Luôn là `null` khi có lỗi.
*   `error`: Một chuỗi mô tả lỗi, thường là thông báo thân thiện với người dùng hoặc thông tin chi tiết cho nhà phát triển.
*   `errorCode`: Mã trạng thái HTTP tương ứng với lỗi (ví dụ: 400 Bad Request, 401 Unauthorized, 403 Forbidden, 404 Not Found, 500 Internal Server Error).
*   `source`: Cho biết nơi lỗi phát sinh trong mã nguồn (ví dụ: tên class và phương thức), rất hữu ích cho việc debug.

## 6. Các Endpoint chính

### 6.1. Quản lý Dòng họ (`/api/family`)

-   `GET /api/Family?ids=id1,id2,...`: Lấy thông tin nhiều dòng họ theo danh sách ID (comma-separated).
    *   **Phản hồi:** `Result<List<Family>>`
-   `GET /api/Family/{id}`: Lấy thông tin dòng họ theo ID.
    *   **Phản hồi:** `Result<Family>`
-   `GET /api/Family/by-ids?ids=id1,id2`: Lấy thông tin nhiều dòng họ theo danh sách ID.
    *   **Phản hồi:** `Result<List<Family>>`

-   `GET /api/Family/search?keyword=...&page=...&itemsPerPage=...`: Tìm kiếm dòng họ theo từ khóa và hỗ trợ phân trang.
    *   **Phản hồi:** `Result<PaginatedList<Family>>`
-   `POST /api/Family`: Tạo dòng họ mới.
    *   **Request Body:** `CreateFamilyCommand`
    -   `name`: `string` - Tên dòng họ (ví dụ: `"Tên dòng họ"`)
    -   `description`: `string` - Mô tả (ví dụ: `"Mô tả"`)
    *   **Phản hồi:** `Result<Guid>` (ID của dòng họ vừa tạo)
-   `POST /api/Family/bulk-create`: Tạo nhiều dòng họ mới cùng lúc.
    *   **Request Body:** `CreateFamiliesCommand` (một mảng các `CreateFamilyCommand`)
    *   **Phản hồi:** `Result<List<Guid>>` (Danh sách ID của các dòng họ vừa tạo)
-   `POST /api/Family/generate-family-data`: Tạo dữ liệu dòng họ mẫu.
    *   **Request Body:** `GenerateFamilyDataCommand`
    *   **Phản hồi:** `Result<List<Family>>`
-   `PUT /api/Family/{id}`: Cập nhật thông tin dòng họ.
    *   **Request Body:** `UpdateFamilyCommand`
    -   `id`: `string (uuid)` - ID của dòng họ (ví dụ: `"uuid"`)
    -   `name`: `string` - Tên mới (ví dụ: `"Tên mới"`)
    -   `description`: `string` - Mô tả mới (ví dụ: `"Mô tả mới"`)
    *   **Phản hồi:** `204 No Content` nếu thành công.
    *   **Phản hồi:** `204 No Content` nếu thành công.

### 6.2. Quản lý Thành viên (`/api/member`)


-   `GET /api/member/search?searchQuery=...&gender=...&familyId=...&pageNumber=...&pageSize=...`: Tìm kiếm thành viên theo các tiêu chí và hỗ trợ phân trang.
    *   **Phản hồi:** `PaginatedListOfMemberListDto`
-   `GET /api/member/{id}`: Lấy thông tin thành viên theo ID.
    *   **Phản hồi:** `MemberDetailDto`
-   `GET /api/member?ids=id1,id2`: Lấy thông tin nhiều thành viên theo danh sách ID.
    *   **Phản hồi:** `List<MemberListDto>`
-   `GET /api/member/managed`: Lấy danh sách các thành viên mà người dùng hiện tại có quyền chỉnh sửa.
    *   **Phản hồi:** `List<MemberListDto>`
-   `POST /api/member`: Thêm thành viên mới.
    *   **Request Body:** `CreateMemberCommand`
    -   `firstName`: `string` - Tên (ví dụ: `"Tên"`)
    -   `lastName`: `string` - Họ (ví dụ: `"Họ"`)
    -   `familyId`: `string (uuid)` - ID dòng họ (ví dụ: `"uuid"`)
    *   **Phản hồi:** `string (uuid)` (ID của thành viên vừa tạo)
-   `POST /api/member/generate-member-data`: Tạo dữ liệu thành viên mẫu.
    *   **Request Body:** `GenerateMemberDataCommand`
    *   **Phản hồi:** `List<MemberDto>`
-   `POST /api/member/bulk-create`: Tạo nhiều thành viên mới cùng lúc.
    *   **Request Body:** `CreateMembersCommand` (một mảng các `CreateMemberCommand`)
    *   **Phản hồi:** `List<string (uuid)>` (Danh sách ID của các thành viên vừa tạo)
-   `PUT /api/member/{id}`: Cập nhật thông tin thành viên.
    *   **Request Body:** `UpdateMemberCommand`
    -   `id`: `string (uuid)` - ID của thành viên (ví dụ: `"uuid"`)
    -   `firstName`: `string` - Tên mới (ví dụ: `"Tên mới"`)
    -   `lastName`: `string` - Họ mới (ví dụ: `"Họ mới"`)
    -   `familyId`: `string (uuid)` - ID dòng họ (ví dụ: `"uuid"`)
    *   **Phản hồi:** `204 No Content` nếu thành công.
-   `DELETE /api/member/{id}`: Xóa thành viên.
    *   **Phản hồi:** `204 No Content` nếu thành công.
-   `PUT /api/member/{id}/biography`: Cập nhật tiểu sử của thành viên.
    *   **Request Body:** `UpdateMemberBiographyCommand`
    -   `memberId`: `string (uuid)` - ID của thành viên (ví dụ: `"uuid"`)
    -   `biography`: `string` - Tiểu sử mới (ví dụ: `"Tiểu sử mới"`)
    *   **Phản hồi:** `204 No Content` nếu cập nhật thành công.

### 6.3. Quản lý Sự kiện (`/api/event`)

-   `GET /api/event?pageNumber=...&pageSize=...&searchTerm=...&eventType=...&familyId=...&startDate=...&endDate=...&location=...&relatedMemberId=...`: Lấy danh sách sự kiện (hỗ trợ phân trang và lọc).
    *   **Phản hồi:** `List<EventDto>`
-   `GET /api/event/{id}`: Lấy thông tin sự kiện theo ID.
    *   **Phản hồi:** `EventDto`
-   `GET /api/event/search?searchQuery=...&startDate=...&endDate=...&type=...&familyId=...&memberId=...&pageNumber=...&pageSize=...`: Tìm kiếm sự kiện theo các tiêu chí và hỗ trợ phân trang.
    *   **Phản hồi:** `PaginatedListOfEventDto`

-   `POST /api/event`: Tạo sự kiện mới.
    *   **Request Body:** `CreateEventCommand`
    -   `name`: `string` - Tên sự kiện (ví dụ: `"Tên sự kiện"`)
    -   `startDate`: `string (date-time)` - Ngày bắt đầu (ví dụ: `"2023-01-01T00:00:00Z"`)
    -   `familyId`: `string (uuid)` - ID dòng họ (ví dụ: `"uuid"`)
    *   **Phản hồi:** `string (uuid)` (ID của sự kiện vừa tạo)
-   `POST /api/event/generate-event-data`: Tạo dữ liệu sự kiện mẫu từ mô tả ngôn ngữ tự nhiên.
    *   **Request Body:** `GenerateEventDataCommand2`
    -   `prompt`: `string` - Mô tả sự kiện bằng ngôn ngữ tự nhiên (ví dụ: `"Tạo một sự kiện sinh nhật cho Nguyễn Văn A vào ngày 1/1/2000"`)
    *   **Phản hồi:** `List<AIEventDto>`
-   `POST /api/event/bulk-create`: Tạo nhiều sự kiện mới cùng lúc.
    *   **Request Body:** `CreateEventsCommand` (một mảng các `CreateEventCommand`)
    *   **Phản hồi:** `List<string (uuid)>` (Danh sách ID của các sự kiện vừa tạo)
-   `PUT /api/event/{id}`: Cập nhật thông tin sự kiện.
    *   **Request Body:** `UpdateEventCommand`
    -   `id`: `string (uuid)` - ID của sự kiện (ví dụ: `"uuid"`)
    -   `name`: `string` - Tên sự kiện mới (ví dụ: `"Tên sự kiện mới"`)
    -   `startDate`: `string (date-time)` - Ngày bắt đầu (ví dụ: `"2023-01-01T00:00:00Z"`)
    *   **Phản hồi:** `204 No Content` nếu thành công.
-   `DELETE /api/event/{id}`: Xóa sự kiện.
    *   **Phản hồi:** `204 No Content` nếu thành công.

-   `GET /api/event/upcoming?familyId=...`: Lấy danh sách các sự kiện sắp tới (trong 30 ngày tới).
    *   **Phản hồi:** `List<EventDto>`

### 6.4. Tìm kiếm chung (`/api/search`)

-   `GET /api/search?keyword=...&page=...&itemsPerPage=...&sortBy=...&sortOrder=...`: Tìm kiếm chung trên cả dòng họ và thành viên theo từ khóa, hỗ trợ phân trang và sắp xếp.
    *   **Phản hồi:** `PaginatedListOfSearchResultDto` (chứa danh sách Family và Member tìm được)

### 6.5. Quản lý Quan hệ (`/api/relationship`)

-   `GET /api/relationship?pageNumber=...&pageSize=...&familyId=...&sourceMemberId=...&targetMemberId=...&type=...`: Lấy danh sách quan hệ (hỗ trợ phân trang và lọc).
    *   **Phản hồi:** `PaginatedListOfRelationshipListDto`
-   `GET /api/relationship/{id}`: Lấy thông tin quan hệ theo ID.
    *   **Phản hồi:** `RelationshipDto`
-   `GET /api/relationship/search?sourceMemberId=...&targetMemberId=...&type=...&page=...&itemsPerPage=...`: Tìm kiếm quan hệ theo các tiêu chí và hỗ trợ phân trang.
    *   **Phản hồi:** `PaginatedListOfRelationshipListDto`
-   `POST /api/relationship`: Tạo quan hệ mới.
    *   **Request Body:** `CreateRelationshipCommand`
    -   `sourceMemberId`: `string (uuid)` - ID thành viên nguồn (ví dụ: `"uuid"`)
    -   `targetMemberId`: `string (uuid)` - ID thành viên đích (ví dụ: `"uuid"`)
    -   `type`: `string (Parent/Child/Spouse/Sibling)` - Loại quan hệ (ví dụ: `"Parent"`)
    *   **Phản hồi:** `string (uuid)` (ID của quan hệ vừa tạo)
-   `POST /api/relationship/generate-relationship-data`: Tạo dữ liệu quan hệ mẫu.
    *   **Request Body:** `GenerateRelationshipDataCommand`
    *   **Phản hồi:** `List<RelationshipDto>`
-   `POST /api/relationship/bulk-create`: Tạo nhiều quan hệ mới cùng lúc.
    *   **Request Body:** `CreateRelationshipsCommand` (một mảng các `CreateRelationshipCommand`)
    *   **Phản hồi:** `List<string (uuid)>` (Danh sách ID của các quan hệ vừa tạo)
-   `PUT /api/relationship/{id}`: Cập nhật thông tin quan hệ.
    *   **Request Body:** `UpdateRelationshipCommand`
    -   `id`: `string (uuid)` - ID của quan hệ (ví dụ: `"uuid"`)
    -   `sourceMemberId`: `string (uuid)` - ID thành viên nguồn (ví dụ: `"uuid"`)
    -   `targetMemberId`: `string (uuid)` - ID thành viên đích (ví dụ: `"uuid"`)
    -   `type`: `string (Parent/Child/Spouse/Sibling)` - Loại quan hệ (ví dụ: `"Spouse"`)
    *   **Phản hồi:** `204 No Content` nếu thành công.
-   `DELETE /api/relationship/{id}`: Xóa quan hệ.
    *   **Phản hồi:** `204 No Content` nếu thành công.

### 6.6. Quản lý Hồ sơ Người dùng (`/api/user-profile`)

-   `GET /api/user-profile/me`: Lấy thông tin hồ sơ của người dùng hiện tại.
    *   **Mô tả:** Endpoint này không yêu cầu ID người dùng trong URL; ID được lấy từ ngữ cảnh xác thực của server.
    *   **Phản hồi:** `Result<UserProfileDto>`
-   `GET /api/user-profile`: Lấy danh sách tất cả hồ sơ người dùng.
    *   **Phản hồi:** `Result<List<UserProfileDto>>`
-   `GET /api/user-profile/{id}`: Lấy thông tin hồ sơ người dùng theo ID nội bộ (GUID).
    *   **Phản hồi:** `Result<UserProfileDto>`
-   `GET /api/user-profile/byExternalId/{externalId}`: Lấy thông tin hồ sơ người dùng theo External ID (ví dụ: Auth0 User ID).
    *   **Phản hồi:** `Result<UserProfileDto>`
-   `PUT /api/user-profile/{userId}`: Cập nhật hồ sơ người dùng.
    *   **Path Parameters:** `userId` (string)
    *   **Request Body:** `UpdateUserProfileCommand`
    *   **Phản hồi:** `Result`

### 6.7. Quản lý Hoạt động Người dùng (`/api/activity`)

-   `GET /api/activity/recent?limit=...&targetType=...&targetId=...&groupId=...`: Lấy danh sách các hoạt động gần đây của người dùng.
    *   **Phản hồi:** `Result<List<UserActivityDto>>`

### 6.8. Quản lý AI (`/api/ai`)

-   `POST /api/ai/biography`: Sinh tiểu sử cho thành viên bằng AI.
    *   **Request Body:** `GenerateBiographyCommand`
    -   `memberId`: `string (uuid)` - ID của thành viên (ví dụ: `"uuid"`)
    -   `style`: `string (enum: Emotional, Historical, Storytelling, Formal, Informal)` - Kiểu giọng văn (ví dụ: `"Emotional"`)
    -   `useDBData`: `boolean` - Sử dụng dữ liệu từ DB (ví dụ: `"true"`)
    -   `userPrompt`: `string (nullable)` - Prompt tùy chỉnh của người dùng (ví dụ: `"string (nullable)"`)
    -   `language`: `string` - Ngôn ngữ (ví dụ: `"string"`)
    *   **Phản hồi:** `Result<BiographyResultDto>`




### 6.9. Quản lý Tùy chọn Người dùng (`/api/UserPreferences`)

-   `GET /api/UserPreferences`: Lấy tùy chọn của người dùng hiện tại.
    *   **Phản hồi:** `Result<UserPreferenceDto>`
-   `PUT /api/UserPreferences`: Cập nhật tùy chọn của người dùng hiện tại.
    *   **Request Body:** `SaveUserPreferencesCommand`
    -   `theme`: `string (enum: Light, Dark)` - Chủ đề (ví dụ: `"Light"`)
    -   `language`: `string (enum: English, Vietnamese)` - Ngôn ngữ (ví dụ: `"Vietnamese"`)
    -   `emailNotificationsEnabled`: `boolean` - Bật thông báo email (ví dụ: `"true"`)
    -   `smsNotificationsEnabled`: `boolean` - Bật thông báo SMS (ví dụ: `"false"`)
    -   `inAppNotificationsEnabled`: `boolean` - Bật thông báo trong ứng dụng (ví dụ: `"true"`)
    *   **Phản hồi:** `Result`

### 6.10. Quản lý Tải lên Tệp (`/api/upload`)

-   `POST /api/upload`: Tải lên một tệp (hình ảnh, tài liệu) lên hệ thống lưu trữ đã cấu hình.
    *   **Request Body:** `multipart/form-data` (chứa `IFormFile`)
    *   **Phản hồi:** `Result<Result<string>>` (chứa URL công khai của tệp đã tải lên)
-   `GET /api/upload/preview/{fileName}`: Lấy nội dung của một tệp đã tải lên để xem trước. Yêu cầu xác thực.
    *   **Phản hồi:** `FileContentResult` (nội dung tệp với `Content-Type` phù hợp)

### 6.11. Xử lý Dữ liệu và Tải lên Chunk (`/api/chunk/upload`)

-   `POST /api/chunk/upload`: Tải lên một tệp (PDF hoặc TXT) để trích xuất văn bản, làm sạch và chia thành các chunk.
    *   **Request Body:** `multipart/form-data`
    -   `file`: `IFormFile` - Tệp để tải lên (PDF hoặc TXT)
    -   `fileId`: `string` - ID của tệp
    -   `familyId`: `string` - ID của dòng họ
    -   `category`: `string` - Danh mục của tệp
    -   `createdBy`: `string` - Người tạo
    *   **Phản hồi:** `Result<List<TextChunk>>`
-   `POST /api/chunk/approve`: Chấp thuận các chunk văn bản đã được xử lý để tạo embeddings.
    *   **Request Body:** `List<TextChunk>`
    *   **Phản hồi:** `Result`

### 6.12. Quản lý Chatbot (`/api/chat`)

-   `POST /api/chat`: Gửi tin nhắn đến chatbot AI và nhận phản hồi.
    *   **Request Body:** `ChatRequest`
    -   `message`: `string` - Tin nhắn gửi đến chatbot (ví dụ: `"Xin chào"`)
    -   `sessionId`: `string (uuid, nullable)` - ID phiên trò chuyện (ví dụ: `"uuid (nullable)"`)
    *   **Phản hồi:** `Result<ChatResponse>`

### 6.13. Quản lý Bảng điều khiển (`/api/dashboard`)

-   `GET /api/dashboard/stats`: Lấy các số liệu thống kê cho bảng điều khiển.
    *   **Query Parameters:** `familyId` (Guid, optional): Lọc số liệu thống kê theo ID dòng họ.
    *   **Phản hồi:** `Result<DashboardStatsDto>`

### 6.14. Xử lý Ngôn ngữ Tự nhiên (`/api/NaturalLanguageInput`)

-   `POST /api/NaturalLanguageInput/generate-event-data`: Tạo dữ liệu sự kiện từ mô tả ngôn ngữ tự nhiên.
    *   **Request Body:** `GenerateEventDataCommand`
    -   `prompt`: `string` - Mô tả sự kiện bằng ngôn ngữ tự nhiên (ví dụ: `"Tạo một sự kiện sinh nhật cho Nguyễn Văn A vào ngày 1/1/2000"`)
    *   **Phản hồi:** `string`

### 6.15. Quản lý Khuôn mặt (`/api/Faces`)

-   `POST /api/Faces/detect`: Phát hiện khuôn mặt trong ảnh được tải lên.
    *   **Request Body:** `multipart/form-data` (chứa `IFormFile` và tùy chọn `returnCrop` (boolean))
    *   **Phản hồi:** `FaceDetectionResponseDto`
-   `GET /api/Faces/detected/{imageId}`: Lấy các khuôn mặt đã được phát hiện cho một ID ảnh.
    *   **Path Parameters:** `imageId` (Guid)
    *   **Phản hồi:** `List<DetectedFaceDto>`
-   `POST /api/Faces/labels`: Lưu nhãn cho các khuôn mặt đã được phát hiện.
    *   **Request Body:** `SaveFaceLabelsCommand`
    *   **Phản hồi:** `Result<Unit>` (hoặc `200 OK` với `Result` thành công)



## 7. Mô hình Dữ liệu (Response Models)

### 7.1. Family

-   `id`: `string (uuid)`
-   `name`: `string`
-   `code`: `string`
-   `description`: `string`
-   `address`: `string (nullable)`
-   `totalMembers`: `number`
-   `totalGenerations`: `number (nullable)`
-   `visibility`: `string (nullable)`
-   `avatarUrl`: `string (nullable)`
-   `validationErrors`: `array of string (nullable)`
-   `created`: `string (date-time)`
-   `createdBy`: `string (nullable)`
-   `lastModified`: `string (date-time, nullable)`
-   `lastModifiedBy`: `string (nullable)`

### 7.2. Member

-   `id`: `string (uuid)`
-   `firstName`: `string`
-   `lastName`: `string`
-   `code`: `string`
-   `nickname`: `string (nullable)`
-   `gender`: `string (nullable)`
-   `dateOfBirth`: `string (date-time, nullable)`
-   `dateOfDeath`: `string (date-time, nullable)`
-   `placeOfBirth`: `string (nullable)`
-   `placeOfDeath`: `string (nullable)`
-   `occupation`: `string (nullable)`
-   `avatarUrl`: `string (nullable)`
-   `biography`: `string (nullable)`
-   `familyId`: `string (uuid)`




### 7.3. Event

-   `id`: `string (uuid)`
-   `name`: `string`
-   `code`: `string`
-   `description`: `string (nullable)`
-   `startDate`: `string (date-time, nullable)`
-   `endDate`: `string (date-time, nullable)`
-   `location`: `string (nullable)`
-   `familyId`: `string (uuid, nullable)`
-   `type`: `string (enum: Birth, Marriage, Death, Migration, Other)`
-   `color`: `string (nullable)`
-   `relatedMembers`: `array of string (uuid)`


### 7.4. Relationship

-   `id`: `string (uuid)`
-   `sourceMemberId`: `string (uuid)`
-   `sourceMember`: `object` (Đối tượng `RelationshipMemberDto`)
-   `targetMemberId`: `string (uuid)`
-   `targetMember`: `object` (Đối tượng `RelationshipMemberDto`)
-   `type`: `string (enum: Father, Mother, Husband, Wife)`
-   `order`: `number (nullable)`
-   `familyId`: `string (uuid)`

### 7.5. RelationshipListDto

-   `id`: `string (uuid)`
-   `sourceMemberId`: `string (uuid)`
-   `targetMemberId`: `string (uuid)`
-   `type`: `string (enum: Father, Mother, Husband, Wife)`
-   `order`: `number (nullable)`
-   `startDate`: `string (date-time, nullable)`
-   `endDate`: `string (date-time, nullable)`
-   `description`: `string (nullable)`
-   `sourceMember`: `object` (Đối tượng `RelationshipMemberDto`)
-   `targetMember`: `object` (Đối tượng `RelationshipMemberDto`)

### 7.6. UserProfile

-   `id`: `string (uuid)`
-   `externalId`: `string`
-   `email`: `string`
-   `name`: `string`
-   `avatar`: `string (url, nullable)`
-   `roles`: `array of string`

### 7.7. UserActivity

-   `id`: `string (uuid)`
-   `userProfileId`: `string (uuid)`
-   `actionType`: `string (enum: Login, CreateFamily, UpdateMember, etc.)`
-   `targetType`: `string (enum: Family, Member, UserProfile, etc.)`
-   `targetId`: `string`
-   `groupId`: `string (uuid, nullable)`
-   `metadata`: `object (nullable)` (Đối tượng `JsonDocument`)
-   `activitySummary`: `string`
-   `created`: `string (date-time)`


### 7.8. BiographyResultDto

-   `content`: `string`


### 7.9. UserPreference

-   `theme`: `string (enum: Light, Dark)`
-   `language`: `string (enum: English, Vietnamese)`
-   `emailNotificationsEnabled`: `boolean`
-   `smsNotificationsEnabled`: `boolean`
-   `inAppNotificationsEnabled`: `boolean`

### 7.10. TextChunk

-   `id`: `string`
-   `content`: `string`
-   `metadata`: `object` (Các thuộc tính bổ sung có kiểu `string`)
-   `embedding`: `array of number (float, nullable)`
-   `score`: `number (float)`



### 7.11. ChatResponse

-   `response`: `string`
-   `context`: `array of string`
-   `sessionId`: `string (uuid, nullable)`
-   `model`: `string (nullable)`
-   `createdAt`: `string (date-time)`

### 7.12. DashboardStatsDto

-   `totalFamilies`: `number`
-   `totalMembers`: `number`
-   `totalRelationships`: `number`
-   `totalGenerations`: `number`

### 7.13. GenerateEventDataCommand

-   `prompt`: `string`


### 7.14. FaceDetectionResultDto

-   `id`: `string`
-   `boundingBox`:
    -   `x`: `number`
    -   `y`: `number`
    -   `width`: `number`
    -   `height`: `number`
-   `confidence`: `number`
-   `thumbnail`: `string (Base64 encoded image, nullable)`

### 7.15. DetectedFaceDto

-   `id`: `string`
-   `boundingBox`:
    -   `x`: `number`
    -   `y`: `number`
    -   `width`: `number`
    -   `height`: `number`
-   `confidence`: `number`
-   `thumbnail`: `string (Base64 encoded image, nullable)`
-   `memberId`: `string (uuid, nullable)`
-   `memberName`: `string (nullable)`

### 7.16. BoundingBoxDto

-   `x`: `number`
-   `y`: `number`
-   `width`: `number`
-   `height`: `number`

### 7.17. LabelFaceCommand

-   `memberId`: `string (uuid)`
-   `faceId`: `string`
-   `boundingBox`:
    -   `x`: `number`
    -   `y`: `number`
    -   `width`: `number`
    -   `height`: `number`
-   `confidence`: `number`
-   `thumbnail`: `string (Base64 encoded image, nullable)`

### 7.18. SystemConfigurationDto

-   `id`: `string (uuid)`
-   `key`: `string`
-   `value`: `string (nullable)`
-   `valueType`: `string (nullable)`
-   `description`: `string (nullable)`

### 7.19. CreateSystemConfigurationCommand

-   `key`: `string`
-   `value`: `string`
-   `valueType`: `string`
-   `description`: `string`

### 7.20. UpdateSystemConfigurationCommand

-   `id`: `string (uuid)`
-   `key`: `string`
-   `value`: `string`
-   `valueType`: `string`
-   `description`: `string`

```
