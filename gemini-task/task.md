## 🎯 Mục tiêu

Cập nhật toàn bộ tài liệu trong thư mục `docs` của project sao cho:

* Không phá vỡ cấu trúc thư mục hiện có.
* Không xóa hay làm sai lệch nội dung gốc.
* Chỉ được phép bổ sung, chỉnh sửa nhẹ các phần lỗi thời hoặc chưa rõ ràng.
* Bảo đảm mọi tài liệu đều đủ rõ ràng để **junior developer** cũng hiểu được.
* Các phần bổ sung phải chi tiết, có ví dụ minh họa cụ thể và giải thích rõ ràng về ngữ cảnh kỹ thuật.

---

## 📦 Phạm vi

1. **Thư mục mục tiêu:** `backend/docs` hoặc `docs` (tùy repo).
2. **Các loại tài liệu bao gồm:**
   * Mọi file `.md` khác trong thư mục `docs`.
---

## 🧩 Quy tắc chỉnh sửa

1. Không thay đổi cấu trúc thư mục hoặc tên file hiện có.
2. Không xóa bất kỳ phần nội dung nào (chỉ có thể thêm hoặc làm rõ).
3. Không thay đổi ý nghĩa gốc của đoạn mô tả kỹ thuật.
4. Nếu gặp nội dung lỗi thời hoặc không chính xác, hãy:

   * Cập nhật lại cho đúng với codebase hiện tại.
   * Giữ nguyên format cũ, chỉ cập nhật nội dung.
5. Nếu có phần thiếu giải thích hoặc thiếu ví dụ, hãy bổ sung giải thích chi tiết, có thể gồm:

   * Code snippet minh họa (C#, Vue, YAML, v.v.)
   * Sơ đồ hoặc pseudocode mô tả luồng xử lý.
   * Giải thích “tại sao” (WHY) để junior hiểu mục đích của thiết kế.
6. Các bổ sung mới nên có format:

### 🔄 [Updated Section]

*(Updated to match current refactor: [tên phần hoặc class/module])*

---

## 🧰 Đầu vào cho Gemini

* Toàn bộ thư mục `docs` hiện có trong repo.
* Cấu trúc codebase thực tế (để đối chiếu khi cần, ví dụ: `Application`, `Domain`, `Infrastructure`, `Web`).
* Các commit gần nhất có refactor lớn (nếu có).
* Không cần can thiệp vào file `.csproj`, `.json`, `.yml` hoặc mã nguồn — chỉ đọc để hiểu ngữ cảnh.

---

## 📘 Output mong muốn

* Cập nhật lại toàn bộ nội dung tài liệu trong `docs`, lưu thành phiên bản mới tương thích với code hiện tại.
* Giữ format Markdown chuẩn (H2, H3, bullet points, tables, code block...).
* Thêm ví dụ minh họa cụ thể, đặc biệt ở phần:

  * Repository & Unit of Work pattern
  * CQRS (Command, Query, Handler)
  * Error Handling & Result Wrapper
  * Integration Tests và Test Coverage
  * API Gateway hoặc Vite proxy (nếu có FE)
* Mỗi phần nên có:

  * Giải thích ngắn gọn (mục đích)
  * Luồng hoạt động (workflow hoặc sequence)
  * Code example
  * Best Practice / Note

---

## ✅ Yêu cầu cuối cùng

* Gemini chỉ cập nhật nội dung lỗi thời và mở rộng tài liệu, không được chỉnh sửa cấu trúc file.
* Mục tiêu là tạo bộ tài liệu hoàn chỉnh, chi tiết, thân thiện cho **junior developer**, giúp họ nắm được:

  * Cấu trúc DDD / Clean Architecture hiện tại.
  * Cách Repository, Handler, và Service tương tác.
  * Các nguyên tắc test, CI/CD, và error handling trong project.

---

**Tóm lại:** Gemini đóng vai trò **người bảo trì tài liệu kỹ thuật** — không phá vỡ cấu trúc, chỉ **bổ sung, cập nhật và làm rõ** mọi phần lỗi thời để đảm bảo tính chính xác và dễ hiểu của tài liệu.

---