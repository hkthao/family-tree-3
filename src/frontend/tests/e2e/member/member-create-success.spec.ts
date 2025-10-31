import { test, expect } from '@playwright/test';
import { login } from '../login.setup';
import { fillVuetifyInput, fillVuetifyTextarea, selectVuetifyOption, waitForSnackbar, takeScreenshotOnFailure } from '../helpers/vuetify';

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
    const memberBiography = 'Đây là tiểu sử của thành viên.';

    console.log('Điều hướng đến trang quản lý thành viên.');
    await page.getByRole('link', { name: 'Quản lý thành viên' }).click();
    await page.waitForLoadState('networkidle');

    console.log('Click nút "Thêm thành viên mới".');
    await page.getByTestId('add-new-member-button').click();
    await page.waitForLoadState('networkidle');

    console.log('Điền thông tin thành viên.');
    await fillVuetifyInput(page, 'member-first-name-input', memberFirstName);
    await fillVuetifyInput(page, 'member-last-name-input', memberLastName);
    await fillVuetifyInput(page, 'member-nickname-input', memberNickname);

    console.log('Chọn ngày sinh.');
    await page.locator('.mdi-calendar').first().click();
    await page.locator('button[class="v-btn v-btn--icon v-theme--dark v-btn--density-default v-btn--size-default v-btn--variant-text v-date-picker-month__day-btn"]').first().click();

    console.log('Chọn ngày mất.');
    await page.locator('.mdi-calendar').nth(1).click();
    await page.locator('button[class="v-btn v-btn--icon v-theme--dark v-btn--density-default v-btn--size-default v-btn--variant-text v-date-picker-month__day-btn"]').last().click();

    await fillVuetifyInput(page, 'member-place-of-birth-input', memberPlaceOfBirth);
    await fillVuetifyInput(page, 'member-place-of-death-input', memberPlaceOfDeath);
    await fillVuetifyInput(page, 'member-occupation-input', memberOccupation);
    // await fillVuetifyTextarea(page, 'member-biography-input', memberBiography); // Biography is optional

    console.log('Chọn giới tính.');
    await selectVuetifyOption(page, 'member-gender-select', 0); // Assuming 'Nam' is the first option

    console.log('Chọn gia đình.');
    await selectVuetifyOption(page, 'member-family-select', 0); // Assuming the first family is available

    console.log('Click nút "Lưu thành viên".');
    await page.getByTestId('save-member-button').click();
    await waitForSnackbar(page, 'success');
    await page.waitForLoadState('networkidle');
    console.log('Đã lưu thành viên thành công.');

    console.log('Mở rộng bộ lọc tìm kiếm.');
    await page.getByTestId('member-search-expand-button').click();
    await page.waitForTimeout(500); // Chờ animation

    console.log('Điền tên thành viên vào ô tìm kiếm.');
    await fillVuetifyInput(page, 'member-search-input', memberLastName);

    console.log('Click nút "Áp dụng bộ lọc".');
    await page.getByTestId('apply-filters-button').click();
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000); // Chờ kết quả tìm kiếm hiển thị

    console.log('Xác minh thành viên mới được tạo hiển thị trong danh sách.');
    await expect(page.getByText(`${memberLastName} ${memberFirstName}`)).toBeVisible();
    console.log('Đã thêm thành viên.');
  });
});
