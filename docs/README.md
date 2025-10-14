# Tài Liệu Dự Án Cây Gia Phả

Chào mừng bạn đến với tài liệu của dự án Cây Gia Phả. Tài liệu này cung cấp thông tin toàn diện về kiến trúc, hướng dẫn phát triển, và các quy trình liên quan đến dự án.

## Mục lục

### 1. Tài liệu Kỹ thuật (Engineering Docs)

-   [Kiến trúc tổng quan](./engineering/architecture.md): Mô tả kiến trúc C4, các thành phần chính và luồng hoạt động của hệ thống.
-   [Hướng dẫn phát triển](./engineering/development-guide.md): Hướng dẫn cài đặt môi trường, build, và deploy dự án.
-   [Frontend](./engineering/frontend-guide.md): Chi tiết về cấu trúc dự án Frontend, state management, và coding style.
-   [Backend](./engineering/backend-guide.md): Chi tiết về cấu trúc dự án Backend, các pattern sử dụng, và coding style.
-   [Hướng dẫn API](./engineering/api-reference.md): Tài liệu chi tiết về các endpoint của API theo chuẩn OpenAPI.
-   [Mô hình dữ liệu](./engineering/data-model.md): Sơ đồ ERD, schema database, và các quan hệ.
-   [Kiểm thử & QA](./engineering/testing-guide.md): Hướng dẫn về chiến lược kiểm thử, các loại test, và quy trình QA.
-   [Bảo mật](./engineering/security-guide.md): Các vấn đề bảo mật và phương pháp kiểm soát truy cập.
-   [Hướng dẫn đóng góp](./engineering/contribution-guide.md): Quy tắc và quy trình để đóng góp vào dự án.

### 2. Tài liệu Dự án (Project Docs)

-   [Product Backlog](./project/backlog.md): Danh sách các User Story.
-   [Kế hoạch Sprint](./project/sprints.md): Kế hoạch cho các sprint phát triển.
-   [Kịch bản Kiểm thử](./project/test-cases.md): Các kịch bản kiểm thử chi tiết.
-   [Ghi chú phát hành](./project/release-notes.md): Lịch sử các phiên bản và thay đổi.
-   [Lộ trình Phát triển](./project/roadmap.md): Định hướng phát triển sản phẩm.
-   [Đội ngũ Phát triển](./project/team.md): Thông tin về các thành viên trong đội.

## Các thay đổi gần đây

-   **Sửa lỗi kiểm thử backend và trình điều khiển AI:** Đã khắc phục nhiều lỗi kiểm thử đơn vị trong `backend/tests/Application.UnitTests` liên quan đến `FileMetadata`, `StorageSettings` và `DateTime` mocking. Đã sửa các thông báo xác nhận trong một số kiểm thử liên quan đến tệp. Đã tiêm `IUser` vào `DeleteFileCommandHandler` và thêm kiểm tra ủy quyền. Đã xóa điểm cuối `SaveAIBiography` không tồn tại và câu lệnh `using` khỏi `AIController`. Đã cập nhật `API specification.json` do các thay đổi của `AIController`.
-   **Bổ sung kiểm thử trình xác thực cho các mô-đun Tệp, Danh tính và Quan hệ:** Đã triển khai các kiểm thử trình xác thực toàn diện cho các lệnh khác nhau trong các mô-đun Tệp, Danh tính và Quan hệ. Đã hợp nhất và xóa trình xác thực `UpdateUserProfile` dư thừa, tích hợp xác thực URL mạnh mẽ.
-   **Bổ sung kiểm thử trình xác thực cho các mô-đun Hoạt động người dùng và Tùy chọn người dùng:** Đã triển khai các kiểm thử trình xác thực toàn diện cho `RecordActivityCommand` trong mô-đun Hoạt động người dùng và `SaveUserPreferencesCommand` trong mô-đun Tùy chọn người dùng.
-   **Quản lý Tùy chọn Người dùng:** Triển khai API riêng biệt để lưu trữ và truy xuất tùy chọn cá nhân của người dùng (chủ đề, ngôn ngữ, cài đặt thông báo qua email/SMS/ứng dụng). Thêm thực thể `UserPreference` và các enum `Theme`, `Language` để lưu trữ các tùy chọn này, đồng thời cập nhật schema database thông qua migration.
-   **Cập nhật tính năng AI Biography:** Chuyển đổi endpoint lấy tiểu sử AI gần nhất từ trả về chuỗi sang đối tượng DTO đầy đủ, cập nhật API và sử dụng AutoMapper cho việc ánh xạ DTO. Cập nhật giao diện người dùng để hiển thị dữ liệu tiểu sử AI đầy đủ, bao gồm tên nhà cung cấp AI và thêm validation cho độ dài prompt.
-   **Refactor `auth0UserId` thành `externalId`:** Đổi tên thuộc tính `Auth0UserId` thành `ExternalId` trong `UserProfile` entity và DTO. Cập nhật các query, handler và controller liên quan để sử dụng `ExternalId` và endpoint `byExternalId`. Cập nhật các interface `User` và `UserProfile`, các service (`auth0Service`, `userProfileService`), và các store (`auth.store`, `userProfile.store`) để sử dụng `externalId` thay cho `auth0UserId` nhằm tách biệt khỏi nhà cung cấp xác thực cụ thể.
-   **Cải thiện trải nghiệm người dùng:** Thêm tooltips cho tất cả các nút hành động (chỉnh sửa, xóa, thêm mới) trong các danh sách (Thành viên, Gia đình, Sự kiện, Quan hệ). Thêm tooltips cho các nút thu gọn/mở rộng trong các bộ lọc tìm kiếm nâng cao.
-   **Dọn dẹp mã nguồn:** Xóa bỏ các import và biến không sử dụng trong các tệp frontend để cải thiện chất lượng mã nguồn và loại bỏ cảnh báo linting.
-   **Xử lý dữ liệu và Chia Chunk**: Triển khai module xử lý và chia nhỏ nội dung từ các tệp PDF/TXT thành các `TextChunk` với đầy đủ metadata (fileId, familyId, category, createdBy) để chuẩn bị cho việc tạo embeddings và tích hợp chatbot.
-   **Quản lý Hồ sơ Người dùng tập trung:** Tái cấu trúc frontend để quản lý thông tin hồ sơ người dùng tập trung trong `userProfileStore`, giảm sự phụ thuộc của các component UI vào `authStore`.
-   **API Hồ sơ Người dùng hiện tại:** Thêm endpoint backend mới `GET /api/UserProfiles/me` để lấy hồ sơ của người dùng hiện tại một cách an toàn.
-   **Trường Avatar cho Hồ sơ Người dùng:** Bổ sung trường `Avatar` vào thực thể `UserProfile` và cập nhật các chức năng liên quan ở cả backend và frontend.
-   **Xử lý kết quả nhất quán:** Triển khai `Result` wrapper cho `GetCurrentUserProfileQueryHandler` để đảm bảo xử lý kết quả nhất quán.
