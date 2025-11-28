import type { MemberStoryDto } from '@/types/memberStory.d'; // Updated
import type { ApiError } from '@/plugins/axios';
import type { Result, Paginated } from '@/types';
import type { ICrudService } from '../common/crud.service.interface';

// Define a filter interface for member stories if needed for searching/filtering // Updated
export interface MemberStoryFilter { // Updated
  memberId?: string; // Filter by member
  searchQuery?: string; // Search by title/story
  // Add other filters as needed (e.g., date range, tags)
  sortBy?: string; // Column name to sort by
  sortOrder?: 'asc' | 'desc'; // Sort order
}

export interface IMemberStoryService extends ICrudService<MemberStoryDto> { // Updated
  // Overriding add/update to use specific DTOs
  add(newItem: MemberStoryDto): Promise<Result<MemberStoryDto, ApiError>>; // Updated
  update(updatedItem: MemberStoryDto): Promise<Result<MemberStoryDto, ApiError>>; // Updated

  // Overriding fetch (or defining a specific loadItems for lists)
  loadItems(
    filters: MemberStoryFilter, // Updated
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<MemberStoryDto>, ApiError>>; // Updated

  getMemberStoriesByMemberId( // Updated
    memberId: string,
    page: number,
    itemsPerPage: number,
    filters: MemberStoryFilter, // Updated
  ): Promise<Result<Paginated<MemberStoryDto>, ApiError>>;
  // Removed analyzePhoto and generateStory methods
}