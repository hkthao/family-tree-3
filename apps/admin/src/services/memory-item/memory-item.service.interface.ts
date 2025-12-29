import type { MemoryItem, EmotionalTag, AddMemoryItemDto, UpdateMemoryItemDto } from '@/types';
import type { ICrudService } from '@/services/common/crud.service.interface';
import type { FilterOptions } from '@/types';
import type { Result, ApiError } from '@/types';

export interface MemoryItemFilter extends FilterOptions {
  searchQuery?: string;
  startDate?: Date;
  endDate?: Date;
  emotionalTag?: EmotionalTag;
  memberId?: string;
}

export interface IMemoryItemService extends ICrudService<MemoryItem, AddMemoryItemDto, UpdateMemoryItemDto> {
  exportMemoryItems(familyId?: string): Promise<Result<string, ApiError>>;
  importMemoryItems(familyId: string, payload: any): Promise<Result<void, ApiError>>;
}
