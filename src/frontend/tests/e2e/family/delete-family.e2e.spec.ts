import { test, expect } from '@playwright/test';
import { E2E_BASE_URL, E2E_ROUTES } from '../e2e.constants';
import { login } from '../login.setup';

test.describe('Family Management - Delete Family', () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test('should allow a user to delete a family tree', async ({ page }) => {
    const familyName = `e2e Delete Family ${new Date().getTime()}`;

    // Create a family first
    await page.getByRole('link', { name: 'Quản lý gia đình/dòng họ' }).click();
    await page.getByTestId('add-new-family-button').click();
    await page.getByTestId('family-name-input').click();
    await page.getByTestId('family-name-input').locator('input').fill(familyName);
    await page.getByTestId('family-address-input').click();
    await page.getByTestId('family-address-input').locator('input').fill(familyName);
    await page.getByTestId('family-description-input').click();
    await page.getByTestId('family-description-input').locator('textarea').fill(familyName);
    await page.getByTestId('button-save').click();
    await expect(page.getByText(familyName)).toBeVisible();

    // Điều hướng đến trang quản lý gia phả
    await page.goto(`${E2E_BASE_URL}${E2E_ROUTES.FAMILY_MANAGEMENT}`);

    // Chọn cây gia phả vừa tạo để xóa
    await page.locator(`[data-testid="delete-family-button"][data-family-name="${familyName}"]`).click();

    // Xác nhận xóa
    await page.getByTestId('confirm-delete-button').click();

    // Chờ thông báo thành công
    await expect(page.locator('[data-testid="snackbar-success"]')).toBeVisible();
    console.log('Đã xóa cây gia phả.');

    // Xác nhận cây gia phả không còn trong danh sách
    await expect(page.getByText(familyName)).not.toBeVisible();
  });
});
