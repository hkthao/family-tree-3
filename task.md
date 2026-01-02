
> Mục tiêu:
> **Backend quản lý “con người + giọng nói + lịch sử sinh audio”**
> Python service chỉ là worker, **không nằm trong domain**.

---

# 1. TƯ DUY DOMAIN ĐÚNG (CHỐT TRƯỚC)

Trong backend, bạn **KHÔNG quản lý AI**, bạn quản lý:

> **GiỌNG NÓI = TÀI SẢN SỐ CỦA MỘT NHÂN VẬT GIA PHẢ**

Nên domain phải xoay quanh:

* Member (nhân vật)
* Voice (giọng nói)
* Voice usage (audio sinh ra)

---

# 2. DOMAIN CORE (BẮT BUỘC)

## 2.2. VoiceProfile (GIỌNG NÓI – TRỌNG TÂM)

> **1 Member có thể có N VoiceProfile**

Mỗi VoiceProfile = **1 “kiểu giọng” ổn định**

```sql
voice_profiles
--------------
id
member_id
label              -- vd: "default", "story", "elder"
audio_url          -- merged.wav (đã preprocess)
duration_seconds
language
consent            -- true/false
status             -- active / archived
created_at
```

📌 Đây chính là nơi backend **giữ `voice_profile_id`**

---
## 2.4. VoiceGeneration (LỊCH SỬ GIỌNG ĐÃ SINH)

Mỗi lần user đọc text → sinh 1 audio.

```sql
voice_generations
-----------------
id
voice_profile_id
text
audio_url
duration
created_at
```

👉 Cực kỳ quan trọng cho:

* giới hạn quota
* lịch sử
* cache

---

# 3. DOMAIN RELATIONSHIP (RÕ RÀNG)

```
Family
  └── Member
        └── VoiceProfile
              └── VoiceGeneration
```

---

# 4. DOMAIN RULES (BUSINESS LOGIC BACKEND)

Backend PHẢI enforce:

### Rule 1

> Không generate voice nếu `consent = false`

---

### Rule 2

> 1 VoiceProfile chỉ dùng cho 1 Member

---

### Rule 3

> Mỗi Member nên có tối đa 1–2 VoiceProfile active

---

### Rule 4

> VoiceProfile chỉ lưu **audio đã preprocess**

❌ Không lưu raw mp3
❌ Không lưu nhiều file rời rạc

---

# 5. FLOW BACKEND THEO DOMAIN

## 5.1. Tạo VoiceProfile

```
FE upload audio
   ↓
Backend lưu raw
   ↓
Backend gọi Python /preprocess
   ↓
Nhận processed_audio_url
   ↓
Create VoiceProfile
```

---

## 5.2. Generate Voice

```
FE gửi voice_profile_id + text
   ↓
Backend validate:
   - ownership
   - consent
   - quota
   ↓
Backend gọi Python /generate
   ↓
Nhận audio_url
   ↓
Create VoiceGeneration
```

---

# 6. API BACKEND GỢI Ý (KHÔNG CODE)

```
POST   /members/{id}/voice-profiles
GET    /members/{id}/voice-profiles
POST   /voice-profiles/{id}/generate
GET    /voice-profiles/{id}/history
```

👉 Backend **chỉ gọi Python bằng audio_url**

---

# 7. NHỮNG THỨ TUYỆT ĐỐI KHÔNG ĐƯA VÀO DOMAIN

❌ speaker embedding
❌ replicate model id
❌ AI config chi tiết
❌ ffmpeg pipeline

👉 Domain phải **AI-agnostic**

---

# 8. CHECKLIST DOMAIN ĐÚNG

✅ Có VoiceProfile entity
✅ VoiceProfile gắn với Member
✅ Backend giữ voice_profile_id
✅ Python service không biết domain
✅ Có VoiceGeneration log

---

# 9. TÓM TẮT CHỐT HẠ

> **Backend quản lý GIỌNG NÓI như 1 thực thể domain**
> **Python chỉ là máy xử lý**

Nếu sau này:

* đổi Replicate → OpenAI / local model
* đổi XTTS → model khác

👉 **DOMAIN KHÔNG CẦN ĐỔI**

---
