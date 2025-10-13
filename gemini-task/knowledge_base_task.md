gemini --prompt "
Bạn là một chuyên gia viết tài liệu hướng dẫn người dùng phần mềm.

Nhiệm vụ:
Đọc tất cả các tệp Markdown (.md) trong thư mục './docs' và tạo một tệp tổng hợp duy nhất tên là 'PROJECT_KNOWLEDGE_BASE.md'.

Mục tiêu:
Tài liệu này sẽ được dùng để huấn luyện chatbot hỗ trợ người dùng cuối, giúp họ hiểu và sử dụng phần mềm. 
Nội dung tập trung vào việc **giải thích, hướng dẫn và hỗ trợ sử dụng**, không đi sâu vào chi tiết kỹ thuật lập trình.

Yêu cầu chi tiết:
1. Tổng hợp tất cả nội dung có trong các tệp tài liệu hiện có, chọn lọc những phần liên quan đến:
   - Giới thiệu phần mềm (mục đích, đối tượng sử dụng, lợi ích chính)
   - Các tính năng chính và cách sử dụng
   - Hướng dẫn cài đặt và bắt đầu
   - Các thao tác thường gặp
   - Cách xử lý lỗi phổ biến hoặc câu hỏi thường gặp (FAQ)
   - Liên hệ hoặc hỗ trợ người dùng (nếu có)
2. **Loại bỏ hoặc giản lược** các phần kỹ thuật chi tiết (như cấu trúc mã, API, kiến trúc hệ thống) trừ khi thật sự cần thiết để người dùng hiểu.
3. Giữ định dạng Markdown rõ ràng:
   - Dùng các tiêu đề #, ##, ### cho từng phần.
   - Dùng danh sách, bảng, hoặc ví dụ minh họa nếu cần.
4. Viết bằng **tiếng Việt tự nhiên, dễ hiểu**, phù hợp cho cả người dùng phổ thông hoặc nhân viên hỗ trợ không chuyên kỹ thuật.
5. Duy trì giọng văn thân thiện, hướng dẫn rõ ràng từng bước.
6. Không viết lại từ đầu; chỉ tổng hợp và làm rõ những gì đã có trong các file tài liệu hiện tại.
7. Nếu thiếu thông tin, có thể thêm ghi chú như: “(Thông tin chưa có trong tài liệu gốc)”.
8. Đảm bảo tài liệu có thể được chia nhỏ (chunk) dễ dàng để đưa vào hệ thống AI tìm kiếm/trả lời.

Đầu ra:
Tạo toàn bộ nội dung của tệp 'PROJECT_KNOWLEDGE_BASE.md' bằng tiếng Việt, trình bày theo định dạng Markdown.
" --source ./docs
