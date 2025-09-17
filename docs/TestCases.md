## Module: Quản lý Dòng họ (Bổ sung)

**Mã Test Case:** TC-FM-011
**Tiêu đề:** Xác minh việc tìm kiếm dòng họ theo tên (tìm kiếm một phần).
**Điều kiện tiên quyết:**
*   Người dùng đã đăng nhập.
*   Nhiều dòng họ tồn tại, trong đó có ít nhất một dòng họ có tên khớp với từ khóa tìm kiếm.
**Dữ liệu kiểm thử:**
*   Từ khóa tìm kiếm: "Nguyễn"
**Các bước:**
1.  Gửi yêu cầu GET đến `/api/families?search={Từ khóa tìm kiếm}`.
2.  Xác minh mã trạng thái phản hồi HTTP.
3.  Xác minh rằng tất cả các dòng họ trong phản hồi đều có `name` chứa "Nguyễn".
**Kết quả mong đợi:**
*   Bước 2: Mã trạng thái HTTP 200 OK.
*   Bước 3: Tất cả các đối tượng dòng họ trong danh sách phản hồi có `name` chứa từ khóa "Nguyễn" (không phân biệt chữ hoa chữ thường).
**Kết quả thực tế:** (Sẽ điền trong quá trình thực thi)
**Trạng thái:** (Sẽ điền trong quá trình thực thi)
**Độ ưu tiên:** Cao
**Tác giả:** Gemini
**Ngày:** 2025-09-17

---

**Mã Test Case:** TC-FM-012
**Tiêu đề:** Xác minh việc tìm kiếm dòng họ theo địa chỉ (tìm kiếm một phần).
**Điều kiện tiên quyết:**
*   Người dùng đã đăng nhập.
*   Nhiều dòng họ tồn tại, trong đó có ít nhất một dòng họ có địa chỉ khớp với từ khóa tìm kiếm.
**Dữ liệu kiểm thử:**
*   Từ khóa tìm kiếm: "Lê Lợi"
**Các bước:**
1.  Gửi yêu cầu GET đến `/api/families?search={Từ khóa tìm kiếm}`.
2.  Xác minh mã trạng thái phản hồi HTTP.
3.  Xác minh rằng tất cả các dòng họ trong phản hồi đều có `address` chứa "Lê Lợi".
**Kết quả mong đợi:**
*   Bước 2: Mã trạng thái HTTP 200 OK.
*   Bước 3: Tất cả các đối tượng dòng họ trong danh sách phản hồi có `address` chứa từ khóa "Lê Lợi" (không phân biệt chữ hoa chữ thường).
**Kết quả thực tế:** (Sẽ điền trong quá trình thực thi)
**Trạng thái:** (Sẽ điền trong quá trình thực thi)
**Độ ưu tiên:** Cao
**Tác giả:** Gemini
**Ngày:** 2025-09-17

---

## Module: Frontend - Quản lý Dòng họ (Bổ sung)

**Mã Test Case:** TC-FE-FM-009
**Tiêu đề:** Xác minh chức năng tìm kiếm dòng họ theo tên trên UI.
**Điều kiện tiên quyết:**
*   Người dùng đã đăng nhập và đang ở trang danh sách Dòng họ.
*   Nhiều dòng họ tồn tại, trong đó có ít nhất một dòng họ có tên khớp với từ khóa tìm kiếm.
**Dữ liệu kiểm thử:**
*   Từ khóa tìm kiếm: "Nguyễn"
**Các bước:**
1.  Trên trang danh sách Dòng họ, sử dụng trường tìm kiếm.
2.  Nhập "Nguyễn" vào trường tìm kiếm.
3.  Xác minh rằng chỉ các dòng họ có `name` chứa "Nguyễn" được hiển thị trong danh sách.
4.  Xác minh rằng các dòng họ khác không khớp với từ khóa tìm kiếm bị ẩn.
**Kết quả mong đợi:**
*   Bước 3: Danh sách dòng họ chỉ hiển thị các dòng họ có tên chứa "Nguyễn".
*   Bước 4: Các dòng họ không khớp bị ẩn.
**Kết quả thực tế:** (Sẽ điền trong quá trình thực thi)
**Trạng thái:** (Sẽ điền trong quá trình thực thi)
**Độ ưu tiên:** Cao
**Tác giả:** Gemini
**Ngày:** 2025-09-17

---

**Mã Test Case:** TC-FE-FM-010
**Tiêu đề:** Xác minh chức năng tìm kiếm dòng họ theo địa chỉ trên UI.
**Điều kiện tiên quyết:**
*   Người dùng đã đăng nhập và đang ở trang danh sách Dòng họ.
*   Nhiều dòng họ tồn tại, trong đó có ít nhất một dòng họ có địa chỉ khớp với từ khóa tìm kiếm.
**Dữ liệu kiểm thử:**
*   Từ khóa tìm kiếm: "Lê Lợi"
**Các bước:**
1.  Trên trang danh sách Dòng họ, sử dụng trường tìm kiếm.
2.  Nhập "Lê Lợi" vào trường tìm kiếm.
3.  Xác minh rằng chỉ các dòng họ có `address` chứa "Lê Lợi" được hiển thị trong danh sách.
4.  Xác minh rằng các dòng họ khác không khớp với từ khóa tìm kiếm bị ẩn.
**Kết quả mong đợi:**
*   Bước 3: Danh sách dòng họ chỉ hiển thị các dòng họ có địa chỉ chứa "Lê Lợi".
*   Bước 4: Các dòng họ không khớp bị ẩn.
**Kết quả thực tế:** (Sẽ điền trong quá trình thực thi)
**Trạng thái:** (Sẽ điền trong quá trình thực thi)
**Độ ưu tiên:** Cao
**Tác giả:** Gemini
**Ngày:** 2025-09-17

---