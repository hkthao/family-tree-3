# Kiến thức cơ bản về AI trong dự án Cây Gia Phả

## 1. Tổng quan

Phần này mô tả cách trí tuệ nhân tạo (AI) được tích hợp và sử dụng trong dự án Cây Gia Phả để cung cấp các tính năng thông minh và nâng cao trải nghiệm người dùng.

## 2. Các tính năng AI chính

### 2.1. Tạo tiểu sử tự động (AI Biography Generation)

*   **Mô tả:** Hệ thống sử dụng AI để tự động tạo ra các đoạn tiểu sử ngắn gọn hoặc chi tiết cho các thành viên trong gia phả dựa trên dữ liệu có sẵn. Endpoint API hiện trả về một đối tượng DTO đầy đủ, bao gồm cả tên nhà cung cấp AI.
*   **Công nghệ:** Sử dụng các mô hình ngôn ngữ lớn (LLM) để phân tích thông tin và tổng hợp nội dung.
*   **Cách hoạt động:** Khi người dùng yêu cầu tạo tiểu sử, hệ thống sẽ thu thập các thông tin liên quan từ hồ sơ thành viên (tên, ngày sinh, ngày mất, nghề nghiệp, sự kiện liên quan, v.v.) và gửi đến mô hình AI. Mô hình sẽ xử lý và trả về một đoạn văn bản tiểu sử. Độ dài của prompt đầu vào cũng được xác thực để đảm bảo hiệu quả và tránh lạm dụng.

### 2.2. Trích xuất thông tin từ tài liệu (Information Extraction from Documents)

*   **Mô tả:** AI giúp trích xuất các thông tin quan trọng từ các tài liệu được tải lên (ví dụ: giấy khai sinh, giấy chứng tử, thư từ) để tự động điền vào hồ sơ thành viên hoặc tạo các sự kiện.
*   **Công nghệ:** Sử dụng các kỹ thuật Xử lý ngôn ngữ tự nhiên (NLP) và Nhận dạng ký tự quang học (OCR) cho các tài liệu hình ảnh/PDF.
*   **Cách hoạt động:** Người dùng tải lên tài liệu, hệ thống AI sẽ quét và phân tích nội dung để tìm kiếm các thực thể (tên, ngày tháng, địa điểm, mối quan hệ) và đề xuất cập nhật dữ liệu.

### 2.3. Xử lý dữ liệu và Chia Chunk (Data Processing and Chunking)

*   **Mô tả:** Triển khai module xử lý và chia nhỏ nội dung từ các tệp PDF/TXT thành các `TextChunk` với đầy đủ metadata (fileId, familyId, category, createdBy) để chuẩn bị cho việc tạo embeddings và tích hợp chatbot.
*   **Công nghệ:** Sử dụng các thư viện xử lý tài liệu và thuật toán chia tách văn bản.
*   **Cách hoạt động:** Khi một tệp PDF hoặc TXT được tải lên, hệ thống sẽ tự động phân tích nội dung, chia thành các đoạn nhỏ (chunks) và lưu trữ cùng với các siêu dữ liệu liên quan, giúp tối ưu hóa cho việc tìm kiếm ngữ nghĩa và tương tác với các mô hình AI khác.

### 2.4. Gợi ý mối quan hệ và kết nối (Relationship and Connection Suggestions)

*   **Mô tả:** AI phân tích dữ liệu gia phả hiện có để gợi ý các mối quan hệ tiềm năng hoặc các kết nối giữa các thành viên mà người dùng có thể chưa biết hoặc chưa thêm vào.
*   **Công nghệ:** Sử dụng thuật toán đồ thị và học máy để phát hiện các mẫu và mối liên hệ.

## 3. Quản lý và cấu hình AI

*   **Nhà cung cấp AI:** Hệ thống hỗ trợ nhiều nhà cung cấp AI khác nhau (ví dụ: OpenAI, Google Gemini, mô hình cục bộ) và cho phép cấu hình linh hoạt.
*   **Cấu hình:** Các cài đặt liên quan đến AI (API keys, tên mô hình, giới hạn token) được quản lý thông qua tệp cấu hình `appsettings.json` ở backend.

## 4. Phát triển và mở rộng

*   **Kiến trúc Plugin:** Các nhà cung cấp AI được thiết kế theo kiến trúc plugin, cho phép dễ dàng thêm hoặc thay thế các mô hình AI khác trong tương lai.
*   **Thêm mô hình mới:** Để tích hợp một mô hình AI mới, cần triển khai giao diện `IChatProvider` hoặc `IEmbeddingProvider` và đăng ký trong `DependencyInjection` của backend.

## 5. Tương lai

Các tính năng AI tiềm năng trong tương lai bao gồm nhận diện khuôn mặt trong ảnh, phân tích cảm xúc từ nhật ký, và tạo cây gia phả tự động từ các nguồn dữ liệu lớn.