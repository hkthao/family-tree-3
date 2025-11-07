import { test, expect } from '@playwright/test';
import { login } from '../login.setup';
import { fillVuetifyInput, selectVuetifyOption, waitForSnackbar, takeScreenshotOnFailure, waitForVDataTableLoaded } from '../helpers/vuetify';

test.describe('Member Management - View Member Details', () => {
  test.beforeEach(async ({ page }) => {
    console.log('Đăng nhập trước khi chạy mỗi bài kiểm thử.');
    await login(page);
  });

  test.afterEach(async ({ page }, testInfo) => {
    console.log('Chụp ảnh màn hình nếu bài kiểm thử thất bại.');
    await takeScreenshotOnFailure(page, testInfo);
  });

  test('should allow a user to view member details', async ({ page }) => {
    const memberFirstName = 'Thành viên';
    const memberLastName = `Xem ${new Date().getTime()}`;
    const memberNickname = 'Biệt danh xem';
    const memberPlaceOfBirth = 'Hà Nội';
    const memberPlaceOfDeath = 'TP. Hồ Chí Minh';
    const memberOccupation = 'Kỹ sư phần mềm';

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
    console.log('✅ Đã thêm thành viên mới để xem chi tiết.');

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

    console.log('Bước 3: Click vào tên thành viên để xem chi tiết.');
    await Promise.all([
      page.waitForURL(/.*\/member\/detail\/.*/),
      page.getByText(`${memberFirstName} ${memberLastName}`).click(),
    ]);
    console.log('✅ Đã vào trang chi tiết thành viên.');

    console.log('Bước 4: Xác minh thông tin chi tiết thành viên.');
    await expect(page.getByTestId('member-first-name-input').locator('input')).toHaveValue(memberFirstName);
    await expect(page.getByTestId('member-last-name-input').locator('input')).toHaveValue(memberLastName);
    await expect(page.getByTestId('member-nickname-input').locator('input')).toHaveValue(memberNickname);
    await expect(page.getByTestId('member-occupation-input').locator('input')).toHaveValue(memberOccupation);
    console.log('✅ Đã xác minh thông tin chi tiết thành viên.');
  });
});
