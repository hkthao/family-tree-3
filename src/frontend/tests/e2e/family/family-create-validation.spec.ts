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
    console.log('Điều hướng đến trang quản lý gia đình/dòng họ.');
    await page.getByRole('link', { name: 'Quản lý gia đình/dòng họ' }).click();

    console.log('Click nút "Thêm mới gia đình" để mở form.');
    await page.getByTestId('add-new-family-button').click();

    console.log('Click nút "Lưu" mà không điền bất kỳ trường nào.');
    await page.getByTestId('button-save').click();

    console.log('Kiểm tra thông báo lỗi cho các trường bắt buộc.');
    await assertValidationMessage(page, 'family-name-input');
    await assertValidationMessage(page, 'family-address-input');
    await assertValidationMessage(page, 'family-description-input');
    await assertValidationMessage(page, 'family-visibility-select');
    await assertValidationMessage(page, 'family-managers-select');
    await assertValidationMessage(page, 'family-viewers-select');

    console.log('Đã xác minh các thông báo lỗi validation.');
  });
});
