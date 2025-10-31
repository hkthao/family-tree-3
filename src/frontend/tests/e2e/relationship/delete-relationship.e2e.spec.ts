import { test, expect } from '@playwright/test';
import { login } from '../login.setup';

test.describe('Relationship Management - Delete Relationship', () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test('should allow a user to delete a relationship', async ({ page }) => {
    const member1FirstName = 'Thành viên 1';
    const member1LastName = `DelRel1 ${new Date().getTime()}`;
    const member2FirstName = 'Thành viên 2';
    const member2LastName = `DelRel2 ${new Date().getTime()}`;
    const relationshipType = 'Anh/Chị/Em';
    const familyName = `Gia đình quan hệ ${new Date().getTime()}`;

    // 1. Create two members and a family first
    // console.log('Tạo thành viên 1.');
    // await page.getByRole('link', { name: 'Quản lý thành viên' }).click();
    // await page.waitForLoadState('networkidle');
    // await page.getByTestId('add-new-member-button').click();
    // await page.waitForLoadState('networkidle');
    // await page.getByTestId('member-first-name-input').locator('input').fill(member1FirstName);
    // await page.getByTestId('member-last-name-input').locator('input').fill(member1LastName);
    // await page.locator('.mdi-calendar').first().click();
    // await page.locator('button[class*="v-date-picker-month__day-btn"]').first().click();
    // await page.getByTestId('member-gender-select').click();
    // await page.getByRole('option', { name: 'Nam' }).click();
    // await page.getByTestId('member-family-select').click();
    // await page.waitForSelector('.v-overlay-container .v-list-item');
    // await page.locator('.v-overlay-container .v-list-item').first().click();
    // await page.waitForTimeout(1000);
    // await page.getByTestId('save-member-button').click();
    // await page.waitForLoadState('networkidle');
    // await expect(page.locator('[data-testid="snackbar-success"]')).toBeVisible();
    // console.log('Đã tạo thành viên 1.');

    // console.log('Tạo thành viên 2.');
    // await page.getByTestId('add-new-member-button').click();
    // await page.waitForLoadState('networkidle');
    // await page.getByTestId('member-first-name-input').locator('input').fill(member2FirstName);
    // await page.getByTestId('member-last-name-input').locator('input').fill(member2LastName);
    // await page.locator('.mdi-calendar').first().click();
    // await page.locator('button[class*="v-date-picker-month__day-btn"]').first().click();
    // await page.getByTestId('member-gender-select').click();
    // await page.getByRole('option', { name: 'Nam' }).click();
    // await page.getByTestId('member-family-select').click();
    // await page.waitForSelector('.v-overlay-container .v-list-item');
    // await page.locator('.v-overlay-container .v-list-item').first().click();
    // await page.waitForTimeout(1000);
    // await page.getByTestId('save-member-button').click();
    // await page.waitForLoadState('networkidle');
    // await expect(page.locator('[data-testid="snackbar-success"]')).toBeVisible();
    // console.log('Đã tạo thành viên 2.');

    // console.log('Tạo gia đình.');
    // await page.getByRole('link', { name: 'Quản lý gia đình/dòng họ' }).click();
    // await page.getByTestId('add-new-family-button').click();
    // await page.getByTestId('family-name-input').locator('input').fill(familyName);
    // await page.getByTestId('button-save').click();
    // await expect(page.locator('[data-testid="snackbar-success"]')).toBeVisible();
    // await page.waitForLoadState('networkidle');
    // console.log('Đã tạo gia đình.');

    // 2. Create a relationship
    console.log('Điều hướng đến trang quản lý Quan hệ để tạo mối quan hệ.');
    await page.getByRole('link', { name: 'Quản lý Quan hệ' }).click();
    await page.waitForLoadState('networkidle');
    await page.getByTestId('relationship-create-button').click();
    await page.waitForLoadState('networkidle');

    console.log('Điền thông tin mối quan hệ.');
    await page.getByTestId('relationship-source-member-autocomplete').click();
    await page.waitForTimeout(1000);
    await page.waitForSelector('.v-overlay-container .v-list-item');
    await page.locator('.v-overlay-container .v-list-item').nth(0).click();

    await page.getByTestId('relationship-target-member-autocomplete').click();
    await page.waitForTimeout(1000);
    await page.waitForSelector('.v-overlay-container .v-list-item');
    await page.locator('.v-overlay-container .v-list-item').nth(1).click();

    await page.getByTestId('relationship-type-select').click();
    await page.waitForSelector('.v-overlay-container .v-list-item');
    await page.locator('.v-overlay-container .v-list-item').nth(0).click();

    await page.getByTestId('relationship-family-autocomplete').click();
    await page.waitForTimeout(1000);
    await page.waitForSelector('.v-overlay-container .v-list-item');
    await page.locator('.v-overlay-container .v-list-item').nth(0).click();
    await page.waitForTimeout(1000);

    await page.getByTestId('relationship-add-save-button').click();
    await page.waitForLoadState('networkidle');
    await expect(page.locator('[data-testid="snackbar-success"]')).toBeVisible();
    console.log('Đã lưu mối quan hệ thành công.');

    // 3. Navigate to relationship list and search for the created relationship
    console.log('Điều hướng về danh sách mối quan hệ và tìm kiếm.');
    await page.getByRole('link', { name: 'Quản lý Quan hệ' }).click();
    await page.waitForLoadState('networkidle');
    await page.getByTestId('relationship-search').getByTestId('relationship-search-expand-button').click();
    await page.waitForTimeout(500);
    // Select the first source member in the search filter
    await page.getByTestId('relationship-search').getByTestId('relationship-search-source-member-autocomplete').click();
    await page.waitForSelector('.v-overlay-container .v-list-item');
    await page.locator('.v-overlay-container .v-list-item').nth(0).click();
    await page.getByTestId('relationship-search').getByTestId('relationship-search-apply-button').click();
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    await expect(page.locator('tr').filter({ hasText: relationshipType }).nth(0)).toBeVisible();
    console.log('Đã tìm thấy mối quan hệ.');

    console.log('Click nút xóa mối quan hệ.');
    // 4. Click delete button
    const relationshipRow = page.locator('tr').filter({ hasText: relationshipType }).nth(0);
    await relationshipRow.getByTestId('relationship-delete-button').click();

    console.log('Xác nhận xóa.');
    // 5. Confirm deletion
    await page.getByTestId('confirm-delete-button').click();

    console.log('Chờ thông báo thành công.');
    // 6. Verify success snackbar
    await expect(page.locator('[data-testid="snackbar-success"]')).toBeVisible();

    console.log('Xác minh mối quan hệ không còn hiển thị trong danh sách.');
    // 7. Verify relationship is no longer visible
    await expect(page.locator('tr').filter({ hasText: relationshipType }).nth(0)).not.toBeVisible();
    console.log('Đã xóa mối quan hệ.');
  });
});
