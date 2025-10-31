import { test, expect } from '@playwright/test';
import { login } from '../login.setup';

test.describe('Relationship Management - Update Relationship', () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test('should allow a user to update an existing relationship', async ({ page }) => {

    // 2. Create a relationship
    console.log('Điều hướng đến trang quản lý Quan hệ để tạo mối quan hệ.');
    await page.getByRole('link', { name: 'Quản lý Quan hệ' }).click();
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    // Select the first source member in the search filter

    await page.getByTestId('relationship-search-expand-button').click();
    await page.getByTestId('relationship-search-source-member-autocomplete').click();
    await page.waitForSelector('.v-overlay-container .v-list-item');
    await page.locator('.v-overlay-container .v-list-item').nth(0).click();
    await page.getByTestId('relationship-search').getByTestId('relationship-search-apply-button').click();
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // 4. Click edit button
    console.log('Click nút chỉnh sửa mối quan hệ.');
    await page.getByRole('row').getByTestId('relationship-edit-button').nth(0).click();
    await page.waitForLoadState('networkidle');

    // 5. Update relationship details
    console.log('Cập nhật thông tin mối quan hệ.');
    await page.getByTestId('relationship-type-select').click();
    await page.waitForSelector('.v-overlay-container .v-list-item');
    await page.getByTestId('relationship-order-input').locator('input').fill('1');
    await page.waitForTimeout(1000);

    // 6. Save the updated relationship
    console.log('Click nút "Lưu mối quan hệ" sau khi cập nhật.');
    await page.getByTestId('button-save').click();
    await page.waitForLoadState('networkidle');
    await expect(page.locator('[data-testid="snackbar-success"]')).toBeVisible();
    console.log('Đã lưu mối quan hệ cập nhật thành công.');

  });
});
