import { test, expect } from '@playwright/test';
import { login } from '../login.setup';
import { fillVuetifyInput, fillVuetifyTextarea, selectVuetifyOption, waitForSnackbar, takeScreenshotOnFailure } from '../helpers/vuetify';

test.describe('Relationship Management - View Relationship Details', () => {
  test.beforeEach(async ({ page }) => {
    console.log('Đăng nhập trước khi chạy mỗi bài kiểm thử.');
    await login(page);
  });

  test.afterEach(async ({ page }, testInfo) => {
    console.log('Chụp ảnh màn hình nếu bài kiểm thử thất bại.');
    await takeScreenshotOnFailure(page, testInfo);
  });

  test('should allow a user to view relationship details', async ({ page }) => {
    const member1FirstName = 'Thành viên 1';
    const member1LastName = `RelView1 ${new Date().getTime()}`;
    const member2FirstName = 'Thành viên 2';
    const member2LastName = `RelView2 ${new Date().getTime()}`;
    const familyName = `Gia đình quan hệ ${new Date().getTime()}`;
    const relationshipType = 'Anh/Chị/Em';

    // Bước 1: Tạo dữ liệu cần thiết (gia đình và thành viên)
    console.log('Bước 1: Tạo dữ liệu cần thiết (gia đình và thành viên).');
    // Tạo gia đình
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

    // Tạo thành viên 1
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

    // Tạo thành viên 2
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

    // Bước 2: Tạo mối quan hệ ban đầu
    console.log('Bước 2: Tạo mối quan hệ ban đầu.');
    await page.getByRole('link', { name: 'Quản lý Quan hệ' }).click();
    await page.waitForLoadState('networkidle');
    await page.getByTestId('relationship-create-button').click();
    await page.waitForLoadState('networkidle');

    await page.getByTestId('relationship-source-member-autocomplete').click();
    await page.waitForSelector('.v-overlay-container .v-list-item');
    await page.locator('.v-overlay-container .v-list-item').filter({ hasText: `${member1LastName} ${member1FirstName}` }).click();

    await page.getByTestId('relationship-target-member-autocomplete').click();
    await page.waitForSelector('.v-overlay-container .v-list-item');
    await page.locator('.v-overlay-container .v-list-item').filter({ hasText: `${member2LastName} ${member2FirstName}` }).click();

    await selectVuetifyOption(page, 'relationship-type-select', 0); // Original type

    await page.getByTestId('relationship-family-autocomplete').click();
    await page.waitForSelector('.v-overlay-container .v-list-item');
    await page.locator('.v-overlay-container .v-list-item').filter({ hasText: familyName }).click();

    await page.getByTestId('relationship-add-save-button').click();
    await waitForSnackbar(page, 'success');
    await page.waitForLoadState('networkidle');
    console.log('Đã lưu mối quan hệ gốc thành công.');

    // Bước 3: Điều hướng về danh sách và tìm kiếm mối quan hệ để xem chi tiết
    console.log('Bước 3: Điều hướng về danh sách và tìm kiếm mối quan hệ để xem chi tiết.');
    await page.getByRole('link', { name: 'Quản lý Quan hệ' }).click();
    await page.waitForLoadState('networkidle');

    await page.getByTestId('relationship-search').getByTestId('relationship-search-expand-button').click();
    await page.waitForTimeout(500); // Chờ animation

    await page.getByTestId('relationship-search').getByTestId('relationship-search-source-member-autocomplete').click();
    await page.waitForSelector('.v-overlay-container .v-list-item');
    await page.locator('.v-overlay-container .v-list-item').filter({ hasText: `${member1LastName} ${member1FirstName}` }).click();

    await page.getByTestId('relationship-search').getByTestId('relationship-search-apply-button').click();
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000); // Chờ kết quả tìm kiếm hiển thị

    await expect(page.locator('tr').filter({ hasText: relationshipType }).filter({ hasText: `${member1LastName} ${member1FirstName}` }).filter({ hasText: `${member2LastName} ${member2FirstName}` })).toBeVisible();
    console.log('Đã tìm thấy mối quan hệ.');

    // Bước 4: Click vào mối quan hệ để xem chi tiết
    console.log('Bước 4: Click vào mối quan hệ để xem chi tiết.');
    await page.locator('tr').filter({ hasText: relationshipType }).filter({ hasText: `${member1LastName} ${member1FirstName}` }).filter({ hasText: `${member2LastName} ${member2FirstName}` }).click();
    await page.waitForLoadState('networkidle');
    console.log('Đã vào trang chi tiết mối quan hệ.');

    // Bước 5: Xác minh thông tin chi tiết mối quan hệ
    console.log('Bước 5: Xác minh thông tin chi tiết mối quan hệ.');
    await expect(page.getByTestId('relationship-type-select').locator('input')).toHaveValue(relationshipType);
    await expect(page.getByTestId('relationship-source-member-autocomplete').locator('input')).toHaveValue(`${member1LastName} ${member1FirstName}`);
    await expect(page.getByTestId('relationship-target-member-autocomplete').locator('input')).toHaveValue(`${member2LastName} ${member2FirstName}`);
    await expect(page.getByTestId('relationship-family-autocomplete').locator('input')).toHaveValue(familyName);
    console.log('Đã xác minh thông tin chi tiết mối quan hệ.');
  });
});