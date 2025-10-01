# Product Backlog

## Mục lục

- [1. Giới thiệu](#1-giới-thiệu)
- [2. To Do](#2-to-do)
  - [2.1. Module: Cây Gia Phả](#21-module-cây-gia-phả)
  - [2.2. Module: Quản lý Quan hệ](#22-module-quản-lý-quan-hệ)
  - [2.3. Module: Dữ liệu & Báo cáo](#23-module-dữ-liệu--báo-cáo)
  - [2.4. Module: AI & Tích hợp](#24-module-ai--tích-hợp)
- [3. In Progress](#3-in-progress)
  - [3.1. Module: Quản lý Thành viên](#31-module-quản-lý-thành-viên)
- [4. Done](#4-done)
  - [4.1. Module: Xác thực](#41-module-xác-thực)
  - [4.2. Module: Quản lý Thành viên](#42-module-quản-lý-thành-viên)

---

## 1. Giới thiệu

Tài liệu này chứa danh sách các User Story (câu chuyện người dùng) cho dự án Cây Gia Phả, được tổ chức theo trạng thái phát triển và nhóm theo các module/epic. Mỗi User Story mô tả một tính năng từ góc nhìn của người dùng cuối, bao gồm vai trò, mong muốn và tiêu chí chấp nhận rõ ràng.

## 2. To Do

### 2.1. Module: Cây Gia Phả

#### US_001: Xem cây gia phả (Đa kiểu hiển thị)
-   **User Story**: Là người dùng, tôi muốn xem cây gia phả với nhiều kiểu hiển thị khác nhau (bao gồm theo giới tính) để dễ dàng theo dõi và hiểu các mối quan hệ.
-   **Priority**: High
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Hệ thống cung cấp ít nhất 2-3 kiểu hiển thị cây gia phả khác nhau (ví dụ: sơ đồ dọc, sơ đồ ngang, sơ đồ hình quạt).
    -   Có chế độ xem riêng biệt theo giới tính (ví dụ: làm nổi bật nam/nữ, hoặc chỉ hiển thị một giới tính).
    -   Người dùng có thể dễ dàng chuyển đổi giữa các kiểu hiển thị.
    -   Mỗi kiểu hiển thị phải thể hiện rõ ràng các mối quan hệ giữa các thành viên.
    -   Các kiểu hiển thị phải có khả năng phóng to/thu nhỏ và di chuyển.

#### US_002: Thay đổi nút gốc cây gia phả
-   **User Story**: Là người dùng, tôi muốn chọn một thành viên bất kỳ và vẽ lại cây gia phả với thành viên đó làm nút gốc.
-   **Priority**: Medium
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Khi người dùng chọn một thành viên, cây gia phả sẽ được vẽ lại với thành viên đó làm nút gốc.
    -   Thành viên được chọn phải hiển thị ở vị trí trung tâm hoặc vị trí gốc của cây mới.
    -   Các mối quan hệ của thành viên gốc mới phải được hiển thị rõ ràng.
    -   Phải có cách để quay lại chế độ xem cây gia phả ban đầu hoặc nút gốc trước đó.

#### US_003: In poster gia phả
-   **User Story**: Là người dùng, tôi muốn có thể tích hợp tính năng in poster cây gia phả để tạo ra bản in lớn, chất lượng cao.
-   **Priority**: Low
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Người dùng có thể chọn bố cục và kích thước cho poster gia phả.
    -   Hệ thống cung cấp giao diện xem trước poster trước khi in.
    -   Có tùy chọn để tùy chỉnh thông tin hiển thị trên poster.
    -   Hệ thống tích hợp với dịch vụ in ấn bên ngoài hoặc cung cấp tệp tin chất lượng cao để người dùng tự in.

#### US_004: In 3D cây gia phả
-   **User Story**: Là người dùng, tôi muốn có thể tích hợp tính năng in 3D cây gia phả để tạo ra mô hình vật lý.
-   **Priority**: Low
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Người dùng có thể chọn các thành viên và mối quan hệ để đưa vào mô hình 3D.
    -   Hệ thống cung cấp giao diện xem trước mô hình 3D trước khi in.
    -   Có các tùy chọn để tùy chỉnh thiết kế mô hình 3D.
    -   Hệ thống tích hợp với dịch vụ in 3D bên ngoài hoặc cung cấp tệp mô hình 3D.

### 2.2. Module: Quản lý Quan hệ

#### US_005: Quản lý mối quan hệ phức tạp
-   **User Story**: Là người dùng, tôi muốn thêm các loại mối quan hệ khác nhau (con nuôi, anh/chị/em cùng cha khác mẹ) để phản ánh chính xác mối quan hệ phức tạp.
-   **Priority**: High
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Khi thêm/chỉnh sửa mối quan hệ, chọn loại từ danh sách (ruột thịt, con nuôi, cha dượng/mẹ kế).
    -   Các mối quan hệ hiển thị rõ ràng trên cây gia phả.

#### US_006: Tìm kiếm mối quan hệ
-   **User Story**: Là người dùng, tôi muốn tìm kiếm mối quan hệ giữa hai thành viên bất kỳ trong cây gia phả.
-   **Priority**: Medium
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Người dùng có thể chọn hai thành viên để tìm kiếm mối quan hệ.
    -   Hệ thống xác định và hiển thị loại mối quan hệ (ví dụ: cha con, vợ chồng, anh chị em).
    -   Hệ thống hiển thị đường dẫn hoặc chuỗi liên kết giữa hai thành viên trên cây gia phả.
    -   Nếu không có mối quan hệ trực tiếp, hệ thống thông báo.

### 2.3. Module: Dữ liệu & Báo cáo

#### US_007: Xuất/Nhập cây gia phả (GEDCOM)
-   **User Story**: Là người dùng, tôi muốn xuất và nhập cây gia phả theo các định dạng phổ biến (bao gồm GEDCOM) để chia sẻ hoặc lưu trữ dữ liệu.
-   **Priority**: High
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Tùy chọn xuất dữ liệu sang PDF và GEDCOM có sẵn.
    -   Tùy chọn nhập dữ liệu từ tệp GEDCOM có sẵn.
    -   Tệp xuất chứa tất cả thông tin và mối quan hệ đã nhập.
    -   Hệ thống xử lý chính xác dữ liệu khi nhập từ tệp GEDCOM.

#### US_008: Xem dòng thời gian sự kiện
-   **User Story**: Là người dùng, tôi muốn xem dòng thời gian các sự kiện chính của gia đình (sinh, kết hôn, mất) được hiển thị theo năm sinh để có cái nhìn tổng quan.
-   **Priority**: Medium
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Dòng thời gian hiển thị các sự kiện chính (sinh, kết hôn, mất) của các thành viên.
    -   Các sự kiện được sắp xếp và hiển thị theo năm sinh của thành viên hoặc theo thời gian diễn ra sự kiện.
    -   Mỗi sự kiện trên dòng thời gian liên kết đến thành viên liên quan.
    -   Người dùng có thể lọc các sự kiện theo loại hoặc theo thành viên.

#### US_009: Đính kèm ghi chú/tài liệu
-   **User Story**: Là người dùng, tôi muốn thêm ghi chú hoặc tài liệu đính kèm (giấy khai sinh, ảnh cũ) vào từng thành viên để lưu giữ thông tin bổ sung.
-   **Priority**: Medium
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Mỗi thành viên có phần thêm ghi chú văn bản.
    -   Tải lên và đính kèm tệp (ảnh, PDF) vào hồ sơ thành viên.
    -   Tệp đính kèm có thể xem hoặc tải xuống.

#### US_010: Báo cáo thống kê & Kỷ lục gia đình
-   **User Story**: Là người dùng, tôi muốn xem và tạo các báo cáo thống kê về cây gia phả, bao gồm các kỷ lục gia đình, để có cái nhìn sâu sắc và phân tích dữ liệu.
-   **Priority**: Medium
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Hệ thống cung cấp các số liệu thống kê cơ bản (tổng số thành viên, phân bố giới tính, độ tuổi trung bình).
    -   Các kỷ lục gia đình (thành viên lớn tuổi nhất, gia đình lớn nhất) hiển thị rõ ràng.
    -   Người dùng có thể tạo các báo cáo tùy chỉnh.
    -   Các báo cáo có thể được trình bày dưới dạng bảng, biểu đồ.

#### US_011: In cây gia phả
-   **User Story**: Là người dùng, tôi muốn in cây gia phả để có bản sao vật lý hoặc chia sẻ ngoại tuyến.
-   **Priority**: Low
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Tùy chọn in có sẵn.
    -   Bản in hiển thị cây gia phả rõ ràng, dễ đọc.
    -   Chọn tùy chọn in (kích thước giấy, hướng).

### 2.4. Module: AI & Tích hợp

#### US_012: Gợi ý tiểu sử bằng AI
-   **User Story**: Là người dùng, tôi muốn AI có thể gợi ý nội dung tiểu sử dựa trên dữ liệu thành viên đã có sẵn.
-   **Priority**: Low
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Khi người dùng chọn một thành viên, hệ thống có thể tạo ra một bản nháp tiểu sử dựa trên dữ liệu có sẵn.
    -   Gợi ý tiểu sử bao gồm các thông tin cơ bản và các sự kiện quan trọng.
    -   Người dùng có thể chỉnh sửa bản nháp tiểu sử được gợi ý trước khi lưu.

#### US_013: Nhận diện khuôn mặt bằng AI để tự động gắn thẻ
-   **User Story**: Là người dùng, tôi muốn AI nhận diện khuôn mặt từ ảnh cũ để tự động gắn thẻ thành viên.
-   **Priority**: Low
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Người dùng có thể tải lên một hoặc nhiều ảnh.
    -   Hệ thống tự động phát hiện các khuôn mặt trong ảnh.
    -   Đối với mỗi khuôn mặt, AI đề xuất tên thành viên có khả năng trùng khớp cao nhất.
    -   Người dùng có thể xác nhận, sửa hoặc từ chối gợi ý gắn thẻ.

#### US_014: Tìm kiếm bằng khuôn mặt
-   **User Story**: Là người dùng, tôi muốn tìm kiếm thành viên bằng cách tải lên một hình ảnh khuôn mặt.
-   **Priority**: Low
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Giao diện cho phép người dùng tải lên một tệp ảnh.
    -   Hệ thống phân tích khuôn mặt trong ảnh tải lên và so sánh với dữ liệu thành viên.
    -   Hệ thống trả về danh sách các thành viên có khả năng trùng khớp cao.

## 3. In Progress

### 3.1. Module: Quản lý Thành viên

#### US_015: Thêm thành viên
-   **User Story**: Là người dùng, tôi muốn thêm thành viên mới vào cây gia phả để mở rộng lịch sử gia đình.
-   **Priority**: High
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Biểu mẫu nhập thông tin thành viên mới (tên, ngày sinh, giới tính, cha mẹ, vợ/chồng, con cái).
    -   Thành viên mới liên kết chính xác với thành viên hiện có.
    -   Xác thực các trường bắt buộc.

#### US_016: Chỉnh sửa thành viên
-   **User Story**: Là người dùng, tôi muốn chỉnh sửa thông tin thành viên hiện có để cập nhật hoặc sửa lỗi.
-   **Priority**: High
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Chọn thành viên và truy cập biểu mẫu chỉnh sửa.
    -   Các trường chỉnh sửa được điền sẵn dữ liệu hiện tại.
    -   Thay đổi được lưu và phản ánh trong cây gia phả.

#### US_017: Tìm kiếm thành viên (Tìm kiếm mở rộng)
-   **User Story**: Là người dùng, tôi muốn tìm kiếm thành viên trong cây gia phả với các tùy chọn tìm kiếm mở rộng để nhanh chóng định vị và xem thông tin.
-   **Priority**: High
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Giao diện tìm kiếm mở rộng có sẵn, cho phép nhập nhiều tiêu chí.
    -   Người dùng có thể tìm kiếm theo tên, ngày sinh, ngày mất, nơi sinh, nơi mất, giới tính, nghề nghiệp.
    -   Kết quả tìm kiếm hiển thị trong danh sách hoặc làm nổi bật/điều hướng đến thành viên trên cây gia phả.

## 4. Done

### 4.1. Module: Xác thực

#### US_018: Đăng nhập hệ thống
-   **User Story**: Là người dùng, tôi muốn đăng nhập vào hệ thống để truy cập dữ liệu cây gia phả của mình.
-   **Priority**: High
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Giao diện đăng nhập có các trường cho email/tên đăng nhập và mật khẩu.
    -   Hệ thống xác thực thành công với thông tin hợp lệ.
    -   Sau khi đăng nhập thành công, người dùng được chuyển hướng đến trang tổng quan.

### 4.2. Module: Quản lý Thành viên

#### US_019: Xem chi tiết thành viên
-   **User Story**: Là người dùng, tôi muốn xem thông tin chi tiết của từng thành viên (ảnh, tiểu sử, sự kiện) để có cái nhìn đầy đủ.
-   **Priority**: High
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Khi nhấp vào thành viên, hiển thị cửa sổ/trang chi tiết.
    -   Hiển thị ảnh đại diện, ngày sinh, ngày mất, nơi sinh, nơi mất, nghề nghiệp, tiểu sử.
    -   Có thể thêm/chỉnh sửa ảnh đại diện và tiểu sử.