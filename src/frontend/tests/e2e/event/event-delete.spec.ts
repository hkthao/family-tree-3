import { test, expect } from '@playwright/test';
import { login } from '../login.setup';
import { fillVuetifyInput, fillVuetifyTextarea, selectVuetifyOption, waitForSnackbar, takeScreenshotOnFailure, waitForVDataTableLoaded, fillVuetifyDateInput } from '../helpers/vuetify';

test.describe('Event Management - Delete Event', () => {
  test.beforeEach(async ({ page }) => {
    console.log('Đăng nhập trước khi chạy mỗi bài kiểm thử.');
    await login(page);
  });

  test.afterEach(async ({ page }, testInfo) => {
    console.log('Chụp ảnh màn hình nếu bài kiểm thử thất bại.');
    await takeScreenshotOnFailure(page, testInfo);
  });

  test('should allow a user to delete an event', async ({ page }) => {
    const eventName = `e2e Delete Event ${new Date().getTime()}`;
    const eventLocation = `e2e location ${new Date().getTime()}`;
    const eventDescription = `e2e descriptions ${new Date().getTime()}`;
    const eventStartDate = '2025-01-01';
    const eventEndDate = '2025-02-28';

    console.log('Bước 1: Tạo dữ liệu sự kiện cần xóa.');
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

    console.log('Bước 2: Tìm sự kiện vừa tạo.');
    await page.getByTestId('event-search-expand-button').click();
    await fillVuetifyInput(page, 'event-search-query-input', eventName);
    await page.getByTestId('event-search-apply-button').click();
    await waitForVDataTableLoaded(page);
    await expect(page.locator('tr').filter({ hasText: eventName })).toBeVisible();
    console.log('✅ Đã tìm thấy sự kiện.');

    console.log('Bước 3: Xóa sự kiện.');
    await page.locator(`[data-testid="delete-event-button"]`).first().click(); // Click the first delete button

    const confirmationDialog = page.locator('.v-dialog');
    await expect(confirmationDialog).toBeVisible();
    await confirmationDialog.getByTestId('confirm-delete-button').click();
    console.log('Bước 4: Xác minh sự kiện đã bị xóa.');
    await waitForSnackbar(page, 'success');
    await expect(confirmationDialog).toBeHidden();
    await expect(page.getByText(eventName)).not.toBeVisible();
    console.log('✅ Đã xóa sự kiện.');
  });
});