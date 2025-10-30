import { test, expect } from '@playwright/test';
import { E2E_BASE_URL, E2E_ROUTES } from '../e2e.constants';
import { login } from '../login.setup';

test.describe('Member Management - Update Member', () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test('should allow a user to update a member', async ({ page }) => {
    const memberFirstName = 'Thành viên';
    const memberLastName = `Cập nhật ${new Date().getTime()}`;
    const memberNickname = 'Biệt danh';
    const memberPlaceOfBirth = 'Hà Nội';
    const memberPlaceOfDeath = 'TP. Hồ Chí Minh';
    const memberOccupation = 'Kỹ sư';
    const memberBiography = 'Đây là tiểu sử của thành viên.';

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
    console.log('Đã thêm thành viên mới để cập nhật.');

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

    // 3. Click the edit button for the newly created member
    // Find the row containing the member's full name and then click its edit button
    const memberRow = page.locator(`tr:has-text("${memberLastName} ${memberFirstName}")`);
    await memberRow.getByTestId('edit-member-button').click();
    await page.waitForLoadState('networkidle');
    console.log('Đã click nút chỉnh sửa thành viên.');

    // 4. Update member information
    const updatedNickname = 'Biệt danh mới';
    const updatedOccupation = 'Kỹ sư phần mềm';
    await page.getByTestId('member-nickname-input').locator('input').fill(updatedNickname);
    await page.getByTestId('member-occupation-input').locator('input').fill(updatedOccupation);
    console.log('Đã cập nhật thông tin thành viên.');

    // 5. Save the changes
    await page.getByTestId('save-member-button').click();
    await page.waitForLoadState('networkidle');
    await expect(page.locator('[data-testid="snackbar-success"]')).toBeVisible();
    console.log('Đã lưu thông tin thành viên cập nhật.');

    // 6. Verify the updated information
    // Navigate back to member list, search, and view details
    await page.getByRole('link', { name: 'Quản lý thành viên' }).click();
    await page.waitForLoadState('networkidle');
    await page.getByTestId('member-search-expand-button').click();
    await page.waitForTimeout(500);
    await page.getByTestId('member-search-input').locator('input').fill(memberLastName);
    await page.getByTestId('apply-filters-button').click();
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    await expect(page.getByText(`${memberLastName} ${memberFirstName}`)).toBeVisible();

    // Click on the member's full name to view details
    await page.getByText(`${memberLastName} ${memberFirstName}`).click();
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    await page.screenshot({
      path: "1.png",
      fullPage: true
    })

    await expect(page.getByTestId('member-nickname-input').locator('input')).toHaveValue(updatedNickname);
    await expect(page.getByTestId('member-occupation-input').locator('input')).toHaveValue(updatedOccupation);
    console.log('Đã xác minh thông tin thành viên đã được cập nhật.');
  });
});