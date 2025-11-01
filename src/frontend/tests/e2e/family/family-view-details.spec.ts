import { test, expect } from '@playwright/test';
import { login } from '../login.setup';
import { fillVuetifyInput, fillVuetifyTextarea, selectVuetifyOption, waitForSnackbar, takeScreenshotOnFailure, waitForVDataTableLoaded } from '../helpers/vuetify';

test.describe('Family Management - View Family Details', () => {
  test.beforeEach(async ({ page }) => {
    console.log('Đăng nhập trước khi chạy mỗi bài kiểm thử.');
    await login(page);
  });

  test.afterEach(async ({ page }, testInfo) => {
    console.log('Chụp ảnh màn hình nếu bài kiểm thử thất bại.');
    await takeScreenshotOnFailure(page, testInfo);
  });

  test('should allow a user to view family tree details', async ({ page }) => {
    const familyName = `e2e View Family ${new Date().getTime()}`;
    const address = `e2e address ${new Date().getTime()}`;
    const description = `e2e descriptions ${new Date().getTime()}`;

    console.log('Bước 1: Tạo dữ liệu gia đình.');
    await Promise.all([
      page.waitForURL('**/family'),
      page.getByRole('link', { name: 'Quản lý gia đình/dòng họ' }).click(),
    ]);

    await Promise.all([
      page.waitForURL('**/family/add'),
      page.getByTestId('add-new-family-button').click(),
    ]);

    await fillVuetifyInput(page, 'family-name-input', familyName);
    await fillVuetifyInput(page, 'family-address-input', address);
    await fillVuetifyTextarea(page, 'family-description-input', description);
    await selectVuetifyOption(page, 'family-visibility-select', 0);
    await selectVuetifyOption(page, 'family-managers-select', 0);
    await selectVuetifyOption(page, 'family-viewers-select', 0);

    await page.getByTestId('button-save').click();
    await waitForSnackbar(page, 'success');
    console.log('✅ Đã tạo gia đình thành công.');

    console.log('Bước 2: Tìm và điều hướng đến trang chi tiết gia đình.');
    await page.getByTestId('family-search-expand-button').click();
    await fillVuetifyInput(page, 'family-search-input', familyName);
    await page.getByTestId('apply-filters-button').click();
    await waitForVDataTableLoaded(page);
    const familyRow = page.locator('tr', { has: page.getByText(familyName) });
    await expect(familyRow).toBeVisible();

    await Promise.all([
      page.waitForURL(/.*\/family\/detail\/.*/),
      familyRow.getByText(familyName).click(),
    ]);
    console.log('Đã vào trang chi tiết gia đình.');

    console.log('Bước 3: Xác minh các trường thông tin hiển thị đúng.');
    await expect(page.getByTestId('family-name-input').locator('input')).toHaveValue(familyName);
    await expect(page.getByTestId('family-address-input').locator('input')).toHaveValue(address);
    await expect(page.getByTestId('family-description-input').locator('textarea')).toHaveValue(description);
    console.log('✅ Đã xem chi tiết cây gia phả.');
  });
});
