
/**
 * @interface Prompt
 * @brief Định nghĩa cấu trúc dữ liệu cho một Prompt.
 *
 * @property {string} id - ID duy nhất của Prompt.
 * @property {string} code - Mã code của Prompt, duy nhất và không được để trống.
 * @property {string} title - Tiêu đề của Prompt, không được để trống.
 * @property {string} content - Nội dung của Prompt, không được để trống.
 * @property {string} [description] - Mô tả chi tiết của Prompt (tùy chọn).
 */
export interface Prompt {
  id: string;
  code: string;
  title: string;
  content: string;
  description?: string;
}

/**
 * @interface PromptFilter
 * @brief Định nghĩa các tiêu chí lọc và phân trang cho danh sách Prompts.
 *
 * @property {string} [searchQuery] - Chuỗi tìm kiếm toàn văn trong các trường của Prompt.
 */
export interface PromptFilter {
  searchQuery?: string;
  sortBy?: string;
  sortOrder?: string;
}
