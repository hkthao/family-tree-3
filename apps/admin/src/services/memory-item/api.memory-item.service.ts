import type { IMemoryItemService, MemoryItemFilter } from './memory-item.service.interface';
import type { ApiClientMethods } from '@/plugins/axios';
import type { MemoryItem, ListOptions, Paginated } from '@/types'; // Changed PaginatedList to Paginated
import { type Result, err } from '@/types/result.d'; // Corrected import for Result and err function
import type { ApiError } from '@/types/apiError.d'; // Corrected import for ApiError
import dayjs from 'dayjs'; // Import dayjs

export class ApiMemoryItemService implements IMemoryItemService {
  constructor(protected api: ApiClientMethods) { }

  async searchMemoryItems(
    familyId: string,
    options: ListOptions = { page: 1, itemsPerPage: 10, sortBy: [] },
    filters: MemoryItemFilter = {},
  ): Promise<Result<Paginated<MemoryItem>, ApiError>> { // Added type arguments to Result
    const params: Record<string, any> = {
      pageNumber: options.page,
      pageSize: options.itemsPerPage,
    };

    if (options.sortBy && options.sortBy.length > 0) {
      params.orderBy = `${options.sortBy[0].key}${options.sortBy[0].order === 'desc' ? 'Desc' : 'Asc'}`;
    }

    if (filters.searchTerm) {
      params.searchTerm = filters.searchTerm;
    }
    if (filters.startDate) {
      params.startDate = dayjs(filters.startDate).format('YYYY-MM-DD');
    }
    if (filters.endDate) {
      params.endDate = dayjs(filters.endDate).format('YYYY-MM-DD');
    }
    if (filters.emotionalTag !== undefined) {
      params.emotionalTag = filters.emotionalTag;
    }
    if (filters.memberId) {
      params.memberId = filters.memberId;
    }

    return await this.api.get<Paginated<MemoryItem>>(`/family/${familyId}/memory-items`, { params });
  }

  async getMemoryItemById(familyId: string, id: string): Promise<Result<MemoryItem, ApiError>> { // Added type arguments to Result
    return await this.api.get<MemoryItem>(`/family/${familyId}/memory-items/${id}`);
  }

  async createMemoryItem(familyId: string, memoryItem: Omit<MemoryItem, 'id' | 'created' | 'createdBy' | 'lastModified' | 'lastModifiedBy' | 'media' | 'persons'> & { media?: { mediaType: number, url: string }[], persons?: { memberId: string }[] }): Promise<Result<string, ApiError>> { // Added type arguments to Result
    return await this.api.post<string>(`/family/${familyId}/memory-items`, { ...memoryItem, familyId });
  }

  async updateMemoryItem(familyId: string, memoryItem: Omit<MemoryItem, 'created' | 'createdBy' | 'lastModified' | 'lastModifiedBy'>): Promise<Result<MemoryItem, ApiError>> { // Added type arguments to Result
    if (!memoryItem.id) {
      return err({ message: 'MemoryItem ID is required for update.', name: 'ValidationError' }); // Replaced Result.failure with err and constructed ApiError
    }
    return await this.api.put(`/family/${familyId}/memory-items/${memoryItem.id}`, { ...memoryItem, familyId });
  }

  async deleteMemoryItem(familyId: string, id: string): Promise<Result<void, ApiError>> { // Added type arguments to Result
    return await this.api.delete(`/family/${familyId}/memory-items/${id}`);
  }
}
