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
        string avatarUrl "URL ảnh đại diện"
        string address "Địa chỉ"
        string visibility "Chế độ hiển thị (Public/Private)"
        int totalMembers "Tổng số thành viên"
    }

    MEMBER {
        string id PK "ID duy nhất"
        string family_id FK "Khóa ngoại đến Families"
        string first_name "Tên"
        string last_name "Họ"
        string full_name "Họ và tên đầy đủ"
        date date_of_birth "Ngày sinh"
        date date_of_death "Ngày mất"
        string gender "Giới tính"
        string avatarUrl "URL ảnh đại diện"
        string nickname "Biệt danh"
        string placeOfBirth "Nơi sinh"
        string placeOfDeath "Nơi mất"
        string occupation "Nghề nghiệp"
        string father_id FK "ID cha"
        string mother_id FK "ID mẹ"
        string spouse_id FK "ID vợ/chồng"
    }

    EVENT {
        string id PK "ID duy nhất"
        string name "Tên sự kiện"
        string description "Mô tả"
        datetime startDate "Ngày bắt đầu"
        datetime endDate "Ngày kết thúc"
        string location "Địa điểm"
        string familyId FK "ID gia đình"
        string type "Loại sự kiện"
        string color "Mã màu"
    }

    EVENT_MEMBER {
        string event_id PK,FK "ID sự kiện"
        string member_id PK,FK "ID thành viên"
    }

    FAMILY ||--o{ MEMBER : "có"
    MEMBER ||--o| MEMBER : "cha của"
    MEMBER ||--o| MEMBER : "mẹ của"
    MEMBER ||--o| MEMBER : "vợ/chồng của"
    FAMILY ||--o{ EVENT : "có"
    EVENT ||--o{ EVENT_MEMBER : "liên quan đến"
    MEMBER ||--o{ EVENT_MEMBER : "liên quan đến"
```
## 3. Mô tả các bảng

### 3.1. Bảng `Families`

Lưu trữ thông tin về các gia đình hoặc dòng họ.

| Tên cột      | Kiểu dữ liệu | Ràng buộc | Mô tả                  |
| :------------ | :----------- | :-------- | :--------------------- |
| `id`          | `varchar(36)`| PK        | ID duy nhất của gia đình |
| `name`        | `varchar(100)`| NOT NULL  | Tên gia đình           |
| `description` | `text`       | NULL      | Mô tả về gia đình      |
| `avatar_url`  | `varchar(255)`| NULL      | URL ảnh đại diện của gia đình |
| `address`     | `varchar(255)`| NULL      | Địa chỉ của gia đình   |
| `visibility`  | `varchar(20)`| NOT NULL  | Chế độ hiển thị (Public, Private) |
| `total_members`| `int`        | NOT NULL  | Tổng số thành viên trong gia đình |

- **Mối quan hệ**: Một `Family` có thể có nhiều `Member` và nhiều `Event`.

### 3.2. Bảng `Members`

Lưu trữ thông tin chi tiết của từng thành viên, bao gồm các mối quan hệ trực tiếp.

| Tên cột         | Kiểu dữ liệu | Ràng buộc | Mô tả                   |
| :-------------- | :----------- | :-------- | :---------------------- |
| `id`            | `varchar(36)`| PK        | ID duy nhất của thành viên |
| `family_id`     | `varchar(36)`| FK, NOT NULL | ID của gia đình mà thành viên thuộc về |
| `first_name`    | `varchar(50)`| NOT NULL  | Tên                     |
| `last_name`     | `varchar(50)`| NOT NULL  | Họ                      |
| `full_name`     | `varchar(100)`| NOT NULL  | Họ và tên đầy đủ (tự động tạo) |
| `date_of_birth` | `date`       | NULL      | Ngày sinh               |
| `date_of_death` | `date`       | NULL      | Ngày mất                |
| `gender`        | `varchar(10)`| NULL      | Giới tính (Male, Female, Other) |
| `avatar_url`    | `varchar(255)`| NULL      | URL ảnh đại diện của thành viên |
| `nickname`      | `varchar(50)`| NULL      | Biệt danh               |
| `place_of_birth`| `varchar(255)`| NULL      | Nơi sinh                |
| `place_of_death`| `varchar(255)`| NULL      | Nơi mất                 |
| `occupation`    | `varchar(100)`| NULL      | Nghề nghiệp             |
| `father_id`     | `varchar(36)`| FK, NULL  | ID của cha              |
| `mother_id`     | `varchar(36)`| FK, NULL  | ID của mẹ               |
| `spouse_id`     | `varchar(36)`| FK, NULL  | ID của vợ/chồng         |

- **Foreign Keys**:
  - `family_id`: tham chiếu đến `Families(id)`.
  - `father_id`: tham chiếu đến `Members(id)` (tự tham chiếu).
  - `mother_id`: tham chiếu đến `Members(id)` (tự tham chiếu).
  - `spouse_id`: tham chiếu đến `Members(id)` (tự tham chiếu).
- **Mối quan hệ**: Một `Member` thuộc về một `Family`. Các mối quan hệ cha, mẹ, vợ/chồng là các mối quan hệ tự tham chiếu trong bảng `Members`.

### 3.3. Bảng `Events`

Lưu trữ thông tin về các sự kiện quan trọng của gia đình.

| Tên cột         | Kiểu dữ liệu | Ràng buộc | Mô tả                   |
| :-------------- | :----------- | :-------- | :---------------------- |
| `id`            | `varchar(36)`| PK        | ID duy nhất của sự kiện |
| `name`          | `varchar(200)`| NOT NULL  | Tên sự kiện             |
| `description`   | `text`       | NULL      | Mô tả chi tiết          |
| `start_date`    | `datetime`   | NOT NULL  | Ngày bắt đầu            |
| `end_date`      | `datetime`   | NULL      | Ngày kết thúc           |
| `location`      | `varchar(200)`| NULL      | Địa điểm diễn ra        |
| `family_id`     | `varchar(36)`| FK, NULL  | ID của gia đình liên quan |
| `type`          | `varchar(50)`| NOT NULL  | Loại sự kiện (Birth, Marriage, Death, Other) |
| `color`         | `varchar(20)`| NULL      | Mã màu để hiển thị      |

- **Foreign Keys**:
  - `family_id`: tham chiếu đến `Families(id)`.
- **Mối quan hệ**: Một `Event` có thể liên quan đến một `Family`.

### 3.4. Bảng `EventMembers` (Bảng trung gian)

Lưu trữ mối quan hệ nhiều-nhiều giữa `Event` và `Member`.

| Tên cột      | Kiểu dữ liệu | Ràng buộc | Mô tả                  |
| :------------ | :----------- | :-------- | :--------------------- |
| `event_id`    | `varchar(36)`| PK, FK    | ID của sự kiện         |
| `member_id`   | `varchar(36)`| PK, FK    | ID của thành viên      |

- **Foreign Keys**:
  - `event_id`: tham chiếu đến `Events(id)`.
  - `member_id`: tham chiếu đến `Members(id)`.
- **Mối quan hệ**: Một `Event` có thể liên quan đến nhiều `Member`, và một `Member` có thể liên quan đến nhiều `Event`.

## 4. Toàn vẹn và Ràng buộc Dữ liệu

Để đảm bảo tính chính xác và nhất quán của dữ liệu, hệ thống áp dụng các ràng buộc và quy tắc toàn vẹn dữ liệu sau:

*   **ID duy nhất**: Tất cả các khóa chính (`id`) đều là `GUID` (Globally Unique Identifier) để đảm bảo tính duy nhất trên toàn hệ thống và dễ dàng trong việc phân tán dữ liệu.
*   **Khóa ngoại (Foreign Keys)**: Đảm bảo tính toàn vẹn tham chiếu giữa các bảng. Ví dụ, `family_id` trong bảng `Members` phải tồn tại trong bảng `Families`.
*   **Ngày sinh/mất**: 
    *   `date_of_death` (nếu có) phải lớn hơn `date_of_birth`.
    *   `date_of_birth` và `date_of_death` không được ở trong tương lai.
*   **Giới tính**: Trường `gender` nên được giới hạn trong một tập các giá trị cụ thể (ví dụ: `Male`, `Female`, `Other`) để đảm bảo tính nhất quán.
*   **Tên và Họ**: Các trường `first_name` và `last_name` là bắt buộc (`NOT NULL`) để đảm bảo mỗi thành viên có thông tin cơ bản đầy đủ.
*   **Chế độ hiển thị (Visibility)**: Trường `visibility` trong bảng `Families` nên được giới hạn trong các giá trị như `Public` hoặc `Private`.
*   **Loại sự kiện (Event Type)**: Trường `type` trong bảng `Events` nên được giới hạn trong các giá trị cụ thể (ví dụ: `Birth`, `Marriage`, `Death`, `Other`).


## 5. Hướng dẫn Mapping

### 5.1. Backend (Entity Framework Core)

Các bảng được map sang các class Entity trong `Domain` layer. EF Core sử dụng Fluent API trong `ApplicationDbContext` để cấu hình chi tiết các mối quan hệ và thuộc tính của Entity.

```csharp
// trong ApplicationDbContext.cs (phương thức OnModelCreating)

modelBuilder.Entity<Family>(builder =>
{
    builder.Property(f => f.Name).HasMaxLength(100).IsRequired();
    builder.Property(f => f.Description).HasMaxLength(1000);
    builder.Property(f => f.AvatarUrl).HasMaxLength(255);
    builder.Property(f => f.Address).HasMaxLength(255);
    builder.Property(f => f.Visibility).HasConversion<string>().HasMaxLength(20).IsRequired();
    builder.Property(f => f.TotalMembers).IsRequired();
});

modelBuilder.Entity<Member>(builder =>
{
    builder.Property(m => m.FirstName).HasMaxLength(50).IsRequired();
    builder.Property(m => m.LastName).HasMaxLength(50).IsRequired();
    builder.Property(m => m.FullName).HasMaxLength(100).IsRequired();
    builder.Property(m => m.Gender).HasConversion<string>().HasMaxLength(10);
    builder.Property(m => m.AvatarUrl).HasMaxLength(255);
    builder.Property(m => m.Nickname).HasMaxLength(50);
    builder.Property(m => m.PlaceOfBirth).HasMaxLength(255);
    builder.Property(m => m.PlaceOfDeath).HasMaxLength(255);
    builder.Property(m => m.Occupation).HasMaxLength(100);

    // Mối quan hệ với Family
    builder.HasOne(m => m.Family)
           .WithMany(f => f.Members)
           .HasForeignKey(m => m.FamilyId)
           .IsRequired();

    // Mối quan hệ tự tham chiếu (cha, mẹ, vợ/chồng)
    builder.HasOne(m => m.Father)
           .WithMany()
           .HasForeignKey(m => m.FatherId)
           .IsRequired(false);

    builder.HasOne(m => m.Mother)
           .WithMany()
           .HasForeignKey(m => m.MotherId)
           .IsRequired(false);

    builder.HasOne(m => m.Spouse)
           .WithMany()
           .HasForeignKey(m => m.SpouseId)
           .IsRequired(false);
});

modelBuilder.Entity<Event>(builder =>
{
    builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
    builder.Property(e => e.Description).HasMaxLength(1000);
    builder.Property(e => e.Location).HasMaxLength(200);
    builder.Property(e => e.Type).HasConversion<string>().HasMaxLength(50).IsRequired();
    builder.Property(e => e.Color).HasMaxLength(20);

    // Mối quan hệ với Family
    builder.HasOne(e => e.Family)
           .WithMany(f => f.Events)
           .HasForeignKey(e => e.FamilyId)
           .IsRequired(false); // Sự kiện có thể không thuộc về một Family cụ thể
});

// Cấu hình bảng trung gian cho mối quan hệ nhiều-nhiều giữa Event và Member
modelBuilder.Entity<EventMember>(builder =>
{
    builder.HasKey(em => new { em.EventId, em.MemberId });

    builder.HasOne(em => em.Event)
           .WithMany(e => e.EventMembers)
           .HasForeignKey(em => em.EventId);

    builder.HasOne(em => em.Member)
           .WithMany(m => m.EventMembers)
           .HasForeignKey(em => em.MemberId);
});
```

### 5.2. Frontend (Vue.js)

Trong Frontend, dữ liệu từ API được map sang các interface/type trong thư mục `src/types`. Điều này giúp đảm bảo tính nhất quán về kiểu dữ liệu giữa Frontend và Backend.

```typescript
// src/types/family/family.ts
export interface Family {
  id: string;
  name: string;
  description?: string;
  avatarUrl?: string;
  address?: string;
  visibility?: 'Public' | 'Private';
  totalMembers?: number;
}

// src/types/family/member.ts
export interface Member {
  id: string;
  lastName: string;
  firstName: string;
  fullName?: string;
  familyId: string;
  gender?: 'Male' | 'Female' | 'Other';
  dateOfBirth?: Date | null;
  dateOfDeath?: Date | null;
  birthDeathYears?: string;
  avatarUrl?: string;
  nickname?: string;
  placeOfBirth?: string;
  placeOfDeath?: string;
  occupation?: string;
  fatherId?: string | null;
  motherId?: string | null;
  spouseId?: string | null;
  biography?: string;
}

// src/types/event/event.ts
export interface Event {
  id: string;
  name: string;
  description?: string;
  startDate: Date;
  endDate?: Date | null;
  location?: string;
  familyId?: string | null;
  type: 'Birth' | 'Marriage' | 'Death' | 'Other';
  color?: string;
  relatedMembers?: string[]; // Chỉ chứa IDs của các thành viên liên quan
}
```

## 6. Ví dụ Dữ liệu JSON

Đây là ví dụ về cách dữ liệu có thể được trả về từ API, Frontend có thể sử dụng để mock hoặc hiểu cấu trúc dữ liệu.

#### Family:

```json
{
  "id": "f7b3b3b3-3b3b-4b3b-8b3b-3b3b3b3b3b3b",
  "name": "Dòng họ Nguyễn",
  "description": "Dòng họ lớn ở Việt Nam với nhiều chi nhánh.",
  "address": "Số 1, Đường ABC, Quận XYZ, TP.HCM",
  "avatarUrl": "https://example.com/avatars/nguyen_family.png",
  "visibility": "Public",
  "totalMembers": 150
}
```

#### Member:

```json
{
  "id": "m1b3b3b3-3b3b-4b3b-8b3b-3b3b3b3b3b3b",
  "familyId": "f7b3b3b3-3b3b-4b3b-8b3b-3b3b3b3b3b3b",
  "firstName": "Văn A",
  "lastName": "Nguyễn",
  "fullName": "Nguyễn Văn A",
  "gender": "Male",
  "dateOfBirth": "1950-01-01T00:00:00Z",
  "dateOfDeath": "2020-12-31T00:00:00Z",
  "birthDeathYears": "1950-2020",
  "avatarUrl": "https://example.com/avatars/nguyen_van_a.png",
  "nickname": "Ông Cả",
  "placeOfBirth": "Hà Nội",
  "placeOfDeath": "TP.HCM",
  "occupation": "Kỹ sư",
  "fatherId": "m2c4c4c4-4c4c-4c4c-8c4c-4c4c4c4c4c4c",
  "motherId": "m3d5d5d5-5d5d-4d5d-8d5d-5d5d5d5d5d5d",
  "spouseId": "m4e6e6e6-6e6e-4e6e-8e6e-6e6e6e6e6e6e",
  "biography": "Nguyễn Văn A là một kỹ sư tài năng..."
}
```

#### Event:

```json
{
  "id": "e1f7f7f7-7f7f-4f7f-8f7f-7f7f7f7f7f7f",
  "name": "Lễ giỗ Tổ",
  "description": "Lễ giỗ Tổ hàng năm của dòng họ Nguyễn.",
  "startDate": "2024-03-10T00:00:00Z",
  "endDate": "2024-03-10T23:59:59Z",
  "location": "Nhà thờ Tổ",
  "familyId": "f7b3b3b3-3b3b-4b3b-8b3b-3b3b3b3b3b3b",
  "type": "Other",
  "color": "#FF5733",
  "relatedMembers": [
    "m1b3b3b3-3b3b-4b3b-8b3b-3b3b3b3b3b3b",
    "m2c4c4c4-4c4c-4c4c-8c4c-4c4c4c4c4c4c"
  ]
}
```