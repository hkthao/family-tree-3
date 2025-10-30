import { test, expect } from '@playwright/test';
import { login } from '../login.setup';

test.describe('Family Management - Update Family', () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test('should allow a user to update an existing family tree', async ({ page }) => {
    const originalFamilyName = `e2e Original Family ${new Date().getTime()}`;
    const updatedFamilyName = `e2e Updated Family ${new Date().getTime()}`;
    const originalAddress = `e2e Original Address ${new Date().getTime()}`;
    const updatedAddress = `e2e Updated Address ${new Date().getTime()}`;
    const originalDescription = `e2e Original Description ${new Date().getTime()}`;
    const updatedDescription = `e2e Updated Description ${new Date().getTime()}`;

    console.log('Điều hướng đến trang quản lý gia đình/dòng họ để tạo gia đình gốc.');
    // 1. Vao danh sach
    await page.getByRole('link', { name: 'Quản lý gia đình/dòng họ' }).click();
    await page.getByTestId('add-new-family-button').click();
    await page.getByTestId('family-name-input').locator('input').fill(originalFamilyName);
    await page.getByTestId('family-address-input').locator('input').fill(originalAddress);
    await page.getByTestId('family-description-input').locator('textarea').fill(originalDescription);
    await page.getByTestId('button-save').click();
    console.log('Đã tạo gia đình gốc.');

    console.log('Tìm kiếm gia đình gốc.');
    // 2. Tim kiem
    await page.waitForLoadState('networkidle');
    await page.getByTestId('family-search-expand-button').click();
    await page.waitForTimeout(500)
    await page.getByTestId('family-search-input').locator('input').fill(originalFamilyName);
    await page.getByTestId('apply-filters-button').click();
    await page.waitForLoadState('networkidle');
    await expect(page.getByText(originalFamilyName)).toBeVisible();
    console.log('Đã tìm thấy gia đình gốc.');

    console.log('Click vào gia đình gốc để xem chi tiết.');
    // 2. click vao 1 item xem chi tiet
    await page.getByText(originalFamilyName).click();
    await page.waitForLoadState('networkidle');

    console.log('Click nút "Cập nhật".');
    // 3. click vao 1 nut cap nhat
    await page.getByTestId('button-edit').click();
    await page.waitForLoadState('networkidle');

    console.log('Nhập dữ liệu cập nhật.');
    // 4. nhap du lieu
    await page.getByTestId('family-name-input').locator('input').fill(updatedFamilyName);
    await page.getByTestId('family-address-input').locator('input').fill(updatedAddress);
    await page.getByTestId('family-description-input').locator('textarea').fill(updatedDescription);
    await page.getByTestId('button-save').click();
    await page.waitForLoadState('networkidle');
    console.log('Đã lưu dữ liệu cập nhật.');

    console.log('Tìm kiếm gia đình đã cập nhật.');
    // 4. ve trang danh sach tim kiem theo du lieu vua cap nhat
    await page.waitForLoadState('networkidle');
    await page.getByTestId('family-search-expand-button').click();
    await page.waitForTimeout(500)
    await page.getByTestId('family-search-input').locator('input').fill(updatedFamilyName);
    await page.getByTestId('apply-filters-button').click();
    await page.waitForLoadState('networkidle');
    await expect(page.getByText(updatedFamilyName)).toBeVisible();
    console.log('Đã xác minh gia đình đã được cập nhật.');

    console.log('Đã cập nhật cây gia phả.');

  });
});