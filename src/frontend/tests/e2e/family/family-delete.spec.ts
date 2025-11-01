import { test, expect } from '@playwright/test';
import { login } from '../login.setup';
import { fillVuetifyInput, fillVuetifyTextarea, selectVuetifyOption, waitForSnackbar, takeScreenshotOnFailure } from '../helpers/vuetify';

test.describe('Family Management - Delete Family', () => {
  test.beforeEach(async ({ page }) => {
    console.log('Đăng nhập trước khi chạy mỗi bài kiểm thử.');
    await login(page);
  });

  test.afterEach(async ({ page }, testInfo) => {
    console.log('Chụp ảnh màn hình nếu bài kiểm thử thất bại.');
    await takeScreenshotOnFailure(page, testInfo);
  });

  test('should allow a user to delete a family tree', async ({ page }) => {
    const familyName = `e2e Delete Family ${new Date().getTime()}`;
    const address = `e2e address ${new Date().getTime()}`;
    const description = `e2e descriptions ${new Date().getTime()}`;

    console.log('Bước 1: Tạo dữ liệu gia đình cần xóa.');
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

    console.log('Bước 2: Tìm gia đình vừa tạo.');
    await page.getByTestId('family-search-expand-button').click();
    await fillVuetifyInput(page, 'family-search-input', familyName);
    await page.getByTestId('apply-filters-button').click();
    await waitForVDataTableLoaded(page);
    await expect(page.getByText(familyName)).toBeVisible();
    console.log('✅ Đã tìm thấy gia đình.');

    console.log('Bước 3: Xóa gia đình.');
    await page.locator(`[data-testid="delete-family-button"][data-family-name="${familyName}"]`).click();

    const confirmationDialog = page.locator('.v-dialog');
    await expect(confirmationDialog).toBeVisible();
    await confirmationDialog.getByTestId('confirm-delete-button').click();

    console.log('Bước 4: Xác minh gia đình đã bị xóa.');
    await waitForSnackbar(page, 'success');
    await expect(page.getByText(familyName)).not.toBeVisible();
    console.log('✅ Đã xóa cây gia phả.');
  });
});
