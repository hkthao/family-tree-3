import { test, expect } from '@playwright/test';
import { login } from '../login.setup';
import { fillVuetifyInput, fillVuetifyTextarea, selectVuetifyOption, waitForSnackbar, takeScreenshotOnFailure, assertValidationMessage, selectVuetifyAutocompleteOption } from '../helpers/vuetify';

test.describe('Relationship Management - Create Relationship', () => {
  test.beforeEach(async ({ page }) => {
    console.log('Đăng nhập trước khi chạy mỗi bài kiểm thử.');
    await login(page);
  });

  test.afterEach(async ({ page }, testInfo) => {
    console.log('Chụp ảnh màn hình nếu bài kiểm thử thất bại.');
    await takeScreenshotOnFailure(page, testInfo);
  });

  test('should create a new relationship successfully', async ({ page }) => {
    const member1FirstName = 'Thành viên 1';
    const member1LastName = `RelAdd1 ${new Date().getTime()}`;
    const member2FirstName = 'Thành viên 2';
    const member2LastName = `RelAdd2 ${new Date().getTime()}`;
    const familyName = `Gia đình quan hệ ${new Date().getTime()}`;

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
    // Bước 2: Điều hướng đến trang tạo mối quan hệ
    console.log('Bước 2: Điều hướng đến trang tạo mối quan hệ.');
    await page.getByRole('link', { name: 'Quản lý Quan hệ' }).waitFor({ state: 'visible' });
    await expect(page.getByRole('link', { name: 'Quản lý Quan hệ' })).toBeEnabled();
    await page.getByRole('link', { name: 'Quản lý Quan hệ' }).click();
    await page.waitForLoadState('networkidle');
    await page.getByTestId('relationship-create-button').waitFor({ state: 'visible' });
    await page.getByTestId('relationship-create-button').click();
    await page.waitForLoadState('networkidle');
    console.log('Đã điều hướng đến trang tạo mối quan hệ.');

    // Bước 3: Điền thông tin mối quan hệ
    console.log('Bước 3: Điền thông tin mối quan hệ.');
    await selectVuetifyAutocompleteOption(page, 'relationship-source-member-autocomplete', member1LastName, `${member1LastName} ${member1FirstName}`);
    await selectVuetifyAutocompleteOption(page, 'relationship-target-member-autocomplete', member2LastName, `${member2LastName} ${member2FirstName}`);
    await selectVuetifyOption(page, 'relationship-type-select', 0); // Chọn loại quan hệ đầu tiên
    await selectVuetifyAutocompleteOption(page, 'relationship-family-autocomplete', familyName, familyName);
    console.log('Đã điền thông tin mối quan hệ.');

    // Bước 4: Lưu mối quan hệ và kiểm tra thông báo thành công
    console.log('Bước 4: Lưu mối quan hệ và kiểm tra thông báo thành công.');
    await page.getByTestId('relationship-add-save-button').click();
    await waitForSnackbar(page, 'success');
    await page.waitForLoadState('networkidle');
    console.log('Đã lưu mối quan hệ thành công.');

    // Bước 5: Điều hướng về danh sách mối quan hệ và tìm kiếm.
    console.log('Mở rộng bộ lọc tìm kiếm.');
    await page.getByTestId('relationship-search').getByTestId('relationship-search-expand-button').click();
    await page.waitForTimeout(500); // Chờ animation
    console.log('Chọn thành viên nguồn trong bộ lọc tìm kiếm.');
    await selectVuetifyAutocompleteOption(page, 'relationship-search-source-member-autocomplete', member1LastName, `${member1LastName} ${member1FirstName}`);

    await page.getByTestId('relationship-search').getByTestId('relationship-search-target-member-autocomplete').click();
    await page.getByTestId('relationship-search').getByTestId('relationship-search-target-member-autocomplete').locator('input').pressSequentially(member2LastName, { delay: 100 });
    await page.waitForTimeout(500);
    await page.waitForSelector('.v-overlay-container .v-list-item');
    await page.locator('.v-overlay-container .v-list-item').filter({ hasText: `${member2LastName} ${member2FirstName}` }).click();

    console.log('Click nút "Áp dụng bộ lọc".');
    await page.getByTestId('relationship-search').getByTestId('relationship-search-apply-button').click();
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000); // Chờ kết quả tìm kiếm hiển thị

    console.log('Xác minh mối quan hệ vừa tạo hiển thị trong danh sách.');
    const text_1 = `${member1LastName} ${member1FirstName}`;
    const text_2 = `${member2LastName} ${member2FirstName}`;
    await expect(page.locator('tr').filter({ hasText: text_1 })).toBeVisible();
    await expect(page.locator('tr').filter({ hasText: text_2 })).toBeVisible();
    console.log('Đã xác minh mối quan hệ mới trong danh sách.');
  });

  test('should show validation errors for empty required fields', async ({ page }) => {
    console.log('Điều hướng đến trang quản lý Quan hệ.');
    await page.getByRole('link', { name: 'Quản lý Quan hệ' }).click();
    await page.waitForLoadState('networkidle');

    console.log('Click nút "Thêm mối quan hệ mới".');
    await page.getByTestId('relationship-create-button').click();
    await page.waitForLoadState('networkidle');

    console.log('Click nút "Lưu mối quan hệ" mà không điền bất kỳ trường nào.');
    await page.getByTestId('relationship-add-save-button').click();

    console.log('Kiểm tra thông báo lỗi cho các trường bắt buộc.');
    await assertValidationMessage(page, 'relationship-source-member-autocomplete');
    await assertValidationMessage(page, 'relationship-target-member-autocomplete');
    await assertValidationMessage(page, 'relationship-type-select');
    await assertValidationMessage(page, 'relationship-family-autocomplete');

    console.log('Đã xác minh các thông báo lỗi validation.');
  });
});
