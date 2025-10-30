import { test, expect } from '@playwright/test';
import { E2E_BASE_URL, E2E_ROUTES } from '../e2e.constants';

test.describe('Family Management - Create Family', () => {
  test('should allow a user to create a new family tree', async ({ page }) => {
    // Điều hướng đến trang quản lý gia phả
    await page.goto(`${E2E_BASE_URL}${E2E_ROUTES.FAMILY_MANAGEMENT}`);

    // Nhấn nút "Thêm mới Gia đình"
    await page.click('[data-testid="add-new-family-button"]');

    // Điền thông tin cây gia phả mới
    const familyName = `Test Family ${Date.now()}`;
    const familyDescription = 'Mô tả cho cây gia phả test.';
    await page.fill('[data-testid="family-name-input"]', familyName);
    await page.fill('[data-testid="family-description-input"]', familyDescription);

    // Nhấn nút "Lưu"
    await page.click('[data-testid="save-family-button"]');

    // Chờ thông báo thành công
    await expect(page.locator('[data-testid="snackbar-success"]')).toBeVisible();
    console.log(`Đã tạo cây gia phả mới: ${familyName}`);

    // TODO: Cần lấy familyId từ URL hoặc từ API response sau khi tạo thành công
    // Tạm thời, chúng ta sẽ tìm kiếm tên gia phả trong danh sách để xác nhận
    await expect(page.locator(`text=${familyName}`)).toBeVisible();
  });
});