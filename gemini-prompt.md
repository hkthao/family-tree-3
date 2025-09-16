Bạn là một senior software architect.  
Repo hiện tại tên là `family-tree-3`, tôi đã clone về và tạo sẵn thư mục `docs/` với 2 file: `ideal.md` và `requirements.md`.  
Hãy scaffold và bổ sung repo theo các yêu cầu sau:  

1. **Cấu trúc thư mục repo**  
   - /backend → ASP.NET Core API theo Clean Architecture (API / Application / Domain / Infrastructure)  
   - /frontend → Vue.js + Vuetify 3 (SPA, responsive, i18n)  
   - /docs → chứa tài liệu (đã có ideal.md, requirements.md, bổ sung thêm system_design.md, api_design.md, user_guide.md, developer_guide.md, contribution.md)  
   - /tests → chứa unit test & integration test  
   - /infra → Dockerfile, docker-compose.yml, CI/CD config (GitHub Actions pipeline)  

2. **Backend (ASP.NET Core)**  
   - Sinh boilerplate project ASP.NET Core với Clean Architecture.  
   - Tích hợp Swagger (mô tả tiếng Việt).  
   - Thêm JWT Authentication, Role-based Authorization.  
   - API cơ bản:  
     * /families (CRUD dòng họ/gia đình)  
     * /members (CRUD thành viên + tìm kiếm theo tên/thế hệ)  
     * /relationships (quản lý quan hệ cha/mẹ/vợ/chồng/con)  
     * /familytree (xuất cây gia phả JSON/PDF)  
   - Viết ví dụ Unit test với xUnit cho service layer.  

3. **Frontend (Vue.js + Vuetify 3)**  
   - Tạo dự án Vue 3 với Vuetify, Vue Router, Pinia.  
   - Trang chính: Dashboard, Quản lý gia đình, Quản lý thành viên, Quan hệ, Cây gia phả.  
   - Component cây gia phả (tree graph) có zoom, kéo, lọc.  
   - i18n: tiếng Việt mặc định, tùy chọn tiếng Anh.  
   - Thêm ví dụ component test với Vitest.  

4. **Database (MongoDB)**  
   - Thiết kế schema:  
     * families: { name, address, logo, members[] }  
     * members: { fullname, dob, dod, status, contact, generation, order, familyId, description }  
     * relationships: { memberId, type (parent/spouse/child), targetId }  
   - Tạo migration/seed data mẫu.  

5. **Triển khai & DevOps**  
   - Dockerfile cho backend + frontend.  
   - docker-compose.yml để chạy full stack (API + UI + MongoDB).  
   - GitHub Actions pipeline: build, test, lint, docker build & push.  

6. **Tài liệu (docs/)**  
   - Bổ sung các file sau (tiếng Việt):  
     * system_design.md – kiến trúc, sơ đồ database, UML/PlantUML.  
     * api_design.md – API contract (OpenAPI/Swagger JSON).  
     * user_guide.md – hướng dẫn end-user (có ví dụ + hình).  
     * developer_guide.md – hướng dẫn dev setup, CI/CD pipeline.  
     * contribution.md – quy trình pull request, branch naming, code review checklist.  
   - Tất cả viết **bằng tiếng Việt**, giữ nguyên thuật ngữ kỹ thuật tiếng Anh (ví dụ: JWT, CI/CD).  

7. **Quản lý công việc**  
   - Sinh file backlog_sample.yaml trong thư mục /docs với danh sách User Story + Acceptance Criteria (Given/When/Then).  
   - User story theo format:  
     `As a [role], I want [feature], so that [benefit]`.  

👉 Hãy sinh đầy đủ cấu trúc thư mục, boilerplate code, file docs rỗng hoặc có khung nội dung, backlog/story mẫu.  
👉 Mục tiêu: repo sẵn sàng cho team dev bắt đầu code và mở Kanban board.  
