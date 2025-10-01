# Hướng dẫn Kiểm thử & QA

Tài liệu này mô tả chiến lược kiểm thử, các loại test, và quy trình đảm bảo chất lượng (QA) của dự án.

## 1. Chiến lược Kiểm thử

Chúng ta áp dụng mô hình kim tự tháp kiểm thử (Test Pyramid):

-   **Unit Tests (Nhiều nhất)**: Kiểm tra các thành phần nhỏ, độc lập (component, service, class).
-   **Integration Tests**: Kiểm tra sự tương tác giữa các thành phần (ví dụ: API và database).
-   **End-to-End (E2E) Tests (Ít nhất)**: Kiểm tra luồng hoạt động hoàn chỉnh từ góc nhìn người dùng.

## 2. Cách chạy các loại test

### 2.1. Backend (.NET)

-   **Chạy tất cả các test**:

    ```bash
    dotnet test backend/backend.sln
    ```

-   **Chạy Unit Tests**:

    ```bash
    dotnet test backend/tests/Application.UnitTests
    ```

-   **Chạy Integration Tests**:

    ```bash
    dotnet test backend/tests/Infrastructure.IntegrationTests
    ```

-   **Chạy Tests với Coverage**:
    ```bash
    dotnet test backend/backend.sln /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=./backend/artifacts/coverage/coverage.cobertura.xml
    ```
    Để xem báo cáo ở định dạng HTML, bạn có thể sử dụng `reportgenerator`:
    ```bash
    reportgenerator "-reports:./backend/artifacts/coverage/coverage.cobertura.xml" "-targetdir:./backend/coverage-report" -reporttypes:Html
    ```

### 2.2. Frontend (Vue)

-   **Chạy Unit Tests**:

    ```bash
    npm run test:unit --prefix frontend
    ```

-   **Chạy và xem code coverage**:

    ```bash
    npm run test:coverage --prefix frontend
    ```

## 3. Quy trình QA

1.  **Developer**: Viết Unit Test cho tất cả các tính năng mới và các bản vá lỗi.
2.  **Pull Request**: GitHub Actions sẽ tự động chạy tất cả các test. Pull request chỉ được merge khi tất cả các test đều pass.
3.  **QA Team**: Trước mỗi release, QA team sẽ thực hiện kiểm thử thủ công (manual testing) theo các kịch bản đã định nghĩa trong `docs/4_testing/TestCases.md`.

## 4. Báo cáo lỗi (Bug Reporting)

-   Sử dụng GitHub Issues để báo cáo lỗi.
-   Mỗi issue cần có:
    -   Tiêu đề rõ ràng.
    -   Các bước để tái hiện lỗi (steps to reproduce).
    -   Kết quả mong đợi và kết quả thực tế.
    -   Ảnh chụp màn hình hoặc video (nếu có).