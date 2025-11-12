import { test, expect } from '@playwright/test';
import { login } from '../login.setup';
import { fillVuetifyInput, selectVuetifyOption, waitForSnackbar, takeScreenshotOnFailure, waitForVDataTableLoaded } from '../helpers/vuetify';

test.describe('Member Management - Delete Member', () => {
  test.beforeEach(async ({ page }) => {
    console.log('Đăng nhập trước khi chạy mỗi bài kiểm thử.');
    await login(page);
  });

  test.afterEach(async ({ page }, testInfo) => {
    console.log('Chụp ảnh màn hình nếu bài kiểm thử thất bại.');
    await takeScreenshotOnFailure(page, testInfo);
  });

  test('should allow a user to delete a member', async ({ page }) => {
    const memberFirstName = 'Thành viên';
    const memberLastName = `Xóa ${new Date().getTime()}`;
    const memberNickname = 'Biệt danh xóa';
    const memberPlaceOfBirth = 'Hà Nội';
    const memberPlaceOfDeath = 'TP. Hồ Chí Minh';
    const memberOccupation = 'Kỹ sư';

    console.log('Bước 1: Tạo dữ liệu thành viên cần xóa.');
    await Promise.all([
      page.waitForURL('**/member'),
      page.getByRole('link', { name: 'Quản lý thành viên' }).click(),
    ]);

    await Promise.all([
      page.waitForURL('**/member/add'),
      page.getByTestId('add-new-member-button').click(),
    ]);

    await fillVuetifyInput(page, 'member-first-name-input', memberFirstName);
    await fillVuetifyInput(page, 'member-last-name-input', memberLastName);
    await fillVuetifyInput(page, 'member-nickname-input', memberNickname);

    await page.locator('.mdi-calendar').first().click();
    await page.locator('.v-date-picker-month__day-btn').first().click();

    await page.locator('.mdi-calendar').nth(1).click();
    await page.locator('.v-date-picker-month__day-btn').last().click();

    await fillVuetifyInput(page, 'member-place-of-birth-input', memberPlaceOfBirth);
    await fillVuetifyInput(page, 'member-place-of-death-input', memberPlaceOfDeath);
    await fillVuetifyInput(page, 'member-occupation-input', memberOccupation);

    await selectVuetifyOption(page, 'member-gender-select', 0);
    await selectVuetifyOption(page, 'member-family-select', 0);

    await page.getByTestId('save-member-button').click();
    await waitForSnackbar(page, 'success');
    console.log('✅ Đã thêm thành viên mới để xóa.');

    console.log('Bước 2: Điều hướng về danh sách thành viên và tìm kiếm.');
    await Promise.all([
      page.waitForURL('**/member'),
      page.getByRole('link', { name: 'Quản lý thành viên' }).click(),
    ]);

    await page.getByTestId('member-search-expand-button').click();
    await fillVuetifyInput(page, 'member-list-search-input', memberLastName);
    await page.getByTestId('apply-filters-button').click();
    await waitForVDataTableLoaded(page);

    await expect(page.getByText(`${memberFirstName} ${memberLastName}`)).toBeVisible();
    console.log('✅ Đã tìm thấy thành viên.');

    console.log('Bước 3: Xóa thành viên.');
    const memberRow = page.locator(`tr:has-text("${memberFirstName} ${memberLastName}")`);
    await memberRow.getByTestId('delete-member-button').click();

    const confirmationDialog = page.locator('.v-dialog');
    await expect(confirmationDialog).toBeVisible();
    await confirmationDialog.getByTestId('confirm-delete-button').click();

    console.log('Bước 4: Xác minh thành viên không còn hiển thị trong danh sách.');
    await waitForSnackbar(page, 'success');
    await expect(page.getByText(`${memberFirstName} ${memberLastName}`)).not.toBeVisible();
    console.log('✅ Đã xóa thành viên.');
  });
});
