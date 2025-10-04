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

Tài liệu này chứa **Product Backlog** của dự án Cây Gia Phả, là một danh sách ưu tiên các tính năng, yêu cầu, cải tiến và sửa lỗi cần được thực hiện. Product Backlog được tổ chức theo trạng thái phát triển (To Do, In Progress, Done) và nhóm theo các module/epic chính của ứng dụng.

Mỗi mục trong backlog được trình bày dưới dạng **User Story** (câu chuyện người dùng), mô tả một tính năng từ góc nhìn của người dùng cuối. Cấu trúc của một User Story bao gồm:

*   **Vai trò (Role)**: Ai là người dùng này?
*   **Mong muốn (Goal)**: Người dùng muốn làm gì?
*   **Lý do (Reason)**: Tại sao người dùng muốn làm điều đó?

Cùng với đó là các thông tin quan trọng khác như `Priority` (mức độ ưu tiên), `Estimate` (ước tính thời gian/công sức), và `Acceptance Criteria` (tiêu chí chấp nhận) để xác định khi nào tính năng được coi là hoàn thành.

## 2. To Do

Phần này liệt kê các User Story đang chờ được phát triển, được sắp xếp theo module và mức độ ưu tiên.

### 2.1. Module: Cây Gia Phả

#### US_001: Xem cây gia phả (Đa kiểu hiển thị)
-   **User Story**: Là người dùng, tôi muốn xem cây gia phả với nhiều kiểu hiển thị khác nhau (bao gồm theo giới tính) để dễ dàng theo dõi và hiểu các mối quan hệ.
-   **Priority**: High
-   **Estimate**: TBD (To Be Determined) - Cần ước tính chi tiết hơn khi bắt đầu phát triển.
-   **Acceptance Criteria**:
    -   Hệ thống cung cấp ít nhất 2-3 kiểu hiển thị cây gia phả khác nhau (ví dụ: sơ đồ dọc, sơ đồ ngang, sơ đồ hình quạt).
    -   Có chế độ xem riêng biệt theo giới tính (ví dụ: làm nổi bật nam/nữ, hoặc chỉ hiển thị một giới tính).
    -   Người dùng có thể dễ dàng chuyển đổi giữa các kiểu hiển thị thông qua giao diện người dùng trực quan.
    -   Mỗi kiểu hiển thị phải thể hiện rõ ràng các mối quan hệ giữa các thành viên (cha, mẹ, con cái, vợ/chồng).
    -   Các kiểu hiển thị phải có khả năng phóng to/thu nhỏ (zoom in/out) và di chuyển (pan) để khám phá cây gia phả lớn.

#### US_002: Thay đổi nút gốc cây gia phả
-   **User Story**: Là người dùng, tôi muốn chọn một thành viên bất kỳ và vẽ lại cây gia phả với thành viên đó làm nút gốc.
-   **Priority**: Medium
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Khi người dùng chọn một thành viên từ cây gia phả hoặc danh sách, cây gia phả sẽ được vẽ lại với thành viên đó làm nút gốc (root node).
    -   Thành viên được chọn phải hiển thị ở vị trí trung tâm hoặc vị trí gốc của cây mới, với các mối quan hệ xung quanh nó.
    -   Các mối quan hệ của thành viên gốc mới (cha mẹ, con cái, anh chị em) phải được hiển thị rõ ràng.
    -   Phải có cách để quay lại chế độ xem cây gia phả ban đầu hoặc nút gốc trước đó (ví dụ: nút "Reset View").

#### US_003: In poster gia phả
-   **User Story**: Là người dùng, tôi muốn có thể tích hợp tính năng in poster cây gia phả để tạo ra bản in lớn, chất lượng cao.
-   **Priority**: Low
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Người dùng có thể chọn bố cục (layout) và kích thước cho poster gia phả (ví dụ: A0, A1, tùy chỉnh).
    -   Hệ thống cung cấp giao diện xem trước poster trước khi in để người dùng có thể kiểm tra.
    -   Có tùy chọn để tùy chỉnh thông tin hiển thị trên poster (ví dụ: chỉ hiển thị tên, ngày sinh, không hiển thị ảnh).
    -   Hệ thống tích hợp với dịch vụ in ấn bên ngoài hoặc cung cấp tệp tin chất lượng cao (ví dụ: PDF, SVG) để người dùng tự in.

#### US_004: In 3D cây gia phả
-   **User Story**: Là người dùng, tôi muốn có thể tích hợp tính năng in 3D cây gia phả để tạo ra mô hình vật lý.
-   **Priority**: Low
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Người dùng có thể chọn các thành viên và mối quan hệ để đưa vào mô hình 3D.
    -   Hệ thống cung cấp giao diện xem trước mô hình 3D trước khi in.
    -   Có các tùy chọn để tùy chỉnh thiết kế mô hình 3D (ví dụ: màu sắc, vật liệu, kiểu dáng).
    -   Hệ thống tích hợp với dịch vụ in 3D bên ngoài hoặc cung cấp tệp mô hình 3D (ví dụ: STL, OBJ) để người dùng tự in.

### 2.2. Module: Quản lý Quan hệ

#### US_005: Quản lý mối quan hệ phức tạp
-   **User Story**: Là người dùng, tôi muốn thêm các loại mối quan hệ khác nhau (con nuôi, anh/chị/em cùng cha khác mẹ) để phản ánh chính xác mối quan hệ phức tạp.
-   **Priority**: High
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Khi thêm/chỉnh sửa mối quan hệ, người dùng có thể chọn loại mối quan hệ từ một danh sách (ví dụ: ruột thịt, con nuôi, cha dượng/mẹ kế, anh/chị/em cùng cha khác mẹ, anh/chị/em cùng mẹ khác cha).
    -   Các mối quan hệ phức tạp này phải hiển thị rõ ràng và chính xác trên cây gia phả.
    -   Backend hỗ trợ lưu trữ và truy vấn các loại mối quan hệ này.

#### US_006: Tìm kiếm mối quan hệ
-   **User Story**: Là người dùng, tôi muốn tìm kiếm mối quan hệ giữa hai thành viên bất kỳ trong cây gia phả.
-   **Priority**: Medium
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Người dùng có thể chọn hai thành viên từ cây gia phả hoặc danh sách để tìm kiếm mối quan hệ.
    -   Hệ thống xác định và hiển thị loại mối quan hệ (ví dụ: cha con, vợ chồng, anh chị em, ông cháu, v.v.).
    -   Hệ thống hiển thị đường dẫn hoặc chuỗi liên kết giữa hai thành viên trên cây gia phả (ví dụ: A là con của B, B là anh của C, C là chồng của D).
    -   Nếu không có mối quan hệ trực tiếp hoặc gián tiếp nào được tìm thấy, hệ thống thông báo rõ ràng.

### 2.3. Module: Dữ liệu & Báo cáo

#### US_007: Xuất/Nhập cây gia phả (GEDCOM)
-   **User Story**: Là người dùng, tôi muốn xuất và nhập cây gia phả theo các định dạng phổ biến (bao gồm GEDCOM) để chia sẻ hoặc lưu trữ dữ liệu.
-   **Priority**: High
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Tùy chọn xuất dữ liệu sang định dạng PDF và GEDCOM có sẵn trong giao diện người dùng.
    -   Tùy chọn nhập dữ liệu từ tệp GEDCOM có sẵn, với khả năng xử lý các xung đột dữ liệu.
    -   Tệp xuất chứa tất cả thông tin thành viên và mối quan hệ đã nhập, bao gồm cả các sự kiện.
    -   Hệ thống xử lý chính xác dữ liệu khi nhập từ tệp GEDCOM, bao gồm việc tạo mới hoặc cập nhật thành viên/mối quan hệ.

#### US_008: Xem dòng thời gian sự kiện
-   **User Story**: Là người dùng, tôi muốn xem dòng thời gian các sự kiện chính của gia đình (sinh, kết hôn, mất) được hiển thị theo năm sinh để có cái nhìn tổng quan.
-   **Priority**: Medium
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Dòng thời gian hiển thị các sự kiện chính (sinh, kết hôn, mất, v.v.) của các thành viên trong gia phả.
    -   Các sự kiện được sắp xếp và hiển thị theo năm sinh của thành viên hoặc theo thời gian diễn ra sự kiện.
    -   Mỗi sự kiện trên dòng thời gian liên kết đến thành viên liên quan và hiển thị thông tin tóm tắt.
    -   Người dùng có thể lọc các sự kiện theo loại (ví dụ: chỉ xem sự kiện sinh) hoặc theo thành viên cụ thể.

#### US_009: Đính kèm ghi chú/tài liệu
-   **User Story**: Là người dùng, tôi muốn thêm ghi chú hoặc tài liệu đính kèm (giấy khai sinh, ảnh cũ) vào từng thành viên để lưu giữ thông tin bổ sung.
-   **Priority**: Medium
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Mỗi thành viên có phần thêm ghi chú văn bản không giới hạn độ dài.
    -   Người dùng có thể tải lên và đính kèm nhiều loại tệp (ảnh, PDF, tài liệu Word) vào hồ sơ thành viên.
    -   Tệp đính kèm có thể được xem trực tiếp trong ứng dụng hoặc tải xuống.
    -   Có cơ chế quản lý tệp đính kèm (xóa, đổi tên).

#### US_010: Báo cáo thống kê & Kỷ lục gia đình
-   **User Story**: Là người dùng, tôi muốn xem và tạo các báo cáo thống kê về cây gia phả, bao gồm các kỷ lục gia đình, để có cái nhìn sâu sắc và phân tích dữ liệu.
-   **Priority**: Medium
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Hệ thống cung cấp các số liệu thống kê cơ bản (tổng số thành viên, phân bố giới tính, độ tuổi trung bình, số lượng dòng họ).
    -   Các kỷ lục gia đình (thành viên lớn tuổi nhất, gia đình lớn nhất, thành viên có nhiều con nhất) hiển thị rõ ràng.
    -   Người dùng có thể tạo các báo cáo tùy chỉnh dựa trên các tiêu chí lọc khác nhau.
    -   Các báo cáo có thể được trình bày dưới dạng bảng, biểu đồ trực quan.

#### US_011: In cây gia phả
-   **User Story**: Là người dùng, tôi muốn in cây gia phả để có bản sao vật lý hoặc chia sẻ ngoại tuyến.
-   **Priority**: Low
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Tùy chọn in có sẵn trong giao diện người dùng.
    -   Bản in hiển thị cây gia phả rõ ràng, dễ đọc, với các thông tin cơ bản của thành viên.
    -   Người dùng có thể chọn các tùy chọn in (kích thước giấy, hướng trang, thông tin hiển thị).
    -   Hệ thống tạo ra một bản xem trước in ấn chính xác.

### 2.4. Module: AI & Tích hợp

#### US_012: Gợi ý tiểu sử bằng AI
-   **User Story**: Là người dùng, tôi muốn AI có thể gợi ý nội dung tiểu sử dựa trên dữ liệu thành viên đã có sẵn.
-   **Priority**: Low
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Khi người dùng chọn một thành viên, hệ thống có thể tạo ra một bản nháp tiểu sử dựa trên dữ liệu có sẵn (tên, ngày sinh, nơi sinh, nghề nghiệp, các sự kiện liên quan).
    -   Gợi ý tiểu sử bao gồm các thông tin cơ bản và các sự kiện quan trọng trong cuộc đời thành viên.
    -   Người dùng có thể chỉnh sửa bản nháp tiểu sử được gợi ý trước khi lưu vào hồ sơ thành viên.

#### US_013: Nhận diện khuôn mặt bằng AI để tự động gắn thẻ
-   **User Story**: Là người dùng, tôi muốn AI nhận diện khuôn mặt từ ảnh cũ để tự động gắn thẻ thành viên.
-   **Priority**: Low
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Người dùng có thể tải lên một hoặc nhiều ảnh có chứa khuôn mặt.
    -   Hệ thống tự động phát hiện các khuôn mặt trong ảnh và khoanh vùng chúng.
    -   Đối với mỗi khuôn mặt được phát hiện, AI đề xuất tên thành viên có khả năng trùng khớp cao nhất từ database.
    -   Người dùng có thể xác nhận, sửa hoặc từ chối gợi ý gắn thẻ của AI.

#### US_014: Tìm kiếm bằng khuôn mặt
-   **User Story**: Là người dùng, tôi muốn tìm kiếm thành viên bằng cách tải lên một hình ảnh khuôn mặt.
-   **Priority**: Low
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Giao diện cho phép người dùng tải lên một tệp ảnh có chứa khuôn mặt.
    -   Hệ thống phân tích khuôn mặt trong ảnh tải lên và so sánh với cơ sở dữ liệu khuôn mặt của các thành viên đã có.
    -   Hệ thống trả về danh sách các thành viên có khả năng trùng khớp cao nhất, kèm theo tỷ lệ tự tin.
    -   Người dùng có thể xem thông tin chi tiết của các thành viên được tìm thấy.

## 3. In Progress

Phần này liệt kê các User Story đang trong quá trình phát triển tích cực.

### 3.1. Module: Quản lý Thành viên

#### US_015: Thêm thành viên
-   **User Story**: Là người dùng, tôi muốn thêm thành viên mới vào cây gia phả để mở rộng lịch sử gia đình.
-   **Priority**: High
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Biểu mẫu nhập thông tin thành viên mới (tên, ngày sinh, giới tính, cha mẹ, vợ/chồng, con cái) phải đầy đủ và dễ sử dụng.
    -   Thành viên mới phải được liên kết chính xác với thành viên hiện có (nếu có) hoặc với một dòng họ cụ thể.
    -   Hệ thống phải xác thực các trường bắt buộc và hiển thị thông báo lỗi rõ ràng nếu dữ liệu không hợp lệ.
    -   Sau khi thêm thành công, thành viên mới phải hiển thị trên cây gia phả và trong danh sách thành viên.

#### US_016: Chỉnh sửa thành viên
-   **User Story**: Là người dùng, tôi muốn chỉnh sửa thông tin thành viên hiện có để cập nhật hoặc sửa lỗi.
-   **Priority**: High
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Người dùng có thể chọn một thành viên và truy cập biểu mẫu chỉnh sửa thông tin của thành viên đó.
    -   Các trường trong biểu mẫu chỉnh sửa phải được điền sẵn dữ liệu hiện tại của thành viên.
    -   Các thay đổi phải được lưu và phản ánh ngay lập tức trong cây gia phả và các nơi hiển thị thông tin thành viên khác.
    -   Hệ thống phải xác thực các trường đã chỉnh sửa và hiển thị thông báo lỗi nếu dữ liệu không hợp lệ.

#### US_017: Tìm kiếm thành viên (Tìm kiếm mở rộng)
-   **User Story**: Là người dùng, tôi muốn tìm kiếm thành viên trong cây gia phả với các tùy chọn tìm kiếm mở rộng để nhanh chóng định vị và xem thông tin.
-   **Priority**: High
-   **Estimate**: TBD
-   **Acceptance Criteria**:
    -   Giao diện tìm kiếm mở rộng có sẵn, cho phép người dùng nhập nhiều tiêu chí tìm kiếm.
    -   Người dùng có thể tìm kiếm theo tên, ngày sinh, ngày mất, nơi sinh, nơi mất, giới tính, nghề nghiệp, hoặc kết hợp các tiêu chí này.
    -   Kết quả tìm kiếm phải hiển thị trong danh sách hoặc làm nổi bật/điều hướng đến thành viên trên cây gia phả.
    -   Tìm kiếm phải hỗ trợ tìm kiếm gần đúng (fuzzy search) hoặc tìm kiếm một phần từ khóa.

## 4. Done

Phần này liệt kê các User Story đã hoàn thành và được triển khai.

### 4.1. Module: Xác thực

#### US_018: Đăng nhập hệ thống
-   **User Story**: Là người dùng, tôi muốn đăng nhập vào hệ thống để truy cập dữ liệu cây gia phả của mình.
-   **Priority**: High
-   **Estimate**: 3 ngày
-   **Acceptance Criteria**:
    -   Giao diện đăng nhập có các trường cho email/tên đăng nhập và mật khẩu.
    -   Hệ thống xác thực thành công với thông tin hợp lệ và trả về JWT.
    -   Sau khi đăng nhập thành công, người dùng được chuyển hướng đến trang tổng quan hoặc trang chính của ứng dụng.
    -   Xử lý lỗi rõ ràng khi thông tin đăng nhập không hợp lệ.

### 4.2. Module: Quản lý Thành viên & Dòng họ

#### US_019: Xem chi tiết thành viên
-   **User Story**: Là người dùng, tôi muốn xem thông tin chi tiết của từng thành viên (ảnh, tiểu sử, sự kiện) để có cái nhìn đầy đủ.
-   **Priority**: High
-   **Estimate**: 2 ngày
-   **Acceptance Criteria**:
    -   Khi nhấp vào thành viên từ cây gia phả hoặc danh sách, hiển thị cửa sổ/trang chi tiết của thành viên.
    -   Hiển thị đầy đủ thông tin: ảnh đại diện, họ tên, ngày sinh, ngày mất, nơi sinh, nơi mất, nghề nghiệp, tiểu sử, giới tính.
    -   Hiển thị các sự kiện liên quan đến thành viên đó.
    -   Có thể thêm/chỉnh sửa ảnh đại diện và tiểu sử (nếu có quyền).

#### US_020: Tạo dòng họ mới (Implicitly done via seeding)
-   **User Story**: Là người dùng, tôi muốn tạo một dòng họ mới để bắt đầu xây dựng cây gia phả của mình.
-   **Priority**: High
-   **Estimate**: 2 ngày
-   **Acceptance Criteria**:
    -   Có giao diện để nhập thông tin dòng họ mới (tên, mô tả, địa chỉ, chế độ hiển thị).
    -   Hệ thống lưu trữ thông tin dòng họ mới vào database.
    -   Sau khi tạo thành công, người dùng được chuyển hướng đến trang quản lý dòng họ hoặc trang chi tiết dòng họ vừa tạo.
    -   Xác thực dữ liệu đầu vào cho các trường thông tin dòng họ.

#### US_021: Xem danh sách dòng họ (Implicitly done via seeding)
-   **User Story**: Là người dùng, tôi muốn xem danh sách các dòng họ đã tạo để dễ dàng quản lý và chọn dòng họ muốn xem.
-   **Priority**: High
-   **Estimate**: 1 ngày
-   **Acceptance Criteria**:
    -   Hiển thị danh sách tất cả các dòng họ mà người dùng có quyền truy cập.
    -   Danh sách hiển thị các thông tin cơ bản của dòng họ (tên, mô tả, số lượng thành viên).
    -   Có chức năng phân trang và tìm kiếm/lọc danh sách dòng họ.
    -   Người dùng có thể nhấp vào một dòng họ để xem chi tiết.