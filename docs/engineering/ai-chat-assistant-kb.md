# Cơ sở Kiến thức Tối ưu cho Chat Assistant về Ứng dụng Gia Phả Việt

Tài liệu này cung cấp cơ sở kiến thức đã được tối ưu hóa cho Chat Assistant, giúp trả lời các câu hỏi của người dùng về ứng dụng Gia Phả Việt một cách chính xác và hiệu quả. Tài liệu tập trung vào kiến thức miền, các quy tắc nghiệp vụ, logic quan hệ gia đình, và hướng dẫn sử dụng giao diện người dùng, đồng thời loại bỏ các chi tiết triển khai kỹ thuật không cần thiết.

---

## Mục lục

- [1. Tổng quan (Overview)](#1-tổng-quan-overview)
- [2. Mô hình miền (Domain Model)](#2-mô-hình-miền-domain-model)
- [3. Các quy tắc nghiệp vụ chính (Key Business Rules)](#3-các-quy-tắc-nghiệp-vụ-chính-key-business-rules)
- [4. Logic quan hệ gia đình (Family Relationship Logic)](#4-logic-quan-hệ-gia-đình-family-relationship-logic)
- [5. Luồng giao diện người dùng (UI/UX Flow)](#5-luồng-giao-diện-người-dùng-uiux-flow)
- [6. Ví dụ câu hỏi người dùng & phản hồi của AI (Examples of User Questions & AI Responses)](#6-ví-dụ-câu-hỏi-người-dùng--phản-hồi-của-ai-examples-of-user-questions--ai-responses)
- [7. Bảng thuật ngữ (Glossary)](#7-bảng-thuật-ngữ-glossary)

---

# 1. Tổng quan (Overview)

Ứng dụng **Gia Phả Việt** là một hệ thống quản lý gia phả toàn diện, được thiết kế để giúp người dùng tạo, quản lý, trực quan hóa và chia sẻ cây gia phả của gia đình hoặc dòng họ một cách dễ dàng và chuyên nghiệp.

*   **Mục tiêu chính:** Bảo tồn và khám phá lịch sử gia đình, cung cấp một nền tảng trực quan và dễ sử dụng.
*   **Đối tượng người dùng:** Bao gồm những người muốn lưu giữ thông tin gia phả, quản lý các thành viên và mối quan hệ, cũng như khám phá nguồn gốc dòng họ của mình.
*   **Giá trị cốt lõi:**
    *   **Trực quan hóa cây gia phả:** Hiển thị mối quan hệ một cách rõ ràng và dễ hiểu.
    *   **Quản lý thông tin đa dạng:** Lưu trữ chi tiết về từng thành viên, sự kiện, mối quan hệ.
    *   **Tích hợp AI:** Hỗ trợ các tính năng thông minh như tạo tiểu sử tự động và nhận diện khuôn mặt.
    *   **Cộng tác:** Cho phép nhiều người cùng đóng góp và quản lý thông tin gia phả.

---

# 2. Mô hình miền (Domain Model)

Các khái niệm và thực thể chính mà ứng dụng Gia Phả Việt quản lý:

*   **Dòng họ (Family)**:
    *   **Mô tả**: Một tập hợp các thành viên có liên quan, tạo thành một cây gia phả duy nhất. Đây là đơn vị tổ chức cao nhất trong ứng dụng.
    *   **Thuộc tính chính**: Tên dòng họ, mô tả, chế độ hiển thị (công khai/riêng tư).
    *   **Hành vi**: Thêm/xóa/cập nhật thành viên, tạo/sửa đổi mối quan hệ, quản lý các sự kiện liên quan đến dòng họ.
    *   **Ràng buộc**: Mỗi dòng họ có một tên duy nhất.
*   **Thành viên (Member)**:
    *   **Mô tả**: Một cá nhân cụ thể trong một dòng họ.
    *   **Thuộc tính chính**: Tên, họ, giới tính, ngày sinh, ngày mất, nơi sinh, nơi mất, ảnh đại diện, tiểu sử, thông tin liên hệ (số điện thoại, email, địa chỉ), nghề nghiệp, và dấu hiệu là thành viên gốc của gia phả.
    *   **Hành vi**: Cập nhật thông tin cá nhân, thêm/xóa ảnh, yêu cầu AI tạo tiểu sử tự động.
    *   **Ràng buộc**: Phải thuộc về một Dòng họ. Tên và họ là bắt buộc. Ngày mất phải sau ngày sinh.
*   **Mối quan hệ (Relationship)**:
    *   **Mô tả**: Sự liên kết giữa hai thành viên trong cùng một dòng họ.
    *   **Thuộc tính chính**: Thành viên nguồn, thành viên đích, loại quan hệ (ví dụ: cha-con, vợ-chồng).
    *   **Hành vi**: Tạo, cập nhật, xóa mối quan hệ.
    *   **Ràng buộc**: Luôn liên kết hai thành viên hiện có. Phải thuộc về một Dòng họ.
*   **Sự kiện (Event)**:
    *   **Mô tả**: Các dịp quan trọng gắn liền với một dòng họ hoặc một thành viên cụ thể.
    *   **Thuộc tính chính**: Tên sự kiện, mô tả, ngày bắt đầu, ngày kết thúc, địa điểm, loại sự kiện (ví dụ: sinh, mất, kết hôn, di cư).
    *   **Hành vi**: Tạo, cập nhật, xóa sự kiện.
    *   **Ràng buộc**: Phải thuộc về một Dòng họ. Tên sự kiện là bắt buộc.
*   **Người dùng (User)**:
    *   **Mô tả**: Tài khoản cá nhân sử dụng ứng dụng.
    *   **Thuộc tính chính**: Email, ID từ nhà cung cấp xác thực bên ngoài.
    *   **Hành vi**: Đăng nhập, quản lý thông tin cá nhân.
*   **Hồ sơ người dùng (UserProfile)**:
    *   **Mô tả**: Thông tin chi tiết về người dùng, liên kết trực tiếp với tài khoản Người dùng.
    *   **Thuộc tính chính**: Tên hiển thị, tên riêng, họ, số điện thoại, ảnh đại diện.
*   **Tùy chọn người dùng (UserPreference)**:
    *   **Mô tả**: Các cài đặt cá nhân của người dùng cho ứng dụng.
    *   **Thuộc tính chính**: Chủ đề giao diện (sáng/tối), ngôn ngữ hiển thị.
*   **Vai trò trong dòng họ (FamilyUser)**:
    *   **Mô tả**: Xác định quyền hạn của một người dùng đối với một dòng họ cụ thể.
    *   **Thuộc tính chính**: Dòng họ, Người dùng, Vai trò (Quản lý/Xem).
*   **Cấu hình hệ thống (SystemConfiguration)**:
    *   **Mô tả**: Các cài đặt chung của ứng dụng có thể được điều chỉnh bởi quản trị viên.
    *   **Thuộc tính chính**: Khóa cấu hình, giá trị, mô tả.

---

# 3. Các quy tắc nghiệp vụ chính (Key Business Rules)

Đây là những quy tắc cốt lõi mà ứng dụng tuân thủ để đảm bảo tính chính xác và nhất quán của dữ liệu gia phả:

*   **BR001: Quy tắc về quan hệ cha/mẹ/con**:
    *   Mỗi thành viên có thể có nhiều cha mẹ (sinh học hoặc nhận nuôi), nhưng chỉ được xác định tối đa một cha ruột và một mẹ ruột.
    *   Cha mẹ phải có ngày sinh trước con cái.
    *   Một thành viên không thể là cha/mẹ của chính mình.
    *   Tất cả các thành viên trong mối quan hệ phải thuộc cùng một dòng họ.
*   **BR002: Quy tắc về quan hệ vợ/chồng**:
    *   Các thành viên trong mối quan hệ vợ chồng phải khác giới tính được khai báo (hoặc giới tính tùy chỉnh phù hợp).
    *   Ngày kết hôn phải sau ngày sinh của cả hai vợ chồng và trước ngày mất của bất kỳ ai trong số họ.
    *   Mối quan hệ vợ chồng phải nằm trong cùng một dòng họ.
*   **BR003: Tránh trùng lặp thành viên**:
    *   Trong một dòng họ, hệ thống sẽ cố gắng ngăn chặn việc tạo ra các thành viên có thông tin quá giống nhau (tên, họ, ngày sinh, giới tính) để tránh trùng lặp dữ liệu.
*   **BR004: Tính toàn vẹn của Dòng họ**:
    *   Mọi thay đổi đối với thành viên, mối quan hệ, hoặc sự kiện phải được thực hiện thông qua dòng họ mà chúng thuộc về. Bạn không thể thêm một thành viên mà không gán cho họ một dòng họ.
*   **BR005: Ngày tháng hợp lệ**:
    *   Ngày mất của một thành viên phải sau ngày sinh của họ.
    *   Ngày bắt đầu của một sự kiện phải trước hoặc bằng ngày kết thúc của sự kiện đó.
*   **BR006: Vai trò người dùng trong dòng họ**:
    *   Người dùng có vai trò **Quản lý** có thể thêm, sửa, xóa bất kỳ thông tin nào trong dòng họ.
    *   Người dùng có vai trò **Xem** chỉ có thể xem thông tin của dòng họ.
*   **BR007: Xóa mềm (Soft Delete)**:
    *   Khi một số loại thông tin (ví dụ: vai trò của người dùng trong dòng họ) bị "xóa", chúng không bị loại bỏ hoàn toàn khỏi hệ thống mà chỉ được đánh dấu là không hoạt động, để có thể khôi phục nếu cần.
*   **BR008: Xác thực dữ liệu đầu vào**:
    *   Mọi thông tin người dùng nhập vào đều được kiểm tra kỹ lưỡng để đảm bảo tính hợp lệ và an toàn, giúp ngăn chặn lỗi và các vấn đề bảo mật.
*   **BR009: Quyền truy cập thông tin**:
    *   Bạn chỉ có thể xem hoặc chỉnh sửa thông tin của một dòng họ nếu bạn được cấp quyền truy cập vào dòng họ đó.

---

# 4. Logic quan hệ gia đình (Family Relationship Logic)

Ứng dụng hiểu và hiển thị nhiều loại mối quan hệ gia đình dựa trên thông tin về cha mẹ, con cái và vợ/chồng.

*   **Mối quan hệ cơ bản**:
    *   **Cha/Mẹ (Parent)**: Là người sinh ra hoặc nhận nuôi một thành viên.
    *   **Con (Child)**: Là người được sinh ra hoặc nhận nuôi bởi một thành viên.
    *   **Vợ/Chồng (Spouse)**: Là người có mối quan hệ hôn nhân với một thành viên.
*   **Mối quan hệ suy ra**:
    *   **Ông/Bà (Grandparent)**: Là cha/mẹ của cha/mẹ bạn.
    *   **Cháu (Grandchild)**: Là con của con bạn.
    *   **Anh/Chị/Em (Sibling)**: Là người có cùng cha/mẹ với bạn.
    *   **Chú/Bác/Cô/Dì (Uncle/Aunt)**: Là anh/chị/em của cha/mẹ bạn, hoặc vợ/chồng của anh/chị/em của cha/mẹ bạn.
    *   **Cháu trai/gái (Niece/Nephew)**: Là con của anh/chị/em bạn.
    *   **Anh/Chị/Em họ (Cousin)**: Là con của chú/bác/cô/dì của bạn.
*   **Cách hệ thống xử lý**:
    *   Bạn chỉ cần nhập các mối quan hệ cơ bản (Cha/Mẹ - Con, Vợ/Chồng).
    *   Hệ thống sẽ tự động suy luận và hiển thị các mối quan hệ phức tạp hơn trên cây gia phả.
    *   Ứng dụng cho phép bạn khai báo giới tính linh hoạt (Nam, Nữ, Khác) và sẽ sử dụng thông tin này để hiển thị các mối quan hệ một cách phù hợp.

---

# 5. Luồng giao diện người dùng (UI/UX Flow)

Dưới đây là các bước hướng dẫn cơ bản để người dùng thực hiện các tác vụ chính trên ứng dụng:

*   **1. Quản lý Danh sách Dòng họ (Family List)**:
    *   **Mục đích**: Xem tất cả các dòng họ mà bạn có quyền truy cập, tạo dòng họ mới, hoặc tìm kiếm dòng họ.
    *   **Hướng dẫn**:
        1.  Đăng nhập vào ứng dụng.
        2.  Tại màn hình chính hoặc menu điều hướng, chọn "Dòng họ" hoặc "Gia đình".
        3.  Để tạo dòng họ mới, nhấn nút "Thêm dòng họ" hoặc biểu tượng (+). Điền Tên, Mô tả, và chọn Chế độ hiển thị. Nhấn "Lưu".
        4.  Để xem chi tiết một dòng họ, nhấp vào tên của dòng họ đó trong danh sách.
        5.  Để tìm kiếm, sử dụng thanh tìm kiếm ở đầu danh sách và nhập từ khóa.
*   **2. Xem & Chỉnh sửa Chi tiết Thành viên (Member Detail)**:
    *   **Mục đích**: Xem thông tin chi tiết của một thành viên, chỉnh sửa thông tin, thêm ảnh, tạo tiểu sử hoặc quản lý mối quan hệ.
    *   **Hướng dẫn**:
        1.  Từ màn hình "Chi tiết dòng họ" (Family Detail), chọn một thành viên từ cây gia phả hoặc danh sách thành viên.
        2.  Để chỉnh sửa thông tin, nhấn nút "Chỉnh sửa" (biểu tượng bút chì). Thay đổi các trường như Tên, Họ, Ngày sinh, Giới tính, v.v. Nhấn "Lưu" để hoàn tất.
        3.  Để thêm ảnh đại diện, nhấn vào khu vực ảnh hoặc nút "Thêm ảnh", tải lên ảnh từ thiết bị của bạn.
        4.  Để tạo tiểu sử bằng AI, tìm phần "Tiểu sử" và nhấn nút "Tạo bằng AI". Ứng dụng sẽ sử dụng thông tin thành viên để gợi ý tiểu sử. Bạn có thể chỉnh sửa trước khi lưu.
*   **3. Quản lý Mối quan hệ (Relationship Editor)**:
    *   **Mục đích**: Thiết lập các mối quan hệ giữa các thành viên, ví dụ: cha mẹ - con cái, vợ - chồng.
    *   **Hướng dẫn**:
        1.  Trên màn hình "Chi tiết thành viên", tìm phần "Mối quan hệ" và nhấn "Thêm mối quan hệ" hoặc "Chỉnh sửa".
        2.  Chọn "Thành viên nguồn" và "Thành viên đích" từ danh sách.
        3.  Chọn "Loại quan hệ" (ví dụ: Cha - Con, Vợ - Chồng).
        4.  Nhấn "Lưu" để thêm hoặc cập nhật mối quan hệ.
*   **4. Quản lý Sự kiện (Event List)**:
    *   **Mục đích**: Ghi lại và xem các sự kiện quan trọng của dòng họ hoặc thành viên.
    *   **Hướng dẫn**:
        1.  Từ màn hình "Chi tiết dòng họ", điều hướng đến tab "Sự kiện".
        2.  Nhấn nút "Thêm sự kiện".
        3.  Điền Tên sự kiện, Mô tả, Ngày bắt đầu/kết thúc, Địa điểm, và Loại sự kiện.
        4.  Nhấn "Lưu".
*   **5. Quản lý Hồ sơ & Tùy chọn người dùng (User Profile & Preferences)**:
    *   **Mục đích**: Cập nhật thông tin cá nhân và cài đặt ứng dụng của bạn.
    *   **Hướng dẫn**:
        1.  Nhấp vào biểu tượng ảnh đại diện hoặc tên người dùng ở góc trên cùng bên phải.
        2.  Chọn "Hồ sơ của tôi" hoặc "Cài đặt tài khoản".
        3.  Trên tab "Hồ sơ", bạn có thể thay đổi tên hiển thị, thêm số điện thoại, ảnh đại diện. Nhấn "Lưu".
        4.  Trên tab "Tùy chọn", bạn có thể chọn Chủ đề giao diện (Sáng/Tối) và Ngôn ngữ hiển thị (Tiếng Việt/Tiếng Anh). Nhấn "Lưu".
*   **6. Bảng điều khiển (Dashboard)**:
    *   **Mục đích**: Xem các số liệu thống kê tổng quan về dòng họ của bạn.
    *   **Hướng dẫn**:
        1.  Chọn "Bảng điều khiển" từ menu điều hướng chính.
        2.  Bạn sẽ thấy các số liệu như tổng số dòng họ, tổng số thành viên, tổng số mối quan hệ.
        3.  Nếu có nhiều dòng họ, bạn có thể chọn một dòng họ cụ thể để xem số liệu chi tiết.

---

# 6. Ví dụ câu hỏi người dùng & phản hồi của AI (Examples of User Questions & AI Responses)

Đây là các ví dụ về cách người dùng có thể tương tác với Chat Assistant và cách AI nên phản hồi:

*   **Câu hỏi người dùng 1**: "Làm thế nào để thêm một thành viên mới vào gia phả của tôi?"
    *   **Phản hồi của AI**: "Để thêm một thành viên mới, bạn hãy thực hiện theo các bước sau:
        1.  Đăng nhập và chọn dòng họ bạn muốn thêm thành viên vào.
        2.  Vào màn hình 'Chi tiết dòng họ', sau đó chọn tab 'Thành viên' hoặc tìm kiếm nút 'Thêm thành viên mới'.
        3.  Điền đầy đủ các thông tin cần thiết như Tên, Họ, Ngày sinh, Giới tính, v.v.
        4.  Nhấn 'Lưu' để hoàn tất. Bạn có muốn tôi hướng dẫn chi tiết hơn về cách điền thông tin thành viên không?"

*   **Câu hỏi người dùng 2**: "Tôi muốn biết ông nội của tôi là ai trong cây gia phả?"
    *   **Phản hồi của AI**: "Để xác định ông nội của bạn trong cây gia phả, bạn cần biết thông tin về cha của cha bạn. Nếu thông tin này đã có trong hệ thống, bạn có thể:
        1.  Tìm hồ sơ của cha bạn trên cây gia phả.
        2.  Xem các mối quan hệ của cha bạn để tìm người được đánh dấu là cha của ông ấy.
        3.  Nếu thông tin chưa có, bạn có thể thêm người cha cho cha bạn và thiết lập mối quan hệ tương ứng. Bạn có muốn tôi hướng dẫn cách thêm mối quan hệ không?"

*   **Câu hỏi người dùng 3**: "Có quy tắc nào về ngày sinh và ngày mất của thành viên không?"
    *   **Phản hồi của AI**: "Vâng, có một quy tắc nghiệp vụ quan trọng về ngày sinh và ngày mất. Trong ứng dụng Gia Phả Việt, **ngày mất của một thành viên phải luôn sau ngày sinh của họ**. Hệ thống sẽ kiểm tra quy tắc này khi bạn nhập hoặc cập nhật thông tin thành viên để đảm bảo tính hợp lệ của dữ liệu."

*   **Câu hỏi người dùng 4**: "Vai trò 'Quản lý' và 'Xem' trong dòng họ khác nhau như thế nào?"
    *   **Phản hồi của AI**: "Trong ứng dụng Gia Phả Việt, vai trò trong dòng họ xác định quyền hạn của bạn:
        *   **Vai trò Quản lý**: Bạn có toàn quyền thêm, sửa đổi hoặc xóa bất kỳ thông tin nào (thành viên, mối quan hệ, sự kiện) trong dòng họ đó.
        *   **Vai trò Xem**: Bạn chỉ có quyền xem thông tin của dòng họ và không thể thực hiện bất kỳ thay đổi nào.
        Bạn có muốn biết cách gán vai trò cho người khác trong dòng họ không?"

*   **Câu hỏi người dùng 5**: "Ứng dụng có thể tự động tạo tiểu sử cho thành viên không?"
    *   **Phản hồi của AI**: "Có, ứng dụng Gia Phả Việt có tính năng tích hợp AI để giúp bạn tạo tiểu sử cho thành viên. Để sử dụng tính năng này:
        1.  Truy cập màn hình 'Chi tiết thành viên' của người bạn muốn tạo tiểu sử.
        2.  Tìm phần 'Tiểu sử' và nhấn nút 'Tạo bằng AI'.
        3.  Hệ thống sẽ sử dụng các thông tin đã có của thành viên (như tên, ngày sinh, các sự kiện liên quan) để gợi ý một bản tiểu sử. Bạn có thể xem lại và chỉnh sửa trước khi lưu."

*   **Câu hỏi người dùng 6**: "Nếu tôi tải ảnh lên, ứng dụng có làm gì với các khuôn mặt trong ảnh không?"
    *   **Phản hồi của AI**: "Vâng, khi bạn tải ảnh lên, ứng dụng Gia Phả Việt có khả năng phát hiện khuôn mặt trong ảnh đó. Sau khi các khuôn mặt được phát hiện, bạn có thể gán nhãn từng khuôn mặt với một thành viên cụ thể trong gia phả của mình. Điều này giúp liên kết hình ảnh với từng cá nhân và có thể hỗ trợ các tính năng khác trong tương lai."

---

# 7. Bảng thuật ngữ (Glossary)

*   **Dòng họ (Family)**: Tập hợp các thành viên có liên quan thông qua các mối quan hệ, tạo thành một cây phả hệ. Là đơn vị tổ chức cơ bản trong ứng dụng.
*   **Thành viên (Member)**: Một cá nhân trong dòng họ.
*   **Mối quan hệ (Relationship)**: Sự liên kết giữa hai thành viên, ví dụ: cha-con, vợ-chồng.
*   **Sự kiện (Event)**: Một dịp quan trọng gắn liền với dòng họ hoặc thành viên, ví dụ: sinh, mất, kết hôn.
*   **Người dùng (User)**: Tài khoản cá nhân sử dụng ứng dụng.
*   **Hồ sơ người dùng (UserProfile)**: Thông tin chi tiết về người dùng (tên, ảnh đại diện, liên hệ).
*   **Tùy chọn người dùng (UserPreference)**: Các cài đặt cá nhân của ứng dụng như chủ đề giao diện, ngôn ngữ.
*   **Vai trò Quản lý (Manager Role)**: Quyền hạn đầy đủ (thêm, sửa, xóa) đối với dữ liệu trong một dòng họ cụ thể.
*   **Vai trò Xem (Viewer Role)**: Quyền hạn chỉ xem dữ liệu trong một dòng họ cụ thể.
*   **Thành viên gốc (Root Member)**: Thành viên đầu tiên hoặc thành viên đại diện cho sự khởi đầu của một nhánh gia phả cụ thể.
*   **AI (Trí tuệ Nhân tạo)**: Các tính năng thông minh trong ứng dụng giúp tự động hóa hoặc hỗ trợ các tác vụ như tạo tiểu sử hay nhận diện khuôn mặt.
*   **Phát hiện khuôn mặt (Face Detection)**: Khả năng của ứng dụng tự động xác định vị trí các khuôn mặt trong một bức ảnh.
*   **Gán nhãn khuôn mặt (Face Labeling)**: Quá trình liên kết một khuôn mặt đã được phát hiện trong ảnh với một thành viên cụ thể trong gia phả.
*   **Tiểu sử AI (AI Biography)**: Bản tóm tắt cuộc đời hoặc thông tin quan trọng về một thành viên được tạo tự động bởi AI.
*   **Xác thực dữ liệu (Data Validation)**: Quá trình kiểm tra dữ liệu đầu vào để đảm bảo tính chính xác, đầy đủ và hợp lệ, tuân thủ các quy tắc nghiệp vụ.
