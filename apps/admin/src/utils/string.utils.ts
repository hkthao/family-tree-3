// apps/admin/src/utils/string.utils.ts

/**
 * Removes diacritics (accent marks) from a string, making it suitable for
 * diacritic-insensitive comparisons.
 * @param text The input string.
 * @returns The string with diacritics removed.
 */
export function removeDiacritics(text: string): string {
  if (!text) {
    return '';
  }

  // Normalize to NFD (Canonical Decomposition) to separate base characters from diacritical marks
  // Then, remove all diacritical marks (Unicode category Mn)
  return text
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '');
}

/**
 * Performs a case-insensitive and diacritic-insensitive comparison of two strings.
 * This function can be used for client-side filtering or preprocessing before sending to a backend
 * that expects diacritics removed.
 * @param source The source string to search within.
 * @param value The value to search for.
 * @returns True if the source contains the value (case and diacritic-insensitive), false otherwise.
 */
export function containsDiacriticInsensitive(source: string, value: string): boolean {
  if (!source || !value) {
    return false;
  }

  const normalizedSource = removeDiacritics(source).toLowerCase();
  const normalizedValue = removeDiacritics(value).toLowerCase();

  return normalizedSource.includes(normalizedValue);
}
