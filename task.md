Triển khai một OCR service bằng Python, sử dụng FastAPI, để backend khác có thể upload file và nhận về text đã OCR.

Mục tiêu:
- Nhận file đính kèm (ảnh hoặc PDF)
- Thực hiện OCR để trích xuất nội dung văn bản
- Trả về text thuần (plain text) cho backend xử lý tiếp
- Service này KHÔNG xử lý logic nghiệp vụ gia phả

Yêu cầu kỹ thuật:
- Ngôn ngữ: Python
- Framework: FastAPI
- OCR engine: Tesseract OCR (qua pytesseract)
- Hỗ trợ tiếng Việt và tiếng Anh
- Chạy được độc lập như một microservice

Endpoint:
POST /ocr
- Nhận multipart/form-data
- Field: file (PDF hoặc image: jpg, png)
- Response: JSON chứa text OCR

Luồng xử lý:
1. Nhận file upload
2. Xác định loại file
   - Nếu PDF: convert từng trang sang image
   - Nếu image: xử lý trực tiếp
3. Preprocess image (grayscale, threshold cơ bản)
4. OCR từng trang / ảnh
5. Gộp kết quả thành một chuỗi text
6. Làm sạch text (loại bỏ dòng trống dư thừa)
7. Trả kết quả cho client

Cấu trúc project đề xuất:
- main.py: FastAPI app + routing
- services/ocr_service.py: logic OCR chính
- utils/file_loader.py: load PDF / image
- utils/image_preprocess.py: tiền xử lý ảnh
- utils/text_cleaner.py: làm sạch text

Yêu cầu code:
- Code rõ ràng, dễ đọc
- Tách logic OCR khỏi layer API
- Có xử lý lỗi cơ bản (file không hỗ trợ, OCR fail)
- Không lưu file upload lâu dài (xử lý tạm thời)

Response format ví dụ:
{
  "success": true,
  "text": "Nội dung văn bản sau OCR..."
}

Ghi chú:
- Service này được dùng để hỗ trợ nhập liệu số lượng lớn
- OCR không cần chính xác tuyệt đối, ưu tiên giữ đúng cấu trúc dòng và năm tháng
