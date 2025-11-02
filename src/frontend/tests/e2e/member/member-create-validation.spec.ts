import { test, expect } from '@playwright/test';
import { login } from '../login.setup';
import { assertValidationMessage, takeScreenshotOnFailure } from '../helpers/vuetify';

test.describe('Member Management - Create Member - Validation Case', () => {
  test.beforeEach(async ({ page }) => {
    console.log('Đăng nhập trước khi chạy mỗi bài kiểm thử.');
    await login(page);
  });

  test.afterEach(async ({ page }, testInfo) => {
    console.log('Chụp ảnh màn hình nếu bài kiểm thử thất bại.');
    await takeScreenshotOnFailure(page, testInfo);
  });

  test('should show validation errors for empty required fields', async ({ page }) => {
    console.log('Bước 1: Điều hướng đến trang quản lý thành viên và tạo mới.');
    await Promise.all([
      page.waitForURL('**/member'),
      page.getByRole('link', { name: 'Quản lý thành viên' }).click(),
    ]);

    await Promise.all([
      page.waitForURL('**/member/add'),
      page.getByTestId('add-new-member-button').click(),
    ]);

    console.log('Bước 2: Click nút "Lưu thành viên" mà không điền bất kỳ trường nào.');
    await page.getByTestId('save-member-button').click();

    console.log('Bước 3: Kiểm tra thông báo lỗi cho các trường bắt buộc.');
    await assertValidationMessage(page, 'member-first-name-input');
    await assertValidationMessage(page, 'member-last-name-input');
    await assertValidationMessage(page, 'member-gender-select');
    await assertValidationMessage(page, 'member-family-select');

    console.log('✅ Đã xác minh các thông báo lỗi validation.');
  });
});
