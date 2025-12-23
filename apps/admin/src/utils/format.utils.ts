import { format } from 'date-fns';

export function formatDate(dateString?: string | Date | null): string {
  if (!dateString) return '';
  const date = typeof dateString === 'string' ? new Date(dateString) : dateString;
  if (isNaN(date.getTime())) return ''; // Handle invalid date
  return format(date, 'dd/MM/yyyy'); // Format as dd/MM/yyyy
}

export function formatBytes(bytes: number, decimals = 2): string {
  if (bytes === 0) return '0 Bytes';

  const k = 1024;
  const dm = decimals < 0 ? 0 : decimals;
  const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];

  const i = Math.floor(Math.log(bytes) / Math.log(k));

  return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
}

/**
 * Formats a Date or date string into a localized date and time string.
 * @param dateInput The date or date string to format.
 * @param locale The locale to use for formatting (default: 'en-US').
 * @param options Intl.DateTimeFormatOptions for customization.
 * @returns A formatted date and time string.
 */
export function formatLocalizedDateTime(
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
export function formatLocalizedDate(
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