import { test, expect } from '@playwright/test';
import { E2E_BASE_URL, E2E_ROUTES } from '../e2e.constants';

// test.describe('Member Management - Update Member', () => {
//   test('should allow a user to update a member', async ({ page }) => {
//     // Điều hướng đến trang quản lý thành viên
//     await page.goto(`${E2E_BASE_URL}${E2E_ROUTES.MEMBER_MANAGEMENT}`);

//     // TODO: Cần chọn thành viên vừa tạo để cập nhật
//     // Tạm thời, chúng ta sẽ click vào nút chỉnh sửa của thành viên đầu tiên trong danh sách
//     await page.click('[data-testid="edit-member-button"]');

//     // Cập nhật thông tin thành viên
//     const updatedPhone = '0123456789';
//     await page.fill('[data-testid="member-phone-input"] input', updatedPhone);

//     // Nhấn nút "Lưu"
//     await page.click('[data-testid="save-member-button"]');

//     // Chờ thông báo thành công
//     await expect(page.locator('[data-testid="snackbar-success"]')).toBeVisible();
//     console.log('Đã cập nhật thông tin thành viên.');

//     // TODO: Xác nhận thông tin đã cập nhật hiển thị đúng
//   });
// });
