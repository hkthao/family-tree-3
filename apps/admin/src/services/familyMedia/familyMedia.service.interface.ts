
import type { Result } from '@/types/result';
import type { ApiError } from '@/types/apiError';
import type { Paginated, ListOptions } from '@/types/pagination';
import type { MediaItem } from '@/types/media'; // Import MediaItem
import type { FamilyMediaFilter } from '@/types/familyMedia'; // Import FamilyMediaFilter

export interface IFamilyMediaService {
  search(
    listOptions: ListOptions,
    filters: FamilyMediaFilter,
  ): Promise<Result<Paginated<MediaItem>, ApiError>>;
  // Add other methods as needed (e.g., getById, create, update, delete)
}