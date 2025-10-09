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
    USER_PROFILE {
        string Id PK "ID duy nhất"
        string ExternalId "ID người dùng từ Auth0"
        string Email "Email người dùng"
        string Name "Tên hiển thị"
    }

    FAMILY {
        string Id PK "ID duy nhất"
        string Name "Tên gia đình"
        string Description "Mô tả"
        string AvatarUrl "URL ảnh đại diện"
        string Address "Địa chỉ"
        string Visibility "Chế độ hiển thị (Public/Private)"
        int TotalMembers "Tổng số thành viên"
    }

    FAMILY_USER {
        string FamilyId PK,FK "ID gia đình"
        string UserProfileId PK,FK "ID hồ sơ người dùng"
        int Role "Vai trò của người dùng trong gia đình (Enum int)"
    }

    MEMBER {
        string Id PK "ID duy nhất"
        string FamilyId FK "Khóa ngoại đến Families"
        string FirstName "Tên"
        string LastName "Họ"
        string FullName "Họ và tên đầy đủ"
        date DateOfBirth "Ngày sinh"
        date DateOfDeath "Ngày mất"
        string Gender "Giới tính"
        string AvatarUrl "URL ảnh đại diện"
        string Nickname "Biệt danh"
        string PlaceOfBirth "Nơi sinh"
        string PlaceOfDeath "Nơi mất"
        string Occupation "Nghề nghiệp"
    }

    EVENT {
        string Id PK "ID duy nhất"
        string Name "Tên sự kiện"
        string Description "Mô tả"
        datetime StartDate "Ngày bắt đầu"
        datetime EndDate "Ngày kết thúc"
        string Location "Địa điểm"
        string FamilyId FK "ID gia đình"
        int Type "Loại sự kiện (Enum int)"
        string Color "Mã màu"
    }

    RELATIONSHIP {
        string Id PK "ID duy nhất"
        string SourceMemberId FK "ID thành viên nguồn" 
        string TargetMemberId FK "ID thành viên đích"
        int Type "Loại quan hệ (Enum int)"
        int Order "Thứ tự (nếu có)"
    }

    EVENT_MEMBER {
        string EventId PK,FK "ID sự kiện"
        string MemberId PK,FK "ID thành viên"
    }

    AI_BIOGRAPHY {
        string Id PK "ID duy nhất"
        string MemberId FK "ID thành viên liên quan"
        int Style "Phong cách tiểu sử (Enum int)"
        string Content "Nội dung tiểu sử"
        int Provider "Nhà cung cấp AI (Enum int)"
        string UserPrompt "Gợi ý ban đầu của người dùng"
        boolean GeneratedFromDB "Có được tạo từ dữ liệu DB không"
        int TokensUsed "Số lượng token đã sử dụng"
        json Metadata "Metadata bổ sung (JSON)"
        datetime Created "Thời gian tạo"
    }

    USER_PREFERENCE {
        string UserProfileId PK,FK "ID hồ sơ người dùng"
        int Theme "Chủ đề giao diện (Enum int)"
        int Language "Ngôn ngữ (Enum int)"
        boolean EmailNotificationsEnabled "Bật/tắt thông báo Email"
        boolean SmsNotificationsEnabled "Bật/tắt thông báo SMS"
        boolean InAppNotificationsEnabled "Bật/tắt thông báo trong ứng dụng"
        datetime Created "Thời gian tạo"
        datetime LastModified "Thời gian cập nhật cuối cùng"
    }

    USER_PROFILE ||--o{ FAMILY_USER : "có vai trò trong"
    FAMILY ||--o{ FAMILY_USER : "có người dùng"
    FAMILY ||--o{ MEMBER : "có"
    FAMILY ||--o{ EVENT : "có"
    MEMBER ||--o{ RELATIONSHIP : "có quan hệ"
    MEMBER ||--o{ EVENT_MEMBER : "liên quan đến"
    EVENT ||--o{ EVENT_MEMBER : "liên quan đến"
    RELATIONSHIP ||--o| MEMBER : "nguồn là"
    RELATIONSHIP ||--o| MEMBER : "đích là"
    MEMBER ||--o{ AI_BIOGRAPHY : "có tiểu sử AI"
    USER_PROFILE ||--o| USER_PREFERENCE : "có tùy chọn"
```
## 3. Mô tả các bảng

### 3.1. Bảng `UserProfiles`

Lưu trữ thông tin hồ sơ người dùng, được liên kết với các tài khoản từ nhà cung cấp xác thực bên ngoài.

| Tên cột       | Kiểu dữ liệu | Ràng buộc | Mô tả                                  |
| :------------ | :----------- | :-------- | :------------------------------------- |
| `Id`          | `varchar(36)`| PK        | ID duy nhất của hồ sơ người dùng       |
| `ExternalId` | `varchar(255)`| NOT NULL  | ID người dùng từ nhà cung cấp xác thực (ví dụ: Auth0) |
| `Email`       | `varchar(255)`| NOT NULL  | Địa chỉ email của người dùng           |
| `Name`        | `varchar(255)`| NOT NULL  | Tên hiển thị của người dùng            |

- **Mối quan hệ**: Một `UserProfile` có thể có nhiều `FamilyUser` (vai trò trong các gia đình).

### 3.2. Bảng `FamilyUsers`

Lưu trữ mối quan hệ nhiều-nhiều giữa `Family` và `UserProfile`, bao gồm vai trò của người dùng trong gia đình đó.

| Tên cột         | Kiểu dữ liệu | Ràng buộc | Mô tả                                  |
| :-------------- | :----------- | :-------- | :------------------------------------- |
| `FamilyId`      | `varchar(36)`| PK, FK    | ID của gia đình                        |
| `UserProfileId` | `varchar(36)`| PK, FK    | ID của hồ sơ người dùng                |
| `Role`          | `int`        | NOT NULL  | Vai trò của người dùng trong gia đình (0: Manager, 1: Viewer) |

- **Foreign Keys**:
  - `FamilyId`: tham chiếu đến `Families(Id)`.
  - `UserProfileId`: tham chiếu đến `UserProfiles(Id)`.
- **Mối quan hệ**: Một `FamilyUser` liên kết một `Family` với một `UserProfile` và định nghĩa `Role` của `UserProfile` đó trong `Family`.

### 3.3. Enum `FamilyRole`

Định nghĩa các vai trò mà một người dùng có thể có trong một gia đình.

| Giá trị | Mô tả                                  |
| :------ | :------------------------------------- |
| `0`     | `Manager`: Người dùng có toàn quyền quản lý gia đình. |
| `1`     | `Viewer`: Người dùng có thể xem dữ liệu gia đình nhưng không thể sửa đổi. |

### 3.4. Bảng `Families` (updated after refactor)

Lưu trữ thông tin về các gia đình hoặc dòng họ.

| Tên cột      | Kiểu dữ liệu | Ràng buộc | Mô tả                  |
| :------------ | :----------- | :-------- | :--------------------- |
| `Id`          | `varchar(36)`| PK        | ID duy nhất của gia đình |
| `Name`        | `varchar(100)`| NOT NULL  | Tên gia đình           |
| `Description` | `text`       | NULL      | Mô tả về gia đình      |
| `AvatarUrl`   | `longtext`   | NULL      | URL ảnh đại diện của gia đình |
| `Address`     | `longtext`   | NULL      | Địa chỉ của gia đình   |
| `Visibility`  | `varchar(20)`| NOT NULL  | Chế độ hiển thị (Public, Private) |
| `TotalMembers`| `int`        | NOT NULL  | Tổng số thành viên trong gia đình |

- **Mối quan hệ**: Một `Family` có thể có nhiều `Member` và nhiều `Event`.

### 3.2. Bảng `Members` (updated after refactor)

Lưu trữ thông tin chi tiết của từng thành viên. Các mối quan hệ giữa các thành viên được quản lý thông qua bảng `Relationships`.

| Tên cột         | Kiểu dữ liệu | Ràng buộc | Mô tả                   |
| :-------------- | :----------- | :-------- | :---------------------- |
| `Id`            | `varchar(36)`| PK        | ID duy nhất của thành viên |
| `FamilyId`      | `varchar(36)`| FK, NOT NULL | ID của gia đình mà thành viên thuộc về |
| `FirstName`     | `varchar(250)`| NOT NULL  | Tên                     |
| `LastName`      | `varchar(250)`| NOT NULL  | Họ                      |
| `FullName`      | `varchar(100)`| NOT NULL  | Họ và tên đầy đủ (tự động tạo) |
| `DateOfBirth`   | `date`       | NULL      | Ngày sinh               |
| `DateOfDeath`   | `date`       | NULL      | Ngày mất                |
| `Gender`        | `varchar(10)`| NULL      | Giới tính (Male, Female, Other) |
| `AvatarUrl`     | `longtext`   | NULL      | URL ảnh đại diện của thành viên |
| `Nickname`      | `varchar(100)`| NULL      | Biệt danh               |
| `PlaceOfBirth`  | `varchar(200)`| NULL      | Nơi sinh                |
| `PlaceOfDeath`  | `varchar(200)`| NULL      | Nơi mất                 |
| `Occupation`    | `varchar(100)`| NULL      | Nghề nghiệp             |

- **Foreign Keys**:
  - `FamilyId`: tham chiếu đến `Families(Id)`.
- **Mối quan hệ**: Một `Member` thuộc về một `Family`. Các mối quan hệ giữa các thành viên (cha, mẹ, vợ/chồng, v.v.) được định nghĩa và lưu trữ trong bảng `Relationships`.

### 3.3. Bảng `Events` (updated after refactor)

Lưu trữ thông tin về các sự kiện quan trọng của gia đình.

| Tên cột         | Kiểu dữ liệu | Ràng buộc | Mô tả                   |
| :-------------- | :----------- | :-------- | :---------------------- |
| `Id`            | `varchar(36)`| PK        | ID duy nhất của sự kiện |
| `Name`          | `varchar(200)`| NOT NULL  | Tên sự kiện             |
| `Description`   | `text`       | NULL      | Mô tả chi tiết          |
| `StartDate`     | `datetime`   | NOT NULL  | Ngày bắt đầu            |
| `EndDate`       | `datetime`   | NULL      | Ngày kết thúc           |
| `Location`      | `varchar(200)`| NULL      | Địa điểm diễn ra        |
| `FamilyId`      | `varchar(36)`| FK, NULL  | ID của gia đình liên quan |
| `Type`          | `int`        | NOT NULL  | Loại sự kiện (Birth, Marriage, Death, Other) |
| `Color`         | `varchar(20)`| NULL      | Mã màu để hiển thị      |

- **Foreign Keys**:
  - `FamilyId`: tham chiếu đến `Families(Id)`.
- **Mối quan hệ**: Một `Event` có thể liên quan đến một `Family`.

### 3.4. Bảng `Relationships` (updated after refactor)

Lưu trữ các mối quan hệ giữa các thành viên (ví dụ: cha, mẹ, vợ/chồng, con cái).

| Tên cột         | Kiểu dữ liệu | Ràng buộc | Mô tả                   |
| :-------------- | :----------- | :-------- | :---------------------- |
| `Id`            | `varchar(36)`| PK        | ID duy nhất của mối quan hệ |
| `SourceMemberId`| `varchar(36)`| FK, NOT NULL | ID của thành viên nguồn (ví dụ: cha/mẹ/vợ/chồng) |
| `TargetMemberId`| `varchar(36)`| FK, NOT NULL | ID của thành viên đích (ví dụ: con/vợ/chồng) |
| `Type`          | `int`        | NOT NULL  | Loại mối quan hệ (ví dụ: Parent, Child, Spouse) |
| `Order`         | `int`        | NULL      | Thứ tự của mối quan hệ (nếu có) |

- **Foreign Keys**:
  - `SourceMemberId`: tham chiếu đến `Members(Id)`.
  - `TargetMemberId`: tham chiếu đến `Members(Id)`.
- **Mối quan hệ**: Một `Member` có thể là `SourceMember` hoặc `TargetMember` trong nhiều `Relationship`.

### 3.5. Bảng `AIBiographies`

Lưu trữ các tiểu sử được tạo bởi AI cho các thành viên.

| Tên cột         | Kiểu dữ liệu | Ràng buộc | Mô tả                   |
| :-------------- | :----------- | :-------- | :---------------------- |
| `Id`            | `varchar(36)`| PK        | ID duy nhất của tiểu sử AI |
| `MemberId`      | `varchar(36)`| FK, NOT NULL | ID của thành viên liên quan |
| `Style`         | `int`        | NOT NULL  | Phong cách tiểu sử (ví dụ: Emotional, Historical) |
| `Content`       | `longtext`   | NOT NULL  | Nội dung tiểu sử         |
| `Provider`      | `int`        | NOT NULL  | Nhà cung cấp AI (ví dụ: Gemini, OpenAI) |
| `UserPrompt`    | `longtext`   | NOT NULL  | Gợi ý ban đầu của người dùng |
| `GeneratedFromDB`| `boolean`    | NOT NULL  | Có được tạo từ dữ liệu DB không |
| `TokensUsed`    | `int`        | NOT NULL  | Số lượng token đã sử dụng |
| `Metadata`      | `json`       | NULL      | Metadata bổ sung (JSON) |
| `Created`       | `datetime`   | NOT NULL  | Thời gian tạo            |

- **Foreign Keys**:
  - `MemberId`: tham chiếu đến `Members(Id)`.
- **Mối quan hệ**: Một `Member` có thể có nhiều `AIBiography`.

### 3.6. Bảng `UserPreferences`

Lưu trữ tùy chọn cá nhân của người dùng.

| Tên cột                 | Kiểu dữ liệu | Ràng buộc | Mô tả                   |
| :---------------------- | :----------- | :-------- | :---------------------- |
| `UserProfileId`         | `varchar(36)`| PK, FK    | ID của hồ sơ người dùng |
| `Theme`                 | `int`        | NOT NULL  | Chủ đề giao diện (Light, Dark) |
| `Language`              | `int`        | NOT NULL  | Ngôn ngữ (English, Vietnamese) |
| `EmailNotificationsEnabled`| `boolean`    | NOT NULL  | Bật/tắt thông báo Email |
| `SmsNotificationsEnabled`  | `boolean`    | NOT NULL  | Bật/tắt thông báo SMS   |
| `InAppNotificationsEnabled`| `boolean`    | NOT NULL  | Bật/tắt thông báo trong ứng dụng |
| `Created`               | `datetime`   | NOT NULL  | Thời gian tạo            |
| `LastModified`          | `datetime`   | NULL      | Thời gian cập nhật cuối cùng |

- **Foreign Keys**:
  - `UserProfileId`: tham chiếu đến `UserProfiles(Id)`.
- **Mối quan hệ**: Một `UserProfile` có một `UserPreference`.

## 4. Toàn vẹn và Ràng buộc Dữ liệu (updated after refactor)

Để đảm bảo tính chính xác và nhất quán của dữ liệu, hệ thống áp dụng các ràng buộc và quy tắc toàn vẹn dữ liệu sau:

*   **ID duy nhất**: Tất cả các khóa chính (`Id`) đều là `GUID` (Globally Unique Identifier) để đảm bảo tính duy nhất trên toàn hệ thống và dễ dàng trong việc phân tán dữ liệu.
*   **Khóa ngoại (Foreign Keys)**: Đảm bảo tính toàn vẹn tham chiếu giữa các bảng. Ví dụ, `FamilyId` trong bảng `Members` phải tồn tại trong bảng `Families`. Tương tự, `SourceMemberId` và `TargetMemberId` trong bảng `Relationships` phải tồn tại trong bảng `Members`.
*   **Ngày sinh/mất**: 
    *   `DateOfDeath` (nếu có) phải lớn hơn `DateOfBirth`.
    *   `DateOfBirth` và `DateOfDeath` không được ở trong tương lai.
*   **Giới tính**: Trường `Gender` nên được giới hạn trong một tập các giá trị cụ thể (ví dụ: `Male`, `Female`, `Other`) để đảm bảo tính nhất quán.
*   **Tên và Họ**: Các trường `FirstName` và `LastName` là bắt buộc (`NOT NULL`) để đảm bảo mỗi thành viên có thông tin cơ bản đầy đủ.
*   **Chế độ hiển thị (Visibility)**: Trường `Visibility` trong bảng `Families` nên được giới hạn trong các giá trị như `Public` hoặc `Private`.
*   **Loại sự kiện (Event Type)**: Trường `Type` trong bảng `Events` nên được giới hạn trong các giá trị cụ thể (ví dụ: `Birth`, `Marriage`, `Death`, `Other`).
*   **Loại mối quan hệ (Relationship Type)**: Trường `Type` trong bảng `Relationships` nên được giới hạn trong các giá trị cụ thể (ví dụ: `Parent`, `Child`, `Spouse`, `Sibling`).


## 5. Hướng dẫn Mapping

### 5.1. Backend (Entity Framework Core) (updated after refactor)

Các bảng được map sang các class Entity trong `Domain` layer. EF Core sử dụng Fluent API trong `ApplicationDbContext` để cấu hình chi tiết các mối quan hệ và thuộc tính của Entity.

```csharp
// trong ApplicationDbContext.cs (phương thức OnModelCreating)

modelBuilder.Entity<UserProfile>(builder =>
{
    builder.Property(u => u.ExternalId).HasMaxLength(255).IsRequired();
    builder.Property(u => u.Email).HasMaxLength(255).IsRequired();
    builder.Property(u => u.Name).HasMaxLength(255).IsRequired();

    builder.HasMany(u => u.FamilyUsers)
           .WithOne(fu => fu.UserProfile)
           .HasForeignKey(fu => fu.UserProfileId)
           .OnDelete(DeleteBehavior.Cascade);
});

modelBuilder.Entity<FamilyUser>(builder =>
{
    builder.HasKey(fu => new { fu.FamilyId, fu.UserProfileId });

    builder.Property(fu => fu.Role).IsRequired(); // Stored as int

    builder.HasOne(fu => fu.Family)
           .WithMany(f => f.FamilyUsers)
           .HasForeignKey(fu => fu.FamilyId)
           .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(fu => fu.UserProfile)
           .WithMany(u => u.FamilyUsers)
           .HasForeignKey(fu => fu.UserProfileId)
           .OnDelete(DeleteBehavior.Cascade);
});

modelBuilder.Entity<Family>(builder =>
{
    builder.Property(f => f.Name).HasMaxLength(100).IsRequired();
    builder.Property(f => f.Description).HasMaxLength(1000);
    builder.Property(f => f.AvatarUrl); // longtext
    builder.Property(f => f.Address); // longtext
    builder.Property(f => f.Visibility).HasConversion<string>().HasMaxLength(20).IsRequired();
    builder.Property(f => f.TotalMembers).IsRequired();
});

modelBuilder.Entity<Member>(builder =>
{
    builder.Property(m => m.FirstName).HasMaxLength(250).IsRequired();
    builder.Property(m => m.LastName).HasMaxLength(250).IsRequired();
    builder.Property(m => m.FullName).HasMaxLength(100).IsRequired();
    builder.Property(m => m.Gender).HasConversion<string>().HasMaxLength(10);
    builder.Property(m => m.AvatarUrl); // longtext
    builder.Property(m => m.Nickname).HasMaxLength(100);
    builder.Property(m => m.PlaceOfBirth).HasMaxLength(200);
    builder.Property(m => m.PlaceOfDeath).HasMaxLength(200);
    builder.Property(m => m.Occupation).HasMaxLength(100);

    // Mối quan hệ với Family
    builder.HasOne(m => m.Family)
           .WithMany(f => f.Members)
           .HasForeignKey(m => m.FamilyId)
           .IsRequired();
});

modelBuilder.Entity<Event>(builder =>
{
    builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
    builder.Property(e => e.Description).HasMaxLength(1000);
    builder.Property(e => e.Location).HasMaxLength(200);
    builder.Property(e => e.Type).IsRequired(); // Stored as int
    builder.Property(e => e.Color).HasMaxLength(20);

    // Mối quan hệ với Family
    builder.HasOne(e => e.Family)
           .WithMany(f => f.Events)
           .HasForeignKey(e => e.FamilyId)
           .IsRequired(false); // Sự kiện có thể không thuộc về một Family cụ thể
});

// Cấu hình bảng Relationships
modelBuilder.Entity<Relationship>(builder =>
{
    builder.HasKey(r => r.Id);

    builder.Property(r => r.Type).IsRequired(); // Stored as int
    builder.Property(r => r.Order);

    builder.HasOne(r => r.SourceMember)
           .WithMany()
           .HasForeignKey(r => r.SourceMemberId)
           .OnDelete(DeleteBehavior.Restrict)
           .IsRequired();

    builder.HasOne(r => r.TargetMember)
           .WithMany()
           .HasForeignKey(r => r.TargetMemberId)
           .OnDelete(DeleteBehavior.Restrict)
           .IsRequired();
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
           .HasForeignKey(em => m.MemberId);
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