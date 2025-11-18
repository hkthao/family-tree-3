# Hướng dẫn Phát triển Backend

Tài liệu này cung cấp hướng dẫn chi tiết về cách thiết lập, phát triển và duy trì phần backend của ứng dụng Family Tree.

## 1. Tổng quan

Backend của ứng dụng Family Tree đóng vai trò là trái tim của hệ thống, chịu trách nhiệm cung cấp các API cho frontend, xử lý logic nghiệp vụ phức tạp, quản lý dữ liệu, xác thực và ủy quyền người dùng. Mục tiêu chính là đảm bảo dữ liệu nhất quán, an toàn và hiệu suất cao.

Backend được xây dựng trên nền tảng .NET 8, tuân thủ kiến trúc Clean Architecture để đảm bảo tính mở rộng, dễ bảo trì và kiểm thử. Các công nghệ và mẫu thiết kế chính được sử dụng bao gồm:

*   **Framework:** ASP.NET 8
*   **Kiến trúc:** Clean Architecture
*   **Ngôn ngữ:** C#
*   **Mẫu thiết kế:** CQRS (Command Query Responsibility Segregation) với MediatR để tách biệt các thao tác đọc và ghi, giúp quản lý logic nghiệp vụ rõ ràng hơn.
*   **ORM:** Entity Framework Core để tương tác với cơ sở dữ liệu.
*   **Cơ sở dữ liệu:** MySQL
*   **Xác thực:** JWT Authentication

### Cấu trúc thư mục `src/backend/`

Dự án backend được tổ chức theo Clean Architecture, với các lớp chính được ánh xạ vào các thư mục tương ứng:

*   **`src/Domain`**: Chứa các thực thể (Entities), giá trị đối tượng (Value Objects), enum, ngoại lệ (Exceptions), giao diện (Interfaces) và các quy tắc nghiệp vụ cốt lõi. Đây là trái tim của ứng dụng, độc lập với các lớp khác.
*   **`src/Application`**: Chứa logic nghiệp vụ của ứng dụng, bao gồm các Command, Query, Handler, DTOs và các dịch vụ ứng dụng (Application Services). Lớp này phụ thuộc vào `Domain`.
*   **`src/Infrastructure`**: Chứa các chi tiết triển khai như truy cập dữ liệu (Entity Framework Core), dịch vụ bên ngoài (ví dụ: gửi email, lưu trữ tệp), và các dịch vụ khác phụ thuộc vào công nghệ cụ thể. Lớp này phụ thuộc vào `Application` và `Domain`.
*   **`src/Web`**: Điểm vào của ứng dụng, chứa các API Controllers, cấu hình xác thực, và các cấu hình liên quan đến HTTP. Lớp này phụ thuộc vào tất cả các lớp khác.
*   **`src/CompositionRoot`**: Quản lý việc đăng ký các dependency và cấu hình IoC container.

## 2. Cài đặt và Chạy

### Yêu cầu

*   .NET 8 SDK
*   Docker & Docker Compose (để chạy toàn bộ ứng dụng)

### Chạy cục bộ (Local)

1.  **Điều hướng đến thư mục backend:**
    ```bash
    cd src/backend
    ```
2.  **Cập nhật các gói NuGet:**
    ```bash
    dotnet restore
    ```
3.  **Cấu hình chuỗi kết nối cơ sở dữ liệu:**
    Mở tệp `src/Web/appsettings.json` và đảm bảo chuỗi kết nối `DefaultConnection` trỏ đến máy chủ MySQL của bạn.
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=localhost;Port=3306;Database=familytree;Uid=root;Pwd=password;"
    }
    ```
    Nếu bạn đang sử dụng Docker Compose, chuỗi kết nối sẽ là `Server=mysql;Port=3306;Database=familytree;Uid=root;Pwd=password;`.
4.  **Chạy Migrations để tạo cơ sở dữ liệu:**
    ```bash
    dotnet ef database update --project src/Infrastructure --startup-project src/Web
    ```
5.  **Chạy ứng dụng:**
    ```bash
    dotnet run --project src/Web
    ```
    API sẽ khả dụng tại `http://localhost:8080` và Swagger UI tại `http://localhost:8080/swagger`.

### Chạy với Docker Compose

Để chạy toàn bộ ứng dụng (backend, frontend, database) bằng Docker Compose, hãy làm theo hướng dẫn trong `README.md` chính của dự án.

```bash
cd ../.. # Quay về thư mục gốc của dự án
docker-compose -f infra/docker-compose.yml up --build
```

## 3. Quy trình Phát triển Tính năng

1.  **Tạo nhánh mới:** Luôn làm việc trên một nhánh tính năng mới (ví dụ: `feature/ten-tinh-nang-moi`).
2.  **Phát triển:**
    *   Thêm hoặc sửa đổi các thực thể trong `Domain`.
    *   Triển khai logic nghiệp vụ trong `Application` (Commands, Queries, Handlers).
    *   Cập nhật hoặc thêm các triển khai cơ sở dữ liệu/dịch vụ trong `Infrastructure`.
    *   Tạo hoặc sửa đổi các API Controller trong `Web` để lộ ra các chức năng mới.
3.  **Đảm bảo tuân thủ Code Style:**
    ```bash
    dotnet format
    ```
4.  **Viết và chạy Unit Tests:** Xem phần 5. Kiểm thử.
5.  **Tạo Pull Request:** Khi hoàn thành, tạo một Pull Request lên nhánh `develop`.

## 4. Kiểm thử

Dự án backend bao gồm các Unit Tests và Integration Tests để đảm bảo chất lượng mã và hành vi đúng đắn của ứng dụng.

### 5.1. Cấu trúc Test

*   **`tests/Application.UnitTests`**: Chứa các unit tests cho lớp `Application`, tập trung vào logic nghiệp vụ của các MediatR Handlers (Commands, Queries) và các dịch vụ ứng dụng.
*   **`tests/Domain.UnitTests`**: Chứa các unit tests cho lớp `Domain`, tập trung vào các thực thể, giá trị đối tượng và quy tắc nghiệp vụ cốt lõi.
*   **`tests/Web.IntegrationTests` (ví dụ)**: Có thể thêm dự án này để chứa các integration tests cho các API Controllers, đảm bảo toàn bộ luồng yêu cầu-phản hồi hoạt động chính xác.

### 5.2. Chạy Tests

#### Chạy cục bộ (Local)

1.  **Điều hướng đến thư mục backend:**
    ```bash
    cd src/backend
    ```
2.  **Chạy tất cả các tests:**
    ```bash
    dotnet test
    ```
3.  **Chạy tests với độ bao phủ mã (Code Coverage):**
    ```bash
    dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
    ```
    Kết quả độ bao phủ mã sẽ được tạo ra trong thư mục `tests/`.

#### Chạy trong CI

Trong môi trường CI (GitHub Actions), các tests được chạy tự động như một phần của job `build-and-test` trong workflow `ci.yml`. Cụ thể, lệnh `dotnet test src/backend/backend.sln --no-build --verbosity normal` sẽ được thực thi để chạy tất cả các tests của backend.

### 5.3. Hướng dẫn Viết Tests

*   **Nguyên tắc chung:**
    *   Sử dụng các framework kiểm thử như NUnit, Moq (để tạo mock đối tượng).
    *   Tham khảo các bài kiểm thử hiện có trong thư mục `tests/` để hiểu các quy ước và phong cách.
    *   Sử dụng `TestBase` để tránh trùng lặp mã trong các bài kiểm thử và thiết lập môi trường kiểm thử chung.
    *   Mỗi test case nên tập trung vào một kịch bản cụ thể (Arrange-Act-Assert).

*   **Kiểm thử MediatR Handlers (Commands/Queries)**:
    *   **Mục tiêu**: Đảm bảo logic nghiệp vụ bên trong handler hoạt động chính xác.
    *   **Cách tiếp cận**: Mock tất cả các dependency mà handler sử dụng (ví dụ: `IRepository`, `ICurrentUserService`, `IDateTime`) bằng Moq.
    *   **Ví dụ**: Khi kiểm thử một `CreateFamilyCommand.Handler`, bạn sẽ mock `IFamilyRepository` để kiểm tra xem phương thức `AddAsync` có được gọi với đúng đối tượng `Family` hay không, và handler có trả về `Result.Success()` hay không.

*   **Kiểm thử Repositories**:
    *   **Mục tiêu**: Đảm bảo các thao tác truy cập dữ liệu (CRUD) hoạt động đúng với cơ sở dữ liệu.
    *   **Cách tiếp cận**: Sử dụng cơ sở dữ liệu trong bộ nhớ (in-memory database) như SQLite in-memory với Entity Framework Core để mô phỏng môi trường cơ sở dữ liệu thật mà không cần phụ thuộc vào một máy chủ DB vật lý. Hoặc có thể mock `DbContext` nếu chỉ muốn kiểm thử logic ánh xạ.
    *   **Ví dụ**: Khi kiểm thử `FamilyRepository`, bạn sẽ tạo một `DbContext` trong bộ nhớ, thêm dữ liệu mẫu, sau đó gọi các phương thức của repository (ví dụ: `GetByIdAsync`, `AddAsync`) và xác minh kết quả.

*   **Kiểm thử Controllers**:
    *   **Mục tiêu**: Đảm bảo các endpoint API hoạt động đúng, bao gồm routing, xác thực, ủy quyền, và định dạng phản hồi.
    *   **Cách tiếp cận**: Sử dụng `WebApplicationFactory<Program>` để khởi tạo một test server trong bộ nhớ. Mock các dịch vụ tầng `Application` (ví dụ: `IMediator`) để cô lập logic của controller. Gửi các HTTP request đến controller và kiểm tra mã trạng thái HTTP, tiêu đề và nội dung phản hồi.
    *   **Ví dụ**: Khi kiểm thử `FamiliesController`, bạn sẽ gửi một `GET` request đến `/api/families`, mock `IMediator.Send()` để trả về danh sách gia đình mẫu, và xác minh rằng phản hồi HTTP là `200 OK` với dữ liệu JSON mong muốn.


## 5. CI/CD và Docker

Dự án sử dụng GitHub Actions cho quy trình CI/CD để tự động hóa việc kiểm tra, xây dựng và đóng gói ứng dụng.

### 5.1. Workflow GitHub Actions (`.github/workflows/ci.yml`)

Workflow `ci.yml` được kích hoạt khi có `push` lên nhánh `main` hoặc khi có `pull_request` nhắm vào nhánh `main`. Nó bao gồm hai job chính:

*   **`build-and-test`**:
    *   **Thiết lập môi trường**: Cài đặt .NET 8 và Node.js 20.
    *   **Backend**:
        *   Khôi phục các gói NuGet (`dotnet restore`).
        *   Xây dựng dự án backend (`dotnet build`).
        *   Kiểm tra định dạng mã nguồn (`dotnet format --verify-no-changes`).
        *   Chạy tất cả các unit tests của backend (`dotnet test`).
    *   **Frontend**:
        *   Cài đặt các phụ thuộc Node.js (`npm install`).
        *   Kiểm tra lỗi cú pháp và phong cách mã nguồn (`npm run lint`).
        *   Chạy các unit tests của frontend và kiểm tra độ bao phủ mã (`npm run test:coverage`).

*   **`docker-build`**:
    *   Job này phụ thuộc vào `build-and-test` (chỉ chạy khi `build-and-test` thành công).
    *   **Xây dựng Docker Image Backend**: Sử dụng `infra/Dockerfile.backend` để xây dựng image Docker cho backend. Image này được gắn thẻ `hkthao/family-tree-backend:latest`.
    *   **Xây dựng Docker Image Frontend**: Sử dụng `infra/Dockerfile.frontend` để xây dựng image Docker cho frontend. Image này được gắn thẻ `hkthao/family-tree-frontend:latest`.
    *   Các image Docker sau khi được xây dựng sẽ được tải lên dưới dạng artifact.

### 5.2. Mô tả Docker Images

Dự án sử dụng Docker để đóng gói ứng dụng, đảm bảo môi trường nhất quán giữa các môi trường phát triển, kiểm thử và triển khai.

*   **Backend (`infra/Dockerfile.backend`)**:
    *   Sử dụng quy trình build đa tầng (multi-stage build).
    *   **Giai đoạn build**: Sử dụng `mcr.microsoft.com/dotnet/sdk:8.0` để khôi phục phụ thuộc, xây dựng và publish ứng dụng .NET.
    *   **Giai đoạn runtime**: Sử dụng `mcr.microsoft.com/dotnet/aspnet:8.0` làm base image cuối cùng, chỉ chứa các thư viện cần thiết để chạy ứng dụng, giúp giảm kích thước image.
    *   Ứng dụng backend được publish vào thư mục `/app` trong container.

*   **Frontend (`infra/Dockerfile.frontend`)**:
    *   Sử dụng quy trình build đa tầng.
    *   **Giai đoạn build**: Sử dụng `node:20-alpine` để cài đặt phụ thuộc và xây dựng ứng dụng Vue.js bằng Vite.
    *   **Giai đoạn phục vụ (serve)**: Sử dụng `nginx:alpine` làm base image cuối cùng. Các tệp tĩnh của ứng dụng frontend đã được build sẽ được sao chép vào thư mục phục vụ của Nginx (`/usr/share/nginx/html`), và Nginx sẽ chịu trách nhiệm phục vụ ứng dụng.

### 5.3. Kiểm thử Local Build giống như CI

Để đảm bảo rằng các thay đổi của bạn sẽ vượt qua CI, bạn có thể chạy các bước kiểm tra tương tự cục bộ:

*   **Kiểm tra Backend**:
    ```bash
    cd src/backend
    dotnet restore
    dotnet build
    dotnet format --verify-no-changes
    dotnet test
    ```
*   **Kiểm tra Frontend**:
    ```bash
    cd src/frontend
    npm install
    npm run lint
    npm run test:coverage
    ```
*   **Xây dựng Docker Images cục bộ**:
    ```bash
    docker build -f infra/Dockerfile.backend -t hkthao/family-tree-backend:latest src/backend
    docker build -f infra/Dockerfile.frontend -t hkthao/family-tree-frontend:latest src/frontend
    ```
    Các lệnh này sẽ xây dựng các image Docker tương tự như cách CI thực hiện, giúp bạn phát hiện sớm các vấn đề liên quan đến Dockerfile.


## 6. Cấu hình và Bảo mật

Việc quản lý cấu hình và đảm bảo bảo mật là rất quan trọng đối với bất kỳ ứng dụng nào. Phần này mô tả cách backend của Family Tree xử lý các khía cạnh này.

### 6.1. Biến môi trường và Tệp cấu hình

*   **`appsettings.json`**: Đây là tệp cấu hình chính của ứng dụng ASP.NET Core. Nó chứa các cài đặt chung, chuỗi kết nối cơ sở dữ liệu mặc định, và các cấu hình khác không nhạy cảm. Các tệp như `appsettings.Development.json` hoặc `appsettings.Production.json` có thể được sử dụng để ghi đè các cài đặt tùy thuộc vào môi trường chạy ứng dụng.
*   **Biến môi trường**: Đối với các thông tin nhạy cảm (ví dụ: mật khẩu cơ sở dữ liệu, khóa API) hoặc các cài đặt cụ thể cho từng môi trường triển khai, chúng ta sử dụng biến môi trường. Khi chạy cục bộ với Docker Compose, các biến môi trường được định nghĩa trực tiếp trong tệp `infra/docker-compose.yml`. Trong môi trường Docker, các biến môi trường được truyền vào container.
*   **`infra/docker-compose.yml`**: Đây là nơi chính để định nghĩa các biến môi trường cho môi trường phát triển cục bộ khi sử dụng Docker Compose. Các biến này sẽ được truyền vào các service (backend, database, v.v.) khi container được khởi tạo.

### 6.2. Secrets trong GitHub Actions

Các thông tin nhạy cảm (ví dụ: Docker Hub credentials, khóa bí mật JWT) được sử dụng trong quy trình CI/CD của GitHub Actions được lưu trữ an toàn dưới dạng GitHub Secrets. Điều này đảm bảo rằng các thông tin này không bao giờ được commit trực tiếp vào mã nguồn và chỉ có thể được truy cập bởi các workflow được ủy quyền.

### 6.3. Nguyên tắc Bảo mật

*   **JWT (JSON Web Tokens)**:
    *   Được sử dụng để xác thực và ủy quyền người dùng. Sau khi người dùng đăng nhập, backend sẽ tạo một JWT và gửi về cho frontend. Frontend sẽ đính kèm JWT này vào các yêu cầu API tiếp theo để chứng minh danh tính và quyền hạn của người dùng.
    *   JWT được ký bằng một khóa bí mật trên server để đảm bảo tính toàn vẹn và xác thực.
*   **CORS (Cross-Origin Resource Sharing)**:
    *   Backend được cấu hình để cho phép các yêu cầu từ các nguồn gốc (origins) được phép (ví dụ: địa chỉ của frontend). Điều này ngăn chặn các cuộc tấn công Cross-Site Request Forgery (CSRF) và đảm bảo rằng chỉ các ứng dụng frontend hợp lệ mới có thể tương tác với API.
*   **HTTPS**: Tất cả các giao tiếp giữa client và server nên được mã hóa bằng HTTPS để bảo vệ dữ liệu khỏi bị nghe lén và giả mạo. Trong môi trường sản phẩm, Nginx (hoặc một reverse proxy khác) sẽ được cấu hình để xử lý HTTPS.
*   **Xử lý lỗi và Logging**: Các lỗi được xử lý một cách an toàn để tránh tiết lộ thông tin nhạy cảm cho người dùng cuối. Logging được sử dụng để ghi lại các sự kiện quan trọng và lỗi, hỗ trợ việc giám sát và phát hiện các vấn đề bảo mật.
*   **Kiểm tra đầu vào**: Tất cả đầu vào từ người dùng đều được xác thực và làm sạch để ngăn chặn các cuộc tấn công như SQL Injection, Cross-Site Scripting (XSS).


## 7. Xử lý sự cố (Troubleshooting)

*   **Lỗi kết nối cơ sở dữ liệu:**
    *   Kiểm tra chuỗi kết nối trong `appsettings.json`.
    *   Đảm bảo máy chủ MySQL đang chạy và có thể truy cập được từ ứng dụng backend.
    *   Kiểm tra tường lửa.
*   **Lỗi Migrations:**
    *   Đảm bảo bạn đã cài đặt công cụ `dotnet ef`.
    *   Kiểm tra xem có bất kỳ thay đổi nào trong các thực thể `Domain` chưa được thêm vào migration mới không.
*   **Lỗi phụ thuộc (Dependency Injection):**
    *   Kiểm tra `DependencyInjection.cs` trong các lớp `Application`, `Infrastructure`, `Web` và `CompositionRoot` để đảm bảo tất cả các dịch vụ đã được đăng ký đúng cách.
*   **Lỗi 401/403 (Unauthorized/Forbidden):**
    *   Kiểm tra token JWT.
    *   Đảm bảo người dùng có các quyền cần thiết.
    *   Kiểm tra các thuộc tính `[Authorize]` trên các controller/action.
