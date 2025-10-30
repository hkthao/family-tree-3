import { test, expect } from '@playwright/test';
import { E2E_BASE_URL, E2E_ROUTES } from '../e2e.constants';

// test.describe('Member Management - Add Member', () => {
//   test('should allow a user to add a member to the family tree', async ({ page }) => {
//     // Điều hướng đến trang chi tiết cây gia phả (cần familyId)
//     // TODO: Cần lấy familyId từ test case trước hoặc từ API
//     // Tạm thời, chúng ta sẽ điều hướng đến trang quản lý gia phả và chọn gia phả vừa tạo
//     await page.goto(`${E2E_BASE_URL}${E2E_ROUTES.FAMILY_MANAGEMENT}`);
//     const familyName = `Test Family`; // Cần lấy tên gia phả đã tạo
//     await page.click(`text=${familyName}`); // Click vào gia phả vừa tạo

//     // Nhấn nút "Thêm Thành viên Mới"
//     await page.click('[data-testid="add-new-member-button"]');

//     // Điền thông tin thành viên mới
//     const memberFirstName = 'Thành viên';
//     const memberLastName = `Test ${Date.now()}`;
//     await page.fill('[data-testid="member-first-name-input"] input', memberFirstName);
//     await page.fill('[data-testid="member-last-name-input"] input', memberLastName);
//     await page.selectOption('[data-testid="member-gender-select"]', 'Male'); // Chọn giới tính
//     // TODO: Cần thêm các trường khác như ngày sinh, nơi sinh, v.v.

//     // Nhấn nút "Lưu"
//     await page.click('[data-testid="save-member-button"]');

//     // Chờ thông báo thành công
//     await expect(page.locator('[data-testid="snackbar-success"]')).toBeVisible();
//     console.log(`Đã thêm thành viên mới: ${memberFirstName} ${memberLastName}`);

//     // TODO: Cần lấy memberId từ URL hoặc từ API response sau khi tạo thành công
//     // Tạm thời, chúng ta sẽ tìm kiếm tên thành viên trong danh sách để xác nhận
//     await expect(page.locator(`text=${memberFirstName} ${memberLastName}`)).toBeVisible();
//   });
// });
