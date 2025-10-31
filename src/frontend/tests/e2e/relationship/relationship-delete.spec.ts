import { test, expect } from '@playwright/test';
import { login } from '../login.setup';
import { fillVuetifyInput, fillVuetifyTextarea, selectVuetifyOption, waitForSnackbar, takeScreenshotOnFailure } from '../helpers/vuetify';

test.describe('Relationship Management - Delete Relationship', () => {
  test.beforeEach(async ({ page }) => {
    console.log('Đăng nhập trước khi chạy mỗi bài kiểm thử.');
    await login(page);
  });

  test.afterEach(async ({ page }, testInfo) => {
    console.log('Chụp ảnh màn hình nếu bài kiểm thử thất bại.');
    await takeScreenshotOnFailure(page, testInfo);
  });

  test('should allow a user to delete a relationship', async ({ page }) => {
    const member1FirstName = 'Thành viên 1';
    const member1LastName = `RelDel1 ${new Date().getTime()}`;
    const member2FirstName = 'Thành viên 2';
    const member2LastName = `RelDel2 ${new Date().getTime()}`;
    const familyName = `Gia đình quan hệ ${new Date().getTime()}`;
    const relationshipType = 'Anh/Chị/Em';

    console.log('Tạo gia đình.');
    await page.getByRole('link', { name: 'Quản lý gia đình/dòng họ' }).click();
    await page.getByTestId('add-new-family-button').click();
    await fillVuetifyInput(page, 'family-name-input', familyName);
    await fillVuetifyInput(page, 'family-address-input', 'Địa chỉ gia đình');
    await fillVuetifyTextarea(page, 'family-description-input', 'Mô tả gia đình');
    await selectVuetifyOption(page, 'family-visibility-select', 0);
    await selectVuetifyOption(page, 'family-managers-select', 0);
    await selectVuetifyOption(page, 'family-viewers-select', 0);
    await page.getByTestId('button-save').click();
    await waitForSnackbar(page, 'success');
    await page.waitForLoadState('networkidle');
    console.log('Đã tạo gia đình.');

    console.log('Tạo thành viên 1.');
    await page.getByRole('link', { name: 'Quản lý thành viên' }).click();
    await page.getByTestId('add-new-member-button').click();
    await fillVuetifyInput(page, 'member-first-name-input', member1FirstName);
    await fillVuetifyInput(page, 'member-last-name-input', member1LastName);
    await page.locator('.mdi-calendar').first().click();
    await page.locator('button[class*="v-date-picker-month__day-btn"]').first().click();
    await selectVuetifyOption(page, 'member-gender-select', 0);
    await selectVuetifyOption(page, 'member-family-select', 0);
    await page.getByTestId('save-member-button').click();
    await waitForSnackbar(page, 'success');
    await page.waitForLoadState('networkidle');
    console.log('Đã tạo thành viên 1.');

    console.log('Tạo thành viên 2.');
    await page.getByTestId('add-new-member-button').click();
    await fillVuetifyInput(page, 'member-first-name-input', member2FirstName);
    await fillVuetifyInput(page, 'member-last-name-input', member2LastName);
    await page.locator('.mdi-calendar').first().click();
    await page.locator('button[class*="v-date-picker-month__day-btn"]').first().click();
    await selectVuetifyOption(page, 'member-gender-select', 0);
    await selectVuetifyOption(page, 'member-family-select', 0);
    await page.getByTestId('save-member-button').click();
    await waitForSnackbar(page, 'success');
    await page.waitForLoadState('networkidle');
    console.log('Đã tạo thành viên 2.');

    console.log('Điều hướng đến trang quản lý Quan hệ để tạo mối quan hệ.');
    await page.getByRole('link', { name: 'Quản lý Quan hệ' }).click();
    await page.waitForLoadState('networkidle');

    console.log('Click nút "Thêm mối quan hệ mới".');
    await page.getByTestId('relationship-create-button').click();
    await page.waitForLoadState('networkidle');

    console.log('Điền thông tin mối quan hệ.');
    await page.getByTestId('relationship-source-member-autocomplete').click();
    await page.waitForSelector('.v-overlay-container .v-list-item');
    await page.locator('.v-overlay-container .v-list-item').filter({ hasText: `${member1LastName} ${member1FirstName}` }).click();

    await page.getByTestId('relationship-target-member-autocomplete').click();
    await page.waitForSelector('.v-overlay-container .v-list-item');
    await page.locator('.v-overlay-container .v-list-item').filter({ hasText: `${member2LastName} ${member2FirstName}` }).click();

    await selectVuetifyOption(page, 'relationship-type-select', 0); // Assuming 'Anh/Chị/Em' is the first option

    await page.getByTestId('relationship-family-autocomplete').click();
    await page.waitForSelector('.v-overlay-container .v-list-item');
    await page.locator('.v-overlay-container .v-list-item').filter({ hasText: familyName }).click();

    console.log('Click nút "Lưu mối quan hệ".');
    await page.getByTestId('relationship-add-save-button').click();
    await waitForSnackbar(page, 'success');
    await page.waitForLoadState('networkidle');
    console.log('Đã lưu mối quan hệ thành công.');

    console.log('Điều hướng về danh sách mối quan hệ và tìm kiếm.');
    await page.getByRole('link', { name: 'Quản lý Quan hệ' }).click();
    await page.waitForLoadState('networkidle');

    console.log('Mở rộng bộ lọc tìm kiếm.');
    await page.getByTestId('relationship-search').getByTestId('relationship-search-expand-button').click();
    await page.waitForTimeout(500); // Chờ animation

    console.log('Chọn thành viên nguồn trong bộ lọc tìm kiếm.');
    await page.getByTestId('relationship-search').getByTestId('relationship-search-source-member-autocomplete').click();
    await page.waitForSelector('.v-overlay-container .v-list-item');
    await page.locator('.v-overlay-container .v-list-item').filter({ hasText: `${member1LastName} ${member1FirstName}` }).click();

    console.log('Click nút "Áp dụng bộ lọc".');
    await page.getByTestId('relationship-search').getByTestId('relationship-search-apply-button').click();
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000); // Chờ kết quả tìm kiếm hiển thị

    console.log('Xác minh mối quan hệ vừa tạo hiển thị trong danh sách.');
    await expect(page.locator('tr').filter({ hasText: relationshipType }).filter({ hasText: `${member1LastName} ${member1FirstName}` }).filter({ hasText: `${member2LastName} ${member2FirstName}` })).toBeVisible();
    console.log('Đã tìm thấy mối quan hệ.');

    console.log('Click nút xóa mối quan hệ.');
    const relationshipRow = page.locator('tr').filter({ hasText: relationshipType }).filter({ hasText: `${member1LastName} ${member1FirstName}` }).filter({ hasText: `${member2LastName} ${member2FirstName}` });
    await relationshipRow.getByTestId('relationship-delete-button').click();

    console.log('Xác nhận xóa.');
    await page.getByTestId('confirm-delete-button').click();

    console.log('Chờ thông báo thành công.');
    await waitForSnackbar(page, 'success');

    console.log('Xác minh mối quan hệ không còn hiển thị trong danh sách.');
    await expect(page.locator('tr').filter({ hasText: relationshipType }).filter({ hasText: `${member1LastName} ${member1FirstName}` }).filter({ hasText: `${member2LastName} ${member2FirstName}` })).not.toBeVisible();
    console.log('Đã xóa mối quan hệ.');
  });
});
