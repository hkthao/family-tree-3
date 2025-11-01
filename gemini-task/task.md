Refactor file test Playwright E2E cho feature 'Create Family' trong app Vue + Vuetify 3.

Yêu cầu chi tiết:

1. Chia test thành 2 nhóm:
   - Nhóm 1: 'Success case' - người dùng nhập dữ liệu hợp lệ và lưu thành công.
   - Nhóm 2: 'Validation case' - hiển thị lỗi khi bỏ trống các trường bắt buộc.

2. Viết helper functions tái sử dụng:
   - fillVuetifyInput(page, testId, value)
   - fillVuetifyTextarea(page, testId, value)
   - selectVuetifyOption(page, testId, optionIndex)
   - assertValidationMessage(page, testId, expectedText)
   - waitForSnackbar(page, type = 'success')
   - takeScreenshotOnFailure(page, testInfo)

3. Trong 'Success case':
   - Nhập tất cả các trường bắt buộc: name, address, description.
   - Chọn dropdown: visibility, managers, viewers.
   - Click nút 'Save'.
   - Kiểm tra snackbar 'success' hiển thị.
   - Quay lại danh sách, filter theo tên vừa tạo và assert dòng hiển thị đúng.
   - Chụp screenshot khi test fail.

4. Trong 'Validation case':
   - Bỏ trống các trường bắt buộc.
   - Click 'Save'.
   - Kiểm tra thông báo lỗi hiển thị đúng từng trường.
   - Chụp screenshot khi validation fail.

5. Sử dụng cấu trúc BDD rõ ràng:
   - test.describe('Family Management - Create Family')
   - test('should create family successfully', ...)
   - test('should show validation errors', ...)

6. Mỗi bước test đều có comment giải thích.
7. Dùng data-testid để thao tác, tránh dùng text hoặc role.
8. Chờ component Vuetify render xong trước khi thao tác (locator.waitFor, waitForSelector).
9. Tối ưu chờ networkidle, tránh timeout cứng.
10. Format code chuẩn Prettier, dùng async/await an toàn.
11. Output: 
    - File test: 'family-create-success.spec.ts'
    - File test: 'family-create-validation.spec.ts'
    - Helper file: 'helpers/vuetify.ts' chứa tất cả helper function trên, có tích hợp screenshot khi fail.
