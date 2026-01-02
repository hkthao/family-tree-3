# 📌 TASK: Implement Python Image Restoration Service using Replicate

## 1. Mục tiêu

Xây dựng **một service Python** dùng để **phục chế ảnh cũ (ảnh tổ tiên)** cho ứng dụng gia phả, sử dụng **Replicate API** với các model AI chuyên phục hồi ảnh.

Service này **không xử lý UI**, chỉ chịu trách nhiệm:

* Nhận URL ảnh gốc
* Gọi AI phục chế
* Trả về URL ảnh đã phục chế
* Lưu metadata phục vụ backend app

---

## 2. Công nghệ & ràng buộc

* Ngôn ngữ: **Python 3.10+**
* AI provider: **Replicate**
* Framework API: **FastAPI**
* Không self-host GPU
* API key lấy từ biến môi trường:

  ```
  REPLICATE_API_TOKEN
  ```

---

## 3. Chức năng chính cần implement

### 3.1 Image Restoration Pipeline

Pipeline mặc định gồm 2 bước theo thứ tự:

1. **Face Restoration**

   * Model: `tencentarc/gfpgan`
   * Mục tiêu: khôi phục khuôn mặt bị mờ, nứt, nhiễu
2. **Upscale Image**

   * Model: `nightmareai/real-esrgan`
   * Mục tiêu: tăng độ phân giải ảnh sau khi phục chế

📌 Không được overwrite ảnh gốc.

---

### 3.2 Input

Service nhận **URL ảnh công khai** (JPEG / PNG).

Ví dụ input:

```json
{
  "imageUrl": "https://storage.example.com/original/photo.jpg"
}
```

---

### 3.3 Output

Trả về **URL ảnh đã phục chế** + metadata.

Ví dụ:

```json
{
  "originalUrl": "...",
  "restoredUrl": "...",
  "pipeline": ["GFPGAN", "Real-ESRGAN"],
  "status": "completed"
}
```

---

## 4. API cần xây dựng

### 4.1 Endpoint: Start restoration

```
POST /restore
```

#### Request body

```json
{
  "imageUrl": "string"
}
```

#### Response (ngay lập tức)

```json
{
  "status": "processing",
  "jobId": "uuid"
}
```

---

### 4.2 Job processing

* Chạy xử lý AI **bất đồng bộ**
* Có thể dùng:

  * FastAPI BackgroundTasks
  * hoặc Celery (simple setup)
* Sau khi hoàn thành:

  * Lưu kết quả vào in-memory store / simple dict (chưa cần DB)

---

### 4.3 Endpoint: Check job status

```
GET /restore/{jobId}
```

#### Response

```json
{
  "status": "completed",
  "originalUrl": "...",
  "restoredUrl": "...",
  "pipeline": ["GFPGAN", "Real-ESRGAN"]
}
```

Hoặc nếu đang xử lý:

```json
{
  "status": "processing"
}
```

---

## 5. Cấu trúc project mong muốn

```
image_restoration_service/
│
├── app/
│   ├── main.py              # FastAPI entrypoint
│   ├── api.py               # API routes
│   ├── services/
│   │   └── replicate_service.py
│   ├── models/
│   │   └── job.py            # Job state model
│   └── config.py            # Env config
│
├── requirements.txt
└── README.md
```

---

## 6. Replicate Service – yêu cầu chi tiết

### 6.1 GFPGAN call

* Input:

  * img: image URL
  * version: v1.4
  * scale: 2
* Output:

  * URL ảnh phục chế

### 6.2 Real-ESRGAN call

* Input:

  * image: URL ảnh đã qua GFPGAN
  * scale: 2
* Output:

  * URL ảnh cuối cùng

📌 Lưu ý:

* Replicate có thể trả về **list URL** → phải xử lý đúng type.

---

## 7. Yêu cầu về code quality

* Tách logic rõ ràng:

  * API layer
  * AI service layer
* Có comment giải thích
* Có error handling:

  * Replicate timeout
  * Invalid image URL
* Không hardcode API key

---

## 8. Giới hạn & giả định (cho MVP)

* Không authentication
* Không rate limit
* Không database thật
* Chỉ phục chế **1 ảnh / request**
* Không colorize ảnh (chưa dùng)

---

## 9. Ghi chú đạo đức (IMPORTANT)

Service chỉ **tăng độ rõ nét**, không thay đổi đặc điểm khuôn mặt.
Không áp dụng filter làm trẻ hóa hoặc biến dạng ảnh.

---

## 10. Output mong muốn từ Gemini CLI

Gemini CLI cần:

1. Generate toàn bộ code Python theo cấu trúc trên
2. Có thể chạy bằng:

   ```bash
   uvicorn app.main:app --reload
   ```
3. Có README hướng dẫn chạy local

---
