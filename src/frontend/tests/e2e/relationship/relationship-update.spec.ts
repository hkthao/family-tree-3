import { test, expect } from '@playwright/test';
import { login } from '../login.setup';
import {
  selectVuetifyAutocompleteOption,
  waitForSnackbar,
  takeScreenshotOnFailure,
  fillVuetifyInput,
  fillVuetifyTextarea,
  selectVuetifyOption,
  assertValidationMessage
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
    const updatedRelationshipTypeIndex = 1; // Index của loại quan hệ mới

    // Bước 1: Tạo dữ liệu cần thiết (gia đình, thành viên, và mối quan hệ ban đầu)
    console.log('Bước 1: Tạo dữ liệu cần thiết.');

    // Tạo gia đình
    await page.getByRole('link', { name: 'Quản lý gia đình/dòng họ' }).click();
    await expect(page).toHaveURL(/.*\/family/);
    await page.getByTestId('add-new-family-button').click();
    await fillVuetifyInput(page, 'family-name-input', familyName);
    await fillVuetifyTextarea(page, 'family-description-input', 'Mô tả');
    await page.getByTestId('button-save').click();
    await waitForSnackbar(page, 'success');
    await page.waitForLoadState('networkidle');

    // Tạo thành viên 1
    await page.getByRole('link', { name: 'Quản lý thành viên' }).click();
    await expect(page).toHaveURL(/.*\/member/);
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
    await page.waitForTimeout(1000);

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
    await selectVuetifyOption(page, 'relationship-type-select', 0); // Chọn loại ban đầu
    await page.getByTestId('relationship-add-save-button').click();
    await waitForSnackbar(page, 'success');
    await page.waitForLoadState('networkidle');
    console.log('Bước 1 hoàn tất: Đã tạo dữ liệu cần thiết.');
    await page.waitForTimeout(1000);

    // Bước 2: Tìm và mở trang chỉnh sửa mối quan hệ
    console.log('Bước 2: Tìm và mở trang chỉnh sửa.');
    await page.getByTestId('relationship-search').getByTestId('relationship-search-expand-button').click();
    await selectVuetifyAutocompleteOption(page, 'relationship-search-source-member-autocomplete', member1LastName, `${member1LastName} ${member1FirstName}`);
    await page.getByTestId('relationship-search').getByTestId('relationship-search-apply-button').click();

    const rowLocator = page.locator('tr', { has: page.getByText(`${member1LastName} ${member1FirstName}`) });
    await expect(rowLocator).toBeVisible();
    await rowLocator.getByTestId('relationship-edit-button').click();
    await expect(page).toHaveURL(/.*\/relationship\/edit\/.*/);
    console.log('Đã vào trang chỉnh sửa.');

    // Bước 3: Cập nhật thông tin
    console.log('Bước 3: Cập nhật loại quan hệ.');

    // Mở dropdown và lấy text của loại quan hệ mới
    await page.getByTestId('relationship-type-select').click();
    await expect(page.locator('.v-overlay-container .v-list-item')).not.toHaveCount(0);
    const allTypeOptions = await page.locator('.v-overlay-container .v-list-item-title').allTextContents();
    const updatedRelationshipTypeText = allTypeOptions[updatedRelationshipTypeIndex].trim();
    await page.locator('.v-overlay-container .v-list-item').nth(updatedRelationshipTypeIndex).click();
    await page.getByTestId('relationship-edit-save-button').click();

    // Bước 4: Xác minh sự thay đổi
    console.log('Bước 4: Xác minh sự thay đổi.');
    await waitForSnackbar(page, 'success');
    await expect(page).toHaveURL(/.*\/relationship/);

    // Áp dụng lại bộ lọc và kiểm tra với text đã lấy được
    await page.getByTestId('relationship-search').getByTestId('relationship-search-expand-button').click();
    await selectVuetifyAutocompleteOption(page, 'relationship-search-source-member-autocomplete', member1LastName, `${member1LastName} ${member1FirstName}`);
    await page.getByTestId('relationship-search').getByTestId('relationship-search-apply-button').click();
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    await expect(page.locator('tr').filter({ hasText: updatedRelationshipTypeText })).toBeVisible();
    console.log('Đã xác minh mối quan hệ được cập nhật thành công.');
  });
});
