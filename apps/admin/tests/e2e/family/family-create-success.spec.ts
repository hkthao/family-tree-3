import { test, expect } from '@playwright/test';
import { login } from '../login.setup';
import { fillVuetifyInput, fillVuetifyTextarea, selectVuetifyOption, waitForSnackbar, takeScreenshotOnFailure, waitForVDataTableLoaded } from '../helpers/vuetify';

test.describe('Family Management - Create Family - Success Case', () => {
  test.beforeEach(async ({ page }) => {
    console.log('Đăng nhập trước khi chạy mỗi bài kiểm thử.');
    await login(page);
  });

  test.afterEach(async ({ page }, testInfo) => {
    console.log('Chụp ảnh màn hình nếu bài kiểm thử thất bại.');
    await takeScreenshotOnFailure(page, testInfo);
  });

  test('should create family successfully', async ({ page }) => {
    const familyName = `Gia đình ${new Date().getTime()}`;
    const familyAddress = 'Địa chỉ gia đình';
    const familyDescription = 'Mô tả gia đình';

    console.log('Bước 1: Điều hướng đến trang quản lý Gia đình và tạo mới.');
    await Promise.all([
      page.waitForURL('**/family'),
      page.getByRole('link', { name: 'Quản lý gia đình/dòng họ' }).click(),
    ]);

    await Promise.all([
      page.waitForURL('**/family/add'),
      page.getByTestId('add-new-family-button').click(),
    ]);

    console.log('Điền thông tin gia đình.');
    await fillVuetifyInput(page, 'family-name-input', familyName);
    await fillVuetifyInput(page, 'family-address-input', familyAddress);
    await fillVuetifyTextarea(page, 'family-description-input', familyDescription);
    await selectVuetifyOption(page, 'family-visibility-select', 0); // Chọn tùy chọn đầu tiên
    await selectVuetifyOption(page, 'family-managers-select', 0); // Chọn tùy chọn đầu tiên
    await selectVuetifyOption(page, 'family-viewers-select', 0); // Chọn tùy chọn đầu tiên

    console.log('Click nút "Lưu".');
    await page.getByTestId('button-save').click();
    await waitForSnackbar(page, 'success');
    console.log('✅ Đã tạo gia đình thành công.');

    console.log('Bước 2: Xác minh gia đình mới hiển thị trong danh sách.');
    await fillVuetifyInput(page, 'family-list-search-input', familyName);
    await waitForVDataTableLoaded(page);
    await expect(page.locator('tr').filter({ hasText: familyName })).toBeVisible();
    console.log('✅ Đã xác minh gia đình mới.');
  });
});