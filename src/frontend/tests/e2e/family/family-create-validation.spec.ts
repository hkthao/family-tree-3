import { test, expect } from '@playwright/test';
import { login } from '../login.setup';
import { assertValidationMessage, takeScreenshotOnFailure } from '../helpers/vuetify';

test.describe('Family Management - Create Family - Validation Case', () => {
  test.beforeEach(async ({ page }) => {
    console.log('Đăng nhập trước khi chạy mỗi bài kiểm thử.');
    await login(page);
  });

  test.afterEach(async ({ page }, testInfo) => {
    console.log('Chụp ảnh màn hình nếu bài kiểm thử thất bại.');
    await takeScreenshotOnFailure(page, testInfo);
  });

  test('should show validation errors for empty required fields', async ({ page }) => {
    console.log('Bước 1: Điều hướng đến trang quản lý Gia đình và tạo mới.');
    await Promise.all([
      page.waitForURL('**/family'),
      page.getByRole('link', { name: 'Quản lý gia đình/dòng họ' }).click(),
    ]);

    await Promise.all([
      page.waitForURL('**/family/add'),
      page.getByTestId('add-new-family-button').click(),
    ]);

    console.log('Bước 2: Click nút "Lưu" mà không điền bất kỳ trường nào.');
    await page.getByTestId('button-save').click();

    console.log('Bước 3: Kiểm tra thông báo lỗi cho các trường bắt buộc.');
    await assertValidationMessage(page, 'family-name-input');
    await assertValidationMessage(page, 'family-address-input');
    await assertValidationMessage(page, 'family-description-input');
    await assertValidationMessage(page, 'family-visibility-select');
    await assertValidationMessage(page, 'family-managers-select');
    await assertValidationMessage(page, 'family-viewers-select');

    console.log('✅ Đã xác minh các thông báo lỗi validation.');
  });
});