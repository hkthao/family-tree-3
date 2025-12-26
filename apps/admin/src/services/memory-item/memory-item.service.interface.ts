import type { MemoryItem, EmotionalTag, AddMemoryItemDto, UpdateMemoryItemDto } from '@/types';
import type { ICrudService } from '@/services/common/crud.service.interface';
import type { FilterOptions } from '@/types'; // Import FilterOptions

export interface MemoryItemFilter extends FilterOptions {
  searchQuery?: string;
  startDate?: Date;
  endDate?: Date;
  emotionalTag?: EmotionalTag;
  memberId?: string;
}

export interface IMemoryItemService extends ICrudService<MemoryItem, AddMemoryItemDto, UpdateMemoryItemDto> {
  // `ICrudService` already provides search, getById, add, update, delete, getByIds
  // If specific search functionality with MemoryItemFilter is needed, it would be added here
  // but for now, we're strictly following the user's instruction to be like family-location
  // which implies no additional methods beyond ICrudService.
}
