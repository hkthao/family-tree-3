import { test, expect } from '@playwright/test';
import { login } from '../login.setup';
import { fillVuetifyInput, fillVuetifyTextarea, selectVuetifyOption, waitForSnackbar, takeScreenshotOnFailure, assertValidationMessage } from '../helpers/vuetify';

test.describe('Relationship Management - Update Relationship', () => {
  test.beforeEach(async ({ page }) => {
    console.log('Đăng nhập trước khi chạy mỗi bài kiểm thử.');
    await login(page);
  });

  test.afterEach(async ({ page }, testInfo) => {
    console.log('Chụp ảnh màn hình nếu bài kiểm thử thất bại.');
    await takeScreenshotOnFailure(page, testInfo);
  });

  test('should allow a user to update an existing relationship successfully', async ({ page }) => {
    const member1FirstName = 'Thành viên 1';
    const member1LastName = `RelUpd1 ${new Date().getTime()}`;
    const member2FirstName = 'Thành viên 2';
    const member2LastName = `RelUpd2 ${new Date().getTime()}`;
    const familyName = `Gia đình quan hệ ${new Date().getTime()}`;
    const originalRelationshipType = 'Anh/Chị/Em';
    const updatedRelationshipType = 'Vợ/Chồng';

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

    // Bước 3: Điều hướng về danh sách và tìm kiếm mối quan hệ để cập nhật
    console.log('Bước 3: Điều hướng về danh sách và tìm kiếm mối quan hệ để cập nhật.');
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

    await expect(page.locator('tr').filter({ hasText: originalRelationshipType }).filter({ hasText: `${member1LastName} ${member1FirstName}` }).filter({ hasText: `${member2LastName} ${member2FirstName}` })).toBeVisible();

    // Bước 4: Chỉnh sửa mối quan hệ
    console.log('Bước 4: Chỉnh sửa mối quan hệ.');
    const relationshipRow = page.locator('tr').filter({ hasText: originalRelationshipType }).filter({ hasText: `${member1LastName} ${member1FirstName}` }).filter({ hasText: `${member2LastName} ${member2FirstName}` });
    await relationshipRow.getByTestId('relationship-edit-button').click();
    await page.waitForLoadState('networkidle');

    await selectVuetifyOption(page, 'relationship-type-select', 1); // Change to a different type
    await fillVuetifyInput(page, 'relationship-order-input', '1');

    await page.getByTestId('button-save').click();
    await waitForSnackbar(page, 'success');
    await page.waitForLoadState('networkidle');
    console.log('Đã lưu mối quan hệ cập nhật thành công.');

    // Bước 5: Xác minh mối quan hệ đã cập nhật hiển thị trong danh sách
    console.log('Bước 5: Xác minh mối quan hệ đã cập nhật hiển thị trong danh sách.');
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

    await expect(page.locator('tr').filter({ hasText: updatedRelationshipType }).filter({ hasText: `${member1LastName} ${member1FirstName}` }).filter({ hasText: `${member2LastName} ${member2FirstName}` })).toBeVisible();
    console.log('Đã cập nhật mối quan hệ.');
  });

  test('should show validation errors for empty required fields on update', async ({ page }) => {
    const member1FirstName = 'Thành viên 1';
    const member1LastName = `RelUpdVal1 ${new Date().getTime()}`;
    const member2FirstName = 'Thành viên 2';
    const member2LastName = `RelUpdVal2 ${new Date().getTime()}`;
    const familyName = `Gia đình quan hệ ${new Date().getTime()}`;
    const originalRelationshipType = 'Anh/Chị/Em';

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

    // Bước 3: Điều hướng về danh sách và tìm kiếm mối quan hệ để cập nhật
    console.log('Bước 3: Điều hướng về danh sách và tìm kiếm mối quan hệ để cập nhật.');
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

    await expect(page.locator('tr').filter({ hasText: originalRelationshipType }).filter({ hasText: `${member1LastName} ${member1FirstName}` }).filter({ hasText: `${member2LastName} ${member2FirstName}` })).toBeVisible();

    // Bước 4: Chỉnh sửa mối quan hệ và xóa trường bắt buộc
    console.log('Bước 4: Chỉnh sửa mối quan hệ và xóa trường bắt buộc.');
    const relationshipRow = page.locator('tr').filter({ hasText: originalRelationshipType }).filter({ hasText: `${member1LastName} ${member1FirstName}` }).filter({ hasText: `${member2LastName} ${member2FirstName}` });
    await relationshipRow.getByTestId('relationship-edit-button').click();
    await page.waitForLoadState('networkidle');

    // Xóa nội dung của trường 'relationship-order-input' (giả sử đây là trường bắt buộc có thể xóa)
    await fillVuetifyInput(page, 'relationship-order-input', '');

    // Bước 5: Lưu và kiểm tra lỗi validation
    console.log('Bước 5: Lưu và kiểm tra lỗi validation.');
    await page.getByTestId('button-save').click();

    console.log('Kiểm tra thông báo lỗi cho các trường bắt buộc.');
    await assertValidationMessage(page, 'relationship-order-input');

    console.log('Đã xác minh các thông báo lỗi validation.');
  });
});
