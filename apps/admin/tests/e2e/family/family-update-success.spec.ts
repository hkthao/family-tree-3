import { test, expect } from '@playwright/test';
import { login } from '../login.setup';
import { fillVuetifyInput, fillVuetifyTextarea, selectVuetifyOption, waitForSnackbar, takeScreenshotOnFailure, waitForVDataTableLoaded } from '../helpers/vuetify';

test.describe('Family Management - Update Family - Success Case', () => {
  test.beforeEach(async ({ page }) => {
    console.log('Đăng nhập trước khi chạy mỗi bài kiểm thử.');
    await login(page);
  });

  test.afterEach(async ({ page }, testInfo) => {
    console.log('Chụp ảnh màn hình nếu bài kiểm thử thất bại.');
    await takeScreenshotOnFailure(page, testInfo);
  });

  test('should allow a user to update an existing family tree successfully', async ({ page }) => {
    const originalFamilyName = `e2e Original Family ${new Date().getTime()}`;
    const updatedFamilyName = `e2e Updated Family ${new Date().getTime()}`;
    const originalAddress = `e2e Original Address ${new Date().getTime()}`;
    const updatedAddress = `e2e Updated Address ${new Date().getTime()}`;
    const originalDescription = `e2e Original Description ${new Date().getTime()}`;
    const updatedDescription = `e2e Updated Description ${new Date().getTime()}`;

    console.log('Bước 1: Tạo dữ liệu gia đình gốc.');
    await Promise.all([
      page.waitForURL('**/family'),
      page.getByRole('link', { name: 'Quản lý gia đình/dòng họ' }).click(),
    ]);

    await Promise.all([
      page.waitForURL('**/family/add'),
      page.getByTestId('add-new-family-button').click(),
    ]);

    await fillVuetifyInput(page, 'family-name-input', originalFamilyName);
    await fillVuetifyInput(page, 'family-address-input', originalAddress);
    await fillVuetifyTextarea(page, 'family-description-input', originalDescription);
    await selectVuetifyOption(page, 'family-visibility-select', 0);
    await selectVuetifyOption(page, 'family-managers-select', 0);
    await selectVuetifyOption(page, 'family-viewers-select', 0);

    await page.getByTestId('button-save').click();
    await waitForSnackbar(page, 'success');
    console.log('✅ Đã tạo gia đình gốc thành công.');

    console.log('Bước 2: Tìm và điều hướng đến trang chỉnh sửa gia đình.');
    await page.getByTestId('family-search-expand-button').click();
    await fillVuetifyInput(page, 'family-list-search-input', originalFamilyName);
    await page.getByTestId('apply-filters-button').click();
    await waitForVDataTableLoaded(page);
    const familyRow = page.locator('tr', { has: page.getByText(originalFamilyName) });
    await expect(familyRow).toBeVisible();

    await Promise.all([
      page.waitForURL(/.*\/family\/detail\/.*/),
      familyRow.getByText(originalFamilyName).click(),
    ]);

    await Promise.all([
      page.waitForURL(/.*\/family\/edit\/.*/),
      page.getByTestId('button-edit').click(),
    ]);
    console.log('Đã vào trang chỉnh sửa gia đình.');

    console.log('Bước 3: Cập nhật thông tin gia đình.');
    await fillVuetifyInput(page, 'family-name-input', updatedFamilyName);
    await fillVuetifyInput(page, 'family-address-input', updatedAddress);
    await fillVuetifyTextarea(page, 'family-description-input', updatedDescription);

    await page.getByTestId('button-save').click();
    await waitForSnackbar(page, 'success');
    console.log('✅ Đã lưu dữ liệu cập nhật thành công.');

    console.log('Bước 4: Xác minh gia đình đã được cập nhật trong danh sách.');
    await page.getByTestId('family-search-expand-button').click();
    await fillVuetifyInput(page, 'family-list-search-input', updatedFamilyName);
    await page.getByTestId('apply-filters-button').click();
    await waitForVDataTableLoaded(page);
    await expect(page.getByText(updatedFamilyName)).toBeVisible();
    console.log('✅ Đã cập nhật cây gia phả thành công.');
  });
});
