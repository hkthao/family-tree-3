# Quy Tắc Đóng Góp (Contribution Guidelines)

Chúng tôi hoan nghênh mọi sự đóng góp từ cộng đồng! Để đảm bảo chất lượng code và quy trình làm việc hiệu quả, vui lòng tuân thủ các quy tắc sau:

## 1. Quy tắc đặt tên Branch (Branch Naming Convention)
Tên branch cần phản ánh rõ ràng mục đích của thay đổi.
- `feature/<tên-tính-năng>`: Cho các tính năng mới. Ví dụ: `feature/user-authentication`, `feature/family-crud`.
- `bugfix/<tên-lỗi>`: Cho việc sửa lỗi. Ví dụ: `bugfix/login-issue`, `bugfix/pdf-export-error`.
- `hotfix/<tên-fix>`: Cho việc sửa lỗi khẩn cấp trên môi trường Production. Ví dụ: `hotfix/critical-security-patch`.
- `docs/<tên-tài-liệu>`: Cho các cập nhật liên quan đến tài liệu. Ví dụ: `docs/update-api-design`, `docs/add-user-guide`.
- `refactor/<tên-refactor>`: Cho các thay đổi cấu trúc code mà không thay đổi chức năng. Ví dụ: `refactor/clean-architecture-migration`.

## 2. Quy trình Pull Request (Pull Request Process)
1.  **Tạo Branch**: Luôn tạo một branch mới từ branch `develop` (hoặc `main` đối với `hotfix`).
    ```bash
    git checkout develop
    git pull origin develop
    git checkout -b feature/your-feature-name
    ```
2.  **Phát triển & Commit**: Viết code, unit tests, và đảm bảo mọi thứ hoạt động đúng. Commit code thường xuyên với các commit message rõ ràng.
3.  **Đảm bảo chất lượng**:
    - Chạy tất cả các unit tests và đảm bảo chúng pass.
    - Chạy linter và formatter để đảm bảo code tuân thủ style guide.
    - Cập nhật tài liệu liên quan (nếu có).
4.  **Push Branch**: Push branch của bạn lên remote repository.
    ```bash
    git push origin feature/your-feature-name
    ```
5.  **Tạo Pull Request (PR)**:
    - Mở một Pull Request từ branch của bạn vào branch `develop` (hoặc `main` đối với `hotfix`).
    - Đặt tiêu đề PR rõ ràng, ngắn gọn, phản ánh nội dung chính của PR.
    - Mô tả chi tiết các thay đổi, lý do thực hiện, và cách kiểm thử.
    - Gắn nhãn (labels) phù hợp (ví dụ: `feature`, `bug`, `documentation`).
    - Gắn người review (reviewers) nếu cần.
6.  **Code Review**: Chờ đợi và phản hồi các bình luận từ người review. Thực hiện các thay đổi cần thiết dựa trên feedback.
7.  **Merge**: Sau khi PR được chấp thuận và tất cả các kiểm tra CI/CD pass, PR sẽ được merge vào branch đích.

## 3. Quy tắc Commit Message (Commit Message Convention)
Dự án tuân thủ [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) để có lịch sử commit rõ ràng và tự động tạo CHANGELOG.
Cấu trúc commit message:
```
<type>(<scope>): <description>

[optional body]

[optional footer(s)]
```
-   **`<type>`**: Bắt buộc, là một trong các loại sau:
    -   `feat`: Một tính năng mới.
    -   `fix`: Sửa lỗi.
    -   `docs`: Thay đổi tài liệu.
    -   `style`: Thay đổi định dạng code, không ảnh hưởng đến ý nghĩa của code (ví dụ: sửa khoảng trắng, định dạng, dấu chấm phẩy bị thiếu).
    -   `refactor`: Thay đổi code không sửa lỗi cũng không thêm tính năng (ví dụ: đổi tên biến, tái cấu trúc).
    -   `test`: Thêm hoặc sửa các bài kiểm tra.
    -   `chore`: Các thay đổi khác không liên quan đến code nguồn hoặc test (ví dụ: cập nhật build process, thư viện).
    -   `perf`: Thay đổi code cải thiện hiệu suất.
    -   `ci`: Thay đổi cấu hình CI/CD.
    -   `build`: Thay đổi liên quan đến hệ thống build hoặc các dependency bên ngoài.
-   **`<scope>` (tùy chọn)**: Phạm vi của thay đổi (ví dụ: `backend`, `frontend`, `auth`, `families`, `docs`).
-   **`<description>`**: Mô tả ngắn gọn, súc tích về thay đổi.
-   **`<body>` (tùy chọn)**: Mô tả chi tiết hơn về thay đổi.
-   **`<footer>` (tùy chọn)**: Chứa thông tin về Breaking Changes, tham chiếu đến các issue (ví dụ: `Closes #123`).

**Ví dụ:**
```
feat(families): Add CRUD operations for family management

This commit introduces full CRUD functionality for family entities,
including API endpoints, application layer commands/queries, and MongoDB integration.

Closes #45
```

## 4. Code Review Checklist
Khi review code, hãy xem xét các điểm sau:
- [ ] Code có dễ hiểu, dễ đọc không?
- [ ] Có tuân thủ các quy tắc đặt tên và định dạng code không?
- [ ] Logic nghiệp vụ có chính xác không?
- [ ] Có đủ unit tests cho các thay đổi không?
- [ ] Có xử lý lỗi đầy đủ không?
- [ ] Có bất kỳ vấn đề bảo mật tiềm ẩn nào không?
- [ ] Hiệu suất có bị ảnh hưởng không?
- [ ] Tài liệu có được cập nhật tương ứng không?