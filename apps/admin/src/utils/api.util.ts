// apps/admin/src/utils/api.util.ts

/**
 * Lấy giá trị biến môi trường từ cấu hình runtime hoặc biến môi trường Vite.
 * Ưu tiên `window.runtimeConfig` (được cấu hình bởi Nginx),
 * sau đó fallback về `import.meta.env` (được cấu hình trong quá trình build Vite).
 *
 * @param {string} key Tên biến môi trường (ví dụ: 'VITE_API_BASE_URL').
 * @returns {string | undefined} Giá trị của biến môi trường hoặc undefined nếu không tìm thấy.
 */
export function getEnvVariable(key: string): string | undefined {
  // @ts-expect-error: window.runtimeConfig is injected by nginx at runtime
  if (window.runtimeConfig && window.runtimeConfig[key]) {
    // @ts-expect-error: window.runtimeConfig is injected by nginx at runtime
    return window.runtimeConfig[key];
  }
  return import.meta.env[key];
}

/**
 * Lấy URL API gốc từ cấu hình runtime hoặc biến môi trường.
 * Ưu tiên `window.runtimeConfig?.VITE_API_BASE_URL` (được cấu hình bởi Nginx),
 * sau đó fallback về `import.meta.env.VITE_API_BASE_URL` (được cấu hình trong quá trình build Vite).
 *
 * @returns {string} URL API gốc.
 */
export function getApiBaseUrl(): string {
  return getEnvVariable('VITE_API_BASE_URL') || '';
}

/**
 * Tạo URL API đầy đủ.
 *
 * @param {string} path Đường dẫn API tương đối (ví dụ: '/auth/login').
 * @returns {string} URL API đầy đủ.
 */
export function getApiUrl(path: string): string {
  const baseUrl = getApiBaseUrl();
  // Đảm bảo baseUrl không kết thúc bằng '/' và path không bắt đầu bằng '/' để tránh trùng lặp
  const trimmedBaseUrl = baseUrl.endsWith('/') ? baseUrl.slice(0, -1) : baseUrl;
  const trimmedPath = path.startsWith('/') ? path.slice(1) : path;
  return `${trimmedBaseUrl}/${trimmedPath}`;
}
