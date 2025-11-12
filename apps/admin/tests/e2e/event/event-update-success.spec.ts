import { test, expect } from '@playwright/test';
import { login } from '../login.setup';
import { fillVuetifyInput, fillVuetifyTextarea, selectVuetifyOption, waitForSnackbar, takeScreenshotOnFailure, waitForVDataTableLoaded, fillVuetifyDateInput } from '../helpers/vuetify';

test.describe('Event Management - Update Event - Success Case', () => {
  test.beforeEach(async ({ page }) => {
    console.log('Đăng nhập trước khi chạy mỗi bài kiểm thử.');
    await login(page);
  });

  test.afterEach(async ({ page }, testInfo) => {
    console.log('Chụp ảnh màn hình nếu bài kiểm thử thất bại.');
    await takeScreenshotOnFailure(page, testInfo);
  });

  test('should allow a user to update an existing event successfully', async ({ page }) => {
    const originalEventName = `e2e Original Event ${new Date().getTime()}`;
    const updatedEventName = `e2e Updated Event ${new Date().getTime()}`;
    const originalLocation = `e2e Original Location ${new Date().getTime()}`;
    const updatedLocation = `e2e Updated Location ${new Date().getTime()}`;
    const originalDescription = `e2e Original Description ${new Date().getTime()}`;
    const updatedDescription = `e2e Updated Description ${new Date().getTime()}`;
    const eventStartDate = '2025-01-01';
    const eventEndDate = '2025-01-02';

    console.log('Bước 1: Tạo dữ liệu sự kiện gốc.');
    await Promise.all([
      page.waitForURL('**/event'),
      page.getByRole('link', { name: 'Danh sách Sự kiện' }).click(),
    ]);

    await Promise.all([
      page.waitForURL('**/event/add'),
      page.getByTestId('add-new-event-button').click(),
    ]);

    await fillVuetifyInput(page, 'event-name-input', originalEventName);
    await selectVuetifyOption(page, 'event-type-select', 0);
    await selectVuetifyOption(page, 'event-family-autocomplete', 0);
    await fillVuetifyDateInput(page, 'event-start-date-input', eventStartDate);
    await fillVuetifyDateInput(page, 'event-end-date-input', eventEndDate);
    await fillVuetifyInput(page, 'event-location-input', originalLocation);
    await fillVuetifyTextarea(page, 'event-description-input', originalDescription);

    await page.getByTestId('event-add-save-button').click();
    await waitForSnackbar(page, 'success');
    console.log('✅ Đã tạo sự kiện gốc thành công.');

    console.log('Bước 2: Tìm và điều hướng đến trang chỉnh sửa sự kiện.');
    // Assuming EventSearch has an expand button and search input
    // await page.getByTestId('event-search-expand-button').click(); // Uncomment if needed
    // await fillVuetifyInput(page, 'event-search-input', originalEventName); // Uncomment if needed
    // await page.getByTestId('apply-filters-button').click(); // Uncomment if needed
    await waitForVDataTableLoaded(page);
    const eventRow = page.locator('tr', { hasText: originalEventName });
    await expect(eventRow).toBeVisible();

    await Promise.all([
      page.waitForURL(/.*\/event\/detail\/.*/),
      eventRow.getByTestId('event-name-link').click(),
    ]);

    await Promise.all([
      page.waitForURL(/.*\/event\/edit\/.*/),
      page.getByTestId('event-detail-edit-button').click(),
    ]);
    console.log('Đã vào trang chỉnh sửa sự kiện.');

    console.log('Bước 3: Cập nhật thông tin sự kiện.');
    await fillVuetifyInput(page, 'event-name-input', updatedEventName);
    await fillVuetifyInput(page, 'event-location-input', updatedLocation);
    await fillVuetifyTextarea(page, 'event-description-input', updatedDescription);

    await page.getByTestId('event-edit-save-button').click();
    await waitForSnackbar(page, 'success');
    console.log('✅ Đã lưu dữ liệu cập nhật thành công.');

    console.log('Bước 4: Xác minh sự kiện đã được cập nhật trong danh sách.');
    // Assuming EventSearch has an expand button and search input
    // await page.getByTestId('event-search-expand-button').click(); // Uncomment if needed
    // await fillVuetifyInput(page, 'event-search-input', updatedEventName); // Uncomment if needed
    // await page.getByTestId('apply-filters-button').click(); // Uncomment if needed
    await waitForVDataTableLoaded(page);
    await expect(page.locator('tr').filter({ hasText: updatedEventName })).toBeVisible();
    console.log('✅ Đã cập nhật sự kiện thành công.');
  });
});