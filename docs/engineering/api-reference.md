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
  - [6.6. Quản lý Hồ sơ Người dùng (`/api/user-profile`)](#66-quản-lý-hồ-sơ-người-dùng-apICurrentUserprofiles)
  - [6.7. Quản lý Hoạt động Người dùng (`/api/activity`)](#67-quản-lý-hoạt-động-người-dùng-apiactivities)
  - [6.8. Quản lý AI (`/api/ai`)](#68-quản-lý-ai-api-ai)
  - [6.9. Quản lý Tùy chọn Người dùng (`/api/UserPreferences`)](#69-quản-lý-tùy-chọn-người-dùng-apICurrentUserpreferences)
  - [6.10. Quản lý Tải lên Tệp (`/api/upload`)](#610-quản-lý-tải-lên-tệp-apiupload)
  - [6.11. Quản lý Bảng điều khiển (`/api/dashboard`)](#611-quản-lý-bảng-điều-khiển-api-dashboard)
  - [6.12. Xử lý Ngôn ngữ Tự nhiên (`/api/NaturalLanguageInput`)](#612-xử-lý-ngôn-ngữ-tự-nhiên-api-naturallanguageinput)
  - [6.13. Quản lý Khuôn mặt (`/api/Faces`)](#613-quản-lý-khuôn-mặt-api-faces)
  - [6.14. Quản lý Phiên bản (`/api/Version`)](#614-quản-lý-phiên-bản)
  - [6.15. Quản lý Người dùng (`/api/user`)](#615-quản-lý-người-dùng-apiuser)
  - [6.16. Quản lý Cấu hình Quyền riêng tư (`/api/PrivacyConfiguration`)](#616-quản-lý-cấu-hình-quyền-riêng-tư-apiprivacyconfiguration)
- [7. Mô hình Dữ liệu (Response Models)](#7-mô-hình-dữ-liệu-response-models)
  - [7.1. Family](#71-family)
  - [7.2. Member](#72-member)
  - [7.3. Event](#73-event)
  - [7.4. Relationship](#74-relationship)
  - [7.5. RelationshipListDto](#75-relationshiplistdto)
  - [7.6. UserProfile](#76-userprofile)
  - [7.7. UserActivity](#77-useractivity)
  - [7.8. BiographyResultDto](#78-biographyresultdto)
  - [7.9. UserPreference](#79-userpreference)
  - [7.10. DashboardStatsDto](#710-dashboardstatsdto)
  - [7.11. FaceDetectionResultDto](#711-facedetectionresultdto)
  - [7.12. DetectedFaceDto](#712-detectedfacedto)
  - [7.13. BoundingBoxDto](#713-boundingboxdto)
  - [7.14. LabelFaceCommand](#714-labelfacecommand)
  - [7.15. SystemConfigurationDto](#715-systemconfigurationdto)
  - [7.16. CreateSystemConfigurationCommand](#716-createsystemconfigurationcommand)
  - [7.17. UpdateSystemConfigurationCommand](#717-updatesystemconfigurationcommand)

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

-   `page` (int, optional, default: 1): Số trang muốn lấy.
-   `pageSize` (int, optional, default: 10): Số lượng mục trên mỗi trang.

**Ví dụ:**

-   **Ví dụ:** Để lấy trang thứ 2 với 20 mục trên mỗi trang:
    -   **Phương thức:** `GET`
    -   **Đường dẫn:** `/api/family?page=2&pageSize=20`

Phản hồi sẽ có cấu trúc `PaginatedList<T>`:

Phản hồi sẽ có cấu trúc `PaginatedList<T>` với các trường:
-   `items`: Một mảng chứa các đối tượng dữ liệu của trang hiện tại.
-   `page`: Số trang hiện tại.
-   `totalPages`: Tổng số trang có sẵn.
-   `totalItems`: Tổng số mục trên tất cả các trang.

## 4. Lọc và Tìm kiếm

Các endpoint danh sách hỗ trợ lọc và tìm kiếm qua query parameter. Các tham số lọc cụ thể sẽ phụ thuộc vào từng tài nguyên (resource).

**Ví dụ với `GET /api/member`:**

-   `searchQuery`: Chuỗi ký tự để tìm kiếm theo tên, nghề nghiệp, v.v. (ví dụ: `searchQuery=Văn`)
-   `gender`: Lọc theo giới tính (ví dụ: `gender=Male`)
-   `familyId`: Lọc theo ID của dòng họ (ví dụ: `familyId=some-uuid`)

**Ví dụ:**

-   **Ví dụ:** Để tìm kiếm thành viên có tên "Văn" và giới tính "Male" trên trang 1 với 10 mục mỗi trang:
    -   **Phương thức:** `GET`
    -   **Đường dẫn:** `/api/member?searchQuery=Văn&gender=Male&page=1&pageSize=10`

**Ví dụ với `GET /api/family/search`:**

-   `keyword`: Từ khóa để tìm kiếm theo tên dòng họ, mô tả, v.v.

-   **Ví dụ:** Để tìm kiếm dòng họ có từ khóa "Royal" trên trang 1 với 5 mục mỗi trang:
    -   **Phương thức:** `GET`
    -   **Đường dẫn:** `/api/family/search?keyword=Royal&page=1&pageSize=5`

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

-   `GET /api/family`: Lấy danh sách tất cả các dòng họ (hỗ trợ phân trang).
    *   **Query Parameters:** `page` (int), `pageSize` (int), `searchQuery` (string, optional)
    *   **Phản hồi:** `Result<PaginatedList<FamilyDto>>`
-   `GET /api/family/{id}`: Lấy thông tin dòng họ theo ID.
    *   **Phản hồi:** `Result<FamilyDto>`
-   `GET /api/family/by-ids?ids=id1,id2,...`: Lấy thông tin nhiều dòng họ theo danh sách ID (comma-separated).
    *   **Phản hồi:** `Result<List<FamilyDto>>`
-   `POST /api/family`: Tạo dòng họ mới.
    *   **Request Body:** `CreateFamilyCommand`
    *   **Phản hồi:** `Result<Guid>` (ID của dòng họ vừa tạo)
-   `PUT /api/family/{id}`: Cập nhật thông tin dòng họ.
    *   **Request Body:** `UpdateFamilyCommand`
    *   **Phản hồi:** `Result`
-   `DELETE /api/family/{id}`: Xóa dòng họ.
    *   **Phản hồi:** `Result`
-   `POST /api/family-data/export/{familyId}`: Xuất tất cả dữ liệu của một gia đình (thành viên, mối quan hệ, sự kiện) ra file JSON.
    *   **Path Parameters:** `familyId` (Guid)
    *   **Phản hồi:** `FileContentResult` (file JSON)
-   `POST /api/family-data/import/{familyId}`: Nhập dữ liệu gia đình từ file JSON vào một gia đình hiện có.
    *   **Path Parameters:** `familyId` (Guid)
    *   **Query Parameters:** `clearExistingData` (boolean, optional, default: true) - Có xóa dữ liệu hiện có của gia đình trước khi nhập không.
    *   **Request Body:** `ImportFamilyCommand` (chứa dữ liệu JSON của gia đình)
    *   **Phản hồi:** `Result<Guid>` (ID của gia đình đã được cập nhật)

### 6.2. Quản lý Thành viên (`/api/member`)

-   `GET /api/member`: Lấy danh sách tất cả các thành viên (hỗ trợ phân trang và lọc).
    *   **Query Parameters:** `page` (int), `pageSize` (int), `searchQuery` (string, optional), `gender` (string, optional), `familyId` (Guid, optional)
    *   **Phản hồi:** `Result<PaginatedList<MemberListDto>>`
-   `GET /api/member/{id}`: Lấy thông tin thành viên theo ID.
    *   **Phản hồi:** `Result<MemberDetailDto>`
-   `GET /api/member/by-ids?ids=id1,id2,...`: Lấy thông tin nhiều thành viên theo danh sách ID (comma-separated).
    *   **Phản hồi:** `Result<List<MemberListDto>>`
-   `GET /api/member/managed`: Lấy danh sách các thành viên mà người dùng hiện tại có quyền chỉnh sửa.
    *   **Phản hồi:** `Result<List<MemberListDto>>`
-   `POST /api/member`: Thêm thành viên mới.
    *   **Request Body:** `CreateMemberCommand`
    *   **Phản hồi:** `Result<Guid>` (ID của thành viên vừa tạo)
-   `POST /api/member/bulk-create`: Tạo nhiều thành viên mới cùng lúc.
    *   **Request Body:** `CreateMembersCommand` (một mảng các `CreateMemberCommand`)
    *   **Phản hồi:** `Result<List<Guid>>` (Danh sách ID của các thành viên vừa tạo)
-   `POST /api/member/generate-member-data`: Tạo dữ liệu thành viên mẫu từ mô tả ngôn ngữ tự nhiên.
    *   **Request Body:** `GenerateMemberDataCommand`
    *   **Phản hồi:** `Result<List<AIMemberDto>>`
-   `PUT /api/member/{id}`: Cập nhật thông tin thành viên.
    *   **Request Body:** `UpdateMemberCommand`
    *   **Phản hồi:** `Result`
-   `DELETE /api/member/{id}`: Xóa thành viên.
    *   **Phản hồi:** `Result`
-   `PUT /api/member/{id}/biography`: Cập nhật tiểu sử của thành viên.
    *   **Request Body:** `UpdateMemberBiographyCommand`
    *   **Phản hồi:** `Result`

### 6.3. Quản lý Sự kiện (`/api/event`)

-   `GET /api/event`: Lấy danh sách tất cả các sự kiện (hỗ trợ phân trang và lọc).
    *   **Query Parameters:** `page` (int), `pageSize` (int), `searchQuery` (string, optional), `eventType` (string, optional), `familyId` (Guid, optional), `startDate` (datetime, optional), `endDate` (datetime, optional), `location` (string, optional), `relatedMemberId` (Guid, optional)
    *   **Phản hồi:** `Result<PaginatedList<EventDto>>`
-   `GET /api/event/{id}`: Lấy thông tin sự kiện theo ID.
    *   **Phản hồi:** `Result<EventDto>`
-   `GET /api/event/upcoming`: Lấy danh sách các sự kiện sắp tới (trong 30 ngày tới).
    *   **Query Parameters:** `familyId` (Guid, optional)
    *   **Phản hồi:** `Result<List<EventDto>>`
-   `POST /api/event`: Tạo sự kiện mới.
    *   **Request Body:** `CreateEventCommand`
    *   **Phản hồi:** `Result<Guid>` (ID của sự kiện vừa tạo)
-   `POST /api/event/bulk-create`: Tạo nhiều sự kiện mới cùng lúc.
    *   **Request Body:** `CreateEventsCommand` (một mảng các `CreateEventCommand`)
    *   **Phản hồi:** `Result<List<Guid>>` (Danh sách ID của các sự kiện vừa tạo)
-   `PUT /api/event/{id}`: Cập nhật thông tin sự kiện.
    *   **Request Body:** `UpdateEventCommand`
    *   **Phản hồi:** `Result`
-   `DELETE /api/event/{id}`: Xóa sự kiện.
    *   **Phản hồi:** `Result`

### 6.4. Tìm kiếm chung (`/api/search`)

-   `GET /api/search?keyword=...&page=...&itemsPerPage=...&sortBy=...&sortOrder=...`: Tìm kiếm chung trên cả dòng họ và thành viên theo từ khóa, hỗ trợ phân trang và sắp xếp.
    *   **Phản hồi:** `PaginatedListOfSearchResultDto` (chứa danh sách Family và Member tìm được)

### 6.5. Quản lý Quan hệ (`/api/relationship`)

-   `GET /api/relationship`: Lấy danh sách tất cả các mối quan hệ (hỗ trợ phân trang và lọc).
    *   **Query Parameters:** `page` (int), `pageSize` (int), `familyId` (Guid, optional), `sourceMemberId` (Guid, optional), `targetMemberId` (Guid, optional), `type` (string, optional)
    *   **Phản hồi:** `Result<PaginatedList<RelationshipListDto>>`
-   `GET /api/relationship/{id}`: Lấy thông tin quan hệ theo ID.
    *   **Phản hồi:** `Result<RelationshipDto>`
-   `POST /api/relationship`: Tạo quan hệ mới.
    *   **Request Body:** `CreateRelationshipCommand`
    *   **Phản hồi:** `Result<Guid>` (ID của quan hệ vừa tạo)
-   `POST /api/relationship/bulk-create`: Tạo nhiều quan hệ mới cùng lúc.
    *   **Request Body:** `CreateRelationshipsCommand` (một mảng các `CreateRelationshipCommand`)
    *   **Phản hồi:** `Result<List<Guid>>` (Danh sách ID của các quan hệ vừa tạo)
-   `POST /api/relationship/generate-relationship-data`: Tạo dữ liệu quan hệ mẫu từ mô tả ngôn ngữ tự nhiên.
    *   **Request Body:** `GenerateRelationshipDataCommand`
    *   **Phản hồi:** `Result<List<AIRelationshipDto>>`
-   `PUT /api/relationship/{id}`: Cập nhật thông tin quan hệ.
    *   **Request Body:** `UpdateRelationshipCommand`
    *   **Phản hồi:** `Result`
-   `DELETE /api/relationship/{id}`: Xóa quan hệ.
    *   **Phản hồi:** `Result`

### 6.6. Quản lý Hồ sơ Người dùng (`/api/user-profile`)

-   `GET /api/user-profile/me`: Lấy thông tin hồ sơ của người dùng hiện tại.
    *   **Phản hồi:** `Result<UserProfileDto>`
-   `GET /api/user-profile`: Lấy danh sách tất cả hồ sơ người dùng.
    *   **Phản hồi:** `Result<List<UserProfileDto>>`
-   `GET /api/user-profile/{id}`: Lấy thông tin hồ sơ người dùng theo ID nội bộ (GUID).
    *   **Phản hồi:** `Result<UserProfileDto>`
-   `GET /api/user-profile/by-external-id/{externalId}`: Lấy thông tin hồ sơ người dùng theo External ID (ví dụ: Auth0 User ID).
    *   **Phản hồi:** `Result<UserProfileDto>`
-   `PUT /api/user-profile/{userId}`: Cập nhật hồ sơ người dùng.
    *   **Request Body:** `UpdateUserProfileCommand`
    *   **Phản hồi:** `Result`

### 6.7. Quản lý Hoạt động Người dùng (`/api/activity`)

-   `GET /api/activity/recent`: Lấy danh sách các hoạt động gần đây của người dùng.
    *   **Query Parameters:** `limit` (int, optional, default: 10), `targetType` (string, optional), `targetId` (Guid, optional), `groupId` (Guid, optional)
    *   **Phản hồi:** `Result<List<UserActivityDto>>`

### 6.8. Quản lý AI (`/api/ai`)

-   `POST /api/ai/biography`: Sinh tiểu sử cho thành viên bằng AI.
    *   **Request Body:** `GenerateBiographyCommand`
    *   **Phản hồi:** `Result<BiographyResultDto>`
-   `GET /api/ai/providers`: Lấy danh sách các nhà cung cấp AI khả dụng.
    *   **Phản hồi:** `Result<List<AIProviderDto>>`
-   `GET /api/ai/last-prompt/{memberId}`: Lấy prompt cuối cùng được sử dụng để tạo tiểu sử cho một thành viên.
    *   **Phản hồi:** `Result<string>`

### 6.9. Quản lý Tùy chọn Người dùng (`/api/UserPreferences`)

-   `GET /api/UserPreferences`: Lấy tùy chọn của người dùng hiện tại.
    *   **Phản hồi:** `Result<UserPreferenceDto>`
-   `PUT /api/UserPreferences`: Cập nhật tùy chọn của người dùng hiện tại.
    *   **Request Body:** `SaveUserPreferencesCommand`
    *   **Phản hồi:** `Result`

### 6.10. Quản lý Tải lên Tệp (`/api/upload`)

-   `POST /api/upload`: Tải lên một tệp (hình ảnh, tài liệu) lên hệ thống lưu trữ đã cấu hình.
    *   **Request Body:** `multipart/form-data` (chứa `IFormFile`)
    *   **Phản hồi:** `Result<string>` (chứa URL công khai của tệp đã tải lên)
-   `GET /api/upload/preview/{fileName}`: Lấy nội dung của một tệp đã tải lên để xem trước. Yêu cầu xác thực.
    *   **Phản hồi:** `FileContentResult` (nội dung tệp với `Content-Type` phù hợp)

### 6.11. Quản lý Bảng điều khiển (`/api/dashboard`)

-   `GET /api/dashboard/stats`: Lấy các số liệu thống kê cho bảng điều khiển.
    *   **Query Parameters:** `familyId` (Guid, optional): Lọc số liệu thống kê theo ID dòng họ.
    *   **Phản hồi:** `Result<DashboardStatsDto>`

### 6.12. Xử lý Ngôn ngữ Tự nhiên (`/api/natural-language`)

-   `POST /api/natural-language/analyze`: Phân tích văn bản ngôn ngữ tự nhiên và tạo prompt cho AI Agent.
    *   **Request Body:** `AnalyzeNaturalLanguageCommand`
    -   `text`: `string` - Văn bản ngôn ngữ tự nhiên cần phân tích.
    *   **Phản hồi:** `AnalyzedDataDto`

### 6.13. Quản lý Khuôn mặt (`/api/Faces`)

-   `POST /api/Faces/detect`: Phát hiện khuôn mặt trong ảnh được tải lên.
    *   **Request Body:** `multipart/form-data` (chứa `IFormFile` và tùy chọn `returnCrop` (boolean))
    *   **Phản hồi:** `FaceDetectionResponseDto`
-   `GET /api/Faces/detected/{imageId}`: Lấy các khuôn mặt đã được phát hiện cho một ID ảnh.
    *   **Path Parameters:** `imageId` (Guid)
    *   **Phản hồi:** `List<DetectedFaceDto>`
-   `POST /api/Faces/labels`: Lưu nhãn cho các khuôn mặt đã được phát hiện.
    *   **Request Body:** `SaveFaceLabelsCommand`
    *   **Phản hồi:** `Result<Unit>` (hoặc `200 OK` với `Result` thành công)

### 6.14. Quản lý Phiên bản (`/api/Version`)

-   `GET /api/Version`: Lấy phiên bản hiện tại của ứng dụng.
    *   **Phản hồi:** `object` (chứa thuộc tính `version` kiểu `string`)

### 6.15. Quản lý Người dùng (`/api/user`)

-   `GET /api/user/search`: Tìm kiếm người dùng dựa trên các tiêu chí được cung cấp.
    *   **Query Parameters:** `page` (int), `pageSize` (int), `searchQuery` (string, optional)
    *   **Phản hồi:** `Result<PaginatedList<UserDto>>`
-   `GET /api/user/by-ids?ids=id1,id2,...`: Lấy danh sách người dùng theo nhiều ID (comma-separated).
    *   **Phản hồi:** `Result<List<UserDto>>`

### 6.16. Quản lý Cấu hình Quyền riêng tư (`/api/PrivacyConfiguration`)

-   `GET /api/PrivacyConfiguration/{familyId}`: Lấy cấu hình quyền riêng tư cho một dòng họ cụ thể.
    *   **Path Parameters:** `familyId` (Guid)
    *   **Phản hồi:** `Result<PrivacyConfigurationDto>`
-   `PUT /api/PrivacyConfiguration/{familyId}`: Cập nhật cấu hình quyền riêng tư cho một dòng họ cụ thể.
    *   **Path Parameters:** `familyId` (Guid)
    *   **Request Body:** `UpdatePrivacyConfigurationCommand`
    *   **Phản hồi:** `Result<Unit>`



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
-   `phone`: `string (nullable)`
-   `email`: `string (nullable)`
-   `address`: `string (nullable)`
-   `occupation`: `string (nullable)`
-   `avatarUrl`: `string (nullable)`
-   `biography`: `string (nullable)`
-   `familyId`: `string (uuid)`
-   `isRoot`: `boolean`




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
-   `userId`: `string (uuid)`
-   `email`: `string`
-   `name`: `string`
-   `avatar`: `string (url, nullable)`
-   `roles`: `array of string`
-   `firstName`: `string (nullable)`
-   `lastName`: `string (nullable)`
-   `phone`: `string (nullable)`

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

-   `content`: `string` (Tiểu sử được tạo)


### 7.9. UserPreference

-   `theme`: `string (enum: Light, Dark)`
-   `language`: `string (enum: English, Vietnamese)`

### 7.10. DashboardStatsDto

-   `totalFamilies`: `number`
-   `totalMembers`: `number`
-   `totalRelationships`: `number`
-   `totalGenerations`: `number`

### 7.11. FaceDetectionResultDto

-   `id`: `string`
-   `boundingBox`:
    -   `x`: `number`
    -   `y`: `number`
    -   `width`: `number`
    -   `height`: `number`
-   `confidence`: `number`
-   `thumbnail`: `string (Base64 encoded image, nullable)`

### 7.13. DetectedFaceDto

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

### 7.14. BoundingBoxDto

-   `x`: `number`
-   `y`: `number`
-   `width`: `number`
-   `height`: `number`

### 7.15. LabelFaceCommand

-   `memberId`: `string (uuid)`
-   `faceId`: `string`
-   `boundingBox`:
    -   `x`: `number`
    -   `y`: `number`
    -   `width`: `number`
    -   `height`: `number`
-   `confidence`: `number`
-   `thumbnail`: `string (Base64 encoded image, nullable)`

### 7.16. SystemConfigurationDto

-   `id`: `string (uuid)`
-   `key`: `string`
-   `value`: `string (nullable)`
-   `valueType`: `string (nullable)`
-   `description`: `string (nullable)`

### 7.17. CreateSystemConfigurationCommand

-   `key`: `string`
-   `value`: `string`
-   `valueType`: `string`
-   `description`: `string`

### 7.18. UpdateSystemConfigurationCommand

-   `id`: `string (uuid)`
-   `key`: `string`
-   `value`: `string`
-   `valueType`: `string`
-   `description`: `string`

```