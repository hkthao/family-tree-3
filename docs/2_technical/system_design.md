# Thiết Kế Hệ Thống

## 1. Kiến trúc tổng quan
Hệ thống Cây Gia Phả được thiết kế theo kiến trúc Clean Architecture, phân tách rõ ràng các lớp trách nhiệm:
- **Domain Layer**: Chứa các thực thể (Entities), giá trị đối tượng (Value Objects), và các quy tắc nghiệp vụ cốt lõi.
- **Application Layer**: Chứa các trường hợp sử dụng (Use Cases), lệnh (Commands), truy vấn (Queries), và các giao diện (Interfaces) cho các dịch vụ bên ngoài.
- **Infrastructure Layer**: Chứa các triển khai cụ thể của các giao diện được định nghĩa trong Application Layer, bao gồm truy cập cơ sở dữ liệu (MongoDB), dịch vụ Identity, và các dịch vụ bên ngoài khác.
- **Web Layer (API)**: Điểm vào của ứng dụng, xử lý các yêu cầu HTTP, ánh xạ chúng tới các lệnh/truy vấn trong Application Layer, và trả về phản hồi.

## 2. Sơ đồ kiến trúc (PlantUML)
```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Container.puml

Person(user, "End User", "Người dùng cuối")
System_Boundary(c1, "Hệ thống Cây Gia Phả") {
    Container(spa, "Single-Page App", "Vue.js + Vuetify", "Giao diện người dùng")
    Container(api, "Web API", "ASP.NET Core", "Xử lý logic nghiệp vụ")
    ContainerDb(db, "Database", "MongoDB", "Lưu trữ dữ liệu gia phả")
}

Rel(user, spa, "Sử dụng", "HTTPS")
Rel(spa, api, "Gọi API", "HTTPS/JSON")
Rel(api, db, "Đọc/Ghi dữ liệu", "MongoDB Driver")

@enduml
```

## 3. Sơ đồ Database (MongoDB Schema)
Dữ liệu được lưu trữ trong MongoDB, một cơ sở dữ liệu NoSQL linh hoạt. Dưới đây là thiết kế schema cơ bản cho các collection chính:

### `families` Collection
Lưu trữ thông tin về các dòng họ hoặc gia đình.
```json
{
  "_id": "ObjectId",        // ID duy nhất của dòng họ/gia đình
  "name": "string",         // Tên dòng họ/gia đình (ví dụ: "Nguyễn Gia Tộc")
  "address": "string",      // Địa chỉ hoặc quê quán
  "logoUrl": "string",      // URL đến logo hoặc hình đại diện của dòng họ
  "description": "string",  // Mô tả lịch sử hoặc thông tin khác
  "createdAt": "Date",      // Thời gian tạo bản ghi
  "updatedAt": "Date"       // Thời gian cập nhật gần nhất
}
```
**Indexes:**
- `name`: text index for search
- `address`: text index for search

### `members` Collection
Lưu trữ thông tin chi tiết của từng thành viên.
```json
{
  "_id": "ObjectId",        // ID duy nhất của thành viên
  "familyId": "ObjectId",   // ID của dòng họ/gia đình mà thành viên thuộc về
  "fullName": "string",     // Họ và tên đầy đủ
  "givenName": "string",    // Tên gọi (nếu có)
  ""dob": "Date",           // Ngày sinh
  "dod": "Date",            // Ngày mất (nếu đã mất)
  "status": "string",       // Trạng thái (ví dụ: "alive", "deceased")
  "avatarUrl": "string",    // URL đến ảnh đại diện
  "contact": {              // Thông tin liên lạc
    "email": "string",
    "phone": "string"
  },
  "generation": "number",   // Thế hệ thứ mấy trong dòng họ
  "orderInFamily": "number",// Thứ tự con trong gia đình (ví dụ: con thứ 1, thứ 2)
  "description": "string",  // Mô tả về cuộc đời, sự nghiệp (có thể là rich-text)
  "metadata": "object",     // Các trường dữ liệu mở rộng khác
  "createdAt": "Date",      // Thời gian tạo bản ghi
  "updatedAt": "Date"       // Thời gian cập nhật gần nhất
}
```
**Indexes:**
- `fullName`: text index for search
- `familyId`: ascending index for filtering members by family
- `generation`: ascending index for filtering members by generation
- `contact.email`: ascending index for searching by email

### `relationships` Collection
Lưu trữ các mối quan hệ giữa các thành viên.
```json
{
  "_id": "ObjectId",        // ID duy nhất của mối quan hệ
  "familyId": "ObjectId",   // ID của dòng họ/gia đình mà mối quan hệ thuộc về
  "memberId": "ObjectId",   // ID của thành viên gốc
  "relationType": "string", // Loại quan hệ (ví dụ: "parent", "spouse", "child")
  "targetMemberId": "ObjectId", // ID của thành viên có quan hệ với memberId
  "startDate": "Date",      // Ngày bắt đầu mối quan hệ (ví dụ: ngày kết hôn)
  "endDate": "Date",        // Ngày kết thúc mối quan hệ (ví dụ: ngày ly hôn, ngày mất của vợ/chồng)
  "metadata": "object",     // Các trường dữ liệu mở rộng khác
  "createdAt": "Date",      // Thời gian tạo bản ghi
  "updatedAt": "Date"       // Thời gian cập nhật gần nhất
}
```
**Indexes:**
- `familyId`: ascending index for filtering relationships by family
- `memberId`: ascending index for filtering relationships by member
- `targetMemberId`: ascending index for filtering relationships by target member
- `relationType`: ascending index for filtering relationships by type

## 4. Sơ đồ Sequence (Ví dụ: Tạo thành viên mới)
```plantuml
@startuml
actor User
participant "Frontend (SPA)" as Frontend
participant "Web API (ASP.NET Core)" as API
participant "Application Layer" as Application
participant "Infrastructure Layer" as Infrastructure
database "MongoDB" as DB

User -> Frontend: Nhập thông tin thành viên mới
Frontend -> API: POST /api/members (CreateMemberCommand)
API -> Application: Gửi CreateMemberCommand
Application -> Infrastructure: Gọi IApplicationDbContext.Members.InsertOneAsync()
Infrastructure -> DB: Lưu document thành viên mới
DB --> Infrastructure: Trả về ID thành viên
Infrastructure --> Application: Trả về ID thành viên
Application --> API: Trả về ID thành viên
API --> Frontend: Trả về 201 Created (ID thành viên)
Frontend -> User: Hiển thị thông báo thành công
@enduml
```

## 5. Sơ đồ Component
```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Component.puml

LAYOUT_WITH_LEGEND()

Person(user, "Người dùng cuối", "Người dùng tương tác với hệ thống để quản lý gia phả.")

System_Boundary(family_tree_system, "Hệ thống Cây Gia Phả") {
    Container(spa, "Single-Page App", "Vue.js + Vuetify", "Cung cấp giao diện người dùng.")
    Container(api, "Web API", "ASP.NET Core", "Cung cấp API cho SPA và các client khác.")
    ContainerDb(db, "Database", "MongoDB", "Lưu trữ dữ liệu gia phả và người dùng.")

    Boundary(web_layer, "Web Layer") {
        Component(controllers, "Controllers", "ASP.NET Core MVC", "Xử lý HTTP requests, xác thực và điều hướng.")
    }
    Boundary(app_layer, "Application Layer") {
        Component(mediatr, "MediatR", "Library", "Điều phối Commands/Queries.")
        Component(handlers, "Command/Query Handlers", "C# Classes", "Chứa logic nghiệp vụ chính.")
        Component(interfaces, "Repository Interfaces", "C# Interfaces", "Định nghĩa hợp đồng cho Infrastructure.")
    }
    Boundary(infra_layer, "Infrastructure Layer") {
        Component(mongodb_repo, "MongoDB Repository", "C# Classes", "Triển khai truy cập dữ liệu MongoDB.")
        Component(identity_service, "Identity Service", "C# Classes", "Triển khai quản lý người dùng và JWT.")
    }
}

' Relationships
Rel(user, spa, "Sử dụng qua giao diện web")
Rel(spa, controllers, "Gọi API", "JSON/HTTPS")
Rel(controllers, mediatr, "Gửi Commands/Queries")
Rel(mediatr, handlers, "Điều phối đến")
Rel(handlers, interfaces, "Sử dụng")
Rel(mongodb_repo, db, "Đọc/Ghi dữ liệu")
Rel(identity_service, db, "Đọc/Ghi dữ liệu người dùng")
Rel(mongodb_repo, interfaces, "Triển khai")
Rel(identity_service, interfaces, "Triển khai")
@enduml
```
```plantuml
@startuml
node "Docker Host (Production)" {
  node "Nginx" {
    artifact "Reverse Proxy"
  }
  node "Frontend App (Vue.js)" {
    artifact "SPA"
  }
  node "Backend API (ASP.NET Core)" {
    artifact "Business Logic"
  }
  database "MongoDB" {
    artifact "Data Storage"
  }
}

actor "Người dùng" as user

user --> "SPA"
"SPA" --> "Nginx"
"Nginx" --> "Backend API"
"Backend API" --> "MongoDB"
@enduml

```