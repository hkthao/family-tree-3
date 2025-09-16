# Quy Tắc Đóng Góp (Contribution)

## 1. Branch Naming
- `feature/<ten-feature>`
- `bugfix/<ma-bug>`
- `hotfix/<ten-fix>`

## 2. Pull Request Process
1. Tạo branch từ `main`.
2. Commit code.
3. Push branch lên remote.
4. Tạo Pull Request vào `main`.
5. Gắn tag, reviewer.
6. Chờ review và pass pipeline.
7. Merge.

## 3. Code Review Checklist
- Code có dễ hiểu, dễ đọc không?
- Có tuân thủ các quy tắc đặt tên và định dạng code không?
- Logic nghiệp vụ có chính xác không?
- Có đủ unit tests cho các thay đổi không?
- Test coverage đạt ngưỡng yêu cầu (>=80%).
- Code đã pass linting.
- Có xử lý lỗi đầy đủ không?
- Có bất kỳ vấn đề bảo mật tiềm ẩn nào không?
- Hiệu suất có bị ảnh hưởng không?
- Tài liệu có được cập nhật tương ứng không?
