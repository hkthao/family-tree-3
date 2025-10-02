# Mô hình Dữ liệu và Schema Database

## Mục lục

- [1. Giới thiệu](#1-giới-thiệu)
- [2. Sơ đồ quan hệ thực thể (ERD)](#2-sơ-đồ-quan-hệ-thực-thể-erd)
- [3. Mô tả các bảng](#3-mô-tả-các-bảng)
  - [3.1. Bảng `Families`](#31-bảng-families)
  - [3.2. Bảng `Members`](#32-bảng-members)
  - [3.3. Bảng `Relationships`](#33-bảng-relationships)
- [4. Toàn vẹn và Ràng buộc Dữ liệu](#4-toàn-vẹn-và-ràng-buộc-dữ-liệu)
- [5. Hướng dẫn Mapping](#5-hướng-dẫn-mapping)
  - [5.1. Backend (Entity Framework Core)](#51-backend-entity-framework-core)
  - [5.2. Frontend (Vue.js)](#52-frontend-vuejs)
- [6. Ví dụ Dữ liệu JSON](#6-ví-dụ-dữ-liệu-json)

---

## 1. Giới thiệu

Tài liệu này mô tả chi tiết về mô hình dữ liệu, schema của database (MySQL), và các quy tắc ràng buộc nhằm đảm bảo tính nhất quán và toàn vẹn của dữ liệu trong hệ thống Cây Gia Phả.

## 2. Sơ đồ quan hệ thực thể (ERD)

```mermaid
erDiagram
    FAMILY {
        string id PK "ID duy nhất"
        string name "Tên gia đình"
        string description "Mô tả"
    }

    MEMBER {
        string id PK "ID duy nhất"
        string family_id FK "Khóa ngoại đến Families"
        string first_name "Tên"
        string last_name "Họ"
        date date_of_birth "Ngày sinh"
        date date_of_death "Ngày mất"
        string gender "Giới tính"
        string father_id FK "ID cha"
        string mother_id FK "ID mẹ"
        string spouse_id FK "ID vợ/chồng"
    }

    FAMILY ||--o{ MEMBER : "có"
    MEMBER ||--o| MEMBER : "cha của"
    MEMBER ||--o| MEMBER : "mẹ của"
    MEMBER ||--o| MEMBER : "vợ/chồng của"
```

## 3. Mô tả các bảng

### 3.1. Bảng `Families`

Lưu trữ thông tin về các gia đình hoặc dòng họ.

| Tên cột      | Kiểu dữ liệu | Ràng buộc | Mô tả                  |
| :------------ | :----------- | :-------- | :--------------------- |
| `id`          | `varchar(36)`| PK        | ID duy nhất của gia đình |
| `name`        | `varchar(100)`| NOT NULL  | Tên gia đình           |
| `description` | `text`       | NULL      | Mô tả về gia đình      |

- **Mối quan hệ**: Một `Family` có thể có nhiều `Member`.

### 3.2. Bảng `Members`

Lưu trữ thông tin chi tiết của từng thành viên, bao gồm các mối quan hệ trực tiếp.

| Tên cột         | Kiểu dữ liệu | Ràng buộc | Mô tả                   |
| :-------------- | :----------- | :-------- | :---------------------- |
| `id`            | `varchar(36)`| PK        | ID duy nhất của thành viên |
| `family_id`     | `varchar(36)`| FK, NOT NULL | ID của gia đình mà thành viên thuộc về |
| `first_name`    | `varchar(50)`| NOT NULL  | Tên                     |
| `last_name`     | `varchar(50)`| NOT NULL  | Họ                      |
| `date_of_birth` | `date`       | NULL      | Ngày sinh               |
| `date_of_death` | `date`       | NULL      | Ngày mất                |
| `gender`        | `varchar(10)`| NULL      | Giới tính (Male, Female, Other) |
| `father_id`     | `varchar(36)`| FK, NULL  | ID của cha              |
| `mother_id`     | `varchar(36)`| FK, NULL  | ID của mẹ               |
| `spouse_id`     | `varchar(36)`| FK, NULL  | ID của vợ/chồng         |

- **Foreign Keys**:
  - `family_id`: tham chiếu đến `Families(id)`.
  - `father_id`: tham chiếu đến `Members(id)`.
  - `mother_id`: tham chiếu đến `Members(id)`.
  - `spouse_id`: tham chiếu đến `Members(id)`.
- **Mối quan hệ**: Một `Member` thuộc về một `Family` và có thể có các mối quan hệ trực tiếp với các `Member` khác (cha, mẹ, vợ/chồng).

## 4. Toàn vẹn và Ràng buộc Dữ liệu

- **ID duy nhất**: Tất cả các khóa chính (`id`) đều là `GUID` để đảm bảo tính duy nhất trên toàn hệ thống.
- **Ngày sinh/mất**: `date_of_death` phải lớn hơn `date_of_birth`.
- **Giới tính**: Trường `gender` nên được giới hạn trong một tập các giá trị cụ thể (ví dụ: `Male`, `Female`, `Other`).


## 5. Hướng dẫn Mapping

### 5.1. Backend (Entity Framework Core)

Các bảng được map sang các class Entity trong `Domain` layer. EF Core sử dụng Fluent API để cấu hình chi tiết các mối quan hệ.

```csharp
// trong ApplicationDbContext.cs
modelBuilder.Entity<Member>(builder =>
{
    builder.HasKey(m => m.Id);
    builder.Property(m => m.FullName).IsRequired().HasMaxLength(100);
    builder.HasOne(m => m.Family)
           .WithMany()
           .HasForeignKey(m => m.FamilyId);

    // Cấu hình mối quan hệ cha, mẹ, vợ/chồng
    builder.HasOne(m => m.Father)
           .WithMany(m => m.Children)
           .HasForeignKey(m => m.FatherId)
           .IsRequired(false); // Cha là tùy chọn

    builder.HasOne(m => m.Mother)
           .WithMany(m => m.Children)
           .HasForeignKey(m => m.MotherId)
           .IsRequired(false); // Mẹ là tùy chọn

    builder.HasOne(m => m.Spouse)
           .WithMany()
           .HasForeignKey(m => m.SpouseId)
           .IsRequired(false); // Vợ/chồng là tùy chọn
});
```

### 5.2. Frontend (Vue.js)

Trong Frontend, dữ liệu từ API được map sang các interface/type trong thư mục `src/types`.

```typescript
// src/types/family/member.ts
export interface Member {
  id: string;
  familyId: string;
  fullName: string;
  gender?: 'Male' | 'Female' | 'Other';
  dateOfBirth?: string; // ISO 8601 format
  fatherId?: string;
  motherId?: string;
  spouseId?: string;
  childrenIds?: string[];
  // ... các trường khác
}
```

## 6. Ví dụ Dữ liệu JSON

Đây là ví dụ về cách dữ liệu có thể được trả về từ API, FE có thể sử dụng để mock.

**Family:**

```json
{
  "id": "f7b3b3b3-3b3b-3b3b-3b3b-3b3b3b3b3b3b",
  "name": "Dòng họ Nguyễn",
  "description": "Dòng họ lớn ở Việt Nam"
}
```

**Member:**

```json
{
  "id": "m1b3b3b3-3b3b-3b3b-3b3b-3b3b3b3b3b3b",
  "familyId": "f7b3b3b3-3b3b-3b3b-3b3b-3b3b3b3b3b3b",
  "fullName": "Văn A Nguyễn",
  "firstName": "Văn A",
  "lastName": "Nguyễn",
  "dateOfBirth": "1950-01-01T00:00:00Z",
  "gender": "Male",
  "fatherId": "m2c4c4c4-4c4c-4c4c-4c4c-4c4c4c4c4c4c",
  "motherId": "m3d5d5d5-5d5d-5d5d-5d5d-5d5d5d5d5d5d",
  "spouseId": "m4e6e6e6-6e6e-6e6e-6e6e-6e6e6e6e6e6e"
}
```