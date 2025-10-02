Bạn là AI hỗ trợ phát triển backend cho dự án Family Tree. theo phong cách Google Team 

Hiện trạng:
- Frontend (Vue3 + Pinia + Vuetify) đã hoàn thiện các phần chính và không được phép chỉnh sửa.
- Backend (.NET Core 8) đang trong giai đoạn dở dang.
- Authentication hiện dùng Auth0 ở FE, nhưng yêu cầu thiết kế BE theo cách lỏng lẻo (dễ thay đổi provider khác trong tương lai).
- FE đã có các chức năng: đăng nhập, quản lý thành viên (CRUD), xem cây gia phả, tìm kiếm.
- BE cần được bổ sung để tương thích với FE, đồng thời xuất ra OpenAPI (Swagger) để FE có thể generate client.

Yêu cầu thực hiện:
1. Hoàn thiện backend .NET Core 8 với các module:
   - **Auth Module**: 
     - Tạo abstraction `IAuthProvider` (interface).
     - Cài đặt tạm `Auth0Provider` cho login, nhưng phải dễ dàng thay thế bằng Google, Firebase, Keycloak…
   - **Member Module**: 
     - API CRUD thành viên (`/api/members`).
     - Support `GET /api/members/{id}` và `GET /api/members?ids=1,2,3`.
   - **Family Module**:
     - API quản lý cây gia phả (`/api/family`).
   - **Search Module**:
     - Endpoint cho tìm kiếm thành viên (`/api/search?keyword=...`).
   - Tích hợp Swagger/OpenAPI.

2. BE phải chạy được **ngay cả khi chưa có DB**:
   - Tạm thời dùng **InMemoryRepository** để giả lập dữ liệu.
   - Tách `IRepository` interface để sau này có thể thay thế bằng MongoDB, PostgreSQL hoặc MySQL.

3. Không chỉnh sửa FE.
   - Chỉ cần BE expose đúng contract cho FE gọi.
   - Nếu FE yêu cầu API mà BE chưa có → tự động bổ sung endpoint.

4. Cấu trúc BE chuyên nghiệp:
   - `Domain/Entities`
   - `Application/Services`
   - `Infrastructure/Persistence`
   - `Infrastructure/Auth`
   - `Api/Controllers`

5. Đảm bảo có Unit Test cơ bản cho các service quan trọng.

6. Cố gắng tận dung tối đa các phần đã thực hiện cũ của BE 

Output mong muốn:
- Code backend đầy đủ (C# .NET 8).
- Swagger/OpenAPI spec.
- Hướng dẫn chạy BE với mock data.
