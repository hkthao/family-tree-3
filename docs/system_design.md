# Thiết Kế Hệ Thống

## 1. Kiến trúc tổng quan
- **Backend**: ASP.NET Core, Clean Architecture
- **Frontend**: Vue.js + Vuetify 3 (SPA)
- **Database**: MongoDB
- **Deployment**: Docker

## 2. Sơ đồ Database (MongoDB Schema)
```json
{
  "families": [
    {
      "_id": "ObjectId",
      "name": "string",
      "address": "string",
      "logo": "string",
      "history": "string",
      "members": ["ObjectId"]
    }
  ],
  "members": [
    {
      "_id": "ObjectId",
      "fullName": "string",
      "dateOfBirth": "Date",
      "dateOfDeath": "Date",
      "status": "string", // (e.g., "alive", "deceased")
      "contact": {
        "phone": "string",
        "email": "string"
      },
      "generation": "number",
      "displayOrder": "number",
      "familyId": "ObjectId",
      "description": "string"
    }
  ],
  "relationships": [
    {
      "_id": "ObjectId",
      "memberId": "ObjectId",
      "type": "string", // "parent", "spouse", "child"
      "targetId": "ObjectId"
    }
  ]
}
```

## 3. Sơ đồ kiến trúc (PlantUML)
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
Rel(api, db, "Đọc/Ghi dữ liệu", "TCP")

@enduml
```
