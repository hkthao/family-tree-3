import type { MemoryItem, ListOptions, Paginated } from '@/types'; // Changed PaginatedList to Paginated
import type { Result } from '@/types/result.d';
import type { EmotionalTag } from '@/types/enums';
import type { ApiError } from '@/types/apiError.d'; // Import ApiError

export interface MemoryItemFilter {
  searchTerm?: string;
  startDate?: Date;
  endDate?: Date;
  emotionalTag?: EmotionalTag;
  memberId?: string;
}

export interface IMemoryItemService {
  searchMemoryItems(
    familyId: string,
    options: ListOptions,
    filters: MemoryItemFilter,
  ): Promise<Result<Paginated<MemoryItem>, ApiError>>; // Added type arguments to Result
  getMemoryItemById(familyId: string, id: string): Promise<Result<MemoryItem, ApiError>>; // Added type arguments to Result
  createMemoryItem(familyId: string, memoryItem: Omit<MemoryItem, 'id' | 'created' | 'createdBy' | 'lastModified' | 'lastModifiedBy' | 'media' | 'persons'> & { media?: { mediaType: number, url: string }[], persons?: { memberId: string }[] }): Promise<Result<string, ApiError>>; // Added type arguments to Result
  updateMemoryItem(familyId: string, memoryItem: Omit<MemoryItem, 'created' | 'createdBy' | 'lastModified' | 'lastModifiedBy'>): Promise<Result<MemoryItem, ApiError>>; // Added type arguments to Result
  deleteMemoryItem(familyId: string, id: string): Promise<Result<void, ApiError>>; // Added type arguments to Result
}
