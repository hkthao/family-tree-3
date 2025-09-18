# Hướng dẫn thiết lập và chạy Code Coverage cho Backend

Hướng dẫn này mô tả cách thiết lập, chạy và xem báo cáo code coverage cho các dịch vụ backend của dự án `family-tree-3`, cũng như cách tích hợp nó vào quy trình CI/CD bằng GitHub Actions.

## 1. Yêu cầu

Đảm bảo rằng các package sau đã được thêm vào các project test của backend:
*   `coverlet.msbuild`: Để thu thập dữ liệu coverage trong quá trình chạy test.
*   `dotnet-reportgenerator-globaltool`: Để tổng hợp và tạo báo cáo coverage.

Chúng ta đã thêm `coverlet.msbuild` vào các project test sau:
*   `backend/tests/Application.UnitTests/Application.UnitTests.csproj`
*   `backend/tests/Domain.UnitTests/Domain.UnitTests.csproj`
*   `backend/tests/Infrastructure.IntegrationTests/Infrastructure.IntegrationTests.csproj`

## 2. Chạy Test và Thu thập Coverage (Cục bộ)

Để chạy test và thu thập dữ liệu coverage cục bộ, hãy điều hướng đến thư mục gốc của dự án và chạy lệnh sau:

```bash
dotnet test backend/backend.sln /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:Threshold=0
```

*   **Giải thích:**
    *   `/p:CollectCoverage=true`: Kích hoạt việc thu thập dữ liệu coverage.
    *   `/p:CoverletOutputFormat=cobertura`: Xuất kết quả ra file `coverage.cobertura.xml` trong thư mục `backend/artifacts/coverage/`.
    *   `/p:Threshold=0`: Tạm thời vô hiệu hóa ngưỡng coverage để test không bị fail nếu coverage thấp.

Lệnh này sẽ tạo ra file `coverage.cobertura.xml` chứa dữ liệu coverage.

## 3. Tổng hợp và Xem kết quả (Cục bộ)

### Bước 1: Cài đặt `reportgenerator` (Nếu chưa có)

Nếu bạn chưa cài đặt `reportgenerator` như một công cụ cục bộ, hãy chạy lệnh sau từ thư mục gốc của dự án:

```bash
dotnet tool install dotnet-reportgenerator-globaltool
```

### Bước 2: Sinh Báo cáo Tóm tắt trên Console

Để xem kết quả tóm tắt trên console, chạy lệnh sau từ thư mục gốc của dự án:

```bash
dotnet tool run reportgenerator "-reports:backend/artifacts/coverage/coverage.cobertura.xml" "-targetdir:backend/coverage-report" "-reporttypes:TextSummary"
```

Kết quả sẽ là một bảng tóm tắt line coverage tổng hợp.

### Bước 3: Sinh Báo cáo HTML chi tiết

Để xem báo cáo chi tiết từng dòng code, chạy lệnh sau từ thư mục gốc của dự án:

```bash
dotnet tool run reportgenerator "-reports:backend/artifacts/coverage/coverage.cobertura.xml" "-targetdir:backend/coverage-report" -reporttypes:Html
```

Sau khi chạy lệnh này, bạn có thể mở tệp `backend/coverage-report/index.html` trong trình duyệt web của mình để xem báo cáo chi tiết.

## 4. Tích hợp vào GitHub Actions

Quy trình CI/CD đã được cập nhật để tự động thu thập và báo cáo code coverage. Các thay đổi được thực hiện trong tệp `.github/workflows/ci.yml`.

Bước `Test Backend` trong workflow đã được sửa đổi để:
*   Chạy test và thu thập coverage ở định dạng `cobertura`.
*   Lưu kết quả vào `backend/artifacts/coverage/coverage.cobertura.xml`.

Các bước mới đã được thêm vào sau bước `Test Backend` để:
*   Cài đặt `reportgenerator`.
*   Tạo báo cáo HTML chi tiết và lưu vào `backend/coverage-report/`.
*   Upload báo cáo HTML này như một artifact của workflow, có thể tải xuống từ trang kết quả của GitHub Actions.

Điều này đảm bảo rằng mỗi khi code được push hoặc pull request được tạo, báo cáo coverage sẽ được tự động tạo và có sẵn để xem xét.
