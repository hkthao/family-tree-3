import { test, expect } from '@playwright/test';
import { login } from '../login.setup';
import { fillVuetifyInput, selectVuetifyOption, waitForSnackbar, takeScreenshotOnFailure, waitForVDataTableLoaded } from '../helpers/vuetify';

test.describe('Member Management - Update Member - Success Case', () => {
  test.beforeEach(async ({ page }) => {
    console.log('Đăng nhập trước khi chạy mỗi bài kiểm thử.');
    await login(page);
  });

  test.afterEach(async ({ page }, testInfo) => {
    console.log('Chụp ảnh màn hình nếu bài kiểm thử thất bại.');
    await takeScreenshotOnFailure(page, testInfo);
  });

  test('should allow a user to update a member successfully', async ({ page }) => {
    const memberFirstName = 'Thành viên';
    const memberLastName = `Cập nhật ${new Date().getTime()}`;
    const memberNickname = 'Biệt danh';
    const memberPlaceOfBirth = 'Hà Nội';
    const memberPlaceOfDeath = 'TP. Hồ Chí Minh';
    const memberOccupation = 'Kỹ sư';

    console.log('Bước 1: Tạo dữ liệu thành viên.');
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
    console.log('✅ Đã thêm thành viên mới để cập nhật.');

    console.log('Bước 2: Điều hướng về danh sách thành viên và tìm kiếm.');
    await Promise.all([
      page.waitForURL('**/member'),
      page.getByRole('link', { name: 'Quản lý thành viên' }).click(),
    ]);

    await page.getByTestId('member-search-expand-button').click();
    await fillVuetifyInput(page, 'member-search-input', memberLastName);
    await page.getByTestId('apply-filters-button').click();
    await waitForVDataTableLoaded(page);

    await expect(page.getByText(`${memberFirstName} ${memberLastName}`)).toBeVisible();

    console.log('Bước 3: Click nút chỉnh sửa thành viên.');
    const memberRow = page.locator(`tr:has-text("${memberFirstName} ${memberLastName}")`);
    await Promise.all([
      page.waitForURL(/.*\/member\/edit\/.*/),
      memberRow.getByTestId('edit-member-button').click(),
    ]);

    console.log('Bước 4: Cập nhật thông tin thành viên.');
    const updatedNickname = 'Biệt danh mới';
    const updatedOccupation = 'Kỹ sư phần mềm';
    await fillVuetifyInput(page, 'member-nickname-input', updatedNickname);
    await fillVuetifyInput(page, 'member-occupation-input', updatedOccupation);

    await page.getByTestId('save-member-button').click();
    await waitForSnackbar(page, 'success');

    console.log('Bước 5: Điều hướng về danh sách thành viên và tìm kiếm lại.');
    await Promise.all([
      page.waitForURL('**/member'),
      page.getByRole('link', { name: 'Quản lý thành viên' }).click(),
    ]);

    await page.getByTestId('member-search-expand-button').click();
    await fillVuetifyInput(page, 'member-search-input', memberLastName);
    await page.getByTestId('apply-filters-button').click();
    await waitForVDataTableLoaded(page);

    await expect(page.getByText(`${memberFirstName} ${memberLastName}`)).toBeVisible();

    console.log('Bước 6: Click vào tên thành viên để xem chi tiết.');
    await page.getByText(`${memberFirstName} ${memberLastName}`).click();
    await Promise.all([
      page.waitForURL(/.*\/member\/detail\/.*/),
    ]);
    console.log('Bước 7: Xác minh thông tin chi tiết thành viên đã được cập nhật.');
    await expect(page.getByTestId('member-nickname-input').locator('input')).toHaveValue(updatedNickname);
    await expect(page.getByTestId('member-occupation-input').locator('input')).toHaveValue(updatedOccupation);
    console.log('✅ Đã xác minh thông tin thành viên đã được cập nhật.');
  });
});
