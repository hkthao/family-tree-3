import { test, expect } from '@playwright/test';
import { E2E_BASE_URL, E2E_ROUTES } from '../e2e.constants';
import { login } from '../login.setup';

test.describe('Family Management - Create Family', () => {
  test('should allow a user to create a new family tree', async ({ page }) => {

    test.beforeEach(async ({ page }) => {
      await login(page);
    });

    var familyName = `'e2e Family ${new Date().getTime()}`
    await page.getByRole('link', { name: 'Quản lý gia đình/dòng họ' }).click();
    await page.goto(`${E2E_BASE_URL}${E2E_ROUTES.FAMILY_MANAGEMENT}`);
    await page.getByTestId('add-new-family-button').click();
    await page.getByRole('textbox', { name: 'Tên Tên' }).click();
    await page.getByRole('textbox', { name: 'Tên Tên' }).fill(familyName);
    await page.getByRole('textbox', { name: 'Địa chỉ Địa chỉ' }).click();
    await page.getByRole('textbox', { name: 'Địa chỉ Địa chỉ' }).fill(familyName);
    await page.getByRole('textbox', { name: 'Mô tả Mô tả' }).click();
    await page.getByRole('textbox', { name: 'Mô tả Mô tả' }).fill(familyName);
    await page.getByTestId('button-save').click();
    await expect(page.getByText(familyName)).toBeVisible();
  });
});