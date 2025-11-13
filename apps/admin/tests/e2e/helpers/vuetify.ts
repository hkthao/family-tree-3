import { Page, expect, TestInfo } from '@playwright/test';

/**
 * Selects an option from a Vuetify autocomplete component identified by its data-testid.
 * @param page Playwright Page object.
 * @param testId The data-testid of the Vuetify autocomplete component.
 * @param searchValue The value to type into the autocomplete input.
 * @param optionText The text of the option to select.
 */
export async function selectVuetifyAutocompleteOption(page: Page, testId: string, searchValue: string, optionText: string) {
  console.log(`Chọn tùy chọn '${optionText}' từ autocomplete có data-testid='${testId}' sau khi tìm kiếm '${searchValue}'.`);
  const autocomplete = page.getByTestId(testId);
  const autocompleteInput = autocomplete.locator('input');

  // 1. Điền toàn bộ giá trị tìm kiếm vào ô input một cách đáng tin cậy.
  await autocompleteInput.fill(searchValue);

  // 2. Chờ cho thanh tiến trình (loading) của autocomplete biến mất,
  //    đảm bảo rằng quá trình tìm kiếm đã hoàn tất.
  await expect(autocomplete.locator('.v-field__loader')).toBeHidden({ timeout: 10000 });

  // 3. Tìm và nhấp vào tùy chọn mong muốn trong danh sách kết quả.
  //    Sử dụng locator('.v-overlay-container') để đảm bảo chúng ta đang tìm trong dropdown đang mở.
  await page.waitForSelector('.v-overlay-container .v-list-item');
  const optionLocator = page.locator('.v-overlay-container').locator('.v-list-item', { hasText: optionText });
  await optionLocator.click({ delay: 300 });
}


/**
 * Fills a Vuetify text input identified by its data-testid.
 * @param page Playwright Page object.
 * @param testId The data-testid of the Vuetify input component.
 * @param value The value to fill into the input.
 */
export async function fillVuetifyInput(page: Page, testId: string, value: string) {
  console.log(`Điền giá trị '${value}' vào trường input có data-testid='${testId}'.`);
  await page.getByTestId(testId).click();
  await page.getByTestId(testId).locator('input').fill(value);
}

/**
 * Fills a Vuetify textarea identified by its data-testid.
 * @param page Playwright Page object.
 * @param testId The data-testid of the Vuetify textarea component.
 * @param value The value to fill into the textarea.
 */
export async function fillVuetifyTextarea(page: Page, testId: string, value: string) {
  console.log(`Điền giá trị '${value}' vào trường textarea có data-testid='${testId}'.`);
  await page.getByTestId(testId).click();
  await page.getByTestId(testId).locator('textarea').fill(value);
}

/**
 * Selects an option from a Vuetify dropdown (v-select) identified by its data-testid.
 * Assumes the dropdown opens a v-overlay-container with v-list-item elements.
 * @param page Playwright Page object.
 * @param testId The data-testid of the Vuetify select component.
 * @param optionIndex The 0-based index of the option to select.
 */
export async function selectVuetifyOption(page: Page, testId: string, optionIndex: number) {
  console.log(`Chọn tùy chọn thứ ${optionIndex} từ dropdown có data-testid='${testId}'.`);
  await page.getByTestId(testId).click();
  await page.waitForSelector('.v-overlay-container .v-list-item');
  await page.locator(`.v-overlay-container .v-list-item`).nth(optionIndex).click({ delay: 500 });
}

/**
 * Asserts that a validation message is displayed for a given Vuetify input/field.
 * @param page Playwright Page object.
 * @param testId The data-testid of the Vuetify input/field component.
 * @param expectedText The expected validation message text.
 */
export async function assertValidationMessage(page: Page, testId: string) {
  console.log(`Kiểm tra thông báo lỗi cho trường có data-testid='${testId}'`);

  // Lấy input
  const input = page.getByTestId(testId).locator('input, textarea');

  // Lấy id của div message từ aria-describedby
  const messageId = await input.getAttribute('aria-describedby');
  if (!messageId) throw new Error(`Không tìm thấy aria-describedby cho ${testId}`);

  // Locator message
  await expect(page.locator(`#${messageId}`)).toBeVisible();
}

/**
 * Waits for a Vuetify snackbar to appear and become visible.
 * @param page Playwright Page object.
 * @param type The type of snackbar to wait for (e.g., 'success', 'error').
 */
export async function waitForSnackbar(page: Page, type: 'success' | 'error' = 'success') {
  console.log(`Chờ snackbar loại '${type}' hiển thị.`);
  await expect(page.locator(`[data-testid="snackbar-${type}"]`)).toBeVisible({ timeout: 10000 });
}

/**
 * Takes a screenshot if the test fails.
 * This function should typically be called in an `afterEach` hook.
 * @param page Playwright Page object.
 * @param testInfo Playwright TestInfo object.
 */
export async function takeScreenshotOnFailure(page: Page, testInfo: TestInfo) {
  if (testInfo.status !== testInfo.expectedStatus) {
    const screenshotPath = testInfo.outputPath(`failure-${testInfo.title.replace(/\s/g, '-')}.png`);
    console.log(`Chụp ảnh màn hình khi thất bại: ${screenshotPath}`);
    await page.screenshot({ path: screenshotPath, fullPage: true });
  }
}


export async function waitForVDataTableLoaded(page: Page) {
  // Kiểm tra có spinner / progress indicator không
  const loadingLocator = page.locator('.v-progress-linear, .v-progress-circular, [data-testid="table-loading"]');

  // Nếu có phần tử loading, chờ nó biến mất
  const isVisible = await loadingLocator.isVisible().catch(() => false);
  if (isVisible) {
    await expect(loadingLocator).toBeHidden({ timeout: 10000 });
  }

  // Chờ ít nhất 1 hàng xuất hiện
  const firstRow = page.locator('table tbody tr').first();
  await expect(firstRow).toBeVisible({ timeout: 10000 });
}

/**
 * Fills a Vuetify date input field identified by its data-testid.
 * Assumes the date input uses a v-text-field and expects a 'YYYY-MM-DD' format.
 * @param page Playwright Page object.
 * @param testId The data-testid of the Vuetify date input component.
 * @param value The date value to fill into the input (format: 'YYYY-MM-DD').
 */
/**
 * Fills a Vuetify date input field identified by its data-testid by interacting with the date picker.
 * Assumes the date input uses a v-text-field that opens a v-menu with a v-date-picker.
 * @param page Playwright Page object.
 * @param testId The data-testid of the Vuetify date input component (the v-text-field).
 * @param value The date value to select (format: 'YYYY-MM-DD').
 */
export async function fillVuetifyDateInput(page: Page, testId: string, value: string) {
  console.log(`Chọn ngày '${value}' từ trường ngày có data-testid='${testId}'.`);

  // 1️⃣ Click vào input để mở date picker
  await page.getByTestId(testId).locator('input').click();

  // 2️⃣ Chờ overlay hiện ra
  await page.waitForSelector('.v-overlay-container .v-date-picker-month__day-btn:visible');

  // 3️⃣ Lấy số ngày từ value (format 'YYYY-MM-DD')
  const day = parseInt(value.split('-')[2], 10);

  // 4️⃣ Click vào nút ngày visible trong picker
  await page
    .locator('.v-overlay-container .v-date-picker-month__day-btn:visible')
    .nth(day-1) // đảm bảo pick nút visible đầu tiên
    .click();
}
