import { test, expect } from '@playwright/test';
import { login } from '../login.setup';
import { fillVuetifyInput, fillVuetifyTextarea, selectVuetifyOption, waitForSnackbar, takeScreenshotOnFailure } from '../helpers/vuetify';

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
    const familyName = `e2e Family ${new Date().getTime()}`;
    const address = `e2e address ${new Date().getTime()}`;
    const description = `e2e descriptions ${new Date().getTime()}`;

    console.log('Điều hướng đến trang quản lý gia đình/dòng họ.');
    await page.getByRole('link', { name: 'Quản lý gia đình/dòng họ' }).click();

    console.log('Click nút "Thêm mới gia đình".');
    await page.getByTestId('add-new-family-button').click();

    console.log('Điền thông tin gia đình.');
    await fillVuetifyInput(page, 'family-name-input', familyName);
    await fillVuetifyInput(page, 'family-address-input', address);
    await fillVuetifyTextarea(page, 'family-description-input', description);

    console.log('Chọn chế độ hiển thị, người quản lý và người xem.');
    await selectVuetifyOption(page, 'family-visibility-select', 0); // Chọn tùy chọn đầu tiên
    await selectVuetifyOption(page, 'family-managers-select', 0); // Chọn tùy chọn đầu tiên
    await selectVuetifyOption(page, 'family-viewers-select', 0); // Chọn tùy chọn đầu tiên

    console.log('Click nút "Lưu".');
    await page.getByTestId('button-save').click();

    console.log('Chờ snackbar thành công hiển thị.');
    await waitForSnackbar(page, 'success');

    console.log('Chờ trạng thái mạng ổn định sau khi lưu.');
    await page.waitForLoadState('networkidle');

    console.log('Mở rộng bộ lọc tìm kiếm.');
    await page.getByTestId('family-search-expand-button').click();
    await page.waitForTimeout(500); // Chờ animation

    console.log('Điền tên gia đình vào ô tìm kiếm và áp dụng bộ lọc.');
    await fillVuetifyInput(page, 'family-search-input', familyName);
    await page.getByTestId('apply-filters-button').click();

    console.log('Chờ trạng thái mạng ổn định sau khi áp dụng bộ lọc.');
    await page.waitForLoadState('networkidle');

    console.log('Xác minh gia đình mới được tạo hiển thị trong danh sách.');
    await expect(page.getByText(familyName)).toBeVisible();
    console.log('Đã tạo mới cây gia phả thành công.');
  });
});
