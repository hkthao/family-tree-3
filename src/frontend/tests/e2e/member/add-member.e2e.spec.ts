import { test, expect } from '@playwright/test';
import { login } from '../login.setup';

test.describe('Member Management - Add Member', () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test('should allow a user to add a member to the family tree', async ({ page }) => {

    // 2. Navigate to the member list page and click "Add New Member"
    await page.getByRole('link', { name: 'Quản lý thành viên' }).click(); // Navigate to member list
    await page.waitForLoadState('networkidle');
    await page.getByTestId('add-new-member-button').click();
    await page.waitForLoadState('networkidle');

    // 3. Fill in member details
    const memberFirstName = 'Thành viên';
    const memberLastName = `Test ${new Date().getTime()}`;
    const memberNickname = 'Biệt danh';
    const memberPlaceOfBirth = 'Hà Nội';
    const memberPlaceOfDeath = 'TP. Hồ Chí Minh';
    const memberOccupation = 'Kỹ sư';
    const memberBiography = 'Đây là tiểu sử của thành viên.';

    await page.getByTestId('member-first-name-input').locator('input').fill(memberFirstName);
    await page.getByTestId('member-last-name-input').locator('input').fill(memberLastName);
    await page.getByTestId('member-nickname-input').locator('input').fill(memberNickname);

    // await page.getByTestId('member-date-of-birth-input').click();
    // await page.getByTestId('member-date-of-death-input').locator('input').fill(memberDateOfDeath);
    await page.locator('.mdi-calendar').first().click();
    await page.locator('button[class="v-btn v-btn--icon v-theme--dark v-btn--density-default v-btn--size-default v-btn--variant-text v-date-picker-month__day-btn"]').first().click();

    await page.locator('.mdi-calendar').nth(1).click();
    await page.locator('button[class="v-btn v-btn--icon v-theme--dark v-btn--density-default v-btn--size-default v-btn--variant-text v-date-picker-month__day-btn"]').last().click();

    await page.getByTestId('member-place-of-birth-input').locator('input').fill(memberPlaceOfBirth);
    await page.getByTestId('member-place-of-death-input').locator('input').fill(memberPlaceOfDeath);

    await page.getByTestId('member-occupation-input').locator('input').fill(memberOccupation);
    // await page.getByTestId('member-biography-input').locator('textarea').fill(memberBiography);

    await page.getByTestId('member-gender-select').click();
    await page.getByRole('option', { name: 'Nam' }).click(); // Assuming 'Nam' is the text for Male

    // Select the family for the member
    await page.getByTestId('member-family-select').click(); // Assuming a data-testid for family select
    await page.waitForSelector('.v-overlay-container .v-list-item');
    await page.locator('.v-overlay-container .v-list-item').first().click();

    // 4. Save the new member
    await page.getByTestId('save-member-button').click();
    await page.waitForLoadState('networkidle'); // Should navigate back to member list after saving member

    // 5. Verify the new member is visible
    await page.getByTestId('member-search-expand-button').click();
    await page.waitForTimeout(500)
    await page.getByTestId('member-search-input').locator('input').fill(memberFirstName);
    await page.getByTestId('apply-filters-button').click();
    await page.waitForLoadState('networkidle');
    await expect(page.getByText(`${memberFirstName}`)).toBeVisible();
    await expect(page.getByText(`${memberLastName}`)).toBeVisible();

    console.log('Đã thêm thành viên.');
  });
});
