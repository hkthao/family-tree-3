// apps/admin/src/services/prompt/prompt.service.interface.ts

import type { ApiError } from '@/plugins/axios';
import type { Result, Paginated } from '@/types';
import type { Prompt, PromptFilter } from '@/types/prompt';

/**
 * @interface IPromptService
 * @brief Giao diện định nghĩa các phương thức để tương tác với các Prompt.
 */
export interface IPromptService {
  /**
   * @brief Lấy danh sách Prompts đã phân trang dựa trên các tiêu chí lọc.
   * @param {PromptFilter} filters - Đối tượng chứa các tiêu chí lọc.
   * @param {number} page - Số trang hiện tại.
   * @param {number} itemsPerPage - Số mục trên mỗi trang.
   * @returns {Promise<Result<Paginated<Prompt>, ApiError>>} - Danh sách Prompts đã phân trang.
   */
  getPaginated(
    filters: PromptFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<Prompt>, ApiError>>;

  /**
   * @brief Lấy thông tin chi tiết của một Prompt bằng ID hoặc Code.
   * @param {string} id - ID của Prompt.
   * @param {string} [code] - Code của Prompt (tùy chọn).
   * @returns {Promise<Result<Prompt>>} - Thông tin chi tiết của Prompt.
   */
  getById(id: string, code?: string): Promise<Result<Prompt>>;

  /**
   * @brief Thêm một Prompt mới.
   * @param {Omit<Prompt, 'id'>} prompt - Đối tượng Prompt cần thêm (không bao gồm ID).
   * @returns {Promise<Result<string>>} - ID của Prompt vừa được tạo.
   */
  add(prompt: Omit<Prompt, 'id'>): Promise<Result<string>>;

  /**
   * @brief Cập nhật thông tin của một Prompt hiện có.
   * @param {Prompt} prompt - Đối tượng Prompt cần cập nhật.
   * @returns {Promise<Result>} - Kết quả của thao tác cập nhật.
   */
  update(prompt: Prompt): Promise<Result<void>>;

  /**
   * @brief Xóa một Prompt bằng ID.
   * @param {string} id - ID của Prompt cần xóa.
   * @returns {Promise<Result>} - Kết quả của thao tác xóa.
   */
  delete(id: string): Promise<Result<void>>;
}
