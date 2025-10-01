# Kịch bản Kiểm thử (Test Cases)

## Mục lục

- [1. Giới thiệu](#1-giới-thiệu)
- [2. Tóm tắt Test Cases](#2-tóm-tắt-test-cases)
- [3. Chi tiết Test Cases](#3-chi-tiết-test-cases)
  - [3.1. Module: Đăng nhập (Authentication)](#31-module-đăng-nhập-authentication)
  - [3.2. Module: Quản lý Thành viên (Member Management)](#32-module-quản-lý-thành-viên-member-management)
  - [3.3. Module: Cây Gia Phả (Family Tree)](#33-module-cây-gia-phả-family-tree)
  - [3.4. Module: Quản lý Dòng họ (Family Management)](#34-module-quản-lý-dòng-họ-family-management)
  - [3.5. Module: Quản lý Quan hệ (Relationship Management)](#35-module-quản-lý-quan-hệ-relationship-management)
  - [3.6. Module: File đính kèm (Attachments)](#36-module-file-đính-kèm-attachments)

---

## 1. Giới thiệu

Tài liệu này cung cấp các kịch bản kiểm thử chi tiết cho dự án Cây Gia Phả, nhằm đảm bảo tính đúng đắn của các tính năng, hỗ trợ quy trình QA và phát triển. Mỗi test case được mô tả rõ ràng để QA Engineer có thể thực hiện và Developer có thể tham khảo.

## 2. Tóm tắt Test Cases

| ID Test Case | Title                                  | Module                 | Priority | Automation/Manual |
| :----------- | :------------------------------------- | :--------------------- | :------- | :---------------- |
| TC_LOGIN_01  | Đăng nhập thành công                   | Authentication         | High     | Manual            |
| TC_LOGIN_02  | Đăng nhập thất bại (mật khẩu sai)      | Authentication         | High     | Manual            |
| TC_MEMBER_01 | Thêm thành viên mới thành công         | Member Management      | High     | Manual            |
| TC_MEMBER_02 | Thêm thành viên thất bại (thiếu họ tên) | Member Management      | High     | Manual            |
| TC_TREE_01   | Hiển thị cây gia phả đúng cấu trúc     | Family Tree            | High     | Manual            |
| TC_FAMILY_01 | Tạo dòng họ mới thành công             | Family Management      | High     | Manual            |

## 3. Chi tiết Test Cases

### 3.1. Module: Đăng nhập (Authentication)

#### TC_LOGIN_01: Đăng nhập thành công với thông tin hợp lệ
-   **Mục tiêu**: Xác minh người dùng có thể đăng nhập thành công với thông tin hợp lệ.
-   **Các bước thực hiện**:
    1.  Mở trang Đăng nhập.
    2.  Nhập email và mật khẩu chính xác.
    3.  Nhấn nút "Đăng nhập".
-   **Kết quả mong đợi**: Người dùng được chuyển hướng đến trang Tổng quan (Dashboard) và hiển thị thông báo đăng nhập thành công.
-   **Thực tế**: (Để trống)
-   **Severity**: Medium
-   **Priority**: High

#### TC_LOGIN_02: Đăng nhập thất bại với mật khẩu sai
-   **Mục tiêu**: Xác minh hệ thống xử lý đúng khi người dùng nhập sai mật khẩu.
-   **Các bước thực hiện**:
    1.  Mở trang Đăng nhập.
    2.  Nhập email đúng, mật khẩu sai.
    3.  Nhấn nút "Đăng nhập".
-   **Kết quả mong đợi**: Hệ thống hiển thị thông báo lỗi "Email hoặc mật khẩu không chính xác" và người dùng vẫn ở trang Đăng nhập.
-   **Thực tế**: (Để trống)
-   **Severity**: Medium
-   **Priority**: High

#### TC_LOGIN_03: Đăng nhập thất bại với email không tồn tại
-   **Mục tiêu**: Xác minh hệ thống xử lý đúng khi người dùng nhập email không tồn tại.
-   **Các bước thực hiện**:
    1.  Mở trang Đăng nhập.
    2.  Nhập một email không có trong hệ thống.
    3.  Nhấn nút "Đăng nhập".
-   **Kết quả mong đợi**: Hệ thống hiển thị thông báo lỗi "Email hoặc mật khẩu không chính xác" và người dùng vẫn ở trang Đăng nhập.
-   **Thực tế**: (Để trống)
-   **Severity**: Medium
-   **Priority**: High

### 3.2. Module: Quản lý Thành viên (Member Management)

#### TC_MEMBER_01: Thêm thành viên mới thành công
-   **Mục tiêu**: Xác minh người dùng có thể thêm thành viên mới với đầy đủ thông tin hợp lệ.
-   **Các bước thực hiện**:
    1.  Truy cập trang quản lý thành viên.
    2.  Nhấn nút "Thêm thành viên mới".
    3.  Điền đầy đủ các trường thông tin hợp lệ (Họ tên, Ngày sinh, Giới tính, v.v.).
    4.  Nhấn "Lưu".
-   **Kết quả mong đợi**: Thành viên mới được thêm vào danh sách và hiển thị đúng thông tin. Hệ thống hiển thị thông báo thành công.
-   **Thực tế**: (Để trống)
-   **Severity**: High
-   **Priority**: High

#### TC_MEMBER_02: Thêm thành viên thất bại khi thiếu họ tên
-   **Mục tiêu**: Xác minh hệ thống không cho phép thêm thành viên khi thiếu trường bắt buộc "Họ tên".
-   **Các bước thực hiện**:
    1.  Truy cập trang quản lý thành viên.
    2.  Nhấn nút "Thêm thành viên mới".
    3.  Để trống trường "Họ tên".
    4.  Điền các trường khác (nếu có).
    5.  Nhấn "Lưu".
-   **Kết quả mong đợi**: Hệ thống hiển thị thông báo lỗi validation tại trường "Họ tên" và không lưu thành viên.
-   **Thực tế**: (Để trống)
-   **Severity**: High
-   **Priority**: High

#### TC_MEMBER_03: Chỉnh sửa thông tin thành viên thành công
-   **Mục tiêu**: Xác minh người dùng có thể chỉnh sửa thông tin của một thành viên hiện có.
-   **Các bước thực hiện**:
    1.  Truy cập trang chi tiết của một thành viên.
    2.  Nhấn nút "Chỉnh sửa".
    3.  Thay đổi một hoặc nhiều trường thông tin (ví dụ: cập nhật nghề nghiệp).
    4.  Nhấn "Lưu".
-   **Kết quả mong đợi**: Thông tin thành viên được cập nhật thành công và hiển thị đúng trên trang chi tiết. Hệ thống hiển thị thông báo thành công.
-   **Thực tế**: (Để trống)
-   **Severity**: High
-   **Priority**: High

### 3.3. Module: Cây Gia Phả (Family Tree)

#### TC_TREE_01: Hiển thị cây gia phả đúng cấu trúc
-   **Mục tiêu**: Xác minh cây gia phả được hiển thị chính xác theo các mối quan hệ đã định nghĩa.
-   **Các bước thực hiện**:
    1.  Tạo một gia đình với ít nhất 3 thế hệ và các mối quan hệ cha-mẹ, vợ-chồng.
    2.  Truy cập trang "Cây Gia Phả".
-   **Kết quả mong đợi**: Cây gia phả hiển thị đúng cấu trúc phả hệ, các thế hệ và mối quan hệ được thể hiện rõ ràng.
-   **Thực tế**: (Để trống)
-   **Severity**: High
-   **Priority**: High

#### TC_TREE_02: Tương tác Zoom và Pan trên cây
-   **Mục tiêu**: Xác minh người dùng có thể tương tác (phóng to, thu nhỏ, kéo) trên cây gia phả.
-   **Các bước thực hiện**:
    1.  Truy cập trang "Cây Gia Phả".
    2.  Sử dụng con lăn chuột hoặc các nút điều khiển để phóng to/thu nhỏ.
    3.  Kéo chuột để di chuyển khung nhìn trên cây.
-   **Kết quả mong đợi**: Cây gia phả phóng to/thu nhỏ và di chuyển mượt mà theo thao tác của người dùng.
-   **Thực tế**: (Để trống)
-   **Severity**: Medium
-   **Priority**: Medium

### 3.4. Module: Quản lý Dòng họ (Family Management)

#### TC_FAMILY_01: Tạo dòng họ mới thành công
-   **Mục tiêu**: Xác minh người dùng có thể tạo một dòng họ/gia đình mới với thông tin hợp lệ.
-   **Các bước thực hiện**:
    1.  Truy cập trang quản lý dòng họ.
    2.  Nhấn nút "Tạo dòng họ mới".
    3.  Điền tên dòng họ và các thông tin khác (nếu có).
    4.  Nhấn "Lưu".
-   **Kết quả mong đợi**: Dòng họ mới được tạo thành công và hiển thị trong danh sách. Hệ thống hiển thị thông báo thành công.
-   **Thực tế**: (Để trống)
-   **Severity**: High
-   **Priority**: High

#### TC_FAMILY_02: Chỉnh sửa thông tin dòng họ
-   **Mục tiêu**: Xác minh người dùng có thể chỉnh sửa thông tin của một dòng họ hiện có.
-   **Các bước thực hiện**:
    1.  Truy cập trang quản lý dòng họ.
    2.  Chọn một dòng họ và nhấn nút "Chỉnh sửa".
    3.  Thay đổi tên hoặc mô tả của dòng họ.
    4.  Nhấn "Lưu".
-   **Kết quả mong đợi**: Thông tin dòng họ được cập nhật thành công và hiển thị đúng. Hệ thống hiển thị thông báo thành công.
-   **Thực tế**: (Để trống)
-   **Severity**: High
-   **Priority**: High

### 3.5. Module: Quản lý Quan hệ (Relationship Management)

#### TC_REL_01: Thêm mối quan hệ cha-con thành công
-   **Mục tiêu**: Xác minh người dùng có thể thêm mối quan hệ cha-con giữa hai thành viên.
-   **Các bước thực hiện**:
    1.  Truy cập trang chi tiết của thành viên A.
    2.  Nhấn nút "Thêm quan hệ".
    3.  Chọn thành viên B và loại quan hệ "Cha" hoặc "Mẹ".
    4.  Nhấn "Lưu".
-   **Kết quả mong đợi**: Mối quan hệ được tạo thành công và hiển thị trên cây gia phả và trang chi tiết thành viên. Hệ thống hiển thị thông báo thành công.
-   **Thực tế**: (Để trống)
-   **Severity**: High
-   **Priority**: High

#### TC_REL_02: Xóa mối quan hệ
-   **Mục tiêu**: Xác minh người dùng có thể xóa một mối quan hệ hiện có.
-   **Các bước thực hiện**:
    1.  Truy cập trang chi tiết của thành viên có mối quan hệ cần xóa.
    2.  Tìm mối quan hệ và nhấn nút "Xóa".
    3.  Xác nhận xóa.
-   **Kết quả mong đợi**: Mối quan hệ bị xóa thành công khỏi hệ thống và không còn hiển thị. Hệ thống hiển thị thông báo thành công.
-   **Thực tế**: (Để trống)
-   **Severity**: Medium
-   **Priority**: Medium

### 3.6. Module: File đính kèm (Attachments)

#### TC_ATTACH_01: Tải lên file đính kèm thành công
-   **Mục tiêu**: Xác minh người dùng có thể tải lên một file đính kèm cho thành viên.
-   **Các bước thực hiện**:
    1.  Truy cập trang chi tiết của một thành viên.
    2.  Nhấn nút "Thêm file đính kèm".
    3.  Chọn một file hợp lệ (ví dụ: ảnh, PDF).
    4.  Nhấn "Tải lên".
-   **Kết quả mong đợi**: File được tải lên thành công và hiển thị trong danh sách file đính kèm của thành viên. Hệ thống hiển thị thông báo thành công.
-   **Thực tế**: (Để trống)
-   **Severity**: Medium
-   **Priority**: Medium

#### TC_ATTACH_02: Xóa file đính kèm
-   **Mục tiêu**: Xác minh người dùng có thể xóa một file đính kèm.
-   **Các bước thực hiện**:
    1.  Truy cập trang chi tiết của thành viên có file đính kèm.
    2.  Tìm file cần xóa và nhấn nút "Xóa".
    3.  Xác nhận xóa.
-   **Kết quả mong đợi**: File bị xóa thành công khỏi hệ thống. Hệ thống hiển thị thông báo thành công.
-   **Thực tế**: (Để trống)
-   **Severity**: Medium
-   **Priority**: Medium