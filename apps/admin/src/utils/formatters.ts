// apps/admin/src/utils/formatters.ts

/**
 * Formats a Date or date string into a localized date and time string.
 * @param dateInput The date or date string to format.
 * @param locale The locale to use for formatting (default: 'en-US').
 * @param options Intl.DateTimeFormatOptions for customization.
 * @returns A formatted date and time string.
 */
export function formatDateTime(
  dateInput: Date | string | null | undefined,
  locale: string = 'en-US',
  options: Intl.DateTimeFormatOptions = {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    hour12: false,
  },
): string {
  if (!dateInput) {
    return 'N/A';
  }
  const date = new Date(dateInput);
  if (isNaN(date.getTime())) {
    return 'Invalid Date';
  }
  return new Intl.DateTimeFormat(locale, options).format(date);
}

/**
 * Formats a Date or date string into a localized date string.
 * @param dateInput The date or date string to format.
 * @param locale The locale to use for formatting (default: 'en-US').
 * @param options Intl.DateTimeFormatOptions for customization.
 * @returns A formatted date string.
 */
export function formatDate(
  dateInput: Date | string | null | undefined,
  locale: string = 'en-US',
  options: Intl.DateTimeFormatOptions = {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  },
): string {
  if (!dateInput) {
    return 'N/A';
  }
  const date = new Date(dateInput);
  if (isNaN(date.getTime())) {
    return 'Invalid Date';
  }
  return new Intl.DateTimeFormat(locale, options).format(date);
}

/**
 * Formats a number as a currency string.
 * @param amount The amount to format.
 * @param locale The locale to use (e.g., 'en-US', 'vi-VN').
 * @param currency The currency code (e.g., 'USD', 'VND').
 * @returns A formatted currency string.
 */
export function formatCurrency(
  amount: number | null | undefined,
  locale: string = 'en-US',
  currency: string = 'USD',
): string {
  if (amount === null || amount === undefined) {
    return '';
  }
  return new Intl.NumberFormat(locale, { style: 'currency', currency }).format(amount);
}
