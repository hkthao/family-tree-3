import { test, expect } from '@playwright/test';
import { login } from '../login.setup';

test.describe('Member Management - Delete Member', () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test('should allow a user to delete a member', async ({ page }) => {
    const memberFirstName = 'Thành viên';
    const memberLastName = `Xóa ${new Date().getTime()}`;
    const memberNickname = 'Biệt danh xóa';
    const memberPlaceOfBirth = 'Hà Nội';
    const memberPlaceOfDeath = 'TP. Hồ Chí Minh';
    const memberOccupation = 'Kỹ sư';

    console.log('Điều hướng đến trang quản lý thành viên để tạo thành viên.');
    // 1. Create a new member first
    await page.getByRole('link', { name: 'Quản lý thành viên' }).click();
    await page.waitForLoadState('networkidle');
    await page.getByTestId('add-new-member-button').click();
    await page.waitForLoadState('networkidle');

    await page.getByTestId('member-first-name-input').locator('input').fill(memberFirstName);
    await page.getByTestId('member-last-name-input').locator('input').fill(memberLastName);
    await page.getByTestId('member-nickname-input').locator('input').fill(memberNickname);
    await page.waitForTimeout(500);

    await page.locator('.mdi-calendar').first().click();
    await page.locator('button[class*="v-date-picker-month__day-btn"]').first().click();

    await page.locator('.mdi-calendar').nth(1).click();
    await page.locator('button[class*="v-date-picker-month__day-btn"]').last().click();

    await page.getByTestId('member-place-of-birth-input').locator('input').fill(memberPlaceOfBirth);
    await page.getByTestId('member-place-of-death-input').locator('input').fill(memberPlaceOfDeath);
    await page.getByTestId('member-occupation-input').locator('input').fill(memberOccupation);

    await page.getByTestId('member-gender-select').click();
    await page.getByRole('option', { name: 'Nam' }).click();

    await page.getByTestId('member-family-select').click();
    await page.waitForSelector('.v-overlay-container .v-list-item');
    await page.locator('.v-overlay-container .v-list-item').first().click();
    await page.waitForTimeout(1000);

    await page.getByTestId('save-member-button').click();
    await page.waitForLoadState('networkidle');
    await expect(page.locator('[data-testid="snackbar-success"]')).toBeVisible();
    console.log('Đã thêm thành viên mới để xóa.');

    console.log('Điều hướng về danh sách thành viên và tìm kiếm.');
    // 2. Navigate back to member list and search for the new member
    await page.getByRole('link', { name: 'Quản lý thành viên' }).click();
    await page.waitForLoadState('networkidle');
    await page.getByTestId('member-search-expand-button').click();
    await page.waitForTimeout(500);
    await page.getByTestId('member-search-input').locator('input').fill(memberLastName);
    await page.getByTestId('apply-filters-button').click();
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    await expect(page.getByText(`${memberLastName} ${memberFirstName}`)).toBeVisible();
    console.log('Đã tìm thấy thành viên.');

    console.log('Click nút xóa thành viên.');
    // 3. Click delete button
    const memberRow = page.locator(`tr:has-text("${memberLastName} ${memberFirstName}")`);
    await memberRow.getByTestId('delete-member-button').click();

    console.log('Xác nhận xóa.');
    // 4. Confirm deletion
    await page.getByTestId('confirm-delete-button').click();

    console.log('Chờ thông báo thành công.');
    // 5. Verify success snackbar
    await expect(page.locator('[data-testid="snackbar-success"]')).toBeVisible();

    console.log('Xác minh thành viên không còn hiển thị trong danh sách.');
    // 6. Verify member is no longer visible
    await expect(page.getByText(`${memberLastName} ${memberFirstName}`)).not.toBeVisible();
    console.log('Đã xóa thành viên.');
  });
});
