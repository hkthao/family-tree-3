# Product Backlog (Danh sách yêu cầu sản phẩm)

Tài liệu này chứa danh sách các User Story (câu chuyện người dùng) cho dự án cây gia phả. Mỗi story mô tả một tính năng từ góc nhìn của người dùng cuối, bao gồm vai trò, mong muốn và lợi ích.

---

### Xem cây gia phả (Đa kiểu hiển thị)

**User Story:** *Là người dùng, tôi muốn xem cây gia phả với nhiều kiểu hiển thị khác nhau (bao gồm theo giới tính) để dễ dàng theo dõi và hiểu các mối quan hệ.*

**Mô tả:**
Chức năng cho phép người dùng lựa chọn các kiểu hiển thị khác nhau cho cây gia phả (ví dụ: sơ đồ phả hệ truyền thống, sơ đồ hình quạt, sơ đồ dạng danh sách, chế độ xem theo giới tính) để phù hợp với nhu cầu xem và phân tích.

**Tiêu chí chấp nhận:**
- Hệ thống phải cung cấp ít nhất 2-3 kiểu hiển thị cây gia phả khác nhau (ví dụ: sơ đồ dọc, sơ đồ ngang, sơ đồ hình quạt).
- Có chế độ xem riêng biệt theo giới tính (ví dụ: làm nổi bật nam/nữ, hoặc chỉ hiển thị một giới tính).
- Người dùng có thể dễ dàng chuyển đổi giữa các kiểu hiển thị.
- Mỗi kiểu hiển thị phải thể hiện rõ ràng các mối quan hệ giữa các thành viên.
- Thông tin cơ bản về mỗi thành viên (tên, ngày sinh) phải hiển thị trong mọi kiểu xem.
- Các kiểu hiển thị phải có khả năng phóng to/thu nhỏ và di chuyển.

---

### Thêm thành viên

**User Story:** *Là người dùng, tôi muốn thêm thành viên mới vào cây gia phả để mở rộng lịch sử gia đình.*

**Mô tả:**
Chức năng thêm cá nhân mới vào cây gia phả.

**Tiêu chí chấp nhận:**
- Biểu mẫu nhập thông tin thành viên mới (tên, ngày sinh, giới tính, cha mẹ, vợ/chồng, con cái).
- Thành viên mới liên kết chính xác với thành viên hiện có.
- Xác thực các trường bắt buộc.

---

### Chỉnh sửa thành viên

**User Story:** *Là người dùng, tôi muốn chỉnh sửa thông tin thành viên hiện có để cập nhật hoặc sửa lỗi.*

**Mô tả:**
Chức năng sửa đổi thông tin thành viên đã tồn tại.

**Tiêu chí chấp nhận:**
- Chọn thành viên và truy cập biểu mẫu chỉnh sửa.
- Các trường chỉnh sửa (tên, ngày sinh, giới tính, v.v.) được điền sẵn dữ liệu hiện tại.
- Thay đổi được lưu và phản ánh trong cây gia phả.

---

### Tìm kiếm thành viên (Tìm kiếm mở rộng)

**User Story:** *Là người dùng, tôi muốn tìm kiếm thành viên trong cây gia phả với các tùy chọn tìm kiếm mở rộng để nhanh chóng định vị và xem thông tin.*

**Mô tả:**
Chức năng cho phép người dùng tìm kiếm thành viên theo nhiều tiêu chí khác nhau (ví dụ: tên, ngày sinh, nơi sinh, giới tính, mối quan hệ), hiển thị kết quả trong danh sách hoặc trực tiếp trên cây gia phả.

**Tiêu chí chấp nhận:**
- Một giao diện tìm kiếm mở rộng phải có sẵn, cho phép nhập nhiều tiêu chí.
- Người dùng có thể tìm kiếm theo tên, ngày sinh, ngày mất, nơi sinh, nơi mất, giới tính, nghề nghiệp, hoặc các mối quan hệ.
- Kết quả tìm kiếm có thể hiển thị trong danh sách hoặc làm nổi bật/điều hướng đến thành viên trên cây gia phả.
- Hệ thống có thể cung cấp chức năng điều hướng giữa các kết quả tìm kiếm (nếu có nhiều).
- Người dùng có thể kết hợp nhiều tiêu chí tìm kiếm (ví dụ: tìm người tên "Nguyễn Văn A" sinh năm 1950).

---

### Xem chi tiết thành viên

**User Story:** *Là người dùng, tôi muốn xem thông tin chi tiết của từng thành viên (ảnh, tiểu sử, sự kiện) để có cái nhìn đầy đủ.*

**Mô tả:**
Bổ sung khả năng xem thông tin cá nhân phong phú hơn.

**Tiêu chí chấp nhận:**
- Khi nhấp vào thành viên, hiển thị cửa sổ/trang chi tiết.
- Hiển thị ảnh đại diện, ngày sinh, ngày mất, nơi sinh, nơi mất, nghề nghiệp, tiểu sử.
- Có thể thêm/chỉnh sửa ảnh đại diện và tiểu sử.

---

### Quản lý mối quan hệ phức tạp

**User Story:** *Là người dùng, tôi muốn thêm các loại mối quan hệ khác nhau (con nuôi, anh/chị/em cùng cha khác mẹ) để phản ánh chính xác mối quan hệ phức tạp.*

**Mô tả:**
Mở rộng các loại mối quan hệ giữa các thành viên.

**Tiêu chí chấp nhận:**
- Khi thêm/chỉnh sửa mối quan hệ, chọn loại từ danh sách (ruột thịt, con nuôi, cha dượng/mẹ kế).
- Các mối quan hệ hiển thị rõ ràng trên cây gia phả.

---

### Xuất/Nhập cây gia phả (GEDCOM)

**User Story:** *Là người dùng, tôi muốn xuất và nhập cây gia phả theo các định dạng phổ biến (bao gồm GEDCOM) để chia sẻ hoặc lưu trữ dữ liệu.*

**Mô tả:**
Chức năng cho phép người dùng xuất dữ liệu cây gia phả sang các định dạng phổ biến như PDF, GEDCOM và nhập dữ liệu từ các tệp GEDCOM, đảm bảo khả năng tương thích và trao đổi dữ liệu với các phần mềm gia phả khác.

**Tiêu chí chấp nhận:**
- Tùy chọn xuất dữ liệu sang PDF và GEDCOM phải có sẵn.
- Tùy chọn nhập dữ liệu từ tệp GEDCOM phải có sẵn.
- Tệp xuất phải chứa tất cả thông tin và mối quan hệ đã nhập.
- Hệ thống phải xử lý chính xác dữ liệu khi nhập từ tệp GEDCOM.
- Người dùng có thể chọn các thông tin cụ thể để xuất/nhập.

---

### Xem dòng thời gian sự kiện

**User Story:** *Là người dùng, tôi muốn xem dòng thời gian các sự kiện chính của gia đình (sinh, kết hôn, mất) được hiển thị theo năm sinh để có cái nhìn tổng quan.*

**Mô tả:**
Chức năng tạo chế độ xem dòng thời gian trực quan, hiển thị các sự kiện quan trọng (sinh, kết hôn, mất) của các thành viên trong gia đình, được sắp xếp theo năm sinh của thành viên hoặc theo thời gian diễn ra sự kiện.

**Tiêu chí chấp nhận:**
- Dòng thời gian phải hiển thị các sự kiện chính (sinh, kết hôn, mất) của các thành viên.
- Các sự kiện phải được sắp xếp và hiển thị theo năm sinh của thành viên hoặc theo thời gian diễn ra sự kiện.
- Mỗi sự kiện trên dòng thời gian phải liên kết đến thành viên liên quan.
- Người dùng có thể lọc các sự kiện theo loại (sinh, kết hôn, mất) hoặc theo thành viên.
- Giao diện dòng thời gian phải trực quan và dễ hiểu.

---

### Đính kèm ghi chú/tài liệu

**User Story:** *Là người dùng, tôi muốn thêm ghi chú hoặc tài liệu đính kèm (giấy khai sinh, ảnh cũ) vào từng thành viên để lưu giữ thông tin bổ sung.*

**Mô tả:**
Cho phép đính kèm thông tin bổ sung vào hồ sơ thành viên.

**Tiêu chí chấp nhận:**
- Mỗi thành viên có phần thêm ghi chú văn bản.
- Tải lên và đính kèm tệp (ảnh, PDF) vào hồ sơ thành viên.
- Tệp đính kèm có thể xem hoặc tải xuống.

---

### Báo cáo thống kê & Kỷ lục gia đình

**User Story:** *Là người dùng, tôi muốn xem và tạo các báo cáo thống kê về cây gia phả, bao gồm các kỷ lục gia đình, để có cái nhìn sâu sắc và phân tích dữ liệu.*

**Mô tả:**
Chức năng cho phép người dùng xem các số liệu thống kê tổng quan và tạo các báo cáo chi tiết về cây gia phả, bao gồm số lượng thành viên, phân bố giới tính, độ tuổi trung bình, số thế hệ, và các kỷ lục gia đình (ví dụ: thành viên lớn tuổi nhất, gia đình lớn nhất, cuộc hôn nhân dài nhất).

**Tiêu chí chấp nhận:**
- Hệ thống phải cung cấp các số liệu thống kê cơ bản như tổng số thành viên, số lượng nam/nữ, độ tuổi trung bình, số thế hệ.
- Các kỷ lục gia đình (thành viên lớn tuổi nhất, gia đình lớn nhất, cuộc hôn nhân dài nhất) phải được hiển thị rõ ràng, có thể trên bảng điều khiển (dashboard).
- Người dùng có thể tạo các báo cáo tùy chỉnh dựa trên các tiêu chí khác nhau (ví dụ: số lượng thành viên theo địa điểm, theo nghề nghiệp).
- Các báo cáo có thể được trình bày dưới dạng bảng, biểu đồ (cột, tròn, đường) hoặc các hình thức trực quan khác.
- Người dùng có thể xuất báo cáo sang các định dạng phổ biến (ví dụ: PDF, CSV).
- Các báo cáo phải cung cấp cái nhìn sâu sắc về cấu trúc và đặc điểm của gia đình.

---

### In cây gia phả

**User Story:** *Là người dùng, tôi muốn in cây gia phả để có bản sao vật lý hoặc chia sẻ ngoại tuyến.*

**Mô tả:**
Chức năng in cây gia phả.

**Tiêu chí chấp nhận:**
- Tùy chọn in có sẵn.
- Bản in hiển thị cây gia phả rõ ràng, dễ đọc.
- Chọn tùy chọn in (kích thước giấy, hướng).

---

### Đăng nhập hệ thống

**User Story:** *Là người dùng, tôi muốn đăng nhập vào hệ thống để truy cập dữ liệu cây gia phả của mình.*

**Mô tả:**
Chức năng đăng nhập để người dùng có thể truy cập dữ liệu cá nhân.

**Tiêu chí chấp nhận:**
- Giao diện đăng nhập có các trường cho email/tên đăng nhập và mật khẩu.
- Khi người dùng nhập thông tin hợp lệ và gửi, hệ thống sẽ xác thực thành công.
- Sau khi đăng nhập thành công, người dùng được chuyển hướng đến trang tổng quan (dashboard).
- Nếu thông tin không hợp lệ, hệ thống phải hiển thị thông báo lỗi rõ ràng.

---

### Quản lý tài khoản và vai trò

**User Story:** *Là quản trị viên hệ thống, tôi muốn quản lý tài khoản người dùng với nhiều vai trò và quyền hạn khác nhau.*

**Mô tả:**
Chức năng cho phép quản trị viên hệ thống tạo, chỉnh sửa, xóa tài khoản người dùng và gán các vai trò khác nhau (ví dụ: quản trị viên, người chỉnh sửa, người xem) với các quyền hạn cụ thể cho từng vai trò.

**Tiêu chí chấp nhận:**
- Quản trị viên có thể tạo, chỉnh sửa và xóa tài khoản người dùng.
- Quản trị viên có thể gán các vai trò khác nhau cho người dùng (ví dụ: Admin, Người quản lý gia phả, Người xem).
- Mỗi vai trò phải có tập hợp các quyền hạn được định nghĩa rõ ràng (ví dụ: quyền chỉnh sửa thông tin thành viên, quyền mời người dùng, quyền xem báo cáo).
- Hệ thống phải kiểm tra quyền của người dùng trước khi cho phép thực hiện bất kỳ hành động nào.
- Người dùng chỉ có thể thực hiện các hành động được phép bởi vai trò của họ.
- Có thể có giao diện để quản lý các vai trò và quyền hạn.

---

### Quản lý chế độ xem thông tin gia đình (Công khai/Riêng tư)

**User Story:** *Là người dùng/Quản trị viên hệ thống, tôi muốn thông tin gia đình có thể được đặt ở chế độ công khai hoặc riêng tư để kiểm soát ai có thể xem và tìm kiếm.*

**Mô tả:**
Chức năng cho phép người dùng hoặc quản trị viên đặt chế độ hiển thị cho thông tin gia đình là công khai (mọi người đều xem được) hoặc riêng tư (chỉ người có quyền mới xem được).

**Tiêu chí chấp nhận:**
- Khi thông tin gia đình ở chế độ 'Công khai', tất cả người dùng (kể cả khách) có thể xem và tìm kiếm thông tin đó.
- Khi thông tin gia đình ở chế độ 'Riêng tư', chỉ người dùng có vai trò 'Admin', người quản lý gia đình, hoặc người được cấp quyền xem cụ thể mới có thể xem thông tin đó.
- Người dùng không có quyền sẽ không thể xem hoặc tìm kiếm thông tin gia đình ở chế độ 'Riêng tư'.
- Phải có tùy chọn để chuyển đổi giữa chế độ 'Công khai' và 'Riêng tư' cho thông tin gia đình.

---

### Thay đổi nút gốc cây gia phả

**User Story:** *Là người dùng, tôi muốn chọn một thành viên bất kỳ và vẽ lại cây gia phả với thành viên đó làm nút gốc.*

**Mô tả:**
Chức năng cho phép người dùng chọn một thành viên trong cây gia phả và hệ thống sẽ tự động vẽ lại cây, đặt thành viên được chọn làm nút gốc, hiển thị các mối quan hệ xung quanh thành viên đó.

**Tiêu chí chấp nhận:**
- Khi người dùng chọn một thành viên (ví dụ: nhấp đúp hoặc chọn từ menu ngữ cảnh), cây gia phả sẽ được vẽ lại.
- Thành viên được chọn phải hiển thị ở vị trí trung tâm hoặc vị trí gốc của cây mới.
- Các mối quan hệ (cha mẹ, con cái, vợ/chồng, anh chị em) của thành viên gốc mới phải được hiển thị rõ ràng.
- Phải có cách để quay lại chế độ xem cây gia phả ban đầu hoặc nút gốc trước đó.

---

### Xác thực dữ liệu họ tên thành viên

**User Story:** *Là người dùng, tôi muốn trường họ tên của thành viên là bắt buộc nhập để đảm bảo tính đúng đắn của dữ liệu.*

**Mô tả:**
Chức năng yêu cầu người dùng phải nhập đầy đủ họ tên khi thêm hoặc chỉnh sửa thông tin thành viên, nhằm duy trì tính toàn vẹn và chính xác của dữ liệu.

**Tiêu chí chấp nhận:**
- Khi thêm thành viên mới, trường "Họ tên" không được để trống.
- Khi chỉnh sửa thông tin thành viên, trường "Họ tên" không được để trống.
- Nếu trường "Họ tên" bị bỏ trống, hệ thống phải hiển thị thông báo lỗi rõ ràng và ngăn không cho lưu dữ liệu.
- Họ tên phải có định dạng hợp lệ (ví dụ: không chứa ký tự đặc biệt không phù hợp).

---

### Xác thực dữ liệu tên gia đình/dòng họ

**User Story:** *Là người dùng, tôi muốn trường tên gia đình/dòng họ là bắt buộc nhập để đảm bảo tính đúng đắn của dữ liệu.*

**Mô tả:**
Chức năng yêu cầu người dùng phải nhập tên gia đình/dòng họ khi tạo hoặc chỉnh sửa thông tin gia phả, nhằm duy trì tính toàn vẹn và chính xác của dữ liệu.

**Tiêu chí chấp nhận:**
- Khi tạo gia phả mới, trường "Tên gia đình/Dòng họ" không được để trống.
- Khi chỉnh sửa thông tin gia phả, trường "Tên gia đình/Dòng họ" không được để trống.
- Nếu trường "Tên gia đình/Dòng họ" bị bỏ trống, hệ thống phải hiển thị thông báo lỗi rõ ràng và ngăn không cho lưu dữ liệu.
- Tên gia đình/dòng họ phải có định dạng hợp lệ (ví dụ: không chứa ký tự đặc biệt không phù hợp).

---

### Xem lịch sử thay đổi

**User Story:** *Là người dùng, tôi muốn xem lịch sử thay đổi của thông tin thành viên và thông tin gia đình để theo dõi các chỉnh sửa.*

**Mô tả:**
Chức năng cho phép người dùng xem lại các thay đổi đã được thực hiện đối với thông tin của từng thành viên và thông tin chung của gia đình, bao gồm ai đã thay đổi, khi nào và nội dung thay đổi.

**Tiêu chí chấp nhận:**
- Mỗi thành viên và thông tin gia đình phải có một phần "Lịch sử thay đổi".
- Lịch sử thay đổi phải hiển thị thời gian thay đổi, người thực hiện thay đổi và nội dung thay đổi (ví dụ: giá trị cũ và giá trị mới).
- Người dùng có thể lọc hoặc sắp xếp lịch sử thay đổi theo thời gian hoặc người thực hiện.
- Chỉ những người dùng có quyền xem thông tin mới có thể xem lịch sử thay đổi tương ứng.

---

### Mời thành viên

**User Story:** *Là người dùng, tôi muốn mời các thành viên khác tham gia hệ thống để cùng xây dựng và quản lý cây gia phả.*

**Mô tả:**
Chức năng cho phép người dùng mời người khác (qua email hoặc mã mời) tham gia vào hệ thống và cấp cho họ các quyền hạn nhất định để cùng chỉnh sửa hoặc xem cây gia phả.

**Tiêu chí chấp nhận:**
- Người dùng có thể gửi lời mời tham gia hệ thống qua email.
- Lời mời có thể bao gồm một mã mời hoặc liên kết đăng ký trực tiếp.
- Người gửi lời mời có thể chỉ định vai trò hoặc quyền hạn ban đầu cho người được mời (ví dụ: chỉ xem, chỉnh sửa).
- Người được mời có thể đăng ký tài khoản và tham gia vào cây gia phả được mời.
- Hệ thống phải có cơ chế quản lý các lời mời đã gửi và trạng thái của chúng.

---

### Quản lý ảnh hồ sơ

**User Story:** *Là người dùng, tôi muốn gắn ảnh hồ sơ cá nhân để cá nhân hóa tài khoản và dễ dàng nhận diện.*

**Mô tả:**
Chức năng cho phép người dùng tải lên, thay đổi hoặc xóa ảnh đại diện cho hồ sơ cá nhân của mình.

**Tiêu chí chấp nhận:**
- Người dùng có thể tải lên một tệp ảnh từ thiết bị của mình để làm ảnh hồ sơ.
- Hệ thống phải hỗ trợ các định dạng ảnh phổ biến (ví dụ: JPG, PNG).
- Người dùng có thể xem trước ảnh hồ sơ trước khi lưu.
- Người dùng có thể thay đổi hoặc xóa ảnh hồ sơ hiện có.
- Ảnh hồ sơ phải hiển thị ở các vị trí thích hợp trong hệ thống (ví dụ: cạnh tên người dùng, trên trang hồ sơ).

---

### Hỗ trợ văn bản dài cho ghi chú và tiểu sử

**User Story:** *Là người dùng, tôi muốn có thể nhập ghi chú và tiểu sử dạng văn bản dài cho thành viên và gia đình.*

**Mô tả:**
Chức năng cho phép người dùng nhập các đoạn văn bản dài cho các trường ghi chú và tiểu sử của thành viên hoặc gia đình, hỗ trợ định dạng cơ bản (ví dụ: xuống dòng, in đậm) để trình bày thông tin rõ ràng.

**Tiêu chí chấp nhận:**
- Các trường "Ghi chú" và "Tiểu sử" phải hỗ trợ nhập nhiều dòng văn bản.
- Không có giới hạn ký tự quá chặt chẽ cho các trường này.
- Hệ thống phải hiển thị đầy đủ nội dung văn bản dài khi xem thông tin.
- (Tùy chọn) Hỗ trợ các định dạng văn bản cơ bản như in đậm, in nghiêng, gạch chân, hoặc danh sách.

---

### Hỗ trợ đa gia phả (Multi-tree)

**User Story:** *Là người dùng, tôi muốn có thể quản lý nhiều cây gia phả khác nhau trong hệ thống.*

**Mô tả:**
Chức năng cho phép người dùng tạo, quản lý và chuyển đổi giữa nhiều cây gia phả độc lập trong cùng một tài khoản, phục vụ nhu cầu quản lý các dòng họ khác nhau hoặc các nhánh gia đình riêng biệt.

**Tiêu chí chấp nhận:**
- Người dùng có thể tạo một cây gia phả mới.
- Người dùng có thể xem danh sách các cây gia phả mà họ đang quản lý hoặc có quyền truy cập.
- Người dùng có thể chuyển đổi dễ dàng giữa các cây gia phả khác nhau.
- Mỗi cây gia phả phải có dữ liệu thành viên và mối quan hệ riêng biệt.
- Quyền truy cập và chỉnh sửa phải được quản lý độc lập cho từng cây gia phả.

---

### Ghép nối cây gia phả

**User Story:** *Là người dùng, tôi muốn có thể ghép nối hai cây gia phả khác nhau thành một cây duy nhất.*

**Mô tả:**
Chức năng cho phép người dùng chọn hai cây gia phả và thực hiện quá trình ghép nối, xử lý các thành viên trùng lặp và tích hợp các mối quan hệ để tạo ra một cây gia phả lớn hơn, hoàn chỉnh hơn.

**Tiêu chí chấp nhận:**
- Người dùng có thể chọn hai cây gia phả để ghép nối.
- Hệ thống phải cung cấp giao diện để người dùng xem xét và giải quyết các xung đột hoặc thành viên trùng lặp trong quá trình ghép nối.
- Người dùng có thể xác định cách xử lý các thành viên trùng lặp (ví dụ: giữ lại một, hợp nhất thông tin).
- Sau khi ghép nối thành công, một cây gia phả mới sẽ được tạo ra hoặc một trong hai cây ban đầu được cập nhật với dữ liệu từ cây kia.
- Các mối quan hệ giữa các thành viên từ hai cây ban đầu phải được duy trì và tích hợp chính xác.
- Phải có khả năng hoàn tác thao tác ghép nối nếu có lỗi hoặc không mong muốn.

---

### Quản lý thế hệ

**User Story:** *Là người dùng, tôi muốn có thể quản lý và xem các thế hệ trong cây gia phả của mình.*

**Mô tả:**
Chức năng cho phép người dùng định nghĩa, xem và điều hướng giữa các thế hệ trong cây gia phả, giúp dễ dàng theo dõi sự phát triển của dòng họ qua thời gian.

**Tiêu chí chấp nhận:**
- Hệ thống tự động xác định và hiển thị các thế hệ dựa trên mối quan hệ cha mẹ-con cái.
- Người dùng có thể xem tổng quan về số lượng thế hệ và các thành viên thuộc mỗi thế hệ.
- Có thể có tùy chọn để gán thủ công một thành viên vào một thế hệ cụ thể nếu cần.
- Giao diện hiển thị thế hệ phải rõ ràng, có thể là dạng số hoặc tên thế hệ.
- Người dùng có thể lọc hoặc tập trung vào một thế hệ cụ thể để xem chi tiết.

---

### Phát hiện trùng lặp thành viên

**User Story:** *Là người dùng, tôi muốn hệ thống tự động phát hiện các thành viên có khả năng bị trùng lặp để duy trì tính chính xác của dữ liệu.*

**Mô tả:**
Chức năng tự động quét và xác định các thành viên có thông tin tương tự nhau (ví dụ: cùng tên, ngày sinh, nơi sinh) và đề xuất các hành động để giải quyết trùng lặp.

**Tiêu chí chấp nhận:**
- Hệ thống phải có khả năng tự động quét và liệt kê các thành viên có khả năng trùng lặp.
- Các tiêu chí phát hiện trùng lặp có thể bao gồm tên, ngày sinh, nơi sinh, mối quan hệ gia đình.
- Người dùng phải được thông báo về các thành viên trùng lặp được phát hiện.
- Hệ thống phải cung cấp các tùy chọn để người dùng giải quyết trùng lặp (ví dụ: hợp nhất thông tin, đánh dấu là không trùng lặp).
- Quá trình hợp nhất phải giữ lại tất cả thông tin liên quan và cập nhật các mối quan hệ một cách chính xác.

---

### Ghi âm/Ký ức giọng nói

**User Story:** *Là người dùng, tôi muốn có thể ghi âm hoặc đính kèm các tệp âm thanh (ký ức giọng nói) vào thông tin thành viên hoặc sự kiện.*

**Mô tả:**
Chức năng cho phép người dùng ghi âm trực tiếp hoặc tải lên các tệp âm thanh để đính kèm vào hồ sơ thành viên hoặc các sự kiện quan trọng, giúp lưu giữ ký ức và câu chuyện gia đình một cách sống động và chân thực.

**Tiêu chí chấp nhận:**
- Người dùng có thể ghi âm giọng nói trực tiếp thông qua ứng dụng (nếu có microphone).
- Người dùng có thể tải lên các tệp âm thanh hiện có (ví dụ: MP3, WAV) để đính kèm.
- Các tệp âm thanh phải được liên kết rõ ràng với thành viên hoặc sự kiện cụ thể.
- Người dùng có thể phát lại các tệp âm thanh đã đính kèm.
- Có thể có giới hạn về kích thước hoặc thời lượng của tệp âm thanh.

---

### Gợi ý tiểu sử bằng AI

**User Story:** *Là người dùng, tôi muốn AI có thể gợi ý nội dung tiểu sử dựa trên dữ liệu thành viên đã có sẵn.*

**Mô tả:**
Chức năng sử dụng trí tuệ nhân tạo để phân tích các thông tin đã nhập về một thành viên (ví dụ: ngày sinh, ngày mất, mối quan hệ, sự kiện, ghi chú) và tự động tạo ra một bản nháp tiểu sử gợi ý.

**Tiêu chí chấp nhận:**
- Khi người dùng chọn một thành viên, hệ thống có thể tạo ra một bản nháp tiểu sử dựa trên dữ liệu có sẵn.
- Gợi ý tiểu sử phải bao gồm các thông tin cơ bản và các sự kiện quan trọng của thành viên.
- Người dùng có thể chỉnh sửa bản nháp tiểu sử được gợi ý trước khi lưu.
- Chất lượng gợi ý phải hợp lý và có thể sử dụng làm điểm khởi đầu.
- Hệ thống phải chỉ rõ rằng đây là nội dung được AI gợi ý.

---

### Hỗ trợ đa ngôn ngữ

**User Story:** *Là người dùng, tôi muốn hệ thống hỗ trợ nhiều ngôn ngữ để tôi có thể sử dụng ứng dụng bằng ngôn ngữ ưa thích của mình.*

**Mô tả:**
Chức năng cho phép người dùng chọn ngôn ngữ hiển thị của giao diện người dùng và nội dung trong ứng dụng, giúp ứng dụng thân thiện hơn với người dùng từ các quốc gia và nền văn hóa khác nhau.

**Tiêu chí chấp nhận:**
- Người dùng có thể chọn ngôn ngữ hiển thị từ danh sách các ngôn ngữ được hỗ trợ.
- Giao diện người dùng (menu, nút, nhãn) phải được dịch sang ngôn ngữ đã chọn.
- Nội dung tĩnh trong ứng dụng (ví dụ: hướng dẫn, thông báo lỗi) phải được dịch.
- Hệ thống phải lưu cài đặt ngôn ngữ của người dùng.
- (Tùy chọn) Hỗ trợ nhập liệu và hiển thị thông tin thành viên bằng nhiều ngôn ngữ.

---

### In poster gia phả

**User Story:** *Là người dùng, tôi muốn có thể tích hợp tính năng in poster cây gia phả để tạo ra bản in lớn, chất lượng cao.*

**Mô tả:**
Chức năng cho phép người dùng thiết kế và đặt in poster cây gia phả trực tiếp từ hệ thống, hỗ trợ các tùy chọn về kích thước, bố cục và chất liệu in.

**Tiêu chí chấp nhận:**
- Người dùng có thể chọn bố cục và kích thước cho poster gia phả.
- Hệ thống phải cung cấp giao diện xem trước poster trước khi in.
- Có tùy chọn để tùy chỉnh thông tin hiển thị trên poster (ví dụ: chỉ hiển thị tên, ngày sinh/mất).
- Hệ thống phải tích hợp với dịch vụ in ấn bên ngoài hoặc cung cấp tệp tin chất lượng cao để người dùng tự in.
- Chất lượng hình ảnh và văn bản trên poster phải rõ nét, không bị vỡ khi in lớn.

---

### Kết nối liên gia đình (Cross-Family Linking)

**User Story:** *Là người dùng, tôi muốn có thể kết nối các thành viên giữa các cây gia phả khác nhau để thể hiện mối quan hệ liên gia đình.*

**Mô tả:**
Chức năng cho phép người dùng tạo liên kết giữa các thành viên thuộc các cây gia phả khác nhau, giúp thể hiện các mối quan hệ hôn nhân hoặc huyết thống chéo giữa các dòng họ mà không cần hợp nhất toàn bộ cây gia phả.

**Tiêu chí chấp nhận:**
- Quản trị viên có thể tạo, chỉnh sửa và xóa tài khoản người dùng.
- Quản trị viên có thể gán các vai trò khác nhau cho người dùng (ví dụ: Admin, Người quản lý gia phả, Người xem).
- Mỗi vai trò phải có tập hợp các quyền hạn được định nghĩa rõ ràng (ví dụ: quyền chỉnh sửa thông tin thành viên, quyền mời người dùng, quyền xem báo cáo).
- Hệ thống phải kiểm tra quyền của người dùng trước khi cho phép thực hiện bất kỳ hành động nào.
- Người dùng chỉ có thể thực hiện các hành động được phép bởi vai trò của họ.
- Có thể có giao diện để quản lý các vai trò và quyền hạn.

---

### Quản lý quyền riêng tư cá nhân

**User Story:** *Là người dùng, tôi muốn có thể quản lý quyền riêng tư cho từng thành viên cụ thể trong cây gia phả.*

**Mô tả:**
Chức năng cho phép người dùng đặt các cấp độ riêng tư khác nhau cho thông tin của từng thành viên trong cây gia phả (ví dụ: công khai, chỉ bạn bè, riêng tư), độc lập với cài đặt riêng tư chung của cây gia phả.

**Tiêu chí chấp nhận:**
- Người dùng có thể truy cập cài đặt riêng tư cho từng thành viên.
- Có các tùy chọn riêng tư khác nhau (ví dụ: hiển thị cho tất cả, chỉ cho thành viên gia đình, chỉ cho người quản lý, ẩn hoàn toàn).
- Các cài đặt riêng tư này phải được áp dụng chính xác khi thông tin thành viên được xem bởi các người dùng khác nhau.
- Người dùng có thể chọn các trường thông tin cụ thể của thành viên để áp dụng cài đặt riêng tư (ví dụ: ẩn ngày sinh nhưng hiển thị tên).
- Hệ thống phải đảm bảo rằng thông tin riêng tư không bị lộ ra ngoài theo cài đặt của người dùng.

---

### In 3D cây gia phả

**User Story:** *Là người dùng, tôi muốn có thể tích hợp tính năng in 3D cây gia phả để tạo ra mô hình vật lý.*

**Mô tả:**
Chức năng cho phép người dùng chuyển đổi dữ liệu cây gia phả thành mô hình 3D và tích hợp với dịch vụ in 3D để tạo ra một bản sao vật lý của cây gia phả.

**Tiêu chí chấp nhận:**
- Người dùng có thể chọn các thành viên và mối quan hệ để đưa vào mô hình 3D.
- Hệ thống phải cung cấp giao diện xem trước mô hình 3D trước khi in.
- Có các tùy chọn để tùy chỉnh thiết kế mô hình 3D (ví dụ: kiểu dáng, vật liệu, màu sắc).
- Hệ thống phải tích hợp với dịch vụ in 3D bên ngoài hoặc cung cấp tệp mô hình 3D (ví dụ: STL, OBJ) để người dùng tự in.
- Mô hình 3D phải thể hiện rõ ràng cấu trúc và mối quan hệ của cây gia phả.

---

### Gắn biệt danh/Tên gọi khác

**User Story:** *Là người dùng, tôi muốn có thể gắn biệt danh hoặc các tên gọi khác cho thành viên để thông tin được đầy đủ và dễ nhận diện.*

**Mô tả:**
Chức năng cho phép người dùng thêm các biệt danh, tên gọi khác, hoặc tên khai sinh (nếu khác với tên hiện tại) cho một thành viên, giúp làm phong phú hồ sơ và hỗ trợ tìm kiếm.

**Tiêu chí chấp nhận:**
- Người dùng có thể thêm một hoặc nhiều biệt danh/tên gọi khác cho mỗi thành viên.
- Các biệt danh/tên gọi khác phải hiển thị rõ ràng trong thông tin chi tiết của thành viên.
- Chức năng tìm kiếm phải có khả năng tìm kiếm thành viên dựa trên biệt danh/tên gọi khác.
- Người dùng có thể chỉnh sửa hoặc xóa các biệt danh/tên gọi khác đã thêm.

---

### Quản lý nghề nghiệp & học vấn

**User Story:** *Là người dùng, tôi muốn có thể quản lý thông tin nghề nghiệp và học vấn của các thành viên.*

**Mô tả:**
Chức năng cho phép người dùng thêm, chỉnh sửa và xem các thông tin liên quan đến nghề nghiệp (ví dụ: chức danh, công ty, thời gian làm việc) và học vấn (ví dụ: trường học, bằng cấp, năm tốt nghiệp) của từng thành viên.

**Tiêu chí chấp nhận:**
- Hồ sơ thành viên phải có các trường riêng biệt để nhập thông tin nghề nghiệp (ví dụ: chức danh, nơi làm việc).
- Hồ sơ thành viên phải có các trường riêng biệt để nhập thông tin học vấn (ví dụ: tên trường, bằng cấp, chuyên ngành, năm tốt nghiệp).
- Người dùng có thể thêm nhiều mục nghề nghiệp và học vấn cho một thành viên (ví dụ: nhiều công việc, nhiều bằng cấp).
- Thông tin này phải hiển thị rõ ràng trong thông tin chi tiết của thành viên.
- (Tùy chọn) Có thể tìm kiếm thành viên dựa trên thông tin nghề nghiệp hoặc học vấn.

---

### Tích hợp mạng xã hội

**User Story:** *Là người dùng, tôi muốn có thể kết nối tài khoản của mình với mạng xã hội để chia sẻ thông tin gia phả hoặc đăng nhập dễ dàng.*

**Mô tả:**
Chức năng cho phép người dùng liên kết tài khoản hệ thống với các tài khoản mạng xã hội phổ biến (ví dụ: Facebook, Google) để chia sẻ các sự kiện, thông tin gia phả hoặc sử dụng tính năng đăng nhập nhanh.

**Tiêu chí chấp nhận:**
- Người dùng có thể liên kết tài khoản hệ thống với tài khoản mạng xã hội.
- Có thể chia sẻ các sự kiện hoặc thông tin thành viên (tùy chọn) lên mạng xã hội.
- Hỗ trợ đăng nhập bằng tài khoản mạng xã hội (OAuth).
- Hệ thống phải tuân thủ các chính sách bảo mật và quyền riêng tư của mạng xã hội.
- Người dùng có thể ngắt kết nối với mạng xã hội bất cứ lúc nào.

---

### Tìm kiếm mối quan hệ

**User Story:** *Là người dùng, tôi muốn tìm kiếm mối quan hệ giữa hai thành viên bất kỳ trong cây gia phả.*

**Mô tả:**
Chức năng cho phép người dùng chọn hai thành viên và hệ thống sẽ tự động xác định và hiển thị mối quan hệ giữa họ (ví dụ: anh em, cô cháu, họ hàng xa) cùng với đường dẫn liên kết trên cây gia phả.

**Tiêu chí chấp nhận:**
- Người dùng có thể chọn hai thành viên để tìm kiếm mối quan hệ.
- Hệ thống phải xác định và hiển thị loại mối quan hệ (ví dụ: cha con, vợ chồng, anh chị em, cô cháu, họ hàng).
- Hệ thống phải hiển thị đường dẫn hoặc chuỗi liên kết giữa hai thành viên trên cây gia phả.
- Nếu không có mối quan hệ trực tiếp, hệ thống phải thông báo.

---

### Nhận diện khuôn mặt bằng AI để tự động gắn thẻ

**User Story:** *Là người dùng, tôi muốn AI nhận diện khuôn mặt từ ảnh cũ để tự động gắn thẻ thành viên.*

**Mô tả:**
Chức năng sử dụng AI để quét các bức ảnh (đơn hoặc nhóm) được tải lên, nhận diện các khuôn mặt và gợi ý gắn thẻ (tag) các thành viên tương ứng trong cây gia phả.

**Tiêu chí chấp nhận:**
- Người dùng có thể tải lên một hoặc nhiều ảnh.
- Hệ thống tự động phát hiện các khuôn mặt trong ảnh.
- Đối với mỗi khuôn mặt, AI so sánh với ảnh đại diện của các thành viên và đề xuất tên thành viên có khả năng trùng khớp cao nhất.
- Người dùng có thể xác nhận, sửa hoặc từ chối gợi ý gắn thẻ.
- Sau khi xác nhận, ảnh được liên kết với hồ sơ của thành viên.

---

### Tìm kiếm bằng khuôn mặt

**User Story:** *Là người dùng, tôi muốn tìm kiếm thành viên bằng cách tải lên một hình ảnh khuôn mặt.*

**Mô tả:**
Chức năng cho phép người dùng tải lên một bức ảnh chân dung và hệ thống sẽ tìm kiếm trong cây gia phả để tìm ra thành viên có khuôn mặt trùng khớp nhất.

**Tiêu chí chấp nhận:**
- Giao diện cho phép người dùng tải lên một tệp ảnh.
- Hệ thống phân tích khuôn mặt trong ảnh tải lên và so sánh với dữ liệu thành viên.
- Hệ thống trả về danh sách các thành viên có khả năng trùng khớp cao, sắp xếp theo độ tương đồng.
- Kết quả tìm kiếm hiển thị ảnh đại diện, tên và liên kết đến hồ sơ thành viên.

---

### Bảo mật dữ liệu nhận diện khuôn mặt

**User Story:** *Là người dùng, tôi muốn quản lý quyền riêng tư cho dữ liệu nhận diện khuôn mặt của mình.*

**Mô tả:**
Chức năng cung cấp các tùy chọn để người dùng quản lý việc sử dụng dữ liệu khuôn mặt của họ cho mục đích nhận diện và tìm kiếm, đồng thời đảm bảo dữ liệu được lưu trữ an toàn.

**Tiêu chí chấp nhận:**
- Người dùng có quyền cho phép hoặc từ chối việc sử dụng ảnh của họ để huấn luyện mô hình nhận diện.
- Dữ liệu vector khuôn mặt phải được lưu trữ an toàn và mã hóa.
- Phải có chính sách rõ ràng về việc thu thập và sử dụng dữ liệu khuôn mặt, yêu cầu sự đồng ý của người dùng.
- Người dùng có thể xóa dữ liệu nhận diện khuôn mặt của mình khỏi hệ thống.

---

### Cộng tác chỉnh sửa trong thời gian thực

**User Story:** *Là người dùng, tôi muốn cộng tác chỉnh sửa cây gia phả với các thành viên khác trong thời gian thực.*

**Mô tả:**
Chức năng cho phép nhiều người dùng cùng chỉnh sửa một cây gia phả đồng thời, với các thay đổi được cập nhật ngay lập tức cho tất cả mọi người.

**Tiêu chí chấp nhận:**
- Khi một người dùng chỉnh sửa thông tin thành viên, các thay đổi phải hiển thị ngay lập tức cho những người dùng khác đang xem.
- Hệ thống phải hiển thị con trỏ hoặc chỉ báo cho biết người dùng nào đang chỉnh sửa trường nào.
- Phải có cơ chế xử lý xung đột khi hai người dùng chỉnh sửa cùng một dữ liệu.
- Lịch sử thay đổi phải ghi lại ai đã thực hiện chỉnh sửa nào.

---

### Quản lý truyền thống và lễ hội gia đình

**User Story:** *Là người dùng, tôi muốn ghi lại các truyền thống và lễ hội quan trọng của gia đình.*

**Mô tả:**
Chức năng cho phép người dùng tạo và quản lý một không gian riêng để ghi lại các truyền thống, lễ hội, hoặc các sự kiện văn hóa đặc trưng của dòng họ (ví dụ: ngày giỗ tổ, lễ mừng thọ, các quy tắc gia phong).

**Tiêu chí chấp nhận:**
- Người dùng có thể tạo một sự kiện/truyền thống mới với tên, mô tả, ngày diễn ra (nếu có).
- Có thể đính kèm hình ảnh, video, hoặc tài liệu liên quan đến sự kiện.
- Các sự kiện/truyền thống được hiển thị trên một trang riêng hoặc trên dòng thời gian của gia đình.
- Người dùng có thể bình luận hoặc chia sẻ kỷ niệm về các sự kiện này.

---

### Thông báo ngày giỗ/ngày kỷ niệm

**User Story:** *Là người dùng, tôi muốn nhận được thông báo về các ngày giỗ và ngày kỷ niệm quan trọng của gia đình.*

**Mô tả:**
Hệ thống tự động gửi thông báo (qua email hoặc trong ứng dụng) cho người dùng khi sắp đến ngày giỗ của một thành viên đã mất hoặc các ngày kỷ niệm quan trọng khác (ngày cưới, sinh nhật).

**Tiêu chí chấp nhận:**
- Hệ thống tự động quét ngày mất và ngày kỷ niệm của các thành viên.
- Người dùng nhận được thông báo trước một khoảng thời gian có thể tùy chỉnh (ví dụ: 1 tuần, 3 ngày).
- Thông báo phải bao gồm tên thành viên và sự kiện sắp diễn ra.
- Người dùng có thể bật/tắt hoặc tùy chỉnh các loại thông báo muốn nhận.

---

### Chatbot AI hỗ trợ

**User Story:** *Là người dùng, tôi muốn hỏi đáp với chatbot AI để được hỗ trợ hoặc tra cứu thông tin nhanh.*

**Mô tả:**
Tích hợp một chatbot sử dụng trí tuệ nhân tạo để trả lời các câu hỏi thường gặp về cách sử dụng ứng dụng, hoặc tra cứu nhanh thông tin về thành viên, sự kiện trong gia phả.

**Tiêu chí chấp nhận:**
- Chatbot có thể trả lời các câu hỏi về chức năng của hệ thống (ví dụ: "Làm thế nào để thêm thành viên?").
- Chatbot có thể tra cứu thông tin khi được hỏi (ví dụ: "Ngày sinh của ông Nguyễn Văn A là khi nào?").
- Chatbot phải tuân thủ quyền riêng tư, không tiết lộ thông tin nhạy cảm.
- Nếu chatbot không biết câu trả lời, nó sẽ hướng dẫn người dùng đến các kênh hỗ trợ khác.

---

### Tìm kiếm thông minh bằng ngôn ngữ tự nhiên

**User Story:** *Là người dùng, tôi muốn tìm kiếm thông tin bằng ngôn ngữ tự nhiên thay vì các bộ lọc phức tạp.*

**Mô tả:**
Chức năng cho phép người dùng nhập các câu hỏi tìm kiếm bằng ngôn ngữ tự nhiên (ví dụ: "tìm tất cả con của ông X", "những ai sinh năm 1980?") và hệ thống sẽ hiểu và trả về kết quả chính xác.

**Tiêu chí chấp nhận:**
- Thanh tìm kiếm chấp nhận các câu lệnh bằng ngôn ngữ tự nhiên.
- Hệ thống có thể phân tích và hiểu các thực thể (tên, ngày tháng, mối quan hệ) trong câu hỏi.
- Kết quả tìm kiếm phải phù hợp với ý định của người dùng.
- Hỗ trợ các câu hỏi phức tạp kết hợp nhiều điều kiện.

---

### AI kể chuyện (Storytelling)

**User Story:** *Là người dùng, tôi muốn AI tự động kể lại câu chuyện cuộc đời của một thành viên dựa trên dữ liệu có sẵn.*

**Mô tả:**
Chức năng sử dụng AI để tổng hợp thông tin từ hồ sơ của một thành viên (ngày sinh/mất, sự kiện, tiểu sử, mối quan hệ, ảnh) và tạo ra một bài tường thuật hoặc video ngắn kể lại câu chuyện cuộc đời của họ.

**Tiêu chí chấp nhận:**
- Người dùng có thể chọn một thành viên và yêu cầu AI tạo câu chuyện.
- Câu chuyện được tạo ra phải có cấu trúc tường thuật (mở đầu, thân bài, kết luận).
- Câu chuyện có thể được trình bày dưới dạng văn bản, slideshow ảnh với chú thích, hoặc video ngắn.
- Người dùng có thể chỉnh sửa hoặc bổ sung nội dung cho câu chuyện do AI tạo ra.