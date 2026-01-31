/**
 * Removes properties with undefined or null values from an object.
 * Mutates the original object.
 * @param {object} obj - The object to clean.
 */
function removeUndefinedProps(obj) {
  if (obj && typeof obj === 'object') {
    Object.keys(obj).forEach(key => {
      if (obj[key] === null || obj[key] === undefined) {
        delete obj[key];
      } else if (typeof obj[key] === 'object' && !Array.isArray(obj[key])) {
        // Recursively clean nested objects, but not arrays
        removeUndefinedProps(obj[key]);
        // If a nested object becomes empty after cleaning, delete it
        if (Object.keys(obj[key]).length === 0) {
          delete obj[key];
        }
      }
    });
  }
}

module.exports = {
  removeUndefinedProps,
};
