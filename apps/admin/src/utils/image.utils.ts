// apps/admin/src/utils/image.utils.ts

/**
 * Prepends the base64 image header to a base64 string if it's not already present.
 * This is necessary for `v-img` and `<img>` tags to correctly render base64 images.
 * Assumes JPEG format if no specific format is detected in the string.
 *
 * @param base64String The raw base64 string of the image.
 * @returns A data URI string (e.g., "data:image/jpeg;base64,...") or undefined if the input is invalid.
 */
export function createBase64ImageSrc(base64String: string | undefined | null): string | undefined {
  if (!base64String) {
    return undefined;
  }
  // Check if the string already contains a data URI header
  if (base64String.startsWith('data:image')) {
    return base64String;
  }
  // Assume JPEG if no specific format is provided or detected
  return `data:image/jpeg;base64,${base64String}`;
}
