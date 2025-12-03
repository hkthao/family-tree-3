* Từ dữ liệu Domain Events (MemberCreated, FamilyUpdated, StoryAdded…) Gemini sẽ **lấy đủ dữ liệu từ API nội bộ**,
* **Tổng hợp** lại thành 4 loại bản ghi AI,
* **Chuẩn hóa format**,
* Thêm **family_id** để đảm bảo tách biệt giữa các gia đình,
* Gửi về webhook n8n để nhét vào vector store.

1. **Fetch full and latest data** from internal APIs (member, family, events, stories, relationships).

2. **Generate up to 4 types of AI Text Records**:

   * **(A) Member Record**
   * **(B) Family Summary Record**
   * **(C) Event Record**
   * **(D) Story Record**

3. Each record must include `family_id` and `record_type`.

4. **POST all generated records to the n8n webhook** in JSON format.

5. Make sure all records are deterministic, complete, and ready for vector embedding.

---

### **FORMAT BẮT BUỘC CHO 4 LOẠI AI TEXT RECORDS**

---

## **(A) Member Record**

**Một người = 1 block text.**

```
{
  "record_type": "member",
  "family_id": "<family-id>",
  "member_id": "<member-id>",
  "text": "
Tên: …
Ngày sinh: …
Ngày mất (nếu có): …
Giới tính: …
Quan hệ: (Cha/Mẹ/Vợ/Chồng/Con của ai)
Sự kiện quan trọng: (list)
Tiểu sử tóm tắt: …
Câu chuyện nổi bật: …
"
}
```

---

## **(B) Family Summary Record**

**Mỗi gia đình = 1 block summary tổng hợp.**

```
{
  "record_type": "family_summary",
  "family_id": "<family-id>",
  "text": "
Tên gia đình: …
Tên các thành viên: …
Sơ đồ quan hệ chính: …
Sự kiện quan trọng của gia đình: …
Điểm nổi bật lịch sử: …
Câu chuyện truyền thống: …
"
}
```

---

## **(C) Event Record**

**Mỗi event = 1 block.**

```
{
  "record_type": "event",
  "family_id": "<family-id>",
  "event_id": "<event-id>",
  "text": "
Tên sự kiện: …
Ngày diễn ra: …
Ai tham gia: …
Ý nghĩa: …
Chi tiết mô tả: …
"
}
```

---

## **(D) Story Record**

**Mỗi story = 1 block (câu chuyện).**

```
{
  "record_type": "story",
  "family_id": "<family-id>",
  "story_id": "<story-id>",
  "text": "
Tiêu đề câu chuyện: …
Nhân vật chính: …
Bối cảnh: …
Cốt truyện: …
Ý nghĩa và cảm xúc: …
"
}
```
