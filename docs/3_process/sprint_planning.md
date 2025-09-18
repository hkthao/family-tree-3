# Kế hoạch Sprint Chi tiết

Tài liệu này mô tả chi tiết các User Story được lựa chọn cho các Sprint đầu tiên của dự án. Mỗi story được phân rã thành các nhiệm vụ kỹ thuật cụ thể (technical tasks) cho cả Backend và Frontend để đội ngũ phát triển có thể bắt đầu làm việc.

---

## Sprint 0: Khởi tạo & Nền tảng

*   **Mục tiêu:** Thiết lập toàn bộ môi trường phát triển, quy trình làm việc và các công cụ cần thiết để các sprint sau có thể tập trung vào việc phát triển tính năng. Sprint này không cung cấp chức năng cho người dùng cuối.
*   **Thời gian:** 1-2 tuần.

### Các nhiệm vụ chính (Technical Chores)

1.  **[Chore] Thiết lập môi trường phát triển Docker:**
    -   [x] Tạo file `docker-compose.yml` để quản lý các service: backend, frontend, database (MongoDB).
    -   [x] Viết `Dockerfile` tối ưu cho backend (.NET) và frontend (Vue.js).
    -   [x] Đảm bảo toàn bộ hệ thống có thể khởi chạy bằng lệnh `docker-compose up`.

2.  **[Chore] Cấu hình CI/CD Pipeline cơ bản:**
    -   [x] Tạo file workflow `.github/workflows/ci.yml`.
    -   [x] Cấu hình các job để tự động:
        -   Build backend và frontend.
        -   Chạy unit test và kiểm tra độ bao phủ mã (code coverage).
        -   Kiểm tra chất lượng mã nguồn (linting).

3.  **[Chore] Khởi tạo cấu trúc dự án:**
    -   [x] Tạo solution .NET theo kiến trúc Clean Architecture.
    -   [x] Khởi tạo dự án Frontend với Vue.js, Vite, và các thư viện cần thiết (Vuetify, Pinia).
    -   [x] Tổ chức cấu trúc thư mục `docs` một cách khoa học.

4.  **[Chore] Thiết kế và khởi tạo Database Schema:**
    -   [x] Định nghĩa schema ban đầu cho các collection chính (`users`, `families`, `members`, `relationships`) trong tài liệu `system_design.md`.
    -   [x] Tạo script seed data ban đầu để có dữ liệu mẫu cho việc phát triển.

5.  **[Chore] Thiết lập công cụ quản lý dự án:**
    -   [x] Tạo GitHub Project (Kanban board).
    -   [x] Định nghĩa các cột (Backlog, To Do, In Progress, In Review, Done) và các nhãn (epic, priority, type) theo tài liệu `kanban_board_setup.md`.

---

## Sprint 1: Nền tảng & Nhập liệu

*   **Mục tiêu:** Người dùng có thể đăng nhập và bắt đầu nhập dữ liệu các thành viên trong gia đình.
*   **Tổng Story Points:** 13

### Issue 1: [Feature] Đăng nhập hệ thống

**User Story:**
*Là người dùng, tôi muốn đăng nhập vào hệ thống để truy cập dữ liệu cây gia phả của mình.*

**Tiêu chí chấp nhận (Acceptance Criteria):**
- Giao diện đăng nhập có các trường cho email/tên đăng nhập và mật khẩu.
- Khi người dùng nhập thông tin hợp lệ và gửi, hệ thống xác thực thành công và trả về một JWT token.
- Sau khi đăng nhập thành công, người dùng được chuyển hướng đến trang tổng quan (Dashboard).
- Nếu thông tin không hợp lệ, hệ thống phải hiển thị thông báo lỗi rõ ràng.
- Token được lưu trữ an toàn ở phía client (ví dụ: Pinia store và localStorage) để sử dụng cho các request sau.

**Nhiệm vụ kỹ thuật (Technical Tasks):**
- [ ] **Backend:**
    - [ ] Tạo endpoint `POST /api/auth/login`.
    - [ ] Implement logic xác thực người dùng (kiểm tra email và mật khẩu đã hash).
    - [ ] Implement logic tạo và ký JWT token khi xác thực thành công.
    - [ ] Cấu hình middleware `[Authorize]` để bảo vệ các endpoint yêu cầu xác thực.
- [ ] **Frontend:**
    - [ ] Tạo trang/component Đăng nhập (`LoginPage.vue`).
    - [ ] Xây dựng form đăng nhập với các trường email, mật khẩu và validation cơ bản.
    - [ ] Implement logic gọi API đăng nhập khi người dùng submit form.
    - [ ] Tạo một Pinia store (`authStore`) để quản lý trạng thái đăng nhập và lưu JWT token.
    - [ ] Điều hướng người dùng đến trang Dashboard sau khi đăng nhập thành công.
    - [ ] Hiển thị thông báo lỗi nếu đăng nhập thất bại.

**Thông tin:**
- **Epic:** `accounts`
- **Story Points:** 3

---

### Issue 2: [Feature] Quản lý tài khoản và vai trò (cơ bản)

**User Story:**
*Là một developer, tôi cần cấu trúc dữ liệu cho người dùng và vai trò để hỗ trợ chức năng đăng nhập và phân quyền sau này.*

**Tiêu chí chấp nhận (Acceptance Criteria):**
- Schema cho collection `users` trong MongoDB được định nghĩa, bao gồm các trường như `email`, `passwordHash`, và `role`.
- Có thể tạo người dùng mới (thông qua API hoặc script seed data) với mật khẩu đã được hash bằng một thuật toán an toàn (ví dụ: BCrypt).
- Mỗi người dùng có thể được gán một vai trò (ví dụ: 'Admin', 'Member').

**Nhiệm vụ kỹ thuật (Technical Tasks):**
- [ ] **Backend:**
    - [ ] Thiết kế và tạo entity `User` và `Role` trong lớp Domain.
    - [ ] Cấu hình MongoDB để tạo collection `users`.
    - [ ] Implement logic hash mật khẩu trong Application layer (ví dụ: khi tạo người dùng).
    - [ ] Cập nhật logic của `LoginCommandHandler` để so sánh mật khẩu người dùng nhập với mật khẩu đã hash.
    - [ ] Tạo một script seed data (`infra/seeds`) để tạo một tài khoản admin và một tài khoản member mẫu khi khởi tạo database.

**Thông tin:**
- **Epic:** `accounts`
- **Story Points:** 3

---

### Issue 3: [Feature] Tạo Dòng họ/Gia đình

**User Story:**
*Là người dùng, tôi muốn tạo một dòng họ/gia đình mới để bắt đầu quản lý gia phả của mình.*

**Tiêu chí chấp nhận (Acceptance Criteria):**
- Người dùng có thể truy cập một form để tạo dòng họ mới.
- Form yêu cầu nhập các thông tin bắt buộc như "Tên dòng họ".
- Sau khi tạo thành công, dòng họ mới được lưu chính xác vào database.
- Giao diện được cập nhật để hiển thị dòng họ vừa tạo.

**Nhiệm vụ kỹ thuật (Technical Tasks):**
- [ ] **Backend:**
    - [ ] Implement đầy đủ các endpoint CRUD cho `families` (tập trung vào `POST` và `GET` cho sprint này).
    - [ ] Tạo `CreateFamilyCommand` và handler tương ứng.
    - [ ] Thêm validation (FluentValidation) cho `CreateFamilyCommand` để đảm bảo `name` không được để trống.
- [ ] **Frontend:**
    - [ ] Tạo trang/component `FamilyListPage.vue` để hiển thị danh sách các dòng họ.
    - [ ] Tạo một dialog hoặc trang riêng (`CreateFamily.vue`) chứa form tạo dòng họ.
    - [ ] Implement logic gọi API `POST /api/families` và xử lý kết quả.
    - [ ] Tự động làm mới danh sách dòng họ sau khi tạo thành công.

**Thông tin:**
- **Epic:** `core-data`
- **Story Points:** 2

---

### Issue 2: [Feature] Thêm & Chỉnh sửa thành viên

**User Story:**
*Là người dùng, tôi muốn thêm thành viên mới và chỉnh sửa thông tin thành viên hiện có.*

**Tiêu chí chấp nhận (Acceptance Criteria):**
- Có một form (`MemberForm.vue`) để nhập/sửa thông tin thành viên (họ tên, ngày sinh, giới tính, v.v.).
- Dữ liệu thành viên mới/cập nhật được lưu thành công vào database.
- Form chỉnh sửa được điền sẵn dữ liệu hiện tại của thành viên.
- Hệ thống xác thực các trường bắt buộc (ví dụ: họ tên).

**Nhiệm vụ kỹ thuật (Technical Tasks):**
- [ ] **Backend:**
    - [ ] Implement các endpoint `POST /api/members`, `PUT /api/members/{id}`, và `GET /api/members/{id}`.
    - [ ] Tạo `CreateMemberCommand` và `UpdateMemberCommand` cùng các handler tương ứng.
    - [ ] Thêm validation cho cả hai command.
- [ ] **Frontend:**
    - [ ] Tạo component `MemberForm.vue` có thể tái sử dụng cho cả hai chức năng.
    - [ ] Implement logic gọi các API tương ứng (POST, PUT, GET).
    - [ ] Hiển thị thông báo thành công và cập nhật giao diện.

**Thông tin:**
- **Epic:** `core-data`
- **Story Points:** 4 (2 cho Thêm, 2 cho Sửa)

---

### Issue 3: [Feature] Xem chi tiết thành viên

**User Story:**
*Là người dùng, tôi muốn xem thông tin chi tiết của từng thành viên để có cái nhìn đầy đủ.*

**Tiêu chí chấp nhận (Acceptance Criteria):**
- Khi người dùng nhấp vào một thành viên trong danh sách, họ được điều hướng đến một trang chi tiết.
- Trang chi tiết hiển thị tất cả các thông tin đã lưu của thành viên.
- Giao diện trình bày thông tin một cách rõ ràng, dễ đọc.

**Nhiệm vụ kỹ thuật (Technical Tasks):**
- [ ] **Backend:**
    - [ ] Đảm bảo endpoint `GET /api/members/{id}` trả về đầy đủ thông tin cần thiết.
- [ ] **Frontend:**
    - [ ] Tạo trang `MemberDetailPage.vue`.
    - [ ] Cấu hình Vue Router để có route động (ví dụ: `/members/:id`).
    - [ ] Implement logic gọi API `GET /api/members/{id}` dựa trên `id` từ URL.
    - [ ] Thiết kế giao diện để hiển thị thông tin chi tiết của thành viên.

**Thông tin:**
- **Epic:** `core-data`
- **Story Points:** 1

---

### Issue 4: [Feature] Quản lý mối quan hệ (cơ bản)

**User Story:**
*Là người dùng, tôi muốn thiết lập các mối quan hệ (cha-mẹ, vợ-chồng) giữa các thành viên để xây dựng cấu trúc cây gia phả.*

**Tiêu chí chấp nhận (Acceptance Criteria):**
- Giao diện cho phép chọn một thành viên và thêm mối quan hệ mới (ví dụ: thêm cha/mẹ, thêm vợ/chồng).
- Khi thêm cha/mẹ hoặc vợ/chồng, người dùng có thể tìm kiếm và chọn một thành viên khác đã tồn tại trong gia phả.
- Mối quan hệ phải được lưu chính xác vào bảng `relationships` trong database.
- Hệ thống phải có validation để ngăn các mối quan hệ không hợp lệ (ví dụ: một người tự làm cha/mẹ của chính mình, một người có nhiều hơn 2 cha/mẹ ruột).
- Các mối quan hệ đã có của thành viên được hiển thị rõ ràng trên trang chi tiết.

**Nhiệm vụ kỹ thuật (Technical Tasks):**
- [ ] **Backend:**
    - [ ] Implement endpoint `POST /api/relationships`.
    - [ ] Tạo `CreateRelationshipCommand` và handler, bao gồm logic validation phức tạp (kiểm tra quan hệ hiện có, giới tính, vòng lặp, v.v.).
    - [ ] Implement endpoint `DELETE /api/relationships/{id}` để xóa quan hệ.
    - [ ] Cập nhật endpoint lấy danh sách thành viên để có thể trả về thông tin quan hệ cơ bản.
- [ ] **Frontend:**
    - [ ] Trong trang chi tiết thành viên (`MemberDetailPage.vue`), thêm các nút/phần để quản lý quan hệ.
    - [ ] Tạo một dialog/component cho phép tìm kiếm và chọn thành viên để tạo liên kết.
    - [ ] Implement logic gọi API `POST /api/relationships` và `DELETE /api/relationships/{id}`.
    - [ ] Hiển thị các mối quan hệ đã có của thành viên trên trang chi tiết.

**Thông tin:**
- **Epic:** `core-data`
- **Story Points:** 5

---

## Sprint 2: Trực quan hóa & Hoàn thiện MVP

*   **Mục tiêu:** Cho phép người dùng thấy được cây gia phả từ dữ liệu đã nhập và hoàn thiện các chức năng cốt lõi của MVP.
*   **Tổng Story Points:** 8

### Issue 1: [Feature] Xem cây gia phả (một kiểu hiển thị chuẩn)

**User Story:**
*Là người dùng, tôi muốn xem cây gia phả dưới dạng một biểu đồ trực quan để hiểu rõ cấu trúc gia đình.*

**Tiêu chí chấp nhận (Acceptance Criteria):**
- Có một trang riêng để hiển thị cây gia phả.
- Cây gia phả được vẽ dưới dạng sơ đồ cây dọc (vertical tree) chuẩn.
- Mỗi node trên cây hiển thị thông tin cơ bản của thành viên (tên, ảnh đại diện nếu có).
- Các đường nối thể hiện rõ ràng mối quan hệ cha-mẹ-con và vợ-chồng.
- Người dùng có thể phóng to/thu nhỏ (zoom) và di chuyển (pan) biểu đồ.
- Khi nhấp vào một node thành viên, có thể điều hướng đến trang chi tiết của thành viên đó.

**Nhiệm vụ kỹ thuật (Technical Tasks):**
- [ ] **Backend:**
    - [ ] Implement endpoint `GET /api/family-trees/{familyId}`.
    - [ ] Endpoint này phải truy vấn các collection `members` và `relationships` để xây dựng cấu trúc dữ liệu cây (dạng node và edge) và trả về cho frontend.
    - [ ] Logic xây dựng cây cần được tối ưu để xử lý các gia phả lớn.
- [ ] **Frontend:**
    - [ ] Nghiên cứu và chọn một thư viện vẽ biểu đồ/đồ thị phù hợp (ví dụ: D3.js, GoJS, hoặc một thư viện Vue-specific).
    - [ ] Tạo trang `FamilyTreePage.vue`.
    - [ ] Implement logic gọi API `GET /api/family-trees/{familyId}` để lấy dữ liệu.
    - [ ] Viết code để render dữ liệu cây nhận được bằng thư viện đã chọn.
    - [ ] Implement các tính năng tương tác: zoom, pan, và click vào node.

**Thông tin:**
- **Epic:** `visualization`
- **Story Points:** 8 8