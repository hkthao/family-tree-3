import type { ApiClientMethods } from '@/plugins/axios';
import { ApiCrudService } from '@/services/common/api.crud.service';
import type { IMemoryItemService } from './memory-item.service.interface';
import type { MemoryItem, AddMemoryItemDto, UpdateMemoryItemDto } from '@/types';

export class ApiMemoryItemService extends ApiCrudService<MemoryItem, AddMemoryItemDto, UpdateMemoryItemDto> implements IMemoryItemService {
  constructor(apiClient: ApiClientMethods) {
    super(apiClient, 'memory-items');
  }
}
