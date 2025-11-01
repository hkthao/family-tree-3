import { test, expect } from '@playwright/test';
import { login } from '../login.setup';
import { fillVuetifyInput, fillVuetifyTextarea, selectVuetifyOption, assertValidationMessage, takeScreenshotOnFailure, waitForVDataTableLoaded } from '../helpers/vuetify';

test.describe('Family Management - Update Family - Validation Case', () => {
  test.beforeEach(async ({ page }) => {
    console.log('Đăng nhập trước khi chạy mỗi bài kiểm thử.');
    await login(page);
  });

  test.afterEach(async ({ page }, testInfo) => {
    console.log('Chụp ảnh màn hình nếu bài kiểm thử thất bại.');
    await takeScreenshotOnFailure(page, testInfo);
  });

  test('should show validation errors for empty required fields on update', async ({ page }) => {
    const originalFamilyName = `e2e Original Family ${new Date().getTime()}`;
    const originalAddress = `e2e Original Address ${new Date().getTime()}`;
    const originalDescription = `e2e Original Description ${new Date().getTime()}`;

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
    console.log('✅ Đã tạo gia đình gốc thành công.');

    console.log('Bước 2: Tìm và điều hướng đến trang chỉnh sửa gia đình.');
    await page.getByTestId('family-search-expand-button').click();
    await fillVuetifyInput(page, 'family-search-input', originalFamilyName);
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

    console.log('Bước 3: Xóa nội dung các trường bắt buộc và lưu.');
    await fillVuetifyInput(page, 'family-name-input', '');
    await fillVuetifyInput(page, 'family-address-input', '');
    await fillVuetifyTextarea(page, 'family-description-input', '');

    await page.getByTestId('button-save').click();

    console.log('Bước 4: Kiểm tra thông báo lỗi cho các trường bắt buộc.');
    await assertValidationMessage(page, 'family-name-input');
    await assertValidationMessage(page, 'family-address-input');
    await assertValidationMessage(page, 'family-description-input');

    console.log('✅ Đã xác minh các thông báo lỗi validation.');
  });
});
