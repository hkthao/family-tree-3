Ok 👍 mình sẽ viết lại file **`docs/requirements.md`** đầy đủ, tiếng Việt, đã bổ sung yêu cầu *“tài liệu phải viết bằng tiếng Việt”* và *“không cần file .bicep hay hạ tầng Azure”*.

Bạn có thể copy nội dung này bỏ vào repo **family-tree-3/docs/requirements.md**:

---

# Tài liệu Yêu cầu (Requirements)

## 1. Giới thiệu

Phần mềm **Quản lý Gia Phả** là hệ thống web chuyên nghiệp hỗ trợ các dòng họ và gia đình trong việc lưu trữ, quản lý, và trực quan hóa thông tin thành viên. Ứng dụng được phát triển theo kiến trúc hiện đại, có tài liệu đầy đủ, quy trình chuẩn chuyên nghiệp như các công ty công nghệ lớn (IBM, Google).

---

## 2. Phạm vi

Hệ thống bao gồm:

* Quản lý thông tin dòng họ/gia đình.
* Quản lý thông tin thành viên.
* Quản lý mối quan hệ giữa các thành viên.
* Vẽ cây gia phả trực quan.
* Hỗ trợ tài liệu đầy đủ cho phát triển, triển khai và sử dụng.

---

## 3. Chức năng chi tiết

### 3.1 Quản lý thông tin dòng họ / gia đình

* Lưu trữ: tên dòng họ/gia đình, địa chỉ, số thành viên, hình đại diện.
* Một dòng họ có thể có nhiều gia đình con.

### 3.2 Quản lý thông tin thành viên

* Họ tên, ngày sinh, ngày mất.
* Trạng thái: còn sống hoặc đã mất.
* Hình đại diện, email, số điện thoại.
* Mô tả về cuộc đời, sự nghiệp.
* Thông tin thế hệ (thuộc thế hệ thứ mấy trong dòng họ, con thứ mấy trong gia đình).
* Mỗi thành viên bắt buộc thuộc về một dòng họ và một gia đình cụ thể.

### 3.3 Quản lý quan hệ thành viên

* Cha, mẹ, vợ/chồng.
* Xác định quan hệ để vẽ được cây gia phả.

### 3.4 Vẽ cây gia phả

* Sinh tự động sơ đồ cây dựa trên dữ liệu thành viên và quan hệ.
* Hiển thị: hình đại diện, họ tên, năm sinh, năm mất, quan hệ với các thành viên khác.
* Giao diện trực quan, dễ đọc, hỗ trợ zoom/pan.

---

## 4. Công nghệ sử dụng

* **Backend**: ASP.NET Core.
* **Database**: MongoDB.
* **Frontend**: Vue.js + Vuetify 3.

---

## 5. Yêu cầu phi chức năng

* Tài liệu phải viết bằng **tiếng Việt**: System Design, Requirement, User Guide, Component Design.
* Tạo project chuyên nghiệp (cấu trúc chuẩn, có unit test, CI/CD cơ bản).
* Không tạo hoặc yêu cầu triển khai hạ tầng Azure (không cần `.bicep`, Terraform).
* Hiệu năng: tải nhanh, hỗ trợ tối thiểu 1.000 thành viên.
* Bảo mật: xác thực người dùng (IAM cơ bản), phân quyền (admin, thành viên thường).

---

## 6. Quy trình phát triển

* Áp dụng **Agile Scrum/Kanban**.
* Tạo backlog, story đầy đủ để quản lý trên Kanban board (GitHub Projects hoặc Jira).
* Mỗi story gắn với task và có định nghĩa hoàn thành (Definition of Done).
* Áp dụng Test-Driven Development (TDD) ở mức unit test.

---

## 7. Tài liệu cần có

* **System Design**: kiến trúc hệ thống, sơ đồ thành phần, cơ sở dữ liệu.
* **Requirement**: tài liệu yêu cầu (tài liệu này).
* **User Guide**: hướng dẫn cài đặt, sử dụng cho người dùng cuối.
* **Component Design**: thiết kế chi tiết từng module, API, UI component.

---

## 8. Gợi ý hướng làm với Gemini CLI

* Viết prompt rõ ràng: yêu cầu tạo hoặc chỉnh sửa file tài liệu trong thư mục `docs/`.
* Yêu cầu **không sinh file `.bicep` hoặc hạ tầng cloud**.
* Chỉ tập trung tạo nội dung tài liệu **tiếng Việt**.
* Có thể yêu cầu Gemini tạo luôn dàn ý backlog/story để import lên Kanban board.