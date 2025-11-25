import type { MemoryDto } from '@/types/memory.d';
import type { ApiError } from '@/plugins/axios';
import type { Result, Paginated } from '@/types';
import type { ICrudService } from '../common/crud.service.interface';

// Define a filter interface for memories if needed for searching/filtering
export interface MemoryFilter {
  memberId?: string; // Filter by member
  searchQuery?: string; // Search by title/story
  // Add other filters as needed (e.g., date range, tags)
  sortBy?: string; // Column name to sort by
  sortOrder?: 'asc' | 'desc'; // Sort order
}

export interface IMemoryService extends ICrudService<MemoryDto> {
  // Overriding add/update to use specific DTOs
  add(newItem: MemoryDto): Promise<Result<MemoryDto, ApiError>>; // Changed from CreateMemoryDto
  update(updatedItem: MemoryDto): Promise<Result<MemoryDto, ApiError>>; // Changed from UpdateMemoryDto

  // Overriding fetch (or defining a specific loadItems for lists)
  loadItems(
    filters: MemoryFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<MemoryDto>, ApiError>>;

  getMemoriesByMemberId(
    memberId: string,
    page: number,
    itemsPerPage: number,
    filters: MemoryFilter,
  ): Promise<Result<Paginated<MemoryDto>, ApiError>>;
  analyzePhoto(command: FormData): Promise<Result<any, ApiError>>; // Replace 'any' with specific DTO
  generateStory(command: any): Promise<Result<any, ApiError>>; // Replace 'any' with specific DTO
}