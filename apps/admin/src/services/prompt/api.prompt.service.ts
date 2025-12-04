// apps/admin/src/services/prompt/api.prompt.service.ts

import type { ApiClientMethods, ApiError } from '@/plugins/axios';
import type { IPromptService } from './prompt.service.interface';
import type { Prompt, PromptFilter } from '@/types/prompt';
import type { Paginated, Result } from '@/types';

/**
 * @class ApiPromptService
 * @implements {IPromptService}
 * @brief Triển khai IPromptService sử dụng các phương thức API thực tế.
 */
export class ApiPromptService implements IPromptService {
  private readonly BASE_URL = '/api/prompts';

  constructor(private readonly apiClient: ApiClientMethods) {}

  /**
   * @brief Lấy danh sách Prompts đã phân trang dựa trên các tiêu chí lọc.
   * @param {PromptFilter} filters - Đối tượng chứa các tiêu chí lọc.
   * @param {number} page - Số trang hiện tại.
   * @param {number} itemsPerPage - Số mục trên mỗi trang.
   * @returns {Promise<Result<Paginated<Prompt>, ApiError>>} - Danh sách Prompts đã phân trang.
   */
  async getPaginated(
    filters: PromptFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<Prompt>, ApiError>> {
    const params: Record<string, any> = {
      page,
      itemsPerPage,
    };

    if (filters.sortBy) {
      params.sortBy = filters.sortBy;
    }
    if (filters.sortOrder) {
      params.sortOrder = filters.sortOrder;
    }

    if (filters.searchQuery) {
      params.searchQuery = filters.searchQuery;
    }

    return await this.apiClient.get<Paginated<Prompt>>(`${this.BASE_URL}/search`, { params });
  }

  /**
   * @brief Lấy thông tin chi tiết của một Prompt bằng ID hoặc Code.
   * @param {string} id - ID của Prompt.
   * @param {string} [code] - Code của Prompt (tùy chọn).
   * @returns {Promise<Result<Prompt>>} - Thông tin chi tiết của Prompt.
   */
  async getById(id: string, code?: string): Promise<Result<Prompt>> {
    if (code) {
      return await this.apiClient.get<Prompt>(`${this.BASE_URL}/by-code/${code}`);
    }
    return await this.apiClient.get<Prompt>(`${this.BASE_URL}/${id}`);
  }

  /**
   * @brief Thêm một Prompt mới.
   * @param {Omit<Prompt, 'id'>} prompt - Đối tượng Prompt cần thêm (không bao gồm ID).
   * @returns {Promise<Result<string>>} - ID của Prompt vừa được tạo.
   */
  async add(prompt: Omit<Prompt, 'id'>): Promise<Result<string>> {
    return await this.apiClient.post<string>(this.BASE_URL, prompt);
  }

  /**
   * @brief Cập nhật thông tin của một Prompt hiện có.
   * @param {Prompt} prompt - Đối tượng Prompt cần cập nhật.
   * @returns {Promise<Result>>} - Kết quả của thao tác cập nhật.
   */
  async update(prompt: Prompt): Promise<Result<void>> {
    return await this.apiClient.put<void>(`${this.BASE_URL}/${prompt.id}`, prompt);
  }

  /**
   * @brief Xóa một Prompt bằng ID.
   * @param {string} id - ID của Prompt cần xóa.
   * @returns {Promise<Result>>} - Kết quả của thao tác xóa.
   */
  async delete(id: string): Promise<Result<void>> {
    return await this.apiClient.delete<void>(`${this.BASE_URL}/${id}`);
  }
}
