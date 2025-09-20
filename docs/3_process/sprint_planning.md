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

**Nhiệm vụ kỹ thuật (Technical Tasks):**
- [ ] **Backend:**
    - [ ] Tạo endpoint `POST /api/auth/login`.
    - [ ] Implement logic xác thực người dùng.
    - [ ] Implement logic tạo và ký JWT token.
- [x] **Frontend:**
    - [x] Tạo trang/component Đăng nhập (`LoginPage.vue`).
    - [x] Xây dựng form đăng nhập.
    - [x] Implement logic gọi API đăng nhập.
    - [x] Tạo Pinia store (`authStore`) để quản lý trạng thái đăng nhập.
    - [x] Điều hướng người dùng đến trang Dashboard.

**Thông tin:**
- **Epic:** `accounts`
- **Story Points:** 3
- **Trạng thái:** Hoàn thành (Frontend)

---

### Issue 2: [Feature] Quản lý tài khoản và vai trò (cơ bản)

**User Story:**
*Là một developer, tôi cần cấu trúc dữ liệu cho người dùng và vai trò để hỗ trợ chức năng đăng nhập và phân quyền sau này.*

**Nhiệm vụ kỹ thuật (Technical Tasks):**
- [ ] **Backend:**
    - [ ] Thiết kế và tạo entity `User` và `Role`.
    - [ ] Cấu hình MongoDB.
    - [ ] Implement logic hash mật khẩu.
    - [ ] Cập nhật `LoginCommandHandler`.
    - [ ] Tạo script seed data.

**Thông tin:**
- **Epic:** `accounts`
- **Story Points:** 3
- **Trạng thái:** Hoàn thành (Frontend)

---

### Issue 3: [Feature] Tạo Dòng họ/Gia đình

**User Story:**
*Là người dùng, tôi muốn tạo một dòng họ/gia đình mới để bắt đầu quản lý gia phả của mình.*

**Nhiệm vụ kỹ thuật (Technical Tasks):**
- [ ] **Backend:**
    - [ ] Implement đầy đủ các endpoint CRUD cho `families`.
- [x] **Frontend:**
    - [x] Tạo trang/component `FamilyListView.vue`.
    - [x] Tạo dialog/trang riêng (`FamilyAddView.vue`) chứa form tạo dòng họ.
    - [x] Implement logic gọi API và xử lý kết quả.

**Thông tin:**
- **Epic:** `core-data`
- **Story Points:** 2
- **Trạng thái:** Hoàn thành

---

### Issue 4: [Feature] Thêm & Chỉnh sửa thành viên

**User Story:**
*Là người dùng, tôi muốn thêm thành viên mới và chỉnh sửa thông tin thành viên hiện có.*

**Nhiệm vụ kỹ thuật (Technical Tasks):**
- [ ] **Backend:**
    - [ ] Implement các endpoint `POST /api/members`, `PUT /api/members/{id}`, và `GET /api/members/{id}`.
- [x] **Frontend:**
    - [x] Tạo component `MemberForm.vue`.
    - [x] Implement logic gọi các API tương ứng.
    - [x] Hiển thị thông báo thành công.

**Thông tin:**
- **Epic:** `core-data`
- **Story Points:** 4
- **Trạng thái:** Hoàn thành

---

### Issue 5: [Feature] Xem chi tiết thành viên

**User Story:**
*Là người dùng, tôi muốn xem thông tin chi tiết của từng thành viên để có cái nhìn đầy đủ.*

**Nhiệm vụ kỹ thuật (Technical Tasks):**
- [ ] **Backend:**
    - [ ] Đảm bảo endpoint `GET /api/members/{id}` trả về đầy đủ thông tin.
- [x] **Frontend:**
    - [x] Tạo dialog trong `MemberListView.vue` để hiển thị chi tiết.
    - [x] Sử dụng `MemberForm.vue` ở chế độ chỉ đọc.

**Thông tin:**
- **Epic:** `core-data`
- **Story Points:** 1
- **Trạng thái:** Hoàn thành

---

### Issue 6: [Feature] Tìm kiếm thành viên (Mở rộng)

**User Story:**
*Là người dùng, tôi muốn tìm kiếm thành viên trong cây gia phả với các tùy chọn tìm kiếm mở rộng.*

**Nhiệm vụ kỹ thuật (Technical Tasks):**
- [x] **Frontend:**
    - [x] Tạo component `MemberSearch.vue`.
    - [x] Hiển thị kết quả trong `v-data-table`.

**Thông tin:**
- **Epic:** `core-data`
- **Story Points:** 3
- **Trạng thái:** Hoàn thành

---

### Issue 7: [Feature] Xem dòng thời gian sự kiện

**User Story:**
*Là người dùng, tôi muốn xem dòng thời gian các sự kiện chính của gia đình.*

**Nhiệm vụ kỹ thuật (Technical Tasks):**
- [x] **Frontend:**
    - [x] Thêm tab "Dòng Thời Gian" vào `MemberForm.vue`.
    - [x] Tạo component `MemberTimeline.vue`.
    - [x] Hiển thị timeline với `v-timeline` và phân trang.

**Thông tin:**
- **Epic:** `visualization`
- **Story Points:** 3
- **Trạng thái:** Hoàn thành

---

### Issue 8: [Feature] Quản lý chế độ xem thông tin gia đình

**User Story:**
*Là người dùng, tôi muốn thông tin gia đình có thể được đặt ở chế độ công khai hoặc riêng tư.*

**Nhiệm vụ kỹ thuật (Technical Tasks):**
- [x] **Frontend:**
    - [x] Thêm trường "Visibility" vào `FamilyForm.vue`.
    - [x] Hiển thị visibility trong `FamilyListView.vue`.

**Thông tin:**
- **Epic:** `core-data`
- **Story Points:** 1
- **Trạng thái:** Hoàn thành

---

### Issue 9: [Feature] Xác thực dữ liệu

**User Story:**
*Là người dùng, tôi muốn các trường thông tin quan trọng phải được xác thực để đảm bảo tính đúng đắn của dữ liệu.*

**Nhiệm vụ kỹ thuật (Technical Tasks):**
- [x] **Frontend:**
    - [x] Thêm validation cho các trường bắt buộc trong `MemberForm.vue` và `FamilyForm.vue`.

**Thông tin:**
- **Epic:** `core-data`
- **Story Points:** 1
- **Trạng thái:** Hoàn thành

---

### Issue 10: [Feature] Hỗ trợ đa ngôn ngữ

**User Story:**
*Là người dùng, tôi muốn hệ thống hỗ trợ nhiều ngôn ngữ.*

**Nhiệm vụ kỹ thuật (Technical Tasks):**
- [x] **Frontend:**
    - [x] Tích hợp i18n và sử dụng các i18n keys cho tất cả các label và message.

**Thông tin:**
- **Epic:** `ui-ux`
- **Story Points:** 2
- **Trạng thái:** Hoàn thành

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