BƯỚC 2 – INPUT CHUẨN (FINAL SPEC)

Dữ liệu đầu vào cho bước phân tích bối cảnh & cảm xúc từ AI

Bạn đã có:

Ảnh

Danh sách bounding box

Emotion local detect

→ Nhưng để AI hoạt động chính xác, cần đủ 7 nhóm thông tin sau.

🎯 1. Ảnh gốc (full image)

Dạng: URL hoặc base64

Resize xuống max 512px để AI phân tích tốt hơn & giảm token.

image_url / image_base64
image_size: "512x512"

🎯 2. Danh sách khuôn mặt đã detect

Chỉ cần:

face_id (unique)

bbox (x, y, w, h)

emotion_local (dominant + confidence)

quality (blur score optional)

Ví dụ:

faces: [
  {
    "face_id": "f1",
    "bbox": [100, 200, 160, 160],
    "emotion_local": { "dominant": "happy", "confidence": 0.82 },
    "quality": "good"
  }
]

🎯 3. Ai là người được chọn (target face)

Step 2 cần biết bạn muốn phân tích cảm xúc & ngữ cảnh cho ai.

target_face_id: "f1"


Nếu không chỉ rõ → AI sẽ đoán sai cảm xúc và sai ngữ cảnh cá nhân.

🎯 4. Thông tin Member (nếu đã match)

Không bắt buộc.
Nhưng nếu Step 1 có match thì nên đưa vào (để AI mô tả đúng phong cách):

name (optional)

age (nếu biết)

gender (nếu có)

relationship (cha/mẹ/ông/bà…)

member_info: {
  "id": "m123",
  "name": "Huỳnh Văn A",
  "gender": "male",
  "age": 42
}

🎯 5. Ảnh crop của target face

AI cần nhìn rõ khuôn mặt người được phân tích.

→ Crop từ bbox
→ Resize 128–256px

target_face_crop_url: "..."

🎯 6. Danh sách các khuôn mặt khác (context people)

Không cần nhiều, chỉ cần để AI hiểu bối cảnh:

other_faces_summary: [
   { "emotion_local": "neutral" },
   { "emotion_local": "happy" }
]


Không cần bbox, không cần crop vì AI đã xem full-image.

🎯 7. EXIF (nếu có) – KHÔNG BẮT BUỘC

Nếu ảnh chụp thật sẽ rất hữu ích:

datetime

gps

camera info

Ví dụ:

exif: {
  "datetime": "2012-05-22 17:30",
  "gps": null
}


Nếu không có cũng OK.

🧱 TỔNG HỢP – FULL INPUT CHUẨN CHO STEP 2

Bạn chỉ cần đưa đúng format này vào AI:

{
  "image_url": "...",
  "faces": [
    {
      "face_id": "f1",
      "bbox": [100, 200, 160, 160],
      "emotion_local": { "dominant": "happy", "confidence": 0.82 },
      "quality": "good"
    },
    {
      "face_id": "f2",
      "bbox": [240, 210, 140, 140],
      "emotion_local": { "dominant": "neutral", "confidence": 0.63 },
      "quality": "medium"
    }
  ],
  "target_face_id": "f1",
  
  "target_face_crop_url": "...",
  
  "member_info": {
    "id": "m123",
    "name": "Huỳnh Văn A",
    "gender": "male",
    "age": 42
  },

  "other_faces_summary": [
    { "emotion_local": "neutral" },
    { "emotion_local": "happy" }
  ],

  "exif": {
    "datetime": "2012-05-22 17:30",
    "gps": null
  }
}

📌 Vậy là đủ chưa?

Đủ 100%.
Chỉ cần 7 nhóm dữ liệu ở trên → AI phân tích 100% đầy đủ:

Bối cảnh sự kiện

Không gian

Ánh sáng

Diễn tả khuôn mặt người target

Cảm xúc target (fusion local + AI)

Mối quan hệ người trong ảnh

Cues để viết story