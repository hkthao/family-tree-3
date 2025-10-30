import { test, expect } from '@playwright/test';
import { E2E_BASE_URL, E2E_ROUTES } from '../e2e.constants';

test.describe('Family Management - View Family', () => {
  test('should allow a user to view family tree details', async ({ page }) => {
    // Điều hướng đến trang xem cây gia phả
    // TODO: Cần familyId để điều hướng chính xác
    await page.goto(`${E2E_BASE_URL}${E2E_ROUTES.FAMILY_TREE}`);

    // TODO: Xác nhận cây gia phả hiển thị đúng
    await expect(page.locator('[data-testid="family-tree-canvas"]')).toBeVisible();
    console.log('Đã xem chi tiết cây gia phả.');
  });
});
