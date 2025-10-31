import { test, expect } from '@playwright/test';
import { login } from '../login.setup';
import { assertValidationMessage, takeScreenshotOnFailure } from '../helpers/vuetify';

test.describe('Relationship Management - Create Relationship - Validation Case', () => {
  test.beforeEach(async ({ page }) => {
    console.log('Đăng nhập trước khi chạy mỗi bài kiểm thử.');
    await login(page);
  });

  test.afterEach(async ({ page }, testInfo) => {
    console.log('Chụp ảnh màn hình nếu bài kiểm thử thất bại.');
    await takeScreenshotOnFailure(page, testInfo);
  });

  test('should show validation errors for empty required fields', async ({ page }) => {
    console.log('Điều hướng đến trang quản lý Quan hệ.');
    await page.getByRole('link', { name: 'Quản lý Quan hệ' }).click();
    await page.waitForLoadState('networkidle');

    console.log('Click nút "Thêm mối quan hệ mới".');
    await page.getByTestId('relationship-create-button').click();
    await page.waitForLoadState('networkidle');

    console.log('Click nút "Lưu mối quan hệ" mà không điền bất kỳ trường nào.');
    await page.getByTestId('relationship-add-save-button').click();

    console.log('Kiểm tra thông báo lỗi cho các trường bắt buộc.');
    await assertValidationMessage(page, 'relationship-source-member-autocomplete');
    await assertValidationMessage(page, 'relationship-target-member-autocomplete');
    await assertValidationMessage(page, 'relationship-type-select');
    await assertValidationMessage(page, 'relationship-family-autocomplete');

    console.log('Đã xác minh các thông báo lỗi validation.');
  });
});
