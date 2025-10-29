# Kịch bản Kiểm thử (Test Cases)

## Mục lục

- [1. Giới thiệu](#1-giới-thiệu)
- [2. Tóm tắt Test Cases](#2-tóm-tắt-test-cases)
- [3. Chi tiết Test Cases](#3-chi-tiết-test-cases)
  - [3.1. Module: Đăng nhập (Authentication)](#31-module-đăng-nhập-authentication)
  - [3.2. Module: Quản lý Thành viên (Member Management)](#32-module-quản-lý-thành-viên-member-management)
  - [3.3. Module: Quản lý Sự kiện (Event Management)](#33-module-quản-lý-sự-kiện-event-management)
  - [3.4. Module: Quản lý Dòng họ (Family Management)](#34-module-quản-lý-dòng-họ-family-management)
  - [3.5. Module: Quản lý Quan hệ (Relationship Management)](#35-module-quản-lý-quan-hệ-relationship-management)
  - [3.6. Module: File đính kèm (Attachments)](#36-module-file-đính-kèm-attachments)
  - [3.7. Module: Quản lý Tùy chọn Người dùng (User Preference Management)](#37-module-quản-lý-tùy-chọn-người-dùng-user-preference-management)
  - [3.8. Module: AI Biography](#38-module-ai-biography)
  - [3.9. Module: Xử lý Dữ liệu và Chia Chunk (Document Chunking)](#39-module-xử-lý-dữ-liệu-và-chia-chunk-document-chunking)
  - [3.10. Module: Cây Gia Phả (Family Tree)](#310-module-cây-gia-phả-family-tree)

---

## 1. Giới thiệu

Tài liệu này cung cấp các **Kịch bản Kiểm thử (Test Cases)** chi tiết cho dự án Cây Gia Phả. Mục đích của tài liệu này là đảm bảo tính đúng đắn, ổn định và chất lượng của các tính năng đã phát triển. Mỗi test case được mô tả rõ ràng, giúp QA Engineer có thể thực hiện kiểm thử một cách có hệ thống và Developer có thể tham khảo để hiểu rõ hơn về yêu cầu của tính năng.

Test cases đóng vai trò quan trọng trong:

*   **Đảm bảo chất lượng**: Xác minh rằng ứng dụng hoạt động đúng theo yêu cầu và không có lỗi.
*   **Hỗ trợ quy trình QA**: Cung cấp hướng dẫn từng bước cho việc kiểm thử thủ công và tự động.
*   **Minh bạch yêu cầu**: Làm rõ các tiêu chí chấp nhận của từng tính năng.
*   **Phát hiện lỗi sớm**: Giúp tìm ra các vấn đề trước khi chúng ảnh hưởng đến người dùng cuối.

## 2. Tóm tắt Test Cases

Bảng dưới đây cung cấp cái nhìn tổng quan về các test case chính trong dự án, được phân loại theo module và mức độ ưu tiên.

| ID Test Case | Title                                  | Module                 | Priority | Type              |
| :----------- | :------------------------------------- | :--------------------- | :------- | :---------------- |
| TC_LOGIN_01  | Đăng nhập thành công                   | Authentication         | High     | Manual/Automated  |
| TC_LOGIN_02  | Đăng nhập thất bại (mật khẩu sai)      | Authentication         | High     | Manual/Automated  |
| TC_LOGIN_03  | Đăng nhập thất bại (email không tồn tại)| Authentication         | High     | Manual/Automated  |
| TC_MEMBER_01 | Thêm thành viên mới thành công         | Member Management      | High     | Manual/Automated  |
| TC_MEMBER_02 | Thêm thành viên thất bại (thiếu họ tên) | Member Management      | High     | Manual/Automated  |
| TC_MEMBER_03 | Chỉnh sửa thông tin thành viên thành công| Member Management      | High     | Manual/Automated  |
| TC_EVENT_04  | Tìm kiếm sự kiện thành công            | Event Management       | High     | Manual/Automated  |
| TC_TREE_01   | Hiển thị cây gia phả đúng cấu trúc     | Family Tree            | High     | Manual/Automated  |
| TC_TREE_02   | Tương tác Zoom và Pan trên cây         | Family Tree            | Medium   | Manual            |
| TC_FAMILY_01 | Tạo dòng họ mới thành công             | Family Management      | High     | Manual/Automated  |
| TC_FAMILY_02 | Chỉnh sửa thông tin dòng họ            | Family Management      | High     | Manual/Automated  |
| TC_REL_01    | Thêm mối quan hệ cha-con thành công    | Relationship Management| High     | Manual/Automated  |
| TC_REL_02    | Xóa mối quan hệ                        | Relationship Management| Medium   | Manual/Automated  |
| TC_REL_03    | Thêm mối quan hệ thành công            | Relationship Management| High     | Manual/Automated  |
| TC_REL_04    | Chỉnh sửa mối quan hệ thành công       | Relationship Management| High     | Manual/Automated  |
| TC_ATTACH_01 | Tải lên file đính kèm thành công       | Attachments            | Medium   | Manual            |
| TC_ATTACH_02 | Xóa file đính kèm                      | Attachments            | Medium   | Manual            |
| TC_USERPREF_01 | Cập nhật tùy chọn người dùng thành công | User Preferences       | High     | Manual/Automated  |
| TC_USERPREF_02 | Cập nhật tùy chọn người dùng thất bại (dữ liệu không hợp lệ) | User Preferences       | Medium   | Manual/Automated  |
| TC_USERPREF_03 | Kiểm tra tùy chọn người dùng mặc định | User Preferences       | Medium   | Manual/Automated  |
| TC_AIBIO_01    | Tạo tiểu sử AI thành công             | AI Biography           | High     | Manual/Automated  |
| TC_CHUNK_01    | Tải lên và xử lý file thành công       | Document Chunking      | High     | Manual/Automated  |

## 3. Chi tiết Test Cases

### 3.1. Module: Đăng nhập (Authentication)

#### TC_LOGIN_01: Đăng nhập thành công với thông tin hợp lệ
-   **Mục tiêu**: Xác minh người dùng có thể đăng nhập thành công vào hệ thống bằng cách sử dụng thông tin đăng nhập hợp lệ.
-   **Điều kiện tiên quyết (Preconditions)**:
    *   Người dùng đã có tài khoản hợp lệ trong hệ thống (ví dụ: `administrator@localhost` với mật khẩu `Administrator1!`).
    *   Ứng dụng Frontend và Backend đang chạy.
-   **Các bước thực hiện**:
    1.  Mở trình duyệt web và truy cập vào URL của ứng dụng Frontend (ví dụ: `http://localhost:5173`).
    2.  Nhấp vào nút hoặc liên kết "Đăng nhập" để điều hướng đến trang đăng nhập.
    3.  Trong trường "Email" hoặc "Tên đăng nhập", nhập `administrator@localhost`.
    4.  Trong trường "Mật khẩu", nhập `Administrator1!`.
    5.  Nhấn nút "Đăng nhập".
-   **Kết quả mong đợi**: 
    *   Người dùng được chuyển hướng thành công đến trang Tổng quan (Dashboard) hoặc trang chính của ứng dụng.
    *   Hiển thị thông báo đăng nhập thành công (nếu có).
    *   Tên người dùng hoặc thông tin tài khoản được hiển thị trên giao diện.
-   **Thực tế**: (Để trống)
-   **Severity**: High
-   **Priority**: High
-   **Type**: Manual/Automated

#### TC_LOGIN_02: Đăng nhập thất bại với mật khẩu sai
-   **Mục tiêu**: Xác minh hệ thống xử lý đúng khi người dùng nhập sai mật khẩu cho một tài khoản tồn tại.
-   **Điều kiện tiên quyết (Preconditions)**:
    *   Người dùng đã có tài khoản hợp lệ trong hệ thống (ví dụ: `administrator@localhost`).
    *   Ứng dụng Frontend và Backend đang chạy.
-   **Các bước thực hiện**:
    1.  Mở trình duyệt web và truy cập vào URL của ứng dụng Frontend.
    2.  Nhấp vào nút hoặc liên kết "Đăng nhập".
    3.  Trong trường "Email" hoặc "Tên đăng nhập", nhập `administrator@localhost`.
    4.  Trong trường "Mật khẩu", nhập một mật khẩu *sai* (ví dụ: `wrongpassword`).
    5.  Nhấn nút "Đăng nhập".
-   **Kết quả mong đợi**: 
    *   Hệ thống hiển thị thông báo lỗi "Email hoặc mật khẩu không chính xác" hoặc thông báo tương tự.
    *   Người dùng vẫn ở trang Đăng nhập.
    *   Không có chuyển hướng đến trang Dashboard.
-   **Thực tế**: (Để trống)
-   **Severity**: High
-   **Priority**: High
-   **Type**: Manual/Automated

#### TC_LOGIN_03: Đăng nhập thất bại với email không tồn tại
-   **Mục tiêu**: Xác minh hệ thống xử lý đúng khi người dùng nhập email không tồn tại trong hệ thống.
-   **Điều kiện tiên quyết (Preconditions)**:
    *   Ứng dụng Frontend và Backend đang chạy.
    *   Email được sử dụng không tồn tại trong cơ sở dữ liệu người dùng.
-   **Các bước thực hiện**:
    1.  Mở trình duyệt web và truy cập vào URL của ứng dụng Frontend.
    2.  Nhấp vào nút hoặc liên kết "Đăng nhập".
    3.  Trong trường "Email" hoặc "Tên đăng nhập", nhập một email *không tồn tại* (ví dụ: `nonexistent@example.com`).
    4.  Trong trường "Mật khẩu", nhập một mật khẩu bất kỳ (ví dụ: `anypassword`).
    5.  Nhấn nút "Đăng nhập".
-   **Kết quả mong đợi**: 
    *   Hệ thống hiển thị thông báo lỗi "Email hoặc mật khẩu không chính xác" hoặc thông báo tương tự.
    *   Người dùng vẫn ở trang Đăng nhập.
    *   Không có chuyển hướng đến trang Dashboard.
-   **Thực tế**: (Để trống)
-   **Severity**: High
-   **Priority**: High
-   **Type**: Manual/Automated

### 3.2. Module: Quản lý Thành viên (Member Management)

#### TC_MEMBER_01: Thêm thành viên mới thành công
-   **Mục tiêu**: Xác minh người dùng có thể thêm thành viên mới vào một dòng họ với đầy đủ thông tin hợp lệ.
-   **Điều kiện tiên quyết (Preconditions)**:
    *   Người dùng đã đăng nhập vào hệ thống.
    *   Đã có ít nhất một dòng họ tồn tại trong hệ thống.
    *   Ứng dụng Frontend và Backend đang chạy.
-   **Các bước thực hiện**:
    1.  Đăng nhập vào hệ thống.
    2.  Truy cập trang quản lý thành viên hoặc trang chi tiết dòng họ.
    3.  Nhấn nút "Thêm thành viên mới" (hoặc biểu tượng tương tự).
    4.  Điền đầy đủ các trường thông tin hợp lệ vào biểu mẫu (ví dụ: Họ, Tên, Ngày sinh, Giới tính, chọn Dòng họ).
    5.  Nhấn nút "Lưu" hoặc "Tạo thành viên".
-   **Kết quả mong đợi**: 
    *   Thành viên mới được thêm vào danh sách thành viên và hiển thị đúng thông tin đã nhập.
    *   Hệ thống hiển thị thông báo thành công (ví dụ: "Thêm thành viên thành công!").
    *   Thành viên mới xuất hiện trên cây gia phả (nếu có).
-   **Thực tế**: (Để trống)
-   **Severity**: High
-   **Priority**: High
-   **Type**: Manual/Automated

#### TC_MEMBER_02: Thêm thành viên thất bại khi thiếu họ tên
-   **Mục tiêu**: Xác minh hệ thống không cho phép thêm thành viên khi thiếu các trường bắt buộc như "Họ" hoặc "Tên".
-   **Điều kiện tiên quyết (Preconditions)**:
    *   Người dùng đã đăng nhập vào hệ thống.
    *   Đã có ít nhất một dòng họ tồn tại trong hệ thống.
    *   Ứng dụng Frontend và Backend đang chạy.
-   **Các bước thực hiện**:
    1.  Đăng nhập vào hệ thống.
    2.  Truy cập trang quản lý thành viên hoặc trang chi tiết dòng họ.
    3.  Nhấn nút "Thêm thành viên mới".
    4.  Để trống trường "Họ" hoặc "Tên" (hoặc cả hai).
    5.  Điền các trường thông tin khác hợp lệ (ví dụ: Ngày sinh, Giới tính, chọn Dòng họ).
    6.  Nhấn nút "Lưu" hoặc "Tạo thành viên".
-   **Kết quả mong đợi**: 
    *   Hệ thống hiển thị thông báo lỗi validation tại trường "Họ" hoặc "Tên" (ví dụ: "Họ không được để trống.").
    *   Thành viên không được lưu vào hệ thống.
    *   Người dùng vẫn ở trang thêm thành viên.
-   **Thực tế**: (Để trống)
-   **Severity**: High
-   **Priority**: High
-   **Type**: Manual/Automated

#### TC_MEMBER_03: Chỉnh sửa thông tin thành viên thành công
-   **Mục tiêu**: Xác minh người dùng có thể chỉnh sửa thông tin của một thành viên hiện có trong hệ thống.
-   **Điều kiện tiên quyết (Preconditions)**:
    *   Người dùng đã đăng nhập vào hệ thống.
    *   Đã có ít nhất một thành viên tồn tại trong hệ thống.
    *   Ứng dụng Frontend và Backend đang chạy.
-   **Các bước thực hiện**:
    1.  Đăng nhập vào hệ thống.
    2.  Truy cập trang quản lý thành viên hoặc trang chi tiết của một thành viên.
    3.  Chọn một thành viên và nhấn nút "Chỉnh sửa" (hoặc biểu tượng bút chì).
    4.  Thay đổi một hoặc nhiều trường thông tin hợp lệ (ví dụ: cập nhật nghề nghiệp, biệt danh, hoặc ngày sinh).
    5.  Nhấn nút "Lưu" hoặc "Cập nhật".
-   **Kết quả mong đợi**: 
    *   Thông tin thành viên được cập nhật thành công và hiển thị đúng trên trang chi tiết thành viên và các nơi khác có liên quan.
    *   Hệ thống hiển thị thông báo thành công (ví dụ: "Cập nhật thành viên thành công!").
-   **Thực tế**: (Để trống)
-   **Severity**: High
-   **Priority**: High
-   **Type**: Manual/Automated

### 3.3. Module: Quản lý Sự kiện (Event Management)

#### TC_EVENT_04: Tìm kiếm sự kiện thành công
-   **Mục tiêu**: Xác minh người dùng có thể tìm kiếm sự kiện thành công với các tiêu chí khác nhau.
-   **Điều kiện tiên quyết (Preconditions)**:
    *   Người dùng đã đăng nhập vào hệ thống.
    *   Đã có ít nhất một sự kiện tồn tại trong hệ thống.
    *   Ứng dụng Frontend và Backend đang chạy.
-   **Các bước thực hiện**:
    1.  Đăng nhập vào hệ thống.
    2.  Truy cập trang quản lý sự kiện.
    3.  Mở rộng phần "Tìm kiếm Nâng cao".
    4.  Nhập "họp" vào ô "Tìm kiếm theo tên hoặc mô tả sự kiện...".
    5.  Chọn "Họp mặt" từ danh sách "Loại Sự kiện".
    6.  Nhấn nút "Áp dụng Bộ lọc".
-   **Kết quả mong đợi**: 
    *   Danh sách sự kiện được cập nhật để chỉ hiển thị các sự kiện có tên hoặc mô tả chứa "họp" và có loại là "Họp mặt".
-   **Thực tế**: (Để trống)
-   **Severity**: High
-   **Priority**: High
-   **Type**: Manual/Automated



### 3.4. Module: Quản lý Dòng họ (Family Management)

#### TC_FAMILY_01: Tạo dòng họ mới thành công
-   **Mục tiêu**: Xác minh người dùng có thể tạo một dòng họ/gia đình mới với thông tin hợp lệ.
-   **Điều kiện tiên quyết (Preconditions)**:
    *   Người dùng đã đăng nhập vào hệ thống.
    *   Ứng dụng Frontend và Backend đang chạy.
-   **Các bước thực hiện**:
    1.  Đăng nhập vào hệ thống.
    2.  Truy cập trang quản lý dòng họ (ví dụ: `/family`).
    3.  Nhấn nút "Tạo dòng họ mới" (hoặc biểu tượng `+`).
    4.  Điền tên dòng họ (ví dụ: "Dòng họ Trần") và các thông tin khác hợp lệ vào biểu mẫu (ví dụ: mô tả, địa chỉ, chế độ hiển thị).
    5.  Nhấn nút "Lưu" hoặc "Tạo".
-   **Kết quả mong đợi**: 
    *   Dòng họ mới được tạo thành công và hiển thị trong danh sách các dòng họ.
    *   Hệ thống hiển thị thông báo thành công (ví dụ: "Tạo dòng họ thành công!").
    *   Người dùng được chuyển hướng đến trang quản lý dòng họ hoặc trang chi tiết dòng họ vừa tạo.
-   **Thực tế**: (Để trống)
-   **Severity**: High
-   **Priority**: High
-   **Type**: Manual/Automated

#### TC_FAMILY_02: Chỉnh sửa thông tin dòng họ
-   **Mục tiêu**: Xác minh người dùng có thể chỉnh sửa thông tin của một dòng họ/gia đình hiện có trong hệ thống.
-   **Điều kiện tiên quyết (Preconditions)**:
    *   Người dùng đã đăng nhập vào hệ thống.
    *   Đã có ít nhất một dòng họ tồn tại trong hệ thống.
    *   Ứng dụng Frontend và Backend đang chạy.
-   **Các bước thực hiện**:
    1.  Đăng nhập vào hệ thống.
    2.  Truy cập trang quản lý dòng họ.
    3.  Chọn một dòng họ từ danh sách và nhấn nút "Chỉnh sửa" (hoặc biểu tượng bút chì).
    4.  Thay đổi một hoặc nhiều trường thông tin hợp lệ (ví dụ: cập nhật tên dòng họ, mô tả, địa chỉ).
    5.  Nhấn nút "Lưu" hoặc "Cập nhật".
-   **Kết quả mong đợi**: 
    *   Thông tin dòng họ được cập nhật thành công và hiển thị đúng trong danh sách và trang chi tiết dòng họ.
    *   Hệ thống hiển thị thông báo thành công (ví dụ: "Cập nhật dòng họ thành công!").
-   **Thực tế**: (Để trống)
-   **Severity**: High
-   **Priority**: High
-   **Type**: Manual/Automated

### 3.5. Module: Quản lý Quan hệ (Relationship Management)

#### TC_REL_01: Thêm mối quan hệ cha-con thành công
-   **Mục tiêu**: Xác minh người dùng có thể thêm mối quan hệ cha-con giữa hai thành viên hiện có trong hệ thống.
-   **Điều kiện tiên quyết (Preconditions)**:
    *   Người dùng đã đăng nhập vào hệ thống.
    *   Đã có ít nhất hai thành viên tồn tại trong cùng một dòng họ.
    *   Ứng dụng Frontend và Backend đang chạy.
-   **Các bước thực hiện**:
    1.  Đăng nhập vào hệ thống.
    2.  Truy cập trang quản lý quan hệ.
    3.  Nhấn nút "Thêm Quan hệ Mới".
    4.  Chọn thành viên nguồn và thành viên đích.
    5.  Chọn loại mối quan hệ (ví dụ: "Cha mẹ").
    6.  Nhấn nút "Lưu".
-   **Kết quả mong đợi**: 
    *   Mối quan hệ mới được tạo thành công và hiển thị trong danh sách quan hệ.
    *   Hệ thống hiển thị thông báo thành công.
-   **Thực tế**: (Để trống)
-   **Severity**: High
-   **Priority**: High
-   **Type**: Manual/Automated

#### TC_REL_02: Chỉnh sửa mối quan hệ thành công
-   **Mục tiêu**: Xác minh người dùng có thể chỉnh sửa thông tin của một mối quan hệ hiện có.
-   **Điều kiện tiên quyết (Preconditions)**:
    *   Người dùng đã đăng nhập vào hệ thống.
    *   Đã có ít nhất một mối quan hệ tồn tại.
    *   Ứng dụng Frontend và Backend đang chạy.
-   **Các bước thực hiện**:
    1.  Đăng nhập vào hệ thống.
    2.  Truy cập trang quản lý quan hệ.
    3.  Chọn một mối quan hệ và nhấn nút "Chỉnh sửa".
    4.  Thay đổi loại mối quan hệ hoặc thành viên.
    5.  Nhấn nút "Lưu".
-   **Kết quả mong đợi**: 
    *   Mối quan hệ được cập nhật thành công và hiển thị đúng trong danh sách.
    *   Hệ thống hiển thị thông báo thành công.
-   **Thực tế**: (Để trống)
-   **Severity**: High
-   **Priority**: High
-   **Type**: Manual/Automated

#### TC_REL_03: Xóa mối quan hệ thành công
-   **Mục tiêu**: Xác minh người dùng có thể xóa một mối quan hệ hiện có.
-   **Điều kiện tiên quyết (Preconditions)**:
    *   Người dùng đã đăng nhập vào hệ thống.
    *   Đã có ít nhất một mối quan hệ tồn tại.
    *   Ứng dụng Frontend và Backend đang chạy.
-   **Các bước thực hiện**:
    1.  Đăng nhập vào hệ thống.
    2.  Truy cập trang quản lý quan hệ.
    3.  Chọn một mối quan hệ và nhấn nút "Xóa".
    4.  Xác nhận xóa trong hộp thoại.
-   **Kết quả mong đợi**: 
    *   Mối quan hệ bị xóa thành công khỏi hệ thống.
    *   Hệ thống hiển thị thông báo thành công.
-   **Thực tế**: (Để trống)
-   **Severity**: High
-   **Priority**: High
-   **Type**: Manual/Automated

#### TC_REL_04: Tìm kiếm mối quan hệ
-   **Mục tiêu**: Xác minh người dùng có thể tìm kiếm mối quan hệ với các tiêu chí khác nhau.
-   **Điều kiện tiên quyết (Preconditions)**:
    *   Người dùng đã đăng nhập vào hệ thống.
    *   Đã có ít nhất một mối quan hệ tồn tại.
    *   Ứng dụng Frontend và Backend đang chạy.
-   **Các bước thực hiện**:
    1.  Đăng nhập vào hệ thống.
    2.  Truy cập trang quản lý quan hệ.
    3.  Mở rộng phần "Tìm kiếm Quan hệ".
    4.  Chọn thành viên nguồn hoặc đích, hoặc loại mối quan hệ.
    5.  Nhấn nút "Áp dụng".
-   **Kết quả mong đợi**: 
    *   Danh sách mối quan hệ được cập nhật để chỉ hiển thị các mối quan hệ phù hợp với tiêu chí tìm kiếm.
-   **Thực tế**: (Để trống)
-   **Severity**: Medium
-   **Priority**: Medium
-   **Type**: Manual/Automated

### 3.6. Module: File đính kèm (Attachments)

#### TC_ATTACH_01: Tải lên file đính kèm thành công
-   **Mục tiêu**: Xác minh người dùng có thể tải lên một file đính kèm hợp lệ cho một thành viên.
-   **Điều kiện tiên quyết (Preconditions)**:
    *   Người dùng đã đăng nhập vào hệ thống.
    *   Đã có ít nhất một thành viên tồn tại trong hệ thống.
    *   Ứng dụng Frontend và Backend đang chạy.
    *   Có sẵn một file hợp lệ để tải lên (ví dụ: ảnh JPG, PDF).
-   **Các bước thực hiện**:
    1.  Đăng nhập vào hệ thống.
    2.  Truy cập trang chi tiết của một thành viên.
    3.  Tìm đến phần "File đính kèm" và nhấn nút "Thêm file đính kèm" (hoặc biểu tượng tải lên).
    4.  Chọn một file hợp lệ từ máy tính của bạn.
    5.  Nhấn nút "Tải lên" hoặc "Xác nhận".
-   **Kết quả mong đợi**: 
    *   File được tải lên thành công và hiển thị trong danh sách file đính kèm của thành viên.
    *   Hệ thống hiển thị thông báo thành công (ví dụ: "Tải file thành công!").
    *   File có thể được xem hoặc tải xuống.
-   **Thực tế**: (Để trống)
-   **Severity**: Medium
-   **Priority**: Medium
-   **Type**: Manual

#### TC_ATTACH_02: Xóa file đính kèm
-   **Mục tiêu**: Xác minh người dùng có thể xóa một file đính kèm đã tải lên cho một thành viên.
-   **Điều kiện tiên quyết (Preconditions)**:
    *   Người dùng đã đăng nhập vào hệ thống.
    *   Đã có ít nhất một thành viên với một file đính kèm tồn tại.
    *   Ứng dụng Frontend và Backend đang chạy.
-   **Các bước thực hiện**:
    1.  Đăng nhập vào hệ thống.
    2.  Truy cập trang chi tiết của thành viên có file đính kèm.
    3.  Tìm đến phần "File đính kèm" và nhấn nút "Xóa" (hoặc biểu tượng thùng rác) bên cạnh file cần xóa.
    4.  Xác nhận hành động xóa trong hộp thoại xác nhận (nếu có).
-   **Kết quả mong đợi**: 
    *   File bị xóa thành công khỏi danh sách file đính kèm của thành viên.
    *   Hệ thống hiển thị thông báo thành công (ví dụ: "Xóa file thành công!").
    *   File không còn truy cập được.
-   **Thực tế**: (Để trống)
-   **Severity**: Medium
-   **Priority**: Medium
-   **Type**: Manual

### 3.7. Module: Quản lý Tùy chọn Người dùng (User Preference Management)

#### TC_USERPREF_01: Cập nhật tùy chọn người dùng thành công
-   **Mục tiêu**: Xác minh người dùng có thể cập nhật các tùy chọn cá nhân (chủ đề, ngôn ngữ, cài đặt thông báo) thành công.
-   **Điều kiện tiên quyết (Preconditions)**:
    *   Người dùng đã đăng nhập vào hệ thống.
    *   Ứng dụng Frontend và Backend đang chạy.
-   **Các bước thực hiện**:
    1.  Đăng nhập vào hệ thống.
    2.  Truy cập trang "Cài đặt người dùng" -> tab "Tùy chọn".
    3.  Thay đổi một hoặc nhiều tùy chọn (ví dụ: chọn chủ đề "Tối", ngôn ngữ "Tiếng Anh", bật "Thông báo qua Email").
    4.  Nhấn nút "Lưu".
-   **Kết quả mong đợi**: 
    *   Các tùy chọn được lưu thành công và áp dụng ngay lập tức (ví dụ: giao diện chuyển sang chủ đề tối, ngôn ngữ thay đổi).
    *   Hệ thống hiển thị thông báo thành công (ví dụ: "Lưu tùy chọn thành công!").
-   **Thực tế**: (Để trống)
-   **Severity**: High
-   **Priority**: High
-   **Type**: Manual/Automated

#### TC_USERPREF_02: Cập nhật tùy chọn người dùng thất bại (dữ liệu không hợp lệ)
-   **Mục tiêu**: Xác minh hệ thống xử lý đúng khi người dùng cố gắng cập nhật tùy chọn với dữ liệu không hợp lệ (ví dụ: định dạng ngôn ngữ không đúng).
-   **Điều kiện tiên quyết (Preconditions)**:
    *   Người dùng đã đăng nhập vào hệ thống.
    *   Ứng dụng Frontend và Backend đang chạy.
-   **Các bước thực hiện**:
    1.  Đăng nhập vào hệ thống.
    2.  Truy cập trang "Cài đặt người dùng" -> tab "Tùy chọn".
    3.  Cố gắng nhập hoặc chọn một giá trị không hợp lệ cho một tùy chọn (ví dụ: nhập "XYZ" vào trường ngôn ngữ nếu nó là trường nhập tự do, hoặc chọn một chủ đề không tồn tại thông qua thao tác chỉnh sửa DOM).
    4.  Nhấn nút "Lưu".
-   **Kết quả mong đợi**: 
    *   Hệ thống hiển thị thông báo lỗi validation hoặc lỗi chung (ví dụ: "Dữ liệu không hợp lệ.").
    *   Các tùy chọn không được lưu.
    *   Người dùng vẫn ở trang cài đặt.
-   **Thực tế**: (Để trống)
-   **Severity**: Medium
-   **Priority**: Medium
-   **Type**: Manual/Automated

#### TC_USERPREF_03: Kiểm tra tùy chọn người dùng mặc định
-   **Mục tiêu**: Xác minh rằng khi người dùng lần đầu truy cập hoặc không có tùy chọn nào được lưu, các giá trị mặc định được áp dụng đúng.
-   **Điều kiện tiên quyết (Preconditions)**:
    *   Người dùng đã đăng nhập vào hệ thống.
    *   Người dùng chưa từng lưu bất kỳ tùy chọn nào hoặc tùy chọn đã được reset về mặc định.
    *   Ứng dụng Frontend và Backend đang chạy.
-   **Các bước thực hiện**:
    1.  Đăng nhập vào hệ thống.
    2.  Truy cập trang "Cài đặt người dùng" -> tab "Tùy chọn".
    3.  Quan sát các giá trị được hiển thị cho chủ đề, ngôn ngữ và cài đặt thông báo.
-   **Kết quả mong đợi**: 
    *   Các tùy chọn hiển thị giá trị mặc định (ví dụ: chủ đề "Sáng", ngôn ngữ "Tiếng Việt", thông báo tắt).
-   **Thực tế**: (Để trống)
-   **Severity**: Medium
-   **Priority**: Medium
-   **Type**: Manual/Automated

### 3.8. Module: AI Biography

#### TC_AIBIO_01: Tạo tiểu sử AI thành công
-   **Mục tiêu**: Xác minh người dùng có thể tạo tiểu sử cho một thành viên bằng AI và lưu nó.
-   **Điều kiện tiên quyết (Preconditions)**:
    *   Người dùng đã đăng nhập vào hệ thống.
    *   Đã có ít nhất một thành viên với thông tin cơ bản (tên, ngày sinh, v.v.).
    *   Ứng dụng Frontend và Backend đang chạy.
    *   Dịch vụ AI đã được cấu hình và hoạt động.
-   **Các bước thực hiện**:
    1.  Đăng nhập vào hệ thống.
    2.  Truy cập trang chi tiết của một thành viên.
    3.  Nhấn nút "Tạo tiểu sử AI" hoặc tương tự.
    4.  Chọn kiểu giọng văn (ví dụ: "Lịch sử").
    5.  Nhập một prompt tùy chỉnh (nếu cần) hoặc sử dụng chế độ tự động.
    6.  Nhấn nút "Tạo tiểu sử".
    7.  Sau khi tiểu sử được tạo, nhấn nút "Lưu vào hồ sơ".
-   **Kết quả mong đợi**: 
    *   Tiểu sử được AI tạo ra và hiển thị trên giao diện.
    *   Tiểu sử được lưu thành công vào hồ sơ thành viên.
    *   Hệ thống hiển thị thông báo thành công.
-   **Thực tế**: (Để trống)
-   **Severity**: High
-   **Priority**: High
-   **Type**: Manual/Automated

### 3.9. Module: Xử lý Dữ liệu và Chia Chunk (Document Chunking)

#### TC_CHUNK_01: Tải lên và xử lý file thành công
-   **Mục tiêu**: Xác minh người dùng có thể tải lên một tệp (PDF/TXT) và hệ thống xử lý, chia nhỏ nó thành các chunk.
-   **Điều kiện tiên quyết (Preconditions)**:
    *   Người dùng đã đăng nhập vào hệ thống.
    *   Đã có ít nhất một dòng họ tồn tại.
    *   Ứng dụng Frontend và Backend đang chạy.
    *   Có sẵn một tệp PDF hoặc TXT hợp lệ để tải lên.
-   **Các bước thực hiện**:
    1.  Đăng nhập vào hệ thống.
    2.  Truy cập trang quản lý chunk (ví dụ: `/admin/chunks`).
    3.  Nhấn nút "Tải lên tài liệu".
    4.  Chọn một tệp PDF hoặc TXT hợp lệ.
    5.  Điền các thông tin metadata cần thiết (File ID, Family ID, Category, Created By).
    6.  Nhấn nút "Tải lên".
-   **Kết quả mong đợi**: 
    *   Tệp được tải lên thành công.
    *   Hệ thống xử lý tệp và hiển thị danh sách các chunk đã được tạo.
    *   Hệ thống hiển thị thông báo thành công (ví dụ: "Đã tải lên và xử lý X chunk thành công.").
-   **Thực tế**: (Để trống)
-   **Severity**: High
-   **Priority**: High
-   **Type**: Manual/Automated

### 3.10. Module: Cây Gia Phả (Family Tree)

#### TC_TREE_01: Hiển thị cây gia phả đúng cấu trúc
-   **Mục tiêu**: Xác minh cây gia phả được hiển thị chính xác theo các mối quan hệ đã định nghĩa trong hệ thống.
-   **Điều kiện tiên quyết (Preconditions)**:
    *   Người dùng đã đăng nhập vào hệ thống.
    *   Đã có ít nhất một dòng họ với một số thành viên và mối quan hệ (cha-mẹ, vợ-chồng) được tạo sẵn.
    *   Ứng dụng Frontend và Backend đang chạy.
-   **Các bước thực hiện**:
    1.  Đăng nhập vào hệ thống.
    2.  Truy cập trang "Cây Gia Phả" hoặc chọn một dòng họ để xem cây gia phả.
    3.  Quan sát cấu trúc hiển thị của cây gia phả.
-   **Kết quả mong đợi**: 
    *   Cây gia phả hiển thị đúng cấu trúc phả hệ, các thế hệ và mối quan hệ (cha-mẹ, vợ-chồng, con cái) được thể hiện rõ ràng và chính xác.
    *   Các thành viên được kết nối bằng các đường liên kết thể hiện mối quan hệ.
-   **Thực tế**: (Để trống)
-   **Severity**: High
-   **Priority**: High
-   **Type**: Manual/Automated

#### TC_TREE_02: Tương tác Zoom và Pan trên cây
-   **Mục tiêu**: Xác minh người dùng có thể tương tác (phóng to, thu nhỏ, kéo) trên cây gia phả một cách mượt mà và chính xác.
-   **Điều kiện tiên quyết (Preconditions)**:
    *   Người dùng đã đăng nhập vào hệ thống.
    *   Đã có ít nhất một dòng họ với cây gia phả được hiển thị trên trang.
    *   Ứng dụng Frontend và Backend đang chạy.
-   **Các bước thực hiện**:
    1.  Đăng nhập vào hệ thống và truy cập trang "Cây Gia Phả".
    2.  Sử dụng con lăn chuột (scroll wheel) hoặc các nút điều khiển trên giao diện để phóng to (zoom in) và thu nhỏ (zoom out) cây gia phả.
    3.  Nhấn giữ chuột trái và kéo (drag) để di chuyển khung nhìn (pan) trên cây gia phả.
-   **Kết quả mong đợi**: 
    *   Cây gia phả phóng to/thu nhỏ và di chuyển mượt mà theo thao tác của người dùng.
    *   Các thành viên và mối quan hệ vẫn hiển thị rõ ràng sau khi phóng to/thu nhỏ.
    *   Không có lỗi hoặc giật lag xảy ra trong quá trình tương tác.
-   **Thực tế**: (Để trống)
-   **Severity**: Medium
-   **Priority**: Medium
-   **Type**: Manual