import { test, expect } from '@playwright/test';
import { login } from '../login.setup';

test.describe('Family Management - Create Family', () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test('should allow a user to create a new family tree', async ({ page }) => {
    const familyName = `e2e Family ${new Date().getTime()}`
    const address = `e2e address ${new Date().getTime()}`
    const description = `e2e descriptions ${new Date().getTime()}`
    await page.getByRole('link', { name: 'Quản lý gia đình/dòng họ' }).click();
    await page.getByTestId('add-new-family-button').click();

    await page.getByTestId('family-name-input').click();
    await page.getByTestId('family-name-input').locator('input').fill(familyName);

    await page.getByTestId('family-address-input').click();
    await page.getByTestId('family-address-input').locator('input').fill(address);

    await page.getByTestId('family-description-input').click();
    await page.getByTestId('family-description-input').locator('textarea').fill(description);

    await page.getByTestId('family-visibility-select').click();
    await page.waitForSelector('.v-overlay-container .v-list-item');
    await page.locator('.v-overlay-container .v-list-item').first().click();


    await page.getByTestId('family-managers-select').click();
    await page.waitForSelector('.v-overlay-container .v-list-item');
    await page.locator('.v-overlay-container .v-list-item').first().click();

    await page.getByTestId('family-viewers-select').click();
    await page.waitForSelector('.v-overlay-container .v-list-item');
    await page.locator('.v-overlay-container .v-list-item').first().click();

    await page.getByTestId('button-save').click();
    await expect(page.locator('[data-testid="snackbar-success"]')).toBeVisible();

    await page.waitForLoadState('networkidle');

    await page.getByTestId('family-search-expand-button').click();
    await page.waitForTimeout(500)
    await page.getByTestId('family-search-input').locator('input').fill(familyName);
    await page.getByTestId('apply-filters-button').click();
    await page.waitForLoadState('networkidle');
    await expect(page.getByText(familyName)).toBeVisible();
  });
});