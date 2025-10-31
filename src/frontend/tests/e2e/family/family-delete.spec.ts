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

    console.log('Điều hướng đến trang quản lý gia đình/dòng họ để tạo gia đình.');
    await page.getByRole('link', { name: 'Quản lý gia đình/dòng họ' }).click();

    console.log('Click nút "Thêm mới gia đình".');
    await page.getByTestId('add-new-family-button').click();

    console.log('Điền thông tin gia đình.');
    await fillVuetifyInput(page, 'family-name-input', familyName);
    await fillVuetifyInput(page, 'family-address-input', address);
    await fillVuetifyTextarea(page, 'family-description-input', description);
    await selectVuetifyOption(page, 'family-visibility-select', 0);
    await selectVuetifyOption(page, 'family-managers-select', 0);
    await selectVuetifyOption(page, 'family-viewers-select', 0);

    console.log('Click nút "Lưu" để tạo gia đình.');
    await page.getByTestId('button-save').click();
    await waitForSnackbar(page, 'success');
    await page.waitForLoadState('networkidle');
    console.log('Đã tạo gia đình thành công.');

    console.log('Mở rộng bộ lọc tìm kiếm.');
    await page.getByTestId('family-search-expand-button').click();
    await page.waitForTimeout(500); // Chờ animation

    console.log('Điền tên gia đình vừa tạo vào ô tìm kiếm và áp dụng bộ lọc.');
    await fillVuetifyInput(page, 'family-search-input', familyName);
    await page.getByTestId('apply-filters-button').click();
    await page.waitForLoadState('networkidle');
    await expect(page.getByText(familyName)).toBeVisible();
    console.log('Đã tìm thấy gia đình.');

    console.log('Chọn cây gia phả vừa tạo để xóa.');
    await page.locator(`[data-testid="delete-family-button"][data-family-name="${familyName}"]`).click();

    console.log('Xác nhận xóa.');
    await page.getByTestId('confirm-delete-button').click();

    console.log('Chờ thông báo thành công.');
    await waitForSnackbar(page, 'success');
    
    console.log('Xác nhận cây gia phả không còn trong danh sách.');
    await expect(page.getByText(familyName)).not.toBeVisible();
    
    console.log('Đã xóa cây gia phả.');
  });
});
