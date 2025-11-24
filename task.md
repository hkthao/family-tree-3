Dưới đây là **ĐẶC TẢ CHI TIẾT – FULL SPEC** dành cho tính năng **AI Memorial Studio**, thiết kế theo chuẩn product spec

# 🕊️ **AI MEMORIAL STUDIO – FULL PRODUCT SPEC**

### *“Phòng ghi nhớ & phục dựng ký ức tổ tiên bằng AI”*

---

# 1️⃣ MỤC TIÊU TÍNH NĂNG (PRODUCT GOALS)

1. Giúp người dùng **tái tạo – lưu giữ – truyền lại ký ức gia đình** qua 3 loại dữ liệu:
   * **Story (text)**
   * **Photo (image)**
   * **Voice (audio)**
2. Cung cấp một trải nghiệm **nhân văn – cảm xúc – không creepy**.
3. Tạo ra **dữ liệu di sản số (digital legacy)** gắn trực tiếp với từng thành viên trong gia phả.
4. Có thể xuất bản:

   * PDF
   * Photo album
   * Audio memories
   * Video slideshow (ở giai đoạn sau)

---

# 2️⃣ KIẾN TRÚC MODULE

```
AI Memorial Studio
 ├── Story Memory (NLP)
 ├── Photo Revival (Image Restoration)
 └── Voice Revival (Audio Reconstruction)
```

## 🔧 Công nghệ chính:

* **Story** → LLM (OpenAI/Gemini)
* **Photo** → Image Restoration pipeline (GFPGAN + Colorization + Upscale)
* **Voice** → Voice Cloning (Edge-TTS, OpenVoice, XTTS, hoặc ElevenLabs API)

---

# 3️⃣ FLOW NGƯỜI DÙNG (USER FLOW)

## 3.1 Chọn thành viên gia đình

User vào hồ sơ > bấm **“AI Memorial Studio”**
→ Chọn 1 trong 3 tool: Story / Photo / Voice.

---

# 4️⃣ MODULE 1 – STORY MEMORY (AI LIFE STORY BUILDER)

## 🎯 Mục đích

Biến:

* ghi chú
* ảnh
* sự kiện
* ký ức rời rạc

→ thành **một câu chuyện hoàn chỉnh**, có giọng văn tự nhiên.

---

## 📌 FLOW

1. Upload dữ liệu:

   * Ảnh
   * Sự kiện (năm sinh, nơi sinh, nghề nghiệp,…)
   * Các đoạn text hoặc voice mô tả ký ức
2. User chọn style:

   * Giọng kể người lớn tuổi
   * Giọng hiện đại
   * Giọng dân dã
   * Giọng nghiêm trang / sử thi
3. AI gợi ý câu hỏi:

   * “Bạn có thể mô tả thêm về tính cách của ông không?”
   * “Gia đình nhớ nhất điều gì về bà?”
4. User trả lời → AI refine story
5. Output:

   * Story dạng chương
   * Timeline tóm tắt

---

## 📌 JSON Structure (Lưu DB)

```json
{
  "memberId": "guild",
  "storyId": "guild",
  "title": "Cuộc đời ông Nguyễn Văn A",
  "storyContent": "string-long",
  "timeline": [
    { "year": 1952, "event": "Sinh tại Bình Định" },
    { "year": 1970, "event": "Đi quân sự" }
  ],
  "createdAt": "2025-11-24T07:00:00Z",
  "style": "traditional"
}
```

---

# 5️⃣ MODULE 2 – PHOTO REVIVAL (IMAGE RESTORATION & COLORIZATION)

## 🎯 Mục đích

* Phục chế ảnh cũ, mờ, rách.
* Tô màu.
* Nâng độ phân giải.
* Giữ lại nét mặt nguyên bản, không deepfake.

---

## 📌 FLOW

1. Upload ảnh (JPG/PNG/HEIC)
2. AI tự phân tích:

   * mức độ hư hại
   * khuôn mặt
   * background
3. 3 chế độ phục chế:

   * **Basic Restore:** làm nét + xóa noise
   * **Colorize:** tô màu tự nhiên
   * **Revive Max:** full pipeline (GFPGAN + ESRGAN + Colorization)
4. Hiển thị Before/After slider
5. Người dùng chọn mức độ:

   * 25% / 50% / 75% / 100%
6. Lưu output vào profile thành viên

---

## 📌 JSON Structure

```json
{
  "photoId": "string",
  "memberId": "string",
  "originalUrl": "string",
  "restoredUrl": "string",
  "mode": "revive-max",
  "intensity": 80,
  "createdAt": "2025-11-24T07:00:00Z"
}
```

---

# 6️⃣ MODULE 3 – VOICE REVIVAL (VOICE RESTORATION & MEMORY PLAYBACK)

## 🎯 Mục đích

* Phục hồi giọng nói từ file cũ.
* Tạo “Voice Memory” (voice sample).
* Cho phép nghe lại hoặc nói chuyện hạn chế.

---

## 📌 2 CHẾ ĐỘ CHÍNH

### **Chế độ 1: Voice Memory Playback (an toàn – nhân văn)**

* AI làm sạch audio cũ (noise reduction).
* Chuẩn hoá giọng.
* Chỉ phát lại các câu đã có trong dữ liệu gốc.

### **Chế độ 2: AI Limited Conversation**

* Tạo voice clone từ sample (nếu gia đình đồng ý).
* Người dùng có thể hỏi:

  * “Ông thích món gì?”
  * “Hồi nhỏ ông làm gì?”
* AI trả lời dựa trên:

  * dữ liệu trong profile
  * story memory
  * ghi âm/thư cũ
    **Không được tạo dự đoán về tương lai → tránh spooky.**

---

## 📌 FLOW

1. Upload file audio (mp3/wav/m4a).
2. AI phân tích:

   * chất lượng
   * noise
   * xác suất clone được hay không
3. User chọn:

   * Chỉ phục chế (không clone)
   * Tạo Voice Memory (clone)
4. AI xử lý
5. Output:

   * Audio file
   * Hoặc WebRTC chat với giọng người thân

---

## 📌 JSON Structure

```json
{
  "voiceId": "string",
  "memberId": "string",
  "mode": "memory-playback",
  "originalUrl": "string",
  "cleanUrl": "string",
  "cloneModelUrl": "string",
  "createdAt": "2025-11-24T07:00:00Z"
}
```

---

# 7️⃣ TRANG UI CHÍNH – AI MEMORIAL STUDIO

### Header:

* Avatar thành viên
* Tên
* Năm sinh – năm mất

### 3 Button lớn:

1. **Story Memory**
2. **Photo Revival**
3. **Voice Revival**

### Mỗi module hiển thị:

* List các sản phẩm đã tạo
* Nút “Tạo mới”
* Modal preview

---

# 8️⃣ GÓC NHẠY CẢM – CẦN LƯU Ý (ETHICS)

### ✔ Thông báo khi dùng Voice Cloning

> “Đây là giọng mô phỏng dựa trên dữ liệu gia đình cung cấp.
> Chúng tôi không tạo nội dung mà người thân chưa từng nói nếu không có ngữ cảnh phù hợp.”

### ✔ Không trả lời tương lai

> “Tôi không thể nói về những điều mà ông/bà chưa từng chia sẻ.”

### ✔ Lưu metadata để kiểm soát

* Ai upload
* Khi nào
* Giọng gốc dài bao nhiêu

---

# 9️⃣ API BACKEND (ASP.NET CORE)

## POST /api/memorial/story

## POST /api/memorial/photo

## POST /api/memorial/voice

## GET /api/memorial/{memberId}

## DELETE /api/memorial/{id}

---