import type { ApiClientMethods } from '@/plugins/axios';
import { ApiCrudService } from '@/services/common/api.crud.service';
import type { IMemoryItemService } from './memory-item.service.interface';
import type { MemoryItem, AddMemoryItemDto, UpdateMemoryItemDto } from '@/types';
import type { Result, ApiError } from '@/types';

export class ApiMemoryItemService extends ApiCrudService<MemoryItem, AddMemoryItemDto, UpdateMemoryItemDto> implements IMemoryItemService {
  constructor(apiClient: ApiClientMethods) {
    super(apiClient, 'memory-items');
  }

  async exportMemoryItems(familyId?: string): Promise<Result<string, ApiError>> {
    const url = `${this.baseUrl}/export`;
    return this.api.get<string>(url, { params: { familyId } });
  }

  async importMemoryItems(familyId: string, payload: any): Promise<Result<void, ApiError>> {
    const url = `${this.baseUrl}/import/${familyId}`;
    return this.api.post<void>(url, payload);
  }
}
