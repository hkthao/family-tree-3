import { test, expect } from '@playwright/test';
import { login } from '../login.setup';
import { fillVuetifyInput, fillVuetifyTextarea, selectVuetifyOption, waitForSnackbar, takeScreenshotOnFailure,  selectVuetifyAutocompleteOption } from '../helpers/vuetify';

test.describe('Relationship Management - View Relationship Details', () => {
  test.beforeEach(async ({ page }) => {
    console.log('Đăng nhập trước khi chạy mỗi bài kiểm thử.');
    await login(page);
  });

  test.afterEach(async ({ page }, testInfo) => {
    console.log('Chụp ảnh màn hình nếu bài kiểm thử thất bại.');
    await takeScreenshotOnFailure(page, testInfo);
  });

  test('should display relationship details correctly', async ({ page }) => {
    const member1FirstName = 'Thành viên 1';
    const member1LastName = `RelView1 ${new Date().getTime()}`;
    const member2FirstName = 'Thành viên 2';
    const member2LastName = `RelView2 ${new Date().getTime()}`;
    const familyName = `Gia đình quan hệ ${new Date().getTime()}`;
    const relationshipType = 'Cha'; // Assuming this is the first option

    // Bước 1: Tạo dữ liệu cần thiết (gia đình và thành viên)
    console.log('Bước 1: Tạo dữ liệu cần thiết (gia đình và thành viên).');
    // Tạo gia đình
    await page.getByRole('link', { name: 'Quản lý gia đình/dòng họ' }).click();
    await page.getByTestId('add-new-family-button').click();
    await fillVuetifyInput(page, 'family-name-input', familyName);
    await fillVuetifyInput(page, 'family-address-input', 'Địa chỉ gia đình');
    await fillVuetifyTextarea(page, 'family-description-input', 'Mô tả gia đình');
    await selectVuetifyOption(page, 'family-visibility-select', 0);
    await selectVuetifyOption(page, 'family-managers-select', 0);
    await selectVuetifyOption(page, 'family-viewers-select', 0);
    await page.getByTestId('button-save').click();
    await waitForSnackbar(page, 'success');
    await page.waitForLoadState('networkidle');
    console.log('Đã tạo gia đình.');

    // Tạo thành viên 1
    await page.getByRole('link', { name: 'Quản lý thành viên' }).click();
    await page.getByTestId('add-new-member-button').click();
    await fillVuetifyInput(page, 'member-first-name-input', member1FirstName);
    await fillVuetifyInput(page, 'member-last-name-input', member1LastName);
    await page.locator('.mdi-calendar').first().click();
    await page.locator('button[class*="v-date-picker-month__day-btn"]').first().click();
    await selectVuetifyOption(page, 'member-gender-select', 0);
    await selectVuetifyAutocompleteOption(page, 'member-family-select', familyName, familyName);
    await page.getByTestId('save-member-button').click();
    await waitForSnackbar(page, 'success');
    await page.waitForLoadState('networkidle');
    console.log('Đã tạo thành viên 1.');

    // Tạo thành viên 2
    await page.getByTestId('add-new-member-button').click();
    await fillVuetifyInput(page, 'member-first-name-input', member2FirstName);
    await fillVuetifyInput(page, 'member-last-name-input', member2LastName);
    await page.locator('.mdi-calendar').first().click();
    await page.locator('button[class*="v-date-picker-month__day-btn"]').first().click();
    await selectVuetifyOption(page, 'member-gender-select', 0);
    await selectVuetifyAutocompleteOption(page, 'member-family-select', familyName, familyName);
    await page.getByTestId('save-member-button').click();
    await waitForSnackbar(page, 'success');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    console.log('Đã tạo thành viên 2.');

    // Tạo mối quan hệ
    console.log('Bước 2: Tạo mối quan hệ.');
    await page.getByRole('link', { name: 'Quản lý Quan hệ' }).first().waitFor({ state: 'visible' });
    await expect(page.getByRole('link', { name: 'Quản lý Quan hệ' })).toBeEnabled();
    await page.getByRole('link', { name: 'Quản lý Quan hệ' }).first().click();
    await page.waitForLoadState('networkidle');
    await page.getByTestId('relationship-create-button').waitFor({ state: 'visible' });
    await page.getByTestId('relationship-create-button').click();
    await page.waitForLoadState('networkidle');

    await page.waitForTimeout(1000); // Give some time for results to populate

    await selectVuetifyAutocompleteOption(page, 'relationship-family-autocomplete', familyName, familyName);
    await selectVuetifyAutocompleteOption(page, 'relationship-source-member-autocomplete', member1LastName, `${member1FirstName} ${member1LastName}`);
    await selectVuetifyAutocompleteOption(page, 'relationship-target-member-autocomplete', member2LastName, `${member2FirstName} ${member2LastName}`);
    await selectVuetifyOption(page, 'relationship-type-select', 0); // Chọn loại quan hệ đầu tiên
    await page.getByTestId('relationship-add-save-button').click();
    await waitForSnackbar(page, 'success');
    await page.waitForLoadState('networkidle');
    console.log('Đã tạo mối quan hệ thành công.');

    // Bước 3: Điều hướng đến trang chi tiết mối quan hệ
    console.log('Bước 3: Điều hướng đến trang chi tiết mối quan hệ.');
    await page.getByRole('link', { name: 'Quản lý Quan hệ' }).first().click();
    await page.waitForLoadState('networkidle');

    // Sử dụng bộ lọc tìm kiếm để tìm mối quan hệ vừa tạo
    console.log('Mở rộng bộ lọc tìm kiếm.');
    await page.getByTestId('relationship-search').getByTestId('relationship-search-expand-button').click();
    await page.waitForTimeout(500); // Chờ animation

    console.log('Chọn thành viên nguồn trong bộ lọc tìm kiếm.');
    await selectVuetifyAutocompleteOption(page, 'relationship-search-source-member-autocomplete', member1LastName, `${member1FirstName} ${member1LastName}`);

    console.log('Chọn thành viên đích trong bộ lọc tìm kiếm.');
    await selectVuetifyAutocompleteOption(page, 'relationship-search-target-member-autocomplete', member2LastName, `${member2FirstName} ${member2LastName}`);

    console.log('Click nút "Áp dụng bộ lọc".');
    await page.getByTestId('relationship-search').getByTestId('relationship-search-apply-button').click();
    await page.waitForLoadState('networkidle');

    // Tìm mối quan hệ vừa tạo trong danh sách và click vào nó
    const relationshipRow = page.locator('tr', { hasText: `${member1FirstName} ${member1LastName}` }).filter({ hasText: `${member2FirstName} ${member2LastName}` });
    await relationshipRow.waitFor({ state: 'visible' });
    await relationshipRow.locator('a', { hasText: relationshipType }).click();
    await page.waitForLoadState('networkidle');
    console.log('Đã điều hướng đến trang chi tiết mối quan hệ.');
    await page.waitForURL('**/relationship/detail/*');

    // Bước 4: Xác minh thông tin chi tiết mối quan hệ
    console.log('Bước 4: Xác minh thông tin chi tiết mối quan hệ.');
    await expect(page.getByTestId('relationship-family-autocomplete')).toContainText(familyName);
    await expect(page.getByTestId('relationship-source-member-autocomplete')).toContainText(`${member1FirstName} ${member1LastName}`);
    await expect(page.getByTestId('relationship-target-member-autocomplete')).toContainText(`${member2FirstName} ${member2LastName}`);
    await expect(page.getByTestId('relationship-type-select')).toContainText(relationshipType);
    console.log('Đã xác minh thông tin chi tiết mối quan hệ.');
  });
});
