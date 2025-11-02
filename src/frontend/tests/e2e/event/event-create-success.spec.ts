import { test, expect } from '@playwright/test';
import { login } from '../login.setup';
import { fillVuetifyInput, fillVuetifyTextarea, selectVuetifyOption, waitForSnackbar, takeScreenshotOnFailure, waitForVDataTableLoaded, fillVuetifyDateInput } from '../helpers/vuetify';

test.describe('Event Management - Create Event - Success Case', () => {
  test.beforeEach(async ({ page }) => {
    console.log('Đăng nhập trước khi chạy mỗi bài kiểm thử.');
    await login(page);
  });

  test.afterEach(async ({ page }, testInfo) => {
    console.log('Chụp ảnh màn hình nếu bài kiểm thử thất bại.');
    await takeScreenshotOnFailure(page, testInfo);
  });

  test('should create event successfully', async ({ page }) => {
    const eventName = `Sự kiện ${new Date().getTime()}`;
    const eventLocation = 'Địa điểm sự kiện';
    const eventDescription = 'Mô tả sự kiện';
    const eventStartDate = '01/01/2025';
    const eventEndDate = '01/02/2025';

    console.log('Bước 1: Điều hướng đến trang quản lý Sự kiện và tạo mới.');
    await Promise.all([
      page.waitForURL('**/event'),
      page.getByRole('link', { name: 'Danh sách Sự kiện' }).click(),
    ]);

    await Promise.all([
      page.waitForURL('**/event/add'),
      page.getByTestId('add-new-event-button').click(),
    ]);

    const eventFormLocator = page.getByTestId('event-form');
    await expect(eventFormLocator).toBeVisible({ timeout: 10000 });

    console.log('Điền thông tin sự kiện.');
    await fillVuetifyInput(page, 'event-name-input', eventName);
    await selectVuetifyOption(page, 'event-type-select', 0); // Assuming 'Birth' is the first option
    await selectVuetifyOption(page, 'event-family-autocomplete', 0); // Assuming a family exists
    await fillVuetifyDateInput(page, 'event-start-date-input', eventStartDate);
    await fillVuetifyDateInput(page, 'event-end-date-input', eventEndDate);
    await fillVuetifyInput(page, 'event-location-input', eventLocation);
    await fillVuetifyTextarea(page, 'event-description-input', eventDescription);
    // No color picker interaction for now, as it's more complex

    console.log('Click nút "Lưu".');
    await page.getByTestId('event-add-save-button').click();
    await waitForSnackbar(page, 'success');
    console.log('✅ Đã tạo sự kiện thành công.');

    console.log('Bước 2: Xác minh sự kiện mới hiển thị trong danh sách.');
    // Assuming EventSearch has an expand button and search input
    // await page.getByTestId('event-search-expand-button').click(); // Uncomment if needed
    // await fillVuetifyInput(page, 'event-search-input', eventName); // Uncomment if needed
    // await page.getByTestId('apply-filters-button').click(); // Uncomment if needed
    await waitForVDataTableLoaded(page);
    await expect(page.locator('tr').filter({ hasText: eventName })).toBeVisible();
    console.log('✅ Đã xác minh sự kiện mới.');
  });
});