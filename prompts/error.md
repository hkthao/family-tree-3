# Hướng dẫn chạy Test Coverage cho Customer Service

Chào bạn, với vai trò là một kỹ sư DevOps, tôi sẽ hướng dẫn bạn cách chạy và xem kết quả test coverage tổng hợp cho project `customer-service`.

## 1. Yêu cầu

Đảm bảo rằng bạn đã thêm package `coverlet.msbuild` vào **tất cả các project test** (`CustomerService.UnitTests` và `CustomerService.IntegrationTests`).

```bash
dotnet add <path-to-test-project.csproj> package coverlet.msbuild
```

## 2. Chạy Test và Thu thập Coverage

Để có được kết quả tổng hợp chính xác, chúng ta cần chạy test và yêu cầu `coverlet` xuất ra file báo cáo theo định dạng `cobertura`.

Chạy lệnh sau từ thư mục `services/customer-service`:

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

*   **Giải thích:**
    *   `/p:CollectCoverage=true`: Kích hoạt việc thu thập dữ liệu coverage.
    *   `/p:CoverletOutputFormat=cobertura`: Xuất kết quả ra file `coverage.cobertura.xml` trong thư mục của mỗi project test. Đây là định dạng chuẩn để các tool khác có thể đọc được.

Lệnh này sẽ không hiển thị bảng tóm tắt ngay lập tức, nhưng nó tạo ra các file cần thiết cho bước tiếp theo.

## 3. Tổng hợp và Xem kết quả trên Console

Để xem kết quả tổng hợp từ các file `cobertura` đã tạo, chúng ta sẽ dùng `reportgenerator`.

### Bước 1: Cài đặt `reportgenerator` (Nếu chưa có)

Cài đặt `reportgenerator` như một công cụ cục bộ (local tool) cho project. Chạy các lệnh sau từ thư mục `services/customer-service`:

```bash
# Tạo file manifest nếu chưa có
dotnet new tool-manifest --force

# Cài đặt tool
dotnet tool install dotnet-reportgenerator-globaltool
```

### Bước 2: Sinh Báo cáo Tóm tắt trên Console

Chạy lệnh sau từ thư mục `services/customer-service`:

```bash
dotnet tool run reportgenerator "-reports:tests/**/coverage.cobertura.xml" "-targetdir:coverage-report" "-reporttypes:TextSummary"
```

*   **Giải thích:**
    *   `"-reports:tests/**/coverage.cobertura.xml"`: Tìm tất cả các file coverage trong các project test.
    *   `"-reporttypes:TextSummary"`: Yêu cầu `reportgenerator` in ra một bảng tóm tắt đơn giản trên console.

Kết quả sẽ là một bảng tóm tắt **line coverage tổng hợp** cho toàn bộ `customer-service`.

## 4. (Tùy chọn) Sinh Report HTML chi tiết

Nếu muốn xem chi tiết từng dòng code, bạn chỉ cần đổi `-reporttypes` thành `Html`:

```bash
dotnet tool run reportgenerator "-reports:tests/**/coverage.cobertura.xml" "-targetdir:coverage-report" -reporttypes:Html
```

Sau đó, mở tệp `index.html` trong thư mục `coverage-report` để xem.

## 3. Tích hợp vào GitHub Actions

Quy trình CI/CD của chúng ta đã được cập nhật để sử dụng phương pháp `coverlet.msbuild`.

Trong tệp `.github/workflows/ci-cd.yml`, bước `test-dotnet` sẽ chạy lệnh sau cho `customer-service`:

```yaml
- name: Test
  run: |
    if [ "${{ matrix.service }}" == "customer-service" ]; then
      dotnet test services/customer-service/CustomerService.sln --no-build /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
    else
      dotnet test services/${{ matrix.service }}/${{ matrix.service }}.csproj --no-restore
    fi
```

Điều này đảm bảo rằng mỗi khi code được push, file `coverage.cobertura.xml` sẽ được tạo ra và có thể được sử dụng bởi các tool phân tích khác trong pipeline.