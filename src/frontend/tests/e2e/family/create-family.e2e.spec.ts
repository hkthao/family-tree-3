import { test, expect } from '@playwright/test';
import { E2E_BASE_URL, E2E_ROUTES } from '../e2e.constants';
import { login } from '../login.setup';

test.describe('Family Management - Create Family', () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test('should allow a user to create a new family tree', async ({ page }) => {
    var familyName = `e2e Family ${new Date().getTime()}`
    var address = `e2e address ${new Date().getTime()}`
    var description = `e2e descriptions ${new Date().getTime()}`
    await page.getByRole('link', { name: 'Quản lý gia đình/dòng họ' }).click();
    await page.goto(`${E2E_BASE_URL}${E2E_ROUTES.FAMILY_MANAGEMENT}`);
    await page.getByTestId('add-new-family-button').click();

    await page.getByTestId('family-name-input').click();
    await page.getByTestId('family-name-input').locator('input').fill(familyName);

    await page.getByTestId('family-address-input').click();
    await page.getByTestId('family-address-input').locator('input').fill(address);

    await page.getByTestId('family-description-input').click();
    await page.getByTestId('family-description-input').locator('textarea').fill(description);

    await page.getByTestId('button-save').click();

    await expect(page.getByText(familyName)).toBeVisible();
  });
});