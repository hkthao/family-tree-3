import type { MemberStoryDto } from '@/types/memberStory.d'; // Updated
import type { ApiError } from '@/plugins/axios';
import type { Result, Paginated, SearchStoriesFilter } from '@/types'; // Import SearchStoriesFilter from '@/types'
import type { ICrudService } from '../common/crud.service.interface';

export interface IMemberStoryService extends ICrudService<MemberStoryDto> { // Updated
  // Overriding add/update to use specific DTOs
  add(newItem: MemberStoryDto): Promise<Result<MemberStoryDto, ApiError>>; // Updated
  update(updatedItem: MemberStoryDto): Promise<Result<MemberStoryDto, ApiError>>; // Updated

  // Overriding fetch (or defining a specific loadItems for lists)
  loadItems(
    filters: SearchStoriesFilter, // Updated
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<MemberStoryDto>, ApiError>>; // Updated

  searchMemberStories( // Updated
    filters: SearchStoriesFilter, // Updated
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<MemberStoryDto>, ApiError>>;
  // Removed analyzePhoto and generateStory methods
}