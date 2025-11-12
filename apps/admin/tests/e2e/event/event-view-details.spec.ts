import { test, expect } from '@playwright/test';
import { login } from '../login.setup';
import { fillVuetifyInput, fillVuetifyTextarea, selectVuetifyOption, waitForSnackbar, takeScreenshotOnFailure, waitForVDataTableLoaded, fillVuetifyDateInput } from '../helpers/vuetify';

test.describe('Event Management - View Event Details', () => {
  test.beforeEach(async ({ page }) => {
    console.log('Đăng nhập trước khi chạy mỗi bài kiểm thử.');
    await login(page);
  });

  test.afterEach(async ({ page }, testInfo) => {
    console.log('Chụp ảnh màn hình nếu bài kiểm thử thất bại.');
    await takeScreenshotOnFailure(page, testInfo);
  });

  test('should allow a user to view event details', async ({ page }) => {
    const eventName = `e2e View Event ${new Date().getTime()}`;
    const eventLocation = `e2e location ${new Date().getTime()}`;
    const eventDescription = `e2e descriptions ${new Date().getTime()}`;
    const eventStartDate = '2025-01-01';
    const eventEndDate = '2025-01-02';

    console.log('Bước 1: Tạo dữ liệu sự kiện.');
    await Promise.all([
      page.waitForURL('**/event'),
      page.getByRole('link', { name: 'Danh sách Sự kiện' }).click(),
    ]);

    await Promise.all([
      page.waitForURL('**/event/add'),
      page.getByTestId('add-new-event-button').click(),
    ]);

    await fillVuetifyInput(page, 'event-name-input', eventName);
    await selectVuetifyOption(page, 'event-type-select', 0);
    await selectVuetifyOption(page, 'event-family-autocomplete', 0);
    await fillVuetifyDateInput(page, 'event-start-date-input', eventStartDate);
    await fillVuetifyDateInput(page, 'event-end-date-input', eventEndDate);
    await fillVuetifyInput(page, 'event-location-input', eventLocation);
    await fillVuetifyTextarea(page, 'event-description-input', eventDescription);

    await page.getByTestId('event-add-save-button').click();
    await waitForSnackbar(page, 'success');
    console.log('✅ Đã tạo sự kiện thành công.');

    console.log('Bước 2: Tìm và điều hướng đến trang chi tiết sự kiện.');
    // Assuming EventSearch has an expand button and search input
    // await page.getByTestId('event-search-expand-button').click(); // Uncomment if needed
    // await fillVuetifyInput(page, 'event-search-input', eventName); // Uncomment if needed
    // await page.getByTestId('apply-filters-button').click(); // Uncomment if needed
    await waitForVDataTableLoaded(page);
    const eventRow = page.locator('tr', { hasText: eventName });
    await expect(eventRow).toBeVisible();

    await Promise.all([
      page.waitForURL(/.*\/event\/detail\/.*/),
      eventRow.getByTestId('event-name-link').click(),
    ]);
    console.log('Đã vào trang chi tiết sự kiện.');

    console.log('Bước 3: Xác minh các trường thông tin hiển thị đúng.');
    await expect(page.getByTestId('event-name-input').locator('input')).toHaveValue(eventName);
    await expect(page.getByTestId('event-location-input').locator('input')).toHaveValue(eventLocation);
    await expect(page.getByTestId('event-description-input').locator('textarea')).toHaveValue(eventDescription);
    console.log('✅ Đã xem chi tiết sự kiện.');
  });
});