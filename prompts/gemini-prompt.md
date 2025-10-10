Bạn là một senior software architect.
Repo hiện tại tên là `family-tree-3`, tôi đã clone về và tạo sẵn thư mục `docs/` với 2 file: `ideal.md` và `requirements.md`.
Hãy scaffold và bổ sung repo theo các yêu cầu sau. Ghi chú: **Tuyệt đối không tạo file .bicep hoặc các file IaC Azure**. Tập trung vào scaffold code, docs (tiếng Việt), branch strategy và CI đơn giản.

1. Cấu trúc thư mục repo
- /backend → ASP.NET Core API theo Clean Architecture (API / Application / Domain / Infrastructure)
- /frontend → Vue.js + Vuetify 3 (SPA, responsive, i18n)
- /docs → chứa tài liệu (đã có ideal.md, requirements.md; bổ sung file mới bên dưới)
- /tests → chứa unit test & integration test
- /infra → Dockerfile, docker-compose.yml, CI/CD config (GitHub Actions pipeline)

2. Backend (ASP.NET Core)
- Sinh boilerplate project ASP.NET Core theo Clean Architecture (layers: API / Application / Domain / Infrastructure).
- Tích hợp Swagger/OpenAPI với mô tả bằng tiếng Việt (có options để xuất OpenAPI JSON).
- Thêm authentication: JWT (basic), support role-based Authorization (roles: Admin, Member, Guest).
- API cơ bản implement (controllers + DTOs + service layer + repository pattern):
  * /families (CRUD dòng họ/gia đình)
  * /members (CRUD thành viên + tìm kiếm theo tên, theo thế hệ, filter theo familyId)
  * /relationships (quản lý quan hệ cha/mẹ/vợ/chồng/con, validate tránh vòng lặp)
  * /familytree (endpoint xuất cây gia phả dạng JSON; export PDF/PNG via server-side render script hoặc báo hướng dẫn)
- Tích hợp validation (FluentValidation), logging (Serilog), global error handling middleware.
- Viết ví dụ Unit test với xUnit cho service layer và mock repository.
- Tạo Postman collection sample trong /docs hoặc OpenAPI exported file.

3. Frontend (Vue.js + Vuetify 3)
- Tạo dự án Vue 3 + Vuetify 3, cấu hình Vue Router và Pinia (store).
- Pages/components:
  - Dashboard
  - Quản lý Dòng họ/Gia đình (Families)
  - Quản lý Thành viên (Members) – form tạo/sửa, trường rich-text cho mô tả
  - Quản lý Quan hệ (Relationships)
  - Cây gia phả (Family Tree) – component tree graph có:
      * zoom, pan (kéo), search, lọc theo thế hệ/nhánh
      * hiển thị avatar, họ tên, năm sinh–năm mất
      * xuất ảnh/PDF client-side (hướng dẫn hoặc button dùng html2canvas / print)
- i18n: mặc định tiếng Việt, support toggling English (phần text guide tiếng Việt; labels kỹ thuật vẫn giữ thuật ngữ Anh kèm chú thích).
- Thêm ví dụ unit/component test với Vitest + testing-library.
- Lint + Prettier config.

4. Database (MongoDB)
- Thiết kế documents cơ bản:
  * families: { _id, name, address, logoUrl, description, createdAt, updatedAt }
  * members: { _id, familyId, fullname, givenName, dob, dod, status, avatarUrl, contact: {email, phone}, generation, orderInFamily, description, metadata }
  * relationships: { _id, familyId, memberId, relationType (parent|spouse|child), targetMemberId, startDate, endDate, metadata }
- Thiết kế index: fullname, familyId, generation, contact.email
- Tạo script seed data mẫu (1 family + 10 members demo) dưới /infra/seeds.
- Hướng dẫn migration (nếu cần) hoặc versioning schema notes trong docs.

5. Triển khai & DevOps
- Dockerfile cho backend (multi-stage) và frontend.
- docker-compose.yml dev stack (api, ui, mongodb, mongo-express optional).
- GitHub Actions workflows:
  - backend-ci.yml: build, test, publish artifacts (với .NET 8)
  - frontend-ci.yml: install, build, test, lint
  - full-stack workflow optional: docker build & push to registry
- README / docs giải thích cách chạy local bằng docker-compose.
- **KHÔNG TẠO .bicep / IaC Azure.** Nếu user cần IaC sau này thì sẽ yêu cầu riêng.

6. Tài liệu (tất cả viết bằng tiếng Việt, giữ thuật ngữ kỹ thuật Anh kèm chú thích)
- Bổ sung các file sau trong /docs:
  * system_design.md – kiến trúc hệ thống (Clean Architecture), sơ đồ component, sequence, deployment (PlantUML snippets), database schema.
  * api_design.md – API contract + sample request/response + OpenAPI JSON link.
  * user_guide.md – hướng dẫn end-user (login, tạo dòng họ, thêm thành viên, vẽ xuất cây).
  * developer_guide.md – hướng dẫn dev: setup local, environment variables, chạy docker-compose, run tests, code style, PR checklist, branch strategy.
  * contribution.md – quy trình đóng góp: branch naming, commit message convention, pull request template, code review checklist.
- Tạo Postman/Insomnia collection và link trong api_design.md.
- Tạo CHANGELOG.md template.

7. Quản lý công việc
- Sinh file /docs/backlog_sample.yaml chứa danh sách User Story + Acceptance Criteria (Given/When/Then). Tối thiểu 10 story: basic CRUD families, CRUD members, create relationship, build family tree view, export PDF, auth flows, user roles, tests, docs tasks.
- Mỗi user story theo format:
  As a [role], I want [feature], so that [benefit].
  Acceptance criteria: Given / When / Then.

8. Branch strategy (tạo sẵn các branch)
- Tạo các branch khởi tạo:
  - main (protected)
  - develop
  - docs/init (nơi để cập nhật files docs ban đầu)
- Quy tắc branch:
  - feature/<ten-tinh-nang>
  - bugfix/<ten-loi>
  - docs/<ten-tai-lieu>
  - hotfix/<ten>
- Tất cả merge vào main phải qua Pull Request. (Nếu repo là solo dev, không yêu cầu approvals, nhưng vẫn enforce PR + status checks).

9. Output mong muốn từ Gemini
- Tạo đầy đủ cấu trúc thư mục như trên.
- Sinh boilerplate code backend + frontend (vừa đủ để chạy dev bằng docker-compose).
- Tạo file docs mẫu (tiếng Việt) với khung nội dung, PlantUML snippets, API contract stub.
- Tạo /docs/backlog_sample.yaml với user stories + acceptance criteria.
- Tạo Git branches (main, develop, docs/init) và commit các file tương ứng.
- Tạo workflows GitHub Actions mẫu trong .github/workflows/.
- Tạo seed data script và ví dụ unit tests.
- KHÔNG sinh file .bicep hoặc file IaC cho Azure.

Hãy thực hiện scaffold, commit và tạo branchs theo yêu cầu. Nếu có bước nào Gemini không chắc chắn (ví dụ: export PDF server-side implementation), hãy tạo TODO notes trong docs bằng tiếng Việt với đề xuất giải pháp.
