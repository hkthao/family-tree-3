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
    .replace(/[̀-ͯ]/g, '');
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

/**
 * Converts a basic markdown string (supporting bullet points and newlines) to HTML.
 * - Converts '*' or '-' prefixed lines to <ul><li>.
 * - Converts newlines to <p> tags if not part of a list.
 * - Converts **bold** text to <strong>.
 * @param markdown The input markdown string.
 * @returns The HTML string.
 */
export function convertMarkdownToHtml(markdown: string): string {
  if (!markdown) {
    return '';
  }

  let html = markdown;

  // Convert bold text (e.g., **text**) to <strong>text</strong>
  html = html.replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>');

  // Convert bullet points (* or - followed by a space) to <li> within <ul>
  // This regex looks for lines starting with * or - and a space, and captures the rest of the line.
  // It handles multiple list items together.
  html = html.replace(/^(?:[*-]\s+(.*(?:\n(?![*-]\s+).*)*))+/gm, (match) => {
    const listItems = match.split('\n').map(line => {
      const trimmedLine = line.replace(/^[*-]\s*/, '').trim();
      return trimmedLine ? `<li>${trimmedLine}</li>` : '';
    }).filter(Boolean).join(''); // Filter out empty li tags from blank lines

    return listItems ? `<ul>${listItems}</ul>` : '';
  });

  // Convert remaining newlines to <p> tags, but avoid wrapping already processed <ul> tags or empty lines
  // This needs to be done carefully to not break the lists
  html = html.split('\n').map(line => {
    if (line.trim().startsWith('<ul>') || line.trim().startsWith('<li')) { // Don't wrap list items
      return line;
    }
    const trimmedLine = line.trim();
    return trimmedLine ? `<p>${trimmedLine}</p>` : '';
  }).filter(Boolean).join('');

  // Clean up any empty <p></p> tags that might have been introduced
  html = html.replace(/<p>\s*<\/p>/g, '');
  
  return html;
}