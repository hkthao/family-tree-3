import { test, expect } from '@playwright/test';
import { login } from '../login.setup';

test.describe('Member Management - Add Member', () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test('should allow a user to add a member to the family tree', async ({ page }) => {
    const familyName = `e2e Member Family ${new Date().getTime()}`;
    const address = `e2e Member Address ${new Date().getTime()}`;
    const description = `e2e Member Description ${new Date().getTime()}`;

    // 1. Create a family first
    await page.getByRole('link', { name: 'Quản lý gia đình/dòng họ' }).click();
    await page.getByTestId('add-new-family-button').click();
    await page.getByTestId('family-name-input').locator('input').fill(familyName);
    await page.getByTestId('family-address-input').locator('input').fill(address);
    await page.getByTestId('family-description-input').locator('textarea').fill(description);
    await page.getByTestId('button-save').click();
    await page.waitForLoadState('networkidle');

    // Search for the newly created family
    await page.getByTestId('family-search-expand-button').click();
    await page.waitForTimeout(500); // Small wait for expand animation
    await page.getByTestId('family-search-input').locator('input').fill(familyName);
    await page.getByTestId('apply-filters-button').click();
    await page.waitForLoadState('networkidle');

    await expect(page.getByText(familyName)).toBeVisible();

    // 2. Navigate to the member list page and click "Add New Member"
    await page.getByRole('link', { name: 'Quản lý thành viên' }).click(); // Navigate to member list
    await page.waitForLoadState('networkidle');
    await page.getByTestId('add-new-member-button').click();
    await page.waitForLoadState('networkidle');

    // 3. Fill in member details
    const memberFirstName = 'Thành viên';
    const memberLastName = `Test ${new Date().getTime()}`;
    await page.getByTestId('member-first-name-input').locator('input').fill(memberFirstName);
    await page.getByTestId('member-last-name-input').locator('input').fill(memberLastName);
    await page.getByTestId('member-gender-select').click();
    await page.getByRole('option', { name: 'Nam' }).click(); // Assuming 'Nam' is the text for Male

    // Select the family for the member
    await page.getByTestId('member-family-select').click(); // Assuming a data-testid for family select
    await page.waitForSelector('.v-overlay-container .v-list-item');
    await page.getByText(familyName).click(); // Select the created family

    // 4. Save the new member
    await page.getByTestId('button-save').click();
    await page.waitForLoadState('networkidle'); // Should navigate back to member list after saving member

    // 5. Verify the new member is visible
    await expect(page.getByText(`${memberFirstName} ${memberLastName}`)).toBeVisible();
  });
});
