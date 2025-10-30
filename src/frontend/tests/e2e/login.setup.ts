import { test, expect } from '@playwright/test';
import { E2E_BASE_URL } from './e2e.constants';

export async function login(page: any) {
  await page.goto(`${E2E_BASE_URL}`);
  await page.waitForTimeout(5000);
  await page.getByRole('textbox', { name: 'Email address' }).click();
  await page.getByRole('textbox', { name: 'Email address' }).fill('thao.hk90@gmail.com');
  await page.getByRole('textbox', { name: 'Email address' }).press('Tab');
  await page.getByRole('textbox', { name: 'Password' }).fill('123456?Aa');
  await page.getByRole('button', { name: 'Continue', exact: true }).click();
  await page.waitForTimeout(1000);
}
