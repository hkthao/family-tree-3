import { test, expect } from '@playwright/test';
import { login } from '../login.setup';
import { fillVuetifyInput, selectVuetifyOption, waitForSnackbar, takeScreenshotOnFailure, waitForVDataTableLoaded } from '../helpers/vuetify';

test.describe('Member Management - Create Member - Success Case', () => {
  test.beforeEach(async ({ page }) => {
    console.log('Đăng nhập trước khi chạy mỗi bài kiểm thử.');
    await login(page);
  });

  test.afterEach(async ({ page }, testInfo) => {
    console.log('Chụp ảnh màn hình nếu bài kiểm thử thất bại.');
    await takeScreenshotOnFailure(page, testInfo);
  });

  test('should allow a user to add a member to the family tree successfully', async ({ page }) => {
    const memberFirstName = 'Thành viên';
    const memberLastName = `Test ${new Date().getTime()}`;
    const memberNickname = 'Biệt danh';
    const memberPlaceOfBirth = 'Hà Nội';
    const memberPlaceOfDeath = 'TP. Hồ Chí Minh';
    const memberOccupation = 'Kỹ sư';

    console.log('Bước 1: Điều hướng đến trang quản lý thành viên và tạo mới.');
    await Promise.all([
      page.waitForURL('**/member'),
      page.getByRole('link', { name: 'Quản lý thành viên' }).click(),
    ]);

    await Promise.all([
      page.waitForURL('**/member/add'),
      page.getByTestId('add-new-member-button').click(),
    ]);

    console.log('Điền thông tin thành viên.');
    await fillVuetifyInput(page, 'member-first-name-input', memberFirstName);
    await fillVuetifyInput(page, 'member-last-name-input', memberLastName);
    await fillVuetifyInput(page, 'member-nickname-input', memberNickname);

    console.log('Chọn ngày sinh.');
    await page.locator('.mdi-calendar').first().click();
    await page.locator('.v-date-picker-month__day-btn').first().click();

    console.log('Chọn ngày mất.');
    await page.locator('.mdi-calendar').nth(1).click();
    await page.locator('.v-date-picker-month__day-btn').last().click();

    await fillVuetifyInput(page, 'member-place-of-birth-input', memberPlaceOfBirth);
    await fillVuetifyInput(page, 'member-place-of-death-input', memberPlaceOfDeath);
    await fillVuetifyInput(page, 'member-occupation-input', memberOccupation);

    console.log('Chọn giới tính.');
    await selectVuetifyOption(page, 'member-gender-select', 0); // Assuming 'Nam' is the first option

    console.log('Chọn gia đình.');
    await selectVuetifyOption(page, 'member-family-select', 0); // Assuming the first family is available

    console.log('Click nút "Lưu thành viên".');
    await page.getByTestId('save-member-button').click();
    await waitForSnackbar(page, 'success');
    console.log('✅ Đã lưu thành viên thành công.');

    console.log('Bước 2: Xác minh thành viên mới được tạo hiển thị trong danh sách.');
    await page.getByTestId('member-search-expand-button').click();
    await fillVuetifyInput(page, 'member-search-input', memberLastName);
    await page.getByTestId('apply-filters-button').click();
    await waitForVDataTableLoaded(page);
    await expect(page.getByText(`${memberFirstName} ${memberLastName}`)).toBeVisible();
    console.log('✅ Đã thêm thành viên.');
  });
});
