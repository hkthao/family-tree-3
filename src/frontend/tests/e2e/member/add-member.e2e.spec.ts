import { test, expect } from '@playwright/test';
import { login } from '../login.setup';

test.describe('Member Management - Add Member', () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test('should allow a user to add a member to the family tree', async ({ page }) => {

    // 2. Navigate to the member list page and click "Add New Member"
    console.log('Điều hướng đến trang quản lý thành viên.');
    await page.getByRole('link', { name: 'Quản lý thành viên' }).click(); // Navigate to member list
    await page.waitForLoadState('networkidle');
    console.log('Click nút "Thêm thành viên mới".');
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

    console.log('Điền thông tin thành viên.');
    await page.getByTestId('member-first-name-input').locator('input').fill(memberFirstName);
    await page.getByTestId('member-last-name-input').locator('input').fill(memberLastName);
    await page.getByTestId('member-nickname-input').locator('input').fill(memberNickname);
    await page.waitForTimeout(500)

    console.log('Chọn ngày sinh.');
    await page.locator('.mdi-calendar').first().click();
    await page.locator('button[class="v-btn v-btn--icon v-theme--dark v-btn--density-default v-btn--size-default v-btn--variant-text v-date-picker-month__day-btn"]').first().click();

    console.log('Chọn ngày mất.');
    await page.locator('.mdi-calendar').nth(1).click();
    await page.locator('button[class="v-btn v-btn--icon v-theme--dark v-btn--density-default v-btn--size-default v-btn--variant-text v-date-picker-month__day-btn"]').last().click();

    await page.getByTestId('member-place-of-birth-input').locator('input').fill(memberPlaceOfBirth);
    await page.getByTestId('member-place-of-death-input').locator('input').fill(memberPlaceOfDeath);

    await page.getByTestId('member-occupation-input').locator('input').fill(memberOccupation);
    // await page.getByTestId('member-biography-input').locator('textarea').fill(memberBiography);

    console.log('Chọn giới tính.');
    await page.getByTestId('member-gender-select').click();
    await page.getByRole('option', { name: 'Nam' }).click(); // Assuming 'Nam' is the text for Male

    console.log('Chọn gia đình.');
    // Select the family for the member
    await page.getByTestId('member-family-select').click(); // Assuming a data-testid for family select
    await page.waitForSelector('.v-overlay-container .v-list-item');
    await page.locator('.v-overlay-container .v-list-item').first().click();
    await page.waitForTimeout(1000)

    // 4. Save the new member
    console.log('Click nút "Lưu thành viên".');
    await page.getByTestId('save-member-button').click();
    await page.waitForLoadState('networkidle'); // Should navigate back to member list after saving member
    await expect(page.locator('[data-testid="snackbar-success"]')).toBeVisible();
    console.log('Đã lưu thành viên thành công.');

    // 5. Verify the new member is visible
    console.log('Mở rộng bộ lọc tìm kiếm.');
    await page.getByTestId('member-search-expand-button').click();
    await page.waitForTimeout(500)
    console.log('Điền tên thành viên vào ô tìm kiếm.');
    await page.getByTestId('member-search-input').locator('input').fill(memberLastName);
    console.log('Click nút "Áp dụng bộ lọc".');
    await page.getByTestId('apply-filters-button').click();
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000)
    console.log('Xác minh thành viên mới được tạo hiển thị trong danh sách.');
    await expect(page.getByText(`${memberLastName} ${memberFirstName}`)).toBeVisible();
    console.log('Đã thêm thành viên.');
  });
});
