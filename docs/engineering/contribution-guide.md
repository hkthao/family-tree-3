# Hướng dẫn Đóng góp

## Mục lục

- [1. Chiến lược Branch](#1-chiến-lược-branch)
- [2. Commit Messages](#2-commit-messages)
- [3. Quy trình Pull Request](#3-quy-trình-pull-request)
- [4. Code Style](#4-code-style)

---

Tài liệu này mô tả các quy tắc và quy trình để đóng góp vào dự án Cây Gia Phả.

## 1. Chiến lược Branch

Dự án này tuân thủ một chiến lược phân nhánh rõ ràng để quản lý quá trình phát triển, đảm bảo tính ổn định và dễ dàng theo dõi các thay đổi. Dưới đây là các loại branch chính và mục đích sử dụng của chúng:

*   `main`: Đây là branch chính, chứa code đã được kiểm thử kỹ lưỡng và sẵn sàng để triển khai lên môi trường sản xuất (production). Mọi thay đổi trên branch này phải thông qua `develop` và các quy trình kiểm thử nghiêm ngặt.
*   `develop`: Branch phát triển chính. Tất cả các `feature` và `bugfix` branches sẽ được merge vào đây. `develop` là nơi tập hợp các tính năng mới và sửa lỗi trước khi chúng được tích hợp vào `main`.
*   `feature/<feature-name>`: Được tạo ra từ `develop` để phát triển một tính năng mới cụ thể. Tên branch nên mô tả ngắn gọn tính năng (ví dụ: `feature/user-profile`, `feature/family-tree-visualization`). Sau khi hoàn thành, branch này sẽ được merge trở lại `develop`.
*   `bugfix/<bug-name>`: Được tạo ra từ `develop` để sửa một lỗi cụ thể. Tên branch nên mô tả lỗi (ví dụ: `bugfix/login-issue`, `bugfix/cors-error`). Sau khi sửa lỗi và kiểm thử, branch này sẽ được merge trở lại `develop`.
*   `docs/<docs-name>`: Được tạo ra từ `develop` để thực hiện các cập nhật hoặc bổ sung tài liệu. Tên branch nên mô tả nội dung tài liệu (ví dụ: `docs/update-architecture-guide`). Sau khi hoàn thành, branch này sẽ được merge trở lại `develop`.
*   `hotfix/<fix-name>`: Được tạo ra trực tiếp từ `main` để sửa các lỗi khẩn cấp trên môi trường production. Sau khi sửa lỗi, branch này sẽ được merge trở lại cả `main` và `develop` để đảm bảo tính đồng bộ.

## 2. Commit Messages

Dự án này tuân theo đặc tả [Conventional Commits](https://www.conventionalcommits.org/). Việc sử dụng Conventional Commits mang lại nhiều lợi ích:

*   **Dễ đọc và hiểu**: Giúp các thành viên trong nhóm dễ dàng hiểu mục đích của mỗi commit.
*   **Tự động tạo Changelog**: Có thể tự động tạo changelog dựa trên các loại commit.
*   **Tích hợp CI/CD**: Hỗ trợ các công cụ CI/CD tự động xác định phiên bản mới và kích hoạt các hành động phù hợp.

#### Cấu trúc Commit Message

Một commit message phải có cấu trúc như sau:

```
<type>(<scope>): <description>

[body]

[footer]
```

*   **`<type>` (Bắt buộc)**: Loại thay đổi. Các loại phổ biến:
    *   `feat`: Một tính năng mới.
    *   `fix`: Sửa lỗi.
    *   `docs`: Thay đổi tài liệu.
    *   `style`: Thay đổi không ảnh hưởng đến ý nghĩa của code (ví dụ: định dạng, dấu chấm phẩy bị thiếu).
    *   `refactor`: Thay đổi code không sửa lỗi cũng không thêm tính năng.
    *   `perf`: Thay đổi code cải thiện hiệu suất.
    *   `test`: Thêm hoặc sửa các bài kiểm thử (bao gồm cả kiểm thử trình xác thực).
    *   `build`: Thay đổi ảnh hưởng đến hệ thống build hoặc các phụ thuộc bên ngoài (ví dụ: gulp, broccoli, npm).
    *   `ci`: Thay đổi đối với các file cấu hình và script CI (ví dụ: Travis, Circle, BrowserStack, SauceLabs).
    *   `chore`: Các thay đổi khác không thuộc các loại trên (ví dụ: cập nhật thư viện, thay đổi cấu hình).
*   **`<scope>` (Tùy chọn)**: Phạm vi của thay đổi. Có thể là tên module, component, hoặc bất kỳ thứ gì mô tả nơi thay đổi diễn ra (ví dụ: `family-module`, `auth-service`, `frontend-ui`).
*   **`<description>` (Bắt buộc)**: Mô tả ngắn gọn, súc tích về thay đổi, không quá 50 ký tự, viết ở thì hiện tại, không viết hoa chữ cái đầu và không có dấu chấm cuối câu.
*   **`[body]` (Tùy chọn)**: Mô tả chi tiết hơn về thay đổi, giải thích lý do `tại sao` thay đổi này được thực hiện và `những gì` nó giải quyết. Mỗi dòng không quá 72 ký tự.
*   **`[footer]` (Tùy chọn)**: Chứa thông tin tham chiếu đến các issue tracker (ví dụ: `Closes #123`, `Fixes #456`).

#### Ví dụ

```
feat(family): add family creation endpoint

This commit introduces a new API endpoint for creating families.
It includes validation for family name and description.

Closes #123
```

```
fix(auth): resolve CORS issue on login

Previously, login requests were blocked due to incorrect CORS headers.
This fix updates the CORS policy in Program.cs to allow requests from the frontend origin.
```

## 3. Quy trình Pull Request

Để đảm bảo chất lượng code và quy trình làm việc hiệu quả, mọi đóng góp vào dự án đều phải tuân thủ quy trình Pull Request (PR) sau:

1.  **Đảm bảo code của bạn được định dạng và lint**: Trước khi tạo PR, hãy chắc chắn rằng code của bạn đã được định dạng theo chuẩn của dự án và không có lỗi linting. Sử dụng các lệnh `dotnet format` cho Backend và `npm run lint` cho Frontend.

    *   **Backend**: `dotnet format backend/src/ --verify-no-changes`
    *   **Frontend**: `npm run lint --prefix frontend`

2.  **Tất cả các test phải qua**: Mọi Unit Tests và Integration Tests phải chạy thành công. Điều này đảm bảo rằng các thay đổi của bạn không phá vỡ các chức năng hiện có.

    *   **Chạy tất cả test**: `dotnet test backend/tests/Application.UnitTests/ && dotnet test backend/tests/Infrastructure.IntegrationTests/`

3.  **Test coverage phải đạt ngưỡng yêu cầu (>=80%)**: Các thay đổi của bạn, đặc biệt là các tính năng hoặc logic nghiệp vụ mới, phải có đủ test coverage. Mục tiêu là duy trì ít nhất 80% test coverage cho các phần quan trọng của ứng dụng. Bạn có thể kiểm tra coverage bằng cách chạy test với công cụ coverage.

    *   **Kiểm tra coverage**: `dotnet test backend/tests/Application.UnitTests/ --collect:"XPlat Code Coverage"`

4.  **Cập nhật tài liệu nếu cần**: Nếu thay đổi của bạn bao gồm các tính năng mới, thay đổi API, hoặc sửa đổi kiến trúc, hãy cập nhật các tài liệu liên quan trong thư mục `docs/`. Tài liệu phải phản ánh chính xác các thay đổi trong code.

5.  **Sử dụng tiêu đề mô tả và cung cấp mô tả rõ ràng về các thay đổi**: Tiêu đề PR nên ngắn gọn, súc tích và tuân thủ [Conventional Commits](#2-commit-messages). Phần mô tả PR cần chi tiết, giải thích `tại sao` thay đổi được thực hiện, `những gì` đã thay đổi, và `cách thức` kiểm thử. Bao gồm các ảnh chụp màn hình hoặc GIF nếu thay đổi liên quan đến UI.

6.  **Yêu cầu Review**: Sau khi tạo PR, hãy yêu cầu ít nhất một thành viên khác trong nhóm review code của bạn. Phản hồi từ reviewer cần được xem xét và áp dụng.

## 4. Code Style

Việc duy trì một code style nhất quán là rất quan trọng để đảm bảo khả năng đọc, dễ bảo trì và hợp tác hiệu quả. Dự án này sử dụng các công cụ tự động để thực thi code style.

### Backend (C#)

*   **Công cụ**: `dotnet format`
*   **Để kiểm tra các vấn đề về định dạng (không sửa đổi file)**:

    ```bash
    dotnet format backend/src/ --verify-no-changes
    ```

*   **Để tự động định dạng code (sửa đổi file)**:

    ```bash
    dotnet format backend/src/
    ```

*   **Cấu hình IDE**: Nên cấu hình Visual Studio hoặc VS Code để tự động chạy `dotnet format` khi lưu file hoặc khi build. Điều này giúp duy trì code style mà không cần chạy lệnh thủ công.

### Frontend (Vue.js, TypeScript)

*   **Công cụ**: ESLint và Prettier (được tích hợp qua `npm run lint`)
*   **Để kiểm tra các vấn đề về linting và định dạng (không sửa đổi file)**:

    ```bash
    npm run lint --prefix frontend
    ```

*   **Để tự động sửa các vấn đề về linting và định dạng (sửa đổi file)**:

    ```bash
    npm run lint:fix --prefix frontend
    ```

*   **Cấu hình IDE**: Nên cài đặt các extension ESLint và Prettier cho VS Code và cấu hình chúng để tự động định dạng và sửa lỗi khi lưu file.