import { test, expect } from '@playwright/test';
import { login } from '../login.setup';

test.describe('Family Management - Create Family', () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test('should allow a user to create a new family tree', async ({ page }) => {
    const familyName = `e2e Family ${new Date().getTime()}`
    const address = `e2e address ${new Date().getTime()}`
    const description = `e2e descriptions ${new Date().getTime()}`
    console.log('Điều hướng đến trang quản lý gia đình/dòng họ.');
    await page.getByRole('link', { name: 'Quản lý gia đình/dòng họ' }).click();
    console.log('Click nút "Thêm mới gia đình".');
    await page.getByTestId('add-new-family-button').click();

    console.log('Điền tên gia đình.');
    await page.getByTestId('family-name-input').click();
    await page.getByTestId('family-name-input').locator('input').fill(familyName);

    console.log('Điền địa chỉ.');
    await page.getByTestId('family-address-input').click();
    await page.getByTestId('family-address-input').locator('input').fill(address);

    console.log('Điền mô tả.');
    await page.getByTestId('family-description-input').click();
    await page.getByTestId('family-description-input').locator('textarea').fill(description);

    console.log('Chọn chế độ hiển thị.');
    await page.getByTestId('family-visibility-select').click();
    await page.waitForSelector('.v-overlay-container .v-list-item');
    await page.locator('.v-overlay-container .v-list-item').first().click();
    console.log('Chọn người quản lý.');

    await page.getByTestId('family-managers-select').click();
    await page.waitForSelector('.v-overlay-container .v-list-item');
    await page.locator('.v-overlay-container .v-list-item').first().click();
    console.log('Chọn người xem.');

    await page.getByTestId('family-viewers-select').click();
    await page.waitForSelector('.v-overlay-container .v-list-item');
    await page.locator('.v-overlay-container .v-list-item').first().click();

    console.log('Click nút "Lưu".');
    await page.getByTestId('button-save').click();
    await expect(page.locator('[data-testid="snackbar-success"]')).toBeVisible();
    console.log('Đã lưu gia đình thành công.');

    await page.waitForLoadState('networkidle');

    console.log('Mở rộng bộ lọc tìm kiếm.');
    await page.getByTestId('family-search-expand-button').click();
    await page.waitForTimeout(500)
    console.log('Điền tên gia đình vào ô tìm kiếm.');
    await page.getByTestId('family-search-input').locator('input').fill(familyName);
    console.log('Click nút "Áp dụng bộ lọc".');
    await page.getByTestId('apply-filters-button').click();
    await page.waitForLoadState('networkidle');
    console.log('Xác minh gia đình mới được tạo hiển thị trong danh sách.');
    await expect(page.getByText(familyName)).toBeVisible();
    console.log('Đã tạo mới cây gia phả.');
  });
});