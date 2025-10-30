import { test, expect, Page } from '@playwright/test';
import { E2E_BASE_URL, E2E_ROUTES } from '../e2e.constants';
import { login } from '../login.setup';

test.describe('Family Management - View Family', () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test('should allow a user to view family tree details', async ({ page }) => {
    const familyName = `e2e View Family ${new Date().getTime()}`;
    const address = `e2e address ${new Date().getTime()}`;
    const description = `e2e descriptions ${new Date().getTime()}`;

    // Create a family first
    await page.getByRole('link', { name: 'Quản lý gia đình/dòng họ' }).click();
    await page.getByTestId('add-new-family-button').click();
    await page.getByTestId('family-name-input').click();
    await page.getByTestId('family-name-input').locator('input').fill(familyName);
    await page.getByTestId('family-address-input').click();
    await page.getByTestId('family-address-input').locator('input').fill(address);
    await page.getByTestId('family-description-input').click();
    await page.getByTestId('family-description-input').locator('textarea').fill(description);
    await page.getByTestId('button-save').click();

    await page.getByTestId('family-search-expand-button').click();
    await page.waitForTimeout(500)
    await page.getByTestId('family-search-input').locator('input').fill(familyName);
    await page.getByTestId('apply-filters-button').click();
    await page.waitForLoadState('networkidle');
    await expect(page.getByText(familyName)).toBeVisible();
    await page.getByText(familyName).click();
    await page.waitForLoadState('networkidle')

    await expect(page.getByTestId('family-description-input').locator('textarea')).toHaveValue(description);
    await expect(page.getByTestId('family-name-input').locator('input')).toHaveValue(familyName);
    await expect(page.getByTestId('family-address-input').locator('input')).toHaveValue(address);
    console.log('Đã xem chi tiết cây gia phả.');
  });
});
