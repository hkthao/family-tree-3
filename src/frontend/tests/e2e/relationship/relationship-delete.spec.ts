import { test, expect } from '@playwright/test';
import { login } from '../login.setup';
import {
  selectVuetifyAutocompleteOption,
  waitForSnackbar,
  takeScreenshotOnFailure,
  fillVuetifyInput,
  fillVuetifyTextarea,
  selectVuetifyOption
} from '../helpers/vuetify';

test.describe('Relationship Management - Delete Relationship', () => {
  test.beforeEach(async ({ page }) => {
    console.log('Đăng nhập trước khi chạy mỗi bài kiểm thử.');
    await login(page);
  });

  test.afterEach(async ({ page }, testInfo) => {
    console.log('Chụp ảnh màn hình nếu bài kiểm thử thất bại.');
    await takeScreenshotOnFailure(page, testInfo);
  });

  test('should delete a relationship successfully', async ({ page }) => {
    const member1FirstName = 'Thành viên 1';
    const member1LastName = `RelDel1 ${new Date().getTime()}`;
    const member2FirstName = 'Thành viên 2';
    const member2LastName = `RelDel2 ${new Date().getTime()}`;
    const familyName = `Gia đình quan hệ Xóa ${new Date().getTime()}`;

    // Bước 1: Tạo dữ liệu cần thiết (gia đình, thành viên, và mối quan hệ)
    console.log('Bước 1: Tạo dữ liệu cần thiết.');

    // Tạo gia đình
    await page.getByRole('link', { name: 'Quản lý gia đình/dòng họ' }).click();
    await expect(page).toHaveURL(/.*\/family/);
    await expect(page.getByTestId('add-new-family-button')).toBeVisible();
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

    // Tạo thành viên 1
    await page.getByRole('link', { name: 'Quản lý thành viên' }).click();
    await expect(page).toHaveURL(/.*\/member/);
    await expect(page.getByTestId('add-new-member-button')).toBeVisible();
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
    await page.waitForTimeout(1000);

    // Tạo mối quan hệ
    await page.getByRole('link', { name: 'Quản lý Quan hệ' }).click();
    await expect(page).toHaveURL(/.*\/relationship/);
    await page.getByTestId('relationship-create-button').click();
    await selectVuetifyAutocompleteOption(page, 'relationship-family-autocomplete', familyName, familyName);
    await selectVuetifyAutocompleteOption(page, 'relationship-source-member-autocomplete', member1LastName, `${member1LastName} ${member1FirstName}`);
    await selectVuetifyAutocompleteOption(page, 'relationship-target-member-autocomplete', member2LastName, `${member2LastName} ${member2FirstName}`);
    await selectVuetifyOption(page, 'relationship-type-select', 0);
    await page.getByTestId('relationship-add-save-button').click();
    await waitForSnackbar(page, 'success');
    await page.waitForLoadState('networkidle');
    console.log('Bước 1 hoàn tất: Đã tạo dữ liệu cần thiết.');

    await page.waitForTimeout(1000);
    // Bước 2: Tìm mối quan hệ cần xóa
    console.log('Bước 2: Tìm mối quan hệ cần xóa.');
    await page.getByTestId('relationship-search').getByTestId('relationship-search-expand-button').click();
    await expect(page.getByTestId('relationship-search-source-member-autocomplete')).toBeVisible();
    await selectVuetifyAutocompleteOption(page, 'relationship-search-source-member-autocomplete', member1LastName, `${member1LastName} ${member1FirstName}`);
    await selectVuetifyAutocompleteOption(page, 'relationship-search-target-member-autocomplete', member2LastName, `${member2LastName} ${member2FirstName}`);
    await page.getByTestId('relationship-search').getByTestId('relationship-search-apply-button').click();
    
    const rowLocator = page.locator('tr', { has: page.getByText(`${member1LastName} ${member1FirstName}`) }).filter({ has: page.getByText(`${member2LastName} ${member2FirstName}`) });
    await expect(rowLocator).toBeVisible();
    console.log('Đã tìm thấy mối quan hệ.');

    // Bước 3: Xóa mối quan hệ
    console.log('Bước 3: Xóa mối quan hệ.');
    await rowLocator.getByTestId('relationship-delete-button').click();

    // Chờ dialog xác nhận xuất hiện và click nút xóa
    const confirmationDialog = page.locator('.v-dialog');
    await expect(confirmationDialog).toBeVisible();
    await confirmationDialog.getByTestId('confirm-delete-button').click();

    // Bước 4: Xác minh mối quan hệ đã bị xóa
    console.log('Bước 4: Xác minh mối quan hệ đã bị xóa.');
    await waitForSnackbar(page, 'success');
    await expect(rowLocator).toBeHidden();
    console.log('Đã xác minh mối quan hệ bị xóa thành công.');
  });
});
