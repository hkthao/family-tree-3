# Hướng dẫn Đóng góp

## Mục lục

- [1. Chiến lược Branch](#1-chiến-lược-branch)
- [2. Commit Messages](#2-commit-messages)
- [3. Quy trình Pull Request](#3-quy-trình-pull-request)
- [4. Code Style](#4-code-style)

---

Tài liệu này mô tả các quy tắc và quy trình để đóng góp vào dự án Cây Gia Phả.

## 1. Chiến lược Branch

-   `main`: Branch ổn định cho code sẵn sàng sản xuất.
-   `develop`: Branch phát triển chính.
-   `feature/<feature-name>`: Cho các tính năng mới.
-   `bugfix/<bug-name>`: Cho các bản sửa lỗi.
-   `docs/<docs-name>`: Cho các cập nhật tài liệu.
-   `hotfix/<fix-name>`: Cho các bản sửa lỗi khẩn cấp trên production.

## 2. Commit Messages

Dự án này tuân theo đặc tả [Conventional Commits](https://www.conventionalcommits.org/).

## 3. Quy trình Pull Request

-   Đảm bảo code của bạn được định dạng và lint.
-   Tất cả các test phải qua.
-   Test coverage phải đạt ngưỡng yêu cầu (>=80%).
-   Cập nhật tài liệu nếu cần.
-   Sử dụng tiêu đề mô tả và cung cấp mô tả rõ ràng về các thay đổi.

## 4. Code Style

### Backend

-   Để kiểm tra các vấn đề về định dạng:
    ```bash
    dotnet format backend/ --verify-no-changes --include-generated
    ```
-   Để tự động định dạng code:
    ```bash
    dotnet format backend/ --include-generated
    ```

### Frontend

-   Để kiểm tra các vấn đề về linting:
    ```bash
    npm run lint --prefix frontend
    ```
-   Để tự động sửa các vấn đề về linting:
    ```bash
    npm run lint:fix --prefix frontend
    ```