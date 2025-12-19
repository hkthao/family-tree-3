# 📌 BACKEND IMPLEMENTATION SPEC

## Feature: **TỦ KỶ VẬT (MEMORY CABINET)**

### 1. Mục tiêu

Xây dựng backend cho tính năng **Tủ Kỷ Vật**, cho phép người dùng lưu giữ các kỷ niệm (kỷ vật) gắn với **cá nhân / gia đình / dòng họ**, có thể chứa **nội dung + media + người liên quan**, và hiển thị theo thời gian.

---

## 2. Phạm vi (Scope – KHÔNG làm ngoài phạm vi này)

* CRUD Memory Item
* Upload & quản lý media
* Gắn người vào kỷ vật

---

## 3. Khái niệm chính

### Memory Item (Kỷ vật)

> Một kỷ niệm hoặc vật kỷ niệm có ý nghĩa, có thể là câu chuyện, hình ảnh, sự kiện, hoặc vật thể.

---

## 4. Database Design

### 4.1 `memory_items`

| Field         | Type     | Description                 |        |       |          |          |
| ------------- | -------- | --------------------------- | ------ | ----- | -------- | -------- |
| id            | UUID     | Primary key                 |        |       |          |          |
| family_id     | UUID     |                             |        |       |          |          |
| title         | string   | Tên kỷ vật                  |        |       |          |          |
| description   | text     | Nội dung chi tiết           |        |       |          |          |
| happened_at   | datetime | Thời điểm xảy ra (nullable) |        |       |          |          |
| emotional_tag | enum     | `happy                      | sad    | proud | memorial | neutral` |
---

### 4.2 `memory_media`

| Field          | Type         |       |       |            |
| -------------- | ------------ | ----- | ----- | ---------- |
| id             | UUID         |       |       |            |
| memory_item_id | UUID (FK)    |       |       |            |
| type           | enum (`image | video | audio | document`) |
| url            | string       |       |       |            |

---

### 4.3 `memory_persons`

| Field          | Type         |           |           |
| -------------- | ------------ | --------- | --------- |
| memory_item_id | UUID         |           |           |
| memeber_id     | UUID         |           |           |

---

## 7. Validation Rules

* `title` bắt buộc
* `happenedAt` ≤ current date

---

## 8. Coding Requirements

* Clean Architecture / Layered Architecture
* DTO rõ ràng
* Enum mapping chặt chẽ
* Soft delete
---