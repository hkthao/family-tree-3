# Cơ sở Kiến thức Tối ưu cho Chat Assistant về Ứng dụng Dòng Họ Việt

Tài liệu này cung cấp cơ sở kiến thức đã được tối ưu hóa cho Chat Assistant, giúp trả lời các câu hỏi của người dùng về ứng dụng Dòng Họ Việt một cách chính xác và hiệu quả.

---

## Mục lục

- [1. Tóm tắt ngắn gọn (Summary)](#1-tóm-tắt-ngắn-gọn-summary)
- [2. Mô hình miền (Domain Model)](#2-mô-hình-miền-domain-model)
- [3. Các quy tắc nghiệp vụ chính (Key Business Rules)](#3-các-quy-tắc-nghiệp-vụ-chính-key-business-rules)
- [4. Logic quan hệ gia đình (Family Relationship Logic)](#4-logic-quan-hệ-gia-đình-family-relationship-logic)
- [5. Luồng giao diện người dùng (UI/UX Flow)](#5-luồng-giao-diện-người-dùng-uiux-flow)
- [6. Ví dụ câu hỏi người dùng & phản hồi của AI (Examples of User Questions & AI Responses)](#6-ví-dụ-câu-hỏi-người-dùng--phản-hồi-của-ai-examples-of-user-questions--ai-responses)
- [8. Thông tin Đội ngũ Phát triển](#8-thông-tin-đội-ngũ-phát-triển)
- [10. Về chúng tôi & Sứ mệnh](#10-về-chúng-tôi--sứ-mệnh)
- [11. Điều khoản dịch vụ](#11-điều-khoản-dịch-vụ)
- [12. Bảng thuật ngữ (Glossary)](#12-bảng-thuật-ngữ-glossary)

---

# 1. Tóm tắt ngắn gọn (Summary)

Ứng dụng **Dòng Họ Việt** là hệ thống quản lý gia phả toàn diện, giúp người dùng tạo, quản lý, trực quan hóa và chia sẻ cây gia phả một cách dễ dàng. Ứng dụng tập trung vào bảo tồn lịch sử gia đình, cung cấp các công cụ trực quan, quản lý thông tin đa dạng, tích hợp AI (như tạo tiểu sử, nhận diện khuôn mặt) và hỗ trợ cộng tác. Tài liệu này cung cấp kiến thức cốt lõi về các khái niệm, quy tắc, logic quan hệ và hướng dẫn sử dụng ứng dụng, được tối ưu hóa cho Chat Assistant để hỗ trợ người dùng cuối.

---

# 2. Mô hình miền (Domain Model)

Đây là các khái niệm và thực thể chính mà ứng dụng Dòng Họ Việt quản lý:

*   **Dòng họ (Family)**:
    *   **Là gì**: Một tập hợp các thành viên có quan hệ, tạo nên một cây gia phả duy nhất.
    *   **Bạn có thể**: Thêm/xóa/cập nhật thành viên, tạo/sửa mối quan hệ, quản lý sự kiện liên quan đến dòng họ.
    *   **Lưu ý**: Mỗi dòng họ có một tên duy nhất.

*   **Thành viên (Member)**:
    *   **Là gì**: Một cá nhân cụ thể trong một dòng họ.
    *   **Thông tin chính**: Tên, họ, giới tính, ngày sinh, ngày mất, nơi sinh, nơi mất, ảnh đại diện, tiểu sử, thông tin liên hệ, nghề nghiệp.
    *   **Bạn có thể**: Cập nhật thông tin cá nhân, thêm/xóa ảnh, yêu cầu AI tạo tiểu sử.
    *   **Lưu ý**: Phải thuộc về một Dòng họ. Tên và họ là bắt buộc. Ngày mất phải sau ngày sinh.

*   **Mối quan hệ (Relationship)**:
    *   **Là gì**: Sự liên kết giữa hai thành viên trong cùng một dòng họ.
    *   **Thông tin chính**: Thành viên nguồn, thành viên đích, loại quan hệ (ví dụ: cha-con, vợ-chồng).
    *   **Bạn có thể**: Tạo, cập nhật, xóa mối quan hệ.
    *   **Lưu ý**: Luôn liên kết hai thành viên hiện có và thuộc cùng một dòng họ.

*   **Sự kiện (Event)**:
    *   **Là gì**: Các dịp quan trọng gắn liền với một dòng họ hoặc một thành viên cụ thể.
    *   **Thông tin chính**: Tên sự kiện, mô tả, ngày bắt đầu, ngày kết thúc, địa điểm, loại sự kiện (ví dụ: sinh, mất, kết hôn, di cư).
    *   **Bạn có thể**: Tạo, cập nhật, xóa sự kiện.
    *   **Lưu ý**: Phải thuộc về một Dòng họ. Tên sự kiện là bắt buộc.

*   **Người dùng (User)**:
    *   **Là gì**: Tài khoản cá nhân sử dụng ứng dụng.
    *   **Bạn có thể**: Đăng nhập, quản lý thông tin cá nhân.

*   **Hồ sơ người dùng (UserProfile)**:
    *   **Là gì**: Thông tin chi tiết về người dùng, liên kết trực tiếp với tài khoản Người dùng.
    *   **Thông tin chính**: Tên hiển thị, tên riêng, họ, số điện thoại, ảnh đại diện.

*   **Tùy chọn người dùng (UserPreference)**:
    *   **Là gì**: Các cài đặt cá nhân của người dùng cho ứng dụng.
    *   **Bạn có thể**: Đặt chủ đề giao diện (sáng/tối), ngôn ngữ hiển thị.

*   **Vai trò trong dòng họ (FamilyUser)**:
    *   **Là gì**: Xác định quyền hạn của một người dùng đối với một dòng họ cụ thể.
    *   **Vai trò**: Quản lý (toàn quyền chỉnh sửa), Xem (chỉ xem thông tin).

*   **Cấu hình hệ thống (SystemConfiguration)**:
    *   **Là gì**: Các cài đặt chung của ứng dụng có thể được điều chỉnh bởi quản trị viên để quản lý các chức năng toàn hệ thống.

---

# 3. Các quy tắc nghiệp vụ chính (Key Business Rules)

Đây là những quy tắc cốt lõi mà ứng dụng tuân thủ để đảm bảo tính chính xác và nhất quán của dữ liệu gia phả:

*   **Quy tắc Quan hệ Cha/Mẹ/Con:**
    *   Mỗi thành viên có thể có nhiều cha mẹ nhưng chỉ được xác định tối đa một cha ruột và một mẹ ruột.
    *   Ngày sinh của cha/mẹ phải trước ngày sinh của con cái.
    *   Một thành viên không thể là cha/mẹ của chính mình.
    *   Tất cả các thành viên trong mối quan hệ phải thuộc cùng một dòng họ.

*   **Quy tắc Quan hệ Vợ/Chồng:**
    *   Các thành viên trong mối quan hệ vợ chồng phải có giới tính khác nhau (hoặc giới tính tùy chỉnh phù hợp).
    *   Ngày kết hôn phải sau ngày sinh của cả hai vợ chồng và trước ngày mất của bất kỳ ai.
    *   Mối quan hệ vợ chồng phải thuộc cùng một dòng họ.

*   **Tránh trùng lặp thành viên:**
    *   Trong một dòng họ, hệ thống sẽ cố gắng ngăn chặn việc tạo ra các thành viên có thông tin quá giống nhau (tên, họ, ngày sinh, giới tính) để tránh trùng lặp dữ liệu.

*   **Tính toàn vẹn Dòng họ:**
    *   Mọi thay đổi đối với thành viên, mối quan hệ, hoặc sự kiện phải được thực hiện thông qua dòng họ mà chúng thuộc về. Bạn không thể thêm một thành viên mà không gán cho họ một dòng họ.

*   **Ngày tháng hợp lệ:**
    *   Ngày mất của một thành viên phải sau ngày sinh của họ.
    *   Ngày bắt đầu của một sự kiện phải trước hoặc bằng ngày kết thúc của sự kiện đó.

*   **Vai trò người dùng trong dòng họ:**
    *   Người dùng có vai trò **Quản lý** có toàn quyền thêm, sửa, xóa bất kỳ thông tin nào trong dòng họ đó.
    *   Người dùng có vai trò **Xem** chỉ có quyền xem thông tin của dòng họ.

*   **Xóa mềm (Soft Delete):**
    *   Khi một số loại thông tin (ví dụ: vai trò của người dùng trong dòng họ) bị "xóa", chúng chỉ được đánh dấu là không hoạt động chứ không bị loại bỏ hoàn toàn, để có thể khôi phục nếu cần.

*   **Xác thực dữ liệu đầu vào:**
    *   Mọi thông tin người dùng nhập vào đều được kiểm tra kỹ lưỡng để đảm bảo tính hợp lệ và an toàn.

*   **Quyền truy cập thông tin:**
    *   Bạn chỉ có thể xem hoặc chỉnh sửa thông tin của một dòng họ nếu bạn được cấp quyền truy cập vào dòng họ đó.

---

# 4. Logic quan hệ gia đình (Family Relationship Logic)

Ứng dụng hiểu và hiển thị nhiều loại mối quan hệ gia đình dựa trên thông tin cơ bản bạn nhập.

*   **Quan hệ cơ bản (Bạn cần nhập):**
    *   **Cha/Mẹ - Con**: Thiết lập ai là cha/mẹ của ai.
    *   **Vợ/Chồng**: Thiết lập ai là bạn đời của ai.

*   **Quan hệ suy luận (Hệ thống tự động hiển thị):**
    *   Nếu A là cha/mẹ của B, và B là cha/mẹ của C **=>** A là **Ông/Bà** của C.
    *   Nếu C là con của B, và B là con của A **=>** C là **Cháu** của A.
    *   Nếu A và B có cùng cha/mẹ **=>** A và B là **Anh/Chị/Em**.
    *   Nếu A là anh/chị/em của cha/mẹ của B **=>** A là **Chú/Bác/Cô/Dì** của B.
    *   Nếu B là con của anh/chị/em của A **=>** B là **Cháu trai/gái** của A.
    *   Nếu A và B là con của hai anh/chị/em ruột (ví dụ: A là con của chú X, B là con của cô Y, và X, Y là anh/chị/em ruột) **=>** A và B là **Anh/Chị/Em họ**.

*   **Lưu ý:** Bạn chỉ cần nhập các mối quan hệ cơ bản. Hệ thống sẽ tự động suy luận và hiển thị các mối quan hệ phức tạp hơn dựa trên giới tính đã khai báo.

---

# 5. Luồng giao diện người dùng (UI/UX Flow)

Dưới đây là các bước hướng dẫn cơ bản để bạn thực hiện các tác vụ chính trên ứng dụng:

*   **1. Quản lý Danh sách Dòng họ:**
    *   **Để xem**: Đăng nhập, chọn "Dòng họ" từ menu chính.
    *   **Để tạo mới**: Nhấn "Thêm dòng họ" hoặc biểu tượng (+), điền Tên, Mô tả, Chế độ hiển thị, rồi "Lưu".
    *   **Để xem chi tiết**: Nhấp vào tên dòng họ trong danh sách.
    *   **Để tìm kiếm**: Sử dụng thanh tìm kiếm ở đầu danh sách.

*   **2. Xem & Chỉnh sửa Chi tiết Thành viên:**
    *   **Để xem/chỉnh sửa**: Từ màn hình "Chi tiết dòng họ", chọn một thành viên. Nhấn nút "Chỉnh sửa" (biểu tượng bút chì), thay đổi thông tin (Tên, Họ, Ngày sinh, Giới tính, v.v.), rồi "Lưu".
    *   **Để thêm ảnh**: Nhấn vào khu vực ảnh hoặc nút "Thêm ảnh", tải ảnh lên từ thiết bị.
    *   **Để tạo tiểu sử AI**: Trong phần "Tiểu sử", nhấn nút "Tạo bằng AI", xem lại và chỉnh sửa nếu cần, rồi "Lưu".

*   **3. Quản lý Mối quan hệ:**
    *   **Để thiết lập**: Trên màn hình "Chi tiết thành viên", tìm phần "Mối quan hệ", nhấn "Thêm mối quan hệ" hoặc "Chỉnh sửa".
    *   Chọn "Thành viên nguồn", "Thành viên đích" và "Loại quan hệ", rồi "Lưu".

*   **4. Quản lý Sự kiện:**
    *   **Để xem/thêm**: Từ màn hình "Chi tiết dòng họ", điều hướng đến tab "Sự kiện".
    *   **Để thêm mới**: Nhấn nút "Thêm sự kiện", điền Tên sự kiện, Mô tả, Ngày bắt đầu/kết thúc, Địa điểm, Loại sự kiện, rồi "Lưu".

*   **5. Quản lý Hồ sơ & Tùy chọn người dùng:**
    *   **Để cập nhật**: Nhấp vào biểu tượng ảnh đại diện hoặc tên người dùng ở góc trên cùng bên phải, chọn "Hồ sơ của tôi" hoặc "Cài đặt tài khoản".
    *   **Để đổi thông tin cá nhân**: Trên tab "Hồ sơ", thay đổi tên hiển thị, số điện thoại, ảnh đại diện, rồi "Lưu".
    *   **Để đổi cài đặt ứng dụng**: Trên tab "Tùy chọn", chọn Chủ đề giao diện (Sáng/Tối) và Ngôn ngữ hiển thị (Tiếng Việt/Tiếng Anh), rồi "Lưu".

*   **6. Bảng điều khiển (Dashboard):**
    *   **Để xem**: Chọn "Bảng điều khiển" từ menu điều hướng chính.
    *   **Bạn sẽ thấy**: Các số liệu thống kê tổng quan về dòng họ của bạn (tổng số dòng họ, thành viên, mối quan hệ).
    *   **Để xem chi tiết**: Nếu có nhiều dòng họ, bạn có thể chọn một dòng họ cụ thể để xem số liệu chi tiết.

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
    *   **Phản hồi của AI**: "Để xác định ông nội của bạn trong cây gia phả, hệ thống cần biết thông tin về cha của cha bạn. Nếu thông tin này đã có, bạn có thể:
        1.  Tìm hồ sơ của cha bạn trên cây gia phả.
        2.  Xem các mối quan hệ của cha bạn để tìm người được đánh dấu là cha của ông ấy.
        3.  Nếu thông tin chưa có, bạn có thể thêm người cha cho cha bạn và thiết lập mối quan hệ tương ứng. Bạn có muốn tôi hướng dẫn cách thêm mối quan hệ không?"

*   **Câu hỏi người dùng 3**: "Có quy tắc nào về ngày sinh và ngày mất của thành viên không?"
    *   **Phản hồi của AI**: "Vâng, có một quy tắc nghiệp vụ quan trọng về ngày sinh và ngày mất. Trong ứng dụng Dòng Họ Việt, **ngày mất của một thành viên phải luôn sau ngày sinh của họ**. Hệ thống sẽ kiểm tra quy tắc này khi bạn nhập hoặc cập nhật thông tin thành viên để đảm bảo tính hợp lệ của dữ liệu."

*   **Câu hỏi người dùng 4**: "Vai trò 'Quản lý' và 'Xem' trong dòng họ khác nhau như thế nào?"
    *   **Phản hồi của AI**: "Trong ứng dụng Dòng Họ Việt, vai trò trong dòng họ xác định quyền hạn của bạn:
        *   **Vai trò Quản lý**: Bạn có toàn quyền thêm, sửa đổi hoặc xóa bất kỳ thông tin nào (thành viên, mối quan hệ, sự kiện) trong dòng họ đó.
        *   **Vai trò Xem**: Bạn chỉ có quyền xem thông tin của dòng họ và không thể thực hiện bất kỳ thay đổi nào.
        Bạn có muốn biết cách gán vai trò cho người khác trong dòng họ không?"

*   **Câu hỏi người dùng 5**: "Ứng dụng có thể tự động tạo tiểu sử cho thành viên không?"
    *   **Phản hồi của AI**: "Có, ứng dụng Dòng Họ Việt có tính năng tích hợp AI để giúp bạn tạo tiểu sử cho thành viên. Để sử dụng tính năng này:
        1.  Truy cập màn hình 'Chi tiết thành viên' của người bạn muốn tạo tiểu sử.
        2.  Tìm phần 'Tiểu sử' và nhấn nút 'Tạo bằng AI'.
        3.  Hệ thống sẽ sử dụng các thông tin đã có của thành viên (như tên, ngày sinh, các sự kiện liên quan) để gợi ý một bản tiểu sử. Bạn có thể xem lại và chỉnh sửa trước khi lưu."

*   **Câu hỏi người dùng 6**: "Nếu tôi tải ảnh lên, ứng dụng có làm gì với các khuôn mặt trong ảnh không?"
    *   **Phản hồi của AI**: "Vâng, khi bạn tải ảnh lên, ứng dụng Dòng Họ Việt có khả năng phát hiện khuôn mặt trong ảnh đó. Sau khi các khuôn mặt được phát hiện, bạn có thể gán nhãn từng khuôn mặt với một thành viên cụ thể trong gia phả của mình. Điều này giúp liên kết hình ảnh với từng cá nhân và có thể hỗ trợ các tính năng khác trong tương lai."

---

# 8. Thông tin Đội ngũ Phát triển

Tài liệu này cung cấp thông tin về đội ngũ phát triển chính của dự án Dòng Họ Việt.

*   **Thành viên:**
    *   **Huynh Kim Thao**: Full Stack Developer
    *   **AI Gemini**: AI Assistant

*   **Trách nhiệm chính:**
    *   **Full Stack Developer (Huynh Kim Thao)**: Thiết kế, phát triển và duy trì cả Backend (ASP.NET Core) và Frontend (Vue.js); Quản lý cơ sở dữ liệu; Đảm bảo kiến trúc hệ thống và best practices; Viết và duy trì kiểm thử.
    *   **AI Assistant (AI Gemini)**: Hỗ trợ duy trì tài liệu, hỗ trợ phát triển phần mềm (sửa lỗi, tính năng, refactor), đảm bảo tuân thủ quy ước và cung cấp thông tin kỹ thuật.

---

# 10. Về chúng tôi & Sứ mệnh

**Về chúng tôi:**

Chào mừng bạn đến với Ứng dụng Cây Gia Phả, giải pháp toàn diện để quản lý và trực quan hóa di sản gia đình bạn. Ứng dụng của chúng tôi được thiết kế để giúp bạn dễ dàng xây dựng, khám phá và bảo tồn lịch sử gia đình mình.

**Sứ mệnh của chúng tôi:**

Sứ mệnh của chúng tôi là cung cấp một nền tảng mạnh mẽ nhưng dễ sử dụng, trao quyền cho các cá nhân kết nối với cội nguồn của họ, khám phá tổ tiên của họ và chia sẻ câu chuyện gia đình của họ với các thế hệ tương lai. Chúng tôi tin vào tầm quan trọng của di sản và niềm vui khám phá.

---

# 11. Điều khoản dịch vụ

**Giới thiệu:**

Chào mừng bạn đến với Ứng dụng Cây Gia Phả! Các Điều khoản dịch vụ ('Điều khoản') này điều chỉnh quyền truy cập và sử dụng trang web, dịch vụ và ứng dụng của chúng tôi (gọi chung là 'Dịch vụ'). Bằng cách truy cập hoặc sử dụng Dịch vụ, bạn đồng ý bị ràng buộc bởi các Điều khoản này.
Ứng dụng Cây Gia Phả giúp bạn quản lý và trực quan hóa các thành viên gia đình, lịch sử và mối quan hệ của bạn, với sự hỗ trợ tùy chọn của AI. Vui lòng đọc kỹ các Điều khoản này trước khi sử dụng Dịch vụ của chúng tôi.

**Trách nhiệm của người dùng:**

Bạn chịu trách nhiệm về việc sử dụng Dịch vụ của mình và đối với bất kỳ nội dung nào bạn cung cấp, bao gồm việc tuân thủ các luật, quy tắc và quy định hiện hành. Chúng tôi mong bạn sử dụng Dịch vụ một cách tôn trọng và có trách nhiệm.
*   Cung cấp thông tin chính xác và trung thực về các thành viên trong gia đình bạn.
*   Sử dụng các tính năng AI một cách có trách nhiệm và không cố ý tạo hoặc chia sẻ dữ liệu sai lệch hoặc gây hiểu lầm.
*   Tôn trọng quyền riêng tư và quyền sở hữu trí tuệ của người khác.

**Quyền sở hữu dữ liệu:**

Bạn giữ quyền sở hữu hoàn toàn đối với tất cả dữ liệu bạn tải lên, tạo hoặc quản lý trong Ứng dụng Cây Gia Phả. Chúng tôi không yêu cầu bất kỳ quyền sở hữu nào đối với dữ liệu gia đình của bạn.
Bạn có thể yêu cầu xóa dữ liệu của mình bất cứ lúc nào và chúng tôi sẽ tuân thủ yêu cầu của bạn theo Chính sách quyền riêng tư của chúng tôi.

**Tuyên bố từ chối trách nhiệm AI:**

Dịch vụ của chúng tôi bao gồm các tính năng được hỗ trợ bởi AI tùy chọn được thiết kế để hỗ trợ bạn quản lý cây gia phả của mình, chẳng hạn như tạo tiểu sử hoặc gợi ý mối quan hệ. Mặc dù chúng tôi cố gắng đạt được độ chính xác, nhưng các gợi ý của AI được tạo ra bởi các thuật toán và có thể không phải lúc nào cũng chính xác hoặc đầy đủ về mặt thực tế.
Bạn thừa nhận và đồng ý rằng bạn hoàn toàn chịu trách nhiệm xem xét, xác minh và chỉnh sửa bất kỳ nội dung nào do AI tạo ra trước khi lưu hoặc chia sẻ. Ứng dụng Cây Gia Phả không chịu trách nhiệm về bất kỳ sự không chính xác hoặc hậu quả nào phát sinh từ việc sử dụng nội dung do AI tạo ra.

**Thay đổi điều khoản:**

Chúng tôi có thể cập nhật các Điều khoản này theo thời gian. Nếu chúng tôi thực hiện các thay đổi đáng kể, chúng tôi sẽ thông báo cho bạn thông qua Dịch vụ hoặc bằng các phương tiện khác, chẳng hạn như email. Việc bạn tiếp tục sử dụng Dịch vụ sau những thay đổi đó đồng nghĩa với việc bạn chấp nhận các Điều khoản mới.

---

# 12. Bảng thuật ngữ (Glossary)

*   **Dòng họ (Family)**: Nhóm các thành viên có quan hệ, tạo thành cây gia phả. Đơn vị tổ chức chính trong ứng dụng.
*   **Thành viên (Member)**: Một cá nhân trong dòng họ.
*   **Mối quan hệ (Relationship)**: Sự liên kết giữa hai thành viên, ví dụ: cha-con, vợ-chồng.
*   **Sự kiện (Event)**: Một dịp quan trọng gắn liền với dòng họ hoặc thành viên, ví dụ: sinh, mất, kết hôn.
*   **Người dùng (User)**: Tài khoản cá nhân sử dụng ứng dụng.
*   **Hồ sơ người dùng (UserProfile)**: Thông tin chi tiết cá nhân của người dùng (tên, ảnh đại diện, liên hệ).
*   **Tùy chọn người dùng (UserPreference)**: Các cài đặt cá nhân của ứng dụng như chủ đề giao diện, ngôn ngữ.
*   **Vai trò Quản lý (Manager Role)**: Có toàn quyền thêm, sửa, xóa dữ liệu trong một dòng họ cụ thể.
*   **Vai trò Xem (Viewer Role)**: Chỉ có quyền xem dữ liệu trong một dòng họ cụ thể.
*   **Thành viên gốc (Root Member)**: Thành viên đầu tiên hoặc thành viên đại diện cho sự khởi đầu của một nhánh gia phả cụ thể.
*   **AI (Trí tuệ Nhân tạo)**: Các tính năng thông minh trong ứng dụng giúp tự động hóa hoặc hỗ trợ các tác vụ như tạo tiểu sử hay nhận diện khuôn mặt.
*   **Phát hiện khuôn mặt (Face Detection)**: Khả năng của ứng dụng tự động xác định vị trí các khuôn mặt trong một bức ảnh.
*   **Gán nhãn khuôn mặt (Face Labeling)**: Quá trình liên kết một khuôn mặt đã được phát hiện trong ảnh với một thành viên cụ thể trong gia phả.
*   **Tiểu sử AI (AI Biography)**: Bản tóm tắt cuộc đời hoặc thông tin quan trọng về một thành viên được tạo tự động bởi AI.
*   **Xác thực dữ liệu (Data Validation)**: Quá trình kiểm tra dữ liệu đầu vào để đảm bảo tính chính xác, đầy đủ và hợp lệ.