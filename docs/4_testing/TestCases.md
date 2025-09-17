# Kịch bản Kiểm thử (Test Cases)

Tài liệu này chứa các kịch bản kiểm thử chi tiết cho các User Story quan trọng của dự án, nhằm đảm bảo chất lượng và tính đúng đắn của các chức năng.

---

## 1. User Story: Đăng nhập hệ thống

**Mục tiêu:** Kiểm tra chức năng đăng nhập, bao gồm các trường hợp thành công, thất bại và các kịch bản về bảo mật.

| ID Test Case | Mô tả | Các bước thực hiện | Kết quả mong đợi | Loại |
| :--- | :--- | :--- | :--- | :--- |
| **TC_LOGIN_01** | Đăng nhập thành công với thông tin hợp lệ | 1. Mở trang Đăng nhập. <br> 2. Nhập email và mật khẩu chính xác. <br> 3. Nhấn nút "Đăng nhập". | 1. Hệ thống xác thực thành công. <br> 2. Người dùng được chuyển hướng đến trang Tổng quan (Dashboard). <br> 3. Hiển thị thông báo đăng nhập thành công. | Happy Path |
| **TC_LOGIN_02** | Đăng nhập thất bại với mật khẩu sai | 1. Mở trang Đăng nhập. <br> 2. Nhập email đúng, mật khẩu sai. <br> 3. Nhấn nút "Đăng nhập". | 1. Hệ thống từ chối xác thực. <br> 2. Người dùng vẫn ở trang Đăng nhập. <br> 3. Hiển thị thông báo lỗi "Email hoặc mật khẩu không chính xác". | Negative |
| **TC_LOGIN_03** | Đăng nhập thất bại với email không tồn tại | 1. Mở trang Đăng nhập. <br> 2. Nhập một email không có trong hệ thống. <br> 3. Nhấn nút "Đăng nhập". | 1. Hệ thống từ chối xác thực. <br> 2. Hiển thị thông báo lỗi "Email hoặc mật khẩu không chính xác". | Negative |
| **TC_LOGIN_04** | Đăng nhập với các trường để trống | 1. Mở trang Đăng nhập. <br> 2. Không nhập email và mật khẩu. <br> 3. Nhấn nút "Đăng nhập". | 1. Nút "Đăng nhập" bị vô hiệu hóa hoặc hiển thị lỗi validation tại các trường bắt buộc. <br> 2. Không có request nào được gửi đến server. | Validation |
| **TC_LOGIN_05** | Đăng nhập với định dạng email không hợp lệ | 1. Mở trang Đăng nhập. <br> 2. Nhập một chuỗi không phải email vào trường email (ví dụ: "abc"). <br> 3. Nhấn nút "Đăng nhập". | 1. Hiển thị lỗi validation ngay tại trường email "Vui lòng nhập một địa chỉ email hợp lệ". <br> 2. Không có request nào được gửi đến server. | Validation |
| **TC_LOGIN_06** | Kiểm tra tính năng "Ghi nhớ đăng nhập" | 1. Mở trang Đăng nhập. <br> 2. Chọn ô "Ghi nhớ đăng nhập". <br> 3. Đăng nhập thành công. <br> 4. Đóng trình duyệt và mở lại. | 1. Người dùng vẫn ở trong trạng thái đã đăng nhập và được chuyển thẳng đến trang Tổng quan. | Functional |
| **TC_LOGIN_07** | Kiểm tra phân biệt chữ hoa/thường của email | 1. Mở trang Đăng nhập. <br> 2. Nhập email với chữ hoa/thường khác với lúc đăng ký (ví dụ: `User@Email.com` thay vì `user@email.com`). <br> 3. Nhập mật khẩu đúng. | 1. Đăng nhập thành công (hệ thống nên chuẩn hóa email về chữ thường trước khi xử lý). | Functional |

---

## 2. User Story: Quản lý mối quan hệ (cơ bản)

**Mục tiêu:** Kiểm tra chức năng tạo, xem, và xóa các mối quan hệ cơ bản (Cha-Mẹ, Vợ-Chồng), bao gồm các quy tắc nghiệp vụ và ràng buộc logic.

**Điều kiện tiên quyết:** Đã đăng nhập và có ít nhất 3 thành viên (A, B, C) trong cùng một gia phả.

| ID Test Case | Mô tả | Các bước thực hiện | Kết quả mong đợi | Loại |
| :--- | :--- | :--- | :--- | :--- |
| **TC_REL_01** | Thêm cha/mẹ thành công | 1. Vào trang chi tiết của thành viên A. <br> 2. Nhấn nút "Thêm cha/mẹ". <br> 3. Tìm kiếm và chọn thành viên B. <br> 4. Lưu lại. | 1. Một mối quan hệ `PARENT_OF` được tạo (B là cha/mẹ của A). <br> 2. Trang chi tiết của A hiển thị B là cha/mẹ. <br> 3. Trang chi tiết của B hiển thị A là con. | Happy Path |
| **TC_REL_02** | Thêm vợ/chồng thành công | 1. Vào trang chi tiết của thành viên A (nam). <br> 2. Nhấn nút "Thêm vợ/chồng". <br> 3. Tìm kiếm và chọn thành viên C (nữ). <br> 4. Lưu lại. | 1. Một mối quan hệ `SPOUSE_OF` được tạo giữa A và C. <br> 2. Trang chi tiết của A hiển thị C là vợ/chồng. <br> 3. Trang chi tiết của C hiển thị A là vợ/chồng. | Happy Path |
| **TC_REL_03** | Xóa một mối quan hệ | 1. Vào trang chi tiết của A, nơi B đang là cha/mẹ. <br> 2. Nhấn nút "Xóa" bên cạnh mối quan hệ với B. <br> 3. Xác nhận. | 1. Mối quan hệ giữa A và B bị xóa khỏi hệ thống. <br> 2. Trang chi tiết của A không còn hiển thị B là cha/mẹ. | Functional |
| **TC_REL_04** | **[Validation]** Ngăn chặn thêm chính mình làm cha/mẹ | 1. Vào trang chi tiết của A. <br> 2. Nhấn "Thêm cha/mẹ". <br> 3. Cố gắng tìm kiếm và chọn chính thành viên A. | 1. Thành viên A không xuất hiện trong danh sách lựa chọn. <br> 2. Nếu có thể chọn qua API, hệ thống trả về lỗi 400 và thông báo "Không thể tạo mối quan hệ với chính mình". | Negative |
| **TC_REL_05** | **[Validation]** Ngăn chặn thêm cha/mẹ đã tồn tại | 1. Vào trang chi tiết của A, nơi B đã là cha/mẹ. <br> 2. Nhấn "Thêm cha/mẹ". <br> 3. Cố gắng tìm kiếm và chọn lại thành viên B. | 1. Thành viên B không xuất hiện trong danh sách lựa chọn. <br> 2. Nếu có thể chọn qua API, hệ thống trả về lỗi 400 và thông báo "Mối quan hệ đã tồn tại". | Negative |
| **TC_REL_06** | **[Validation]** Ngăn chặn thêm cha/mẹ thứ ba | 1. Vào trang chi tiết của A, nơi A đã có 2 cha/mẹ là B và C. <br> 2. Nhấn "Thêm cha/mẹ". <br> 3. Cố gắng chọn thành viên D. | 1. Giao diện hiển thị thông báo "Thành viên đã có đủ 2 cha/mẹ". <br> 2. Nếu gửi request qua API, hệ thống trả về lỗi 400. | Negative |
| **TC_REL_07** | **[Validation]** Ngăn chặn tạo quan hệ vòng lặp (con làm cha) | 1. Vào trang chi tiết của B, nơi B là con của A. <br> 2. Nhấn "Thêm cha/mẹ". <br> 3. Cố gắng chọn thành viên A. | 1. Hệ thống trả về lỗi 400 và thông báo "Không thể tạo mối quan hệ vòng lặp (con không thể là cha/mẹ của cha/mẹ mình)". | Negative |
| **TC_REL_08** | **[Validation]** Ngăn chặn tạo quan hệ vòng lặp (cháu làm ông) | 1. A là cha của B, B là cha của C. <br> 2. Vào trang chi tiết của A. <br> 3. Nhấn "Thêm cha/mẹ". <br> 4. Cố gắng chọn thành viên C. | 1. Hệ thống trả về lỗi 400 và thông báo "Không thể tạo mối quan hệ vòng lặp". | Negative |
| **TC_REL_09** | **[Validation]** Ngăn chặn thêm vợ/chồng khi đã có | 1. Vào trang chi tiết của A, nơi A đã có vợ/chồng là C. <br> 2. Nhấn "Thêm vợ/chồng". | 1. Nút "Thêm vợ/chồng" bị vô hiệu hóa hoặc hiển thị thông báo "Thành viên đã có vợ/chồng". (Giả định hệ thống chưa hỗ trợ đa thê trong MVP). | Negative |

---

## 3. User Story: Xem cây gia phả (cơ bản)

**Mục tiêu:** Kiểm tra chức năng hiển thị cây gia phả và các tương tác cơ bản.

| ID Test Case | Mô tả | Các bước thực hiện | Kết quả mong đợi | Loại |
| :--- | :--- | :--- | :--- | :--- |
| **TC_TREE_01** | Hiển thị cây gia phả đúng cấu trúc | 1. Tạo một gia đình với 3 thế hệ (Ông/Bà -> Cha/Mẹ -> Con). <br> 2. Mở trang "Cây Gia Phả". | 1. Cây được vẽ đúng cấu trúc, thể hiện rõ các thế hệ và mối quan hệ. <br> 2. Các node Ông/Bà ở trên cùng, các node con cháu ở dưới. | Happy Path |
| **TC_TREE_02** | Hiển thị thông tin trên node | 1. Quan sát một node bất kỳ trên cây. | 1. Node hiển thị các thông tin cơ bản: ảnh đại diện (nếu có), họ tên. | UI/UX |
| **TC_TREE_03** | Tương tác Zoom | 1. Sử dụng con lăn chuột hoặc nút `+`/`-`. | 1. Cây gia phả được phóng to/thu nhỏ một cách mượt mà. | Functional |
| **TC_TREE_04** | Tương tác Pan (Kéo) | 1. Nhấn giữ chuột và kéo trên khu vực biểu đồ. | 1. Khung nhìn của cây gia phả di chuyển theo con trỏ chuột. | Functional |
| **TC_TREE_05** | Nhấp vào node để xem chi tiết | 1. Nhấp vào một node thành viên trên cây. | 1. Người dùng được điều hướng đến trang chi tiết của thành viên đó. | Functional |
| **TC_TREE_06** | Xử lý cây gia phả rỗng | 1. Tạo một dòng họ mới nhưng chưa thêm thành viên nào. <br> 2. Mở trang "Cây Gia Phả". | 1. Hiển thị một thông báo thân thiện như "Chưa có thành viên nào trong gia phả. Hãy bắt đầu bằng cách thêm thành viên đầu tiên!". | Edge Case |
| **TC_TREE_07** | Xử lý cây có thành viên nhưng chưa có quan hệ | 1. Thêm 3 thành viên A, B, C nhưng không tạo quan hệ nào. <br> 2. Mở trang "Cây Gia Phả". | 1. Hiển thị 3 node riêng lẻ, không có đường nối. | Edge Case |

---

## 4. User Story: Thêm & Chỉnh sửa thành viên

**Mục tiêu:** Kiểm tra chức năng tạo mới và cập nhật thông tin thành viên, bao gồm validation và các trường hợp biên.

| ID Test Case | Mô tả | Các bước thực hiện | Kết quả mong đợi | Loại |
| :--- | :--- | :--- | :--- | :--- |
| **TC_MEMBER_01** | Thêm thành viên mới thành công với đầy đủ thông tin | 1. Mở form "Thêm thành viên". <br> 2. Điền tất cả các trường hợp lệ (họ tên, ngày sinh, giới tính, tiểu sử...). <br> 3. Nhấn "Lưu". | 1. Hệ thống lưu thành công. <br> 2. Thành viên mới xuất hiện trong danh sách thành viên. <br> 3. Hiển thị thông báo thành công. | Happy Path |
| **TC_MEMBER_02** | **[Validation]** Thêm thành viên thất bại khi thiếu trường bắt buộc (Họ tên) | 1. Mở form "Thêm thành viên". <br> 2. Để trống trường "Họ tên". <br> 3. Điền các trường khác. <br> 4. Nhấn "Lưu". | 1. Hiển thị lỗi validation tại trường "Họ tên". <br> 2. Dữ liệu không được lưu. | Validation |
| **TC_MEMBER_03** | **[Validation]** Thêm thành viên với ngày sinh trong tương lai | 1. Mở form "Thêm thành viên". <br> 2. Nhập ngày sinh là một ngày trong tương lai. <br> 3. Nhấn "Lưu". | 1. Hiển thị lỗi validation tại trường "Ngày sinh" (ví dụ: "Ngày sinh không được lớn hơn ngày hiện tại"). <br> 2. Dữ liệu không được lưu. | Validation |
| **TC_MEMBER_04** | **[Validation]** Thêm thành viên với ngày mất trước ngày sinh | 1. Mở form "Thêm thành viên". <br> 2. Nhập ngày mất trước ngày sinh. <br> 3. Nhấn "Lưu". | 1. Hiển thị lỗi validation (ví dụ: "Ngày mất phải sau ngày sinh"). <br> 2. Dữ liệu không được lưu. | Validation |
| **TC_MEMBER_05** | Chỉnh sửa thông tin thành viên thành công | 1. Mở trang chi tiết của một thành viên. <br> 2. Nhấn "Sửa". <br> 3. Thay đổi một vài thông tin (ví dụ: tiểu sử, nơi sinh). <br> 4. Nhấn "Lưu". | 1. Hệ thống lưu thành công. <br> 2. Trang chi tiết thành viên hiển thị thông tin đã được cập nhật. | Functional |
| **TC_MEMBER_06** | Chỉnh sửa thành viên và để trống trường bắt buộc | 1. Mở form chỉnh sửa của một thành viên. <br> 2. Xóa hết nội dung trong trường "Họ tên". <br> 3. Nhấn "Lưu". | 1. Hiển thị lỗi validation tại trường "Họ tên". <br> 2. Thay đổi không được lưu. | Validation |

---

## 5. User Story: Tạo Dòng họ/Gia đình

**Mục tiêu:** Kiểm tra chức năng tạo mới một dòng họ/gia đình.

**Điều kiện tiên quyết:** Đã đăng nhập vào hệ thống.

| ID Test Case | Mô tả | Các bước thực hiện | Kết quả mong đợi | Loại |
| :--- | :--- | :--- | :--- | :--- |
| **TC_FAMILY_01** | Tạo dòng họ mới thành công | 1. Mở trang quản lý dòng họ. <br> 2. Nhấn nút "Tạo mới". <br> 3. Nhập tên dòng họ hợp lệ (ví dụ: "Dòng họ Nguyễn Văn"). <br> 4. Nhấn "Lưu". | 1. Dòng họ mới được tạo thành công. <br> 2. Dòng họ mới xuất hiện trong danh sách. <br> 3. Người dùng được chuyển đến trang quản lý của dòng họ mới. | Happy Path |
| **TC_FAMILY_02** | **[Validation]** Tạo dòng họ thất bại khi để trống tên | 1. Mở form "Tạo dòng họ mới". <br> 2. Không nhập tên. <br> 3. Nhấn "Lưu". | 1. Hiển thị lỗi validation tại trường "Tên dòng họ". <br> 2. Dòng họ không được tạo. | Validation |
| **TC_FAMILY_03** | Tạo dòng họ với tên chứa ký tự đặc biệt | 1. Mở form "Tạo dòng họ mới". <br> 2. Nhập tên chứa ký tự đặc biệt (ví dụ: "Dòng họ Nguyễn-Trần #1"). <br> 3. Nhấn "Lưu". | 1. Dòng họ được tạo thành công với đúng tên đã nhập. | Functional |
| **TC_FAMILY_04** | Hủy thao tác tạo dòng họ | 1. Mở form "Tạo dòng họ mới". <br> 2. Nhập một vài thông tin. <br> 3. Nhấn nút "Hủy" hoặc đóng dialog. | 1. Form được đóng lại. <br> 2. Không có dòng họ nào được tạo. | Functional |

---

## 6. User Story: Quản lý tài khoản và vai trò (cơ bản)

**Mục tiêu:** Kiểm tra các logic liên quan đến phân quyền và vai trò người dùng (chủ yếu ở cấp độ API và logic nghiệp vụ).

| ID Test Case | Mô tả | Các bước thực hiện | Kết quả mong đợi | Loại |
| :--- | :--- | :--- | :--- | :--- |
| **TC_ROLE_01** | Người dùng mới đăng ký có vai trò 'Member' mặc định | 1. Thực hiện chức năng đăng ký tài khoản mới. <br> 2. Kiểm tra bản ghi người dùng trong database. | 1. Tài khoản được tạo thành công. <br> 2. Trường `role` của người dùng mới trong database có giá trị là 'Member'. | Backend |
| **TC_ROLE_02** | Người dùng 'Member' có thể tạo/sửa/xóa dữ liệu trong gia phả của mình | 1. Đăng nhập bằng tài khoản 'Member'. <br> 2. Thử thực hiện các thao tác: thêm thành viên, sửa thông tin, tạo quan hệ trong gia phả mà người đó quản lý. | 1. Tất cả các thao tác đều thành công. | Authorization |
| **TC_ROLE_03** | Người dùng 'Member' không thể sửa dữ liệu của gia phả khác | 1. Đăng nhập bằng tài khoản 'Member' A, người quản lý gia phả X. <br> 2. Cố gắng gửi một request API để sửa thông tin một thành viên trong gia phả Y (do người dùng B quản lý). | 1. API trả về lỗi 403 (Forbidden) hoặc 404 (Not Found). <br> 2. Dữ liệu của gia phả Y không bị thay đổi. | Authorization |
| **TC_ROLE_04** | Người dùng chưa đăng nhập không thể truy cập API được bảo vệ | 1. Không đăng nhập. <br> 2. Cố gắng gửi một request API đến một endpoint được bảo vệ (ví dụ: `GET /api/members`). | 1. API trả về lỗi 401 (Unauthorized). | Authentication |

---

## 7. User Story: Xem chi tiết thành viên

**Mục tiêu:** Kiểm tra chức năng hiển thị thông tin chi tiết của một thành viên.

**Điều kiện tiên quyết:** Đã đăng nhập và có ít nhất một thành viên trong gia phả.

| ID Test Case | Mô tả | Các bước thực hiện | Kết quả mong đợi | Loại |
| :--- | :--- | :--- | :--- | :--- |
| **TC_DETAIL_01** | Xem chi tiết thành viên có đầy đủ thông tin | 1. Đăng nhập, vào danh sách thành viên. <br> 2. Chọn thành viên A có đầy đủ thông tin (tiểu sử, ngày sinh/mất, nơi sinh...). | 1. Trang chi tiết hiển thị chính xác tất cả thông tin của thành viên A. | Happy Path |
| **TC_DETAIL_02** | Xem chi tiết thành viên chỉ có thông tin tối thiểu | 1. Đăng nhập, vào danh sách thành viên. <br> 2. Chọn thành viên B chỉ có họ tên. | 1. Trang chi tiết hiển thị họ tên. <br> 2. Các trường thông tin khác hiển thị "Chưa có thông tin" hoặc được ẩn đi một cách hợp lý. | Edge Case |
| **TC_DETAIL_03** | Điều hướng từ cây gia phả đến trang chi tiết | 1. Mở trang "Cây Gia Phả". <br> 2. Nhấp vào node của thành viên A. | 1. Được chuyển hướng đến trang chi tiết của thành viên A. | Functional |
| **TC_DETAIL_04** | Truy cập trang chi tiết của thành viên không tồn tại | 1. Đăng nhập. <br> 2. Nhập thủ công URL `/members/invalid-id` vào thanh địa chỉ. | 1. Hiển thị trang lỗi 404 với thông báo thân thiện "Không tìm thấy thành viên". | Negative |
| **TC_DETAIL_05** | Truy cập trang chi tiết của thành viên thuộc gia phả khác | 1. Đăng nhập bằng tài khoản User A (quản lý gia phả X). <br> 2. Nhập thủ công URL của một thành viên thuộc gia phả Y. | 1. API trả về lỗi 403 (Forbidden) hoặc 404 (Not Found). <br> 2. Giao diện hiển thị trang lỗi "Không có quyền truy cập" hoặc "Không tìm thấy". | Authorization |
| **TC_DETAIL_06** | Giao diện hiển thị responsive | 1. Mở trang chi tiết trên các kích thước màn hình khác nhau (desktop, tablet, mobile). | 1. Bố cục trang được điều chỉnh hợp lý, tất cả thông tin đều dễ đọc, không bị vỡ layout. | UI/UX |