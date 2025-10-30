import { test, expect } from '@playwright/test';
import { E2E_BASE_URL, E2E_ROUTES } from '../e2e.constants';

test.describe('Family Management - Delete Family', () => {
  test('should allow a user to delete a family tree', async ({ page }) => {
    // Điều hướng đến trang quản lý gia phả
    await page.goto(`${E2E_BASE_URL}${E2E_ROUTES.FAMILY_MANAGEMENT}`);

    // TODO: Cần chọn cây gia phả vừa tạo để xóa
    // Tạm thời, chúng ta sẽ click vào nút xóa của cây gia phả đầu tiên trong danh sách
    await page.click('[data-testid="delete-family-button"]');

    // Xác nhận xóa
    await page.click('[data-testid="confirm-delete-button"]');

    // Chờ thông báo thành công
    await expect(page.locator('[data-testid="snackbar-success"]')).toBeVisible();
    console.log('Đã xóa cây gia phả.');

    // TODO: Xác nhận cây gia phả không còn trong danh sách
  });
});
