Bạn là một senior software architect.  
Repo hiện tại tên là `family-tree-3`, đã scaffold theo cấu trúc Clean Architecture (backend ASP.NET Core, frontend Vue 3 + Vuetify 3, MongoDB, tests, docs).  

👉 Nhiệm vụ: Bổ sung **linting và test coverage (>=80%)** cho repo.  

## 1. Backend (ASP.NET Core)
- **Lint**:  
  - Tích hợp `dotnet format` để check coding style và convention.  
  - Thêm config `.editorconfig` với rule chuẩn C# (naming, spacing, braces).  

- **Test Coverage**:  
  - Sử dụng `coverlet.collector` để đo coverage.  
  - Xuất kết quả coverage ra `lcov.info` trong thư mục `tests/coverage/backend`.  
  - Thêm script kiểm tra coverage >=80% (fail nếu thấp hơn).  

- **Pipeline**:  
  - Cập nhật GitHub Actions CI để chạy:  
    ```bash
    dotnet format --verify-no-changes
    dotnet test /p:CollectCoverage=true /p:CoverletOutput=./tests/coverage/backend/ /p:CoverletOutputFormat=lcov /p:Threshold=80
    ```

## 2. Frontend (Vue.js + Vuetify 3)
- **Lint**:  
  - Thêm ESLint + Prettier với config chuẩn Vue 3 + TypeScript.  
  - Bật rule bắt buộc: indent, quotes, semi, no-unused-vars, vue/no-unused-components.  
  - Tạo script npm:  
    ```json
    "lint": "eslint --ext .js,.ts,.vue src",
    "lint:fix": "eslint --fix --ext .js,.ts,.vue src"
    ```

- **Test Coverage**:  
  - Sử dụng Vitest + @vitest/coverage-v8.  
  - Cấu hình để khi chạy test tạo báo cáo coverage trong `tests/coverage/frontend`.  
  - Coverage threshold: statements, branches, functions, lines ≥80%.  
  - Script npm:  
    ```json
    "test:unit": "vitest run --coverage",
    "test:coverage": "vitest run --coverage --coverage.threshold.statements=80 --coverage.threshold.branches=80 --coverage.threshold.functions=80 --coverage.threshold.lines=80"
    ```

## 3. CI/CD (GitHub Actions)
- Trong file `.github/workflows/ci.yml`:  
  - Job backend: chạy lint + test + coverage ≥80%.  
  - Job frontend: chạy lint + test + coverage ≥80%.  
  - Upload artifact coverage report để dễ theo dõi.  
  - Nếu coverage <80% → pipeline fail.  

## 4. Docs
- Cập nhật `docs/developer_guide.md` với hướng dẫn:  
  ```bash
  # Backend
  dotnet format
  dotnet test /p:CollectCoverage=true /p:CoverletOutput=./tests/coverage/backend/ /p:CoverletOutputFormat=lcov /p:Threshold=80

  # Frontend
  npm run lint
  npm run test:coverage

  Thêm mục Yêu cầu chất lượng trong docs/contribution.md:
Code phải pass lint.
Test coverage >=80% mới được merge vào main.
👉 Hãy scaffold đầy đủ config, script, và pipeline. Repo sau khi chạy npm run lint hoặc dotnet format không được còn lỗi. CI/CD pipeline phải fail nếu coverage <80%.
