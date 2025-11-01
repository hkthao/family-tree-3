import { test, expect } from '@playwright/test';
import { login } from '../login.setup';
import {
  selectVuetifyAutocompleteOption,
  waitForSnackbar,
  takeScreenshotOnFailure,
  fillVuetifyInput,
  fillVuetifyTextarea,
  selectVuetifyOption,
  waitForVDataTableLoaded,
} from '../helpers/vuetify';

test.describe('Relationship Management - Update Relationship', () => {
  test.beforeEach(async ({ page }) => {
    console.log('Đăng nhập trước khi chạy mỗi bài kiểm thử.');
    await login(page);
  });

  test.afterEach(async ({ page }, testInfo) => {
    console.log('Chụp ảnh màn hình nếu bài kiểm thử thất bại.');
    await takeScreenshotOnFailure(page, testInfo);
  });

  test('should update a relationship successfully', async ({ page }) => {
    const member1FirstName = 'Thành viên 1';
    const member1LastName = `RelUpd1 ${new Date().getTime()}`;
    const member2FirstName = 'Thành viên 2';
    const member2LastName = `RelUpd2 ${new Date().getTime()}`;
    const familyName = `Gia đình quan hệ Cập nhật ${new Date().getTime()}`;

    // ==========================================================
    // Bước 1: Tạo dữ liệu cần thiết (Gia đình, Thành viên, Quan hệ)
    // ==========================================================
    console.log('Bước 1: Tạo dữ liệu cần thiết.');

    // Tạo gia đình
    await page.getByRole('link', { name: 'Quản lý gia đình/dòng họ' }).click();
    await Promise.all([
      page.waitForURL('**/family'),
      page.getByTestId('add-new-family-button').waitFor(),
    ]);

    await page.getByTestId('add-new-family-button').click();
    await fillVuetifyInput(page, 'family-name-input', familyName);
    await fillVuetifyTextarea(page, 'family-description-input', 'Mô tả');
    await page.getByTestId('button-save').click();
    await waitForSnackbar(page, 'success');

    // Tạo thành viên 1
    console.log('Tạo thành viên 1');
    await page.getByRole('link', { name: 'Quản lý thành viên' }).click();
    await Promise.all([
      page.waitForURL('**/member'),
      page.getByTestId('add-new-member-button').waitFor(),
    ]);

    await page.getByTestId('add-new-member-button').click();
    await fillVuetifyInput(page, 'member-first-name-input', member1FirstName);
    await fillVuetifyInput(page, 'member-last-name-input', member1LastName);
    await page.locator('.mdi-calendar').first().click();
    await page.locator('button[class*="v-date-picker-month__day-btn"]').first().click();
    await selectVuetifyOption(page, 'member-gender-select', 0);
    await selectVuetifyAutocompleteOption(page, 'member-family-select', familyName, familyName);
    await page.getByTestId('save-member-button').click();
    await waitForSnackbar(page, 'success');

    // Tạo thành viên 2
    console.log('Tạo thành viên 2');
    await page.getByTestId('add-new-member-button').click();
    await fillVuetifyInput(page, 'member-first-name-input', member2FirstName);
    await fillVuetifyInput(page, 'member-last-name-input', member2LastName);
    await page.locator('.mdi-calendar').first().click();
    await page.locator('button[class*="v-date-picker-month__day-btn"]').first().click();
    await selectVuetifyOption(page, 'member-gender-select', 0);
    await selectVuetifyAutocompleteOption(page, 'member-family-select', familyName, familyName);
    await page.getByTestId('save-member-button').click();
    await waitForSnackbar(page, 'success');
    await page.waitForTimeout(1000)

    // Tạo mối quan hệ
    console.log('Tạo mối quan hệ');
    await page.getByRole('link', { name: 'Quản lý Quan hệ' }).click();
    await Promise.all([
      page.waitForURL('**/relationship'),
      page.getByTestId('relationship-create-button').waitFor(),
    ]);

    await page.getByTestId('relationship-create-button').click();
    await page.getByTestId('relationship-family-autocomplete').waitFor();

    await selectVuetifyAutocompleteOption(page, 'relationship-family-autocomplete', familyName, familyName);
    await selectVuetifyAutocompleteOption(page, 'relationship-source-member-autocomplete', member1LastName, `${member1FirstName} ${member1LastName}`);
    await selectVuetifyAutocompleteOption(page, 'relationship-target-member-autocomplete', member2LastName, `${member2FirstName} ${member2LastName}`);
    await selectVuetifyOption(page, 'relationship-type-select', 0);
    await page.getByTestId('relationship-add-save-button').click();
    await waitForSnackbar(page, 'success');

    console.log('✅ Bước 1 hoàn tất: Đã tạo dữ liệu cần thiết.');

    // ==========================================================
    // Bước 2: Tìm và mở trang chỉnh sửa mối quan hệ
    // ==========================================================
    console.log('Bước 2: Tìm và mở trang chỉnh sửa.');
    await page.getByTestId('relationship-search').getByTestId('relationship-search-expand-button').click();
    await selectVuetifyAutocompleteOption(page, 'relationship-search-source-member-autocomplete', member1LastName, `${member1FirstName} ${member1LastName}`);
    await page.getByTestId('relationship-search').getByTestId('relationship-search-apply-button').click();
    await waitForVDataTableLoaded(page);
    const rowLocator = page.locator('tr', { has: page.getByText(`${member1FirstName} ${member1LastName}`) });
    await expect(rowLocator).toBeVisible({ timeout: 10000 });
    await rowLocator.getByTestId('relationship-edit-button').click();

    await Promise.all([
      page.waitForURL(/.*\/relationship\/edit\/.*/),
      page.getByTestId('relationship-edit-save-button').waitFor(),
    ]);

    console.log('Đã vào trang chỉnh sửa.');

    // ==========================================================
    // Bước 3: Cập nhật loại quan hệ
    // ==========================================================
    console.log('Bước 3: Cập nhật loại quan hệ.');
    await selectVuetifyOption(page, 'relationship-type-select', 1);
    await page.getByTestId('relationship-edit-save-button').click();
    await waitForSnackbar(page, 'success');

    // ==========================================================
    // Bước 4: Xác minh sự thay đổi
    // ==========================================================
    console.log('Bước 4: Xác minh sự thay đổi.');
    await Promise.all([
      page.waitForURL('**/relationship'),
      page.getByTestId('relationship-search').waitFor(),
    ]);

    await page.getByTestId('relationship-search').getByTestId('relationship-search-expand-button').click();
    await selectVuetifyAutocompleteOption(page, 'relationship-search-source-member-autocomplete', member1LastName, `${member1FirstName} ${member1LastName}`);
    await page.getByTestId('relationship-search').getByTestId('relationship-search-apply-button').click();

    await expect(page.locator('tr').filter({ hasText: "Mẹ" })).toBeVisible({ timeout: 10000 });
    console.log('✅ Đã xác minh mối quan hệ được cập nhật thành công.');
  });
});
