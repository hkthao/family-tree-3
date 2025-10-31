import { test, expect } from '@playwright/test';
import { login } from '../login.setup';

test.describe('Relationship Management - Add Relationship', () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test('should allow a user to add a new relationship', async ({ page }) => {

    // 2. Navigate to relationship list and click "Add New Relationship"
    console.log('Điều hướng đến trang quản lý Quan hệ.');
    await page.getByRole('link', { name: 'Quản lý Quan hệ' }).click();
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    console.log('Click nút "Thêm mối quan hệ mới".');
    await page.getByTestId('relationship-create-button').click();
    await page.waitForLoadState('networkidle');

    // 3. Fill in relationship details
    console.log('Điền thông tin mối quan hệ.');
    await page.getByTestId('relationship-source-member-autocomplete').click();
    await page.waitForTimeout(1000);
    await page.waitForSelector('.v-overlay-container .v-list-item');
    await page.locator('.v-overlay-container .v-list-item').nth(0).click();

    await page.getByTestId('relationship-target-member-autocomplete').click();
    await page.waitForTimeout(1000);
    await page.waitForSelector('.v-overlay-container .v-list-item');
    await page.locator('.v-overlay-container .v-list-item').nth(1).click();

    await page.getByTestId('relationship-type-select').click();
    await page.waitForSelector('.v-overlay-container .v-list-item');
    await page.locator('.v-overlay-container .v-list-item').nth(2).click();

    await page.getByTestId('relationship-family-autocomplete').click();
    await page.waitForTimeout(1000);

    await page.waitForSelector('.v-overlay-container .v-list-item');
    await page.locator('.v-overlay-container .v-list-item').nth(0).click();

    await page.waitForTimeout(1000);
    // 4. Save the new relationship
    console.log('Click nút "Lưu mối quan hệ".');
    await page.getByTestId('relationship-add-save-button').click();
    await page.waitForLoadState('networkidle');

    await page.waitForTimeout(1000);
    await page.screenshot({
      path: "1.png"
    })
    await expect(page.locator('[data-testid="snackbar-success"]')).toBeVisible();
    console.log('Đã lưu mối quan hệ thành công.');
  });
});
