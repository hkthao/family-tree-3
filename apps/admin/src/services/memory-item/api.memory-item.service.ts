import type { ApiClientMethods } from '@/plugins/axios';
import { ApiCrudService } from '@/services/common/api.crud.service';
import type { IMemoryItemService } from './memory-item.service.interface';
import type { MemoryItem } from '@/types';

export class ApiMemoryItemService extends ApiCrudService<MemoryItem> implements IMemoryItemService {
  constructor(apiClient: ApiClientMethods) {
    super(apiClient, 'memory-items');
  }
}
