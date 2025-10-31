import { test, expect } from '@playwright/test';
import { login } from '../login.setup';
import { fillVuetifyInput, fillVuetifyTextarea, selectVuetifyOption, assertValidationMessage, takeScreenshotOnFailure } from '../helpers/vuetify';

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

    console.log('Điều hướng đến trang quản lý gia đình/dòng họ để tạo gia đình gốc.');
    await page.getByRole('link', { name: 'Quản lý gia đình/dòng họ' }).click();

    console.log('Click nút "Thêm mới gia đình".');
    await page.getByTestId('add-new-family-button').click();

    console.log('Điền thông tin gia đình gốc.');
    await fillVuetifyInput(page, 'family-name-input', originalFamilyName);
    await fillVuetifyInput(page, 'family-address-input', originalAddress);
    await fillVuetifyTextarea(page, 'family-description-input', originalDescription);
    await selectVuetifyOption(page, 'family-visibility-select', 0);
    await selectVuetifyOption(page, 'family-managers-select', 0);
    await selectVuetifyOption(page, 'family-viewers-select', 0);

    console.log('Click nút "Lưu" để tạo gia đình gốc.');
    await page.getByTestId('button-save').click();
    await page.waitForLoadState('networkidle');
    console.log('Đã tạo gia đình gốc thành công.');

    console.log('Mở rộng bộ lọc tìm kiếm.');
    await page.getByTestId('family-search-expand-button').click();
    await page.waitForTimeout(500); // Chờ animation

    console.log('Điền tên gia đình gốc vào ô tìm kiếm và áp dụng bộ lọc.');
    await fillVuetifyInput(page, 'family-search-input', originalFamilyName);
    await page.getByTestId('apply-filters-button').click();
    await page.waitForLoadState('networkidle');
    await expect(page.getByText(originalFamilyName)).toBeVisible();
    console.log('Đã tìm thấy gia đình gốc.');

    console.log('Click vào gia đình gốc để xem chi tiết.');
    await page.getByText(originalFamilyName).click();
    await page.waitForLoadState('networkidle');

    console.log('Click nút "Cập nhật".');
    await page.getByTestId('button-edit').click();
    await page.waitForLoadState('networkidle');

    console.log('Xóa nội dung các trường bắt buộc.');
    await fillVuetifyInput(page, 'family-name-input', '');
    await fillVuetifyInput(page, 'family-address-input', '');
    await fillVuetifyTextarea(page, 'family-description-input', '');

    console.log('Click nút "Lưu" mà không điền bất kỳ trường nào.');
    await page.getByTestId('button-save').click();

    console.log('Kiểm tra thông báo lỗi cho các trường bắt buộc.');
    await assertValidationMessage(page, 'family-name-input');
    await assertValidationMessage(page, 'family-address-input');
    await assertValidationMessage(page, 'family-description-input');
    // Validation for select fields might be different, assuming they can't be truly 'empty' after initial selection

    console.log('Đã xác minh các thông báo lỗi validation.');
  });
});
