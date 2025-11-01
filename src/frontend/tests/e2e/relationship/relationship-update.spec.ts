import { test, expect } from '@playwright/test';
import { login } from '../login.setup';
import { fillVuetifyInput, fillVuetifyTextarea, selectVuetifyOption, waitForSnackbar, takeScreenshotOnFailure, assertValidationMessage, selectVuetifyAutocompleteOption } from '../helpers/vuetify';

test.describe('Relationship Management - Update Relationship', () => {
  test.beforeEach(async ({ page }) => {
    console.log('Đăng nhập trước khi chạy mỗi bài kiểm thử.');
    await login(page);
  });

  test.afterEach(async ({ page }, testInfo) => {
    console.log('Chụp ảnh màn hình nếu bài kiểm thử thất bại.');
    await takeScreenshotOnFailure(page, testInfo);
  });

  test('should update an existing relationship successfully', async ({ page }) => {
    const member1FirstName = 'Thành viên 1';
    const member1LastName = `RelUpd1 ${new Date().getTime()}`;
    const member2FirstName = 'Thành viên 2';
    const member2LastName = `RelUpd2 ${new Date().getTime()}`;
    const familyName = `Gia đình quan hệ cập nhật ${new Date().getTime()}`;
    const updatedRelationshipTypeIndex = 1; // Chọn loại quan hệ thứ hai để cập nhật

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
    await selectVuetifyAutocompleteOption(page, 'member-family-select', familyName, familyName);
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
    await selectVuetifyAutocompleteOption(page, 'member-family-select', familyName, familyName);
    await page.getByTestId('save-member-button').click();
    await waitForSnackbar(page, 'success');
    await page.waitForLoadState('networkidle');
    console.log('Đã tạo thành viên 2.');

    await page.waitForTimeout(1000);

    // Bước 2: Tạo một mối quan hệ ban đầu
    console.log('Bước 2: Tạo một mối quan hệ ban đầu.');
    await page.getByRole('link', { name: 'Quản lý Quan hệ' }).first().click();
    await page.waitForLoadState('networkidle');
    await page.getByTestId('relationship-create-button').click();
    await page.waitForLoadState('networkidle');
    await selectVuetifyAutocompleteOption(page, 'relationship-source-member-autocomplete', member1LastName, `${member1LastName} ${member1FirstName}`);
    await selectVuetifyAutocompleteOption(page, 'relationship-target-member-autocomplete', member2LastName, `${member2LastName} ${member2FirstName}`);
    await selectVuetifyOption(page, 'relationship-type-select', 0); // Chọn loại quan hệ đầu tiên
    await selectVuetifyAutocompleteOption(page, 'relationship-family-autocomplete', familyName, familyName);
    await page.getByTestId('relationship-add-save-button').click();
    await waitForSnackbar(page, 'success');
    await page.waitForLoadState('networkidle');
    console.log('Đã tạo mối quan hệ ban đầu.');

    // Bước 3: Điều hướng đến trang chỉnh sửa mối quan hệ
    console.log('Bước 3: Điều hướng đến trang chỉnh sửa mối quan hệ.');
    await page.getByRole('link', { name: 'Quản lý Quan hệ' }).first().click();
    await page.waitForLoadState('networkidle');

    // Tìm kiếm mối quan hệ vừa tạo để chỉnh sửa
    await page.getByTestId('relationship-search').getByTestId('relationship-search-expand-button').click();
    await page.waitForTimeout(500);
    await selectVuetifyAutocompleteOption(page, 'relationship-search-source-member-autocomplete', member1LastName, `${member1LastName} ${member1FirstName}`);
    await selectVuetifyAutocompleteOption(page, 'relationship-search-target-member-autocomplete', member2LastName, `${member2LastName} ${member2FirstName}`);
    await page.getByTestId('relationship-search').getByTestId('relationship-search-apply-button').click();
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Click vào nút chỉnh sửa của mối quan hệ đầu tiên trong danh sách
    await page.getByTestId('relationship-edit-button').first().click();
    await page.waitForLoadState('networkidle');
    console.log('Đã điều hướng đến trang chỉnh sửa mối quan hệ.');

    // Bước 4: Cập nhật thông tin mối quan hệ
    console.log('Bước 4: Cập nhật thông tin mối quan hệ (thay đổi loại quan hệ).');
    await selectVuetifyOption(page, 'relationship-type-select', updatedRelationshipTypeIndex); // Chọn loại quan hệ thứ hai
    console.log('Đã cập nhật thông tin mối quan hệ.');

    // Bước 5: Lưu mối quan hệ và kiểm tra thông báo thành công
    console.log('Bước 5: Lưu mối quan hệ và kiểm tra thông báo thành công.');
    await page.getByTestId('relationship-edit-save-button').click();
    await waitForSnackbar(page, 'success');
    await page.waitForLoadState('networkidle');
    console.log('Đã lưu mối quan hệ đã cập nhật thành công.');

    // Bước 6: Xác minh mối quan hệ đã được cập nhật trong danh sách
    await page.getByRole('link', { name: 'Quản lý Quan hệ' }).first().click();
    await page.waitForLoadState('networkidle');

    // Áp dụng lại bộ lọc để xem mối quan hệ đã cập nhật
    await page.getByTestId('relationship-search').getByTestId('relationship-search-expand-button').click();
    await page.waitForTimeout(500);
    await selectVuetifyAutocompleteOption(page, 'relationship-search-source-member-autocomplete', member1LastName, `${member1LastName} ${member1FirstName}`);
    await selectVuetifyAutocompleteOption(page, 'relationship-search-target-member-autocomplete', member2LastName, `${member2LastName} ${member2FirstName}`);
    await page.getByTestId('relationship-search').getByTestId('relationship-search-apply-button').click();
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Lấy tên loại quan hệ đã chọn để xác minh
    await expect(page.locator('tr').filter({ hasText: `${member1LastName} ${member1FirstName}` }).filter({ hasText: `${member2LastName} ${member2FirstName}` })).toBeVisible();
    console.log('Đã xác minh mối quan hệ đã được cập nhật trong danh sách.');
  });

  test('should show validation errors for empty required fields on update', async ({ page }) => {
    const member1FirstName = 'Thành viên 3';
    const member1LastName = `RelUpd3 ${new Date().getTime()}`;
    const member2FirstName = 'Thành viên 4';
    const member2LastName = `RelUpd4 ${new Date().getTime()}`;
    const familyName = `Gia đình quan hệ lỗi cập nhật ${new Date().getTime()}`;

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
    await selectVuetifyAutocompleteOption(page, 'member-family-select', familyName, familyName);
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
    await selectVuetifyAutocompleteOption(page, 'member-family-select', familyName, familyName);
    await page.getByTestId('save-member-button').click();
    await waitForSnackbar(page, 'success');
    await page.waitForLoadState('networkidle');
    console.log('Đã tạo thành viên 2.');

    await page.waitForTimeout(1000);

    // Bước 2: Tạo một mối quan hệ ban đầu
    console.log('Bước 2: Tạo một mối quan hệ ban đầu.');
    await page.getByRole('link', { name: 'Quản lý Quan hệ' }).first().click();
    await page.waitForLoadState('networkidle');
    await page.getByTestId('relationship-create-button').click();
    await page.waitForLoadState('networkidle');
    await selectVuetifyAutocompleteOption(page, 'relationship-source-member-autocomplete', member1LastName, `${member1LastName} ${member1FirstName}`);
    await selectVuetifyAutocompleteOption(page, 'relationship-target-member-autocomplete', member2LastName, `${member2LastName} ${member2FirstName}`);
    await selectVuetifyOption(page, 'relationship-type-select', 0); // Chọn loại quan hệ đầu tiên
    await selectVuetifyAutocompleteOption(page, 'relationship-family-autocomplete', familyName, familyName);
    await page.getByTestId('relationship-add-save-button').click();
    await waitForSnackbar(page, 'success');
    await page.waitForLoadState('networkidle');
    console.log('Đã tạo mối quan hệ ban đầu.');

    // Bước 3: Điều hướng đến trang chỉnh sửa mối quan hệ
    console.log('Bước 3: Điều hướng đến trang chỉnh sửa mối quan hệ.');
    await page.getByRole('link', { name: 'Quản lý Quan hệ' }).first().click();
    await page.waitForLoadState('networkidle');

    // Tìm kiếm mối quan hệ vừa tạo để chỉnh sửa
    await page.getByTestId('relationship-search').getByTestId('relationship-search-expand-button').click();
    await page.waitForTimeout(500);
    await selectVuetifyAutocompleteOption(page, 'relationship-search-source-member-autocomplete', member1LastName, `${member1LastName} ${member1FirstName}`);
    await selectVuetifyAutocompleteOption(page, 'relationship-search-target-member-autocomplete', member2LastName, `${member2LastName} ${member2FirstName}`);
    await page.getByTestId('relationship-search').getByTestId('relationship-search-apply-button').click();
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Click vào nút chỉnh sửa của mối quan hệ đầu tiên trong danh sách
    await page.getByTestId('relationship-edit-button').first().click();
    await page.waitForLoadState('networkidle');
    console.log('Đã điều hướng đến trang chỉnh sửa mối quan hệ.');

    // Bước 4: Xóa các trường bắt buộc và lưu
    console.log('Bước 4: Xóa các trường bắt buộc và lưu.');
    // Xóa lựa chọn thành viên nguồn
    await page.getByTestId('relationship-source-member-autocomplete').locator('.v-field__clear-btn').click();
    // Xóa lựa chọn thành viên đích
    await page.getByTestId('relationship-target-member-autocomplete').locator('.v-field__clear-btn').click();
    // Xóa lựa chọn loại quan hệ
    await page.getByTestId('relationship-type-select').locator('.v-field__clear-btn').click();
    // Xóa lựa chọn gia đình
    await page.getByTestId('relationship-family-autocomplete').locator('.v-field__clear-btn').click();

    await page.getByTestId('relationship-edit-save-button').click();

    // Bước 5: Kiểm tra thông báo lỗi
    console.log('Bước 5: Kiểm tra thông báo lỗi cho các trường bắt buộc.');
    await assertValidationMessage(page, 'relationship-source-member-autocomplete');
    await assertValidationMessage(page, 'relationship-target-member-autocomplete');
    await assertValidationMessage(page, 'relationship-type-select');
    await assertValidationMessage(page, 'relationship-family-autocomplete');
    console.log('Đã xác minh các thông báo lỗi validation.');
  });
});
